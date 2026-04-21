using EFCore.BulkExtensions;
using ErrorOr;
using LaptopServer.DB;
using LaptopServer.DTO.NovaPost;
using Microsoft.EntityFrameworkCore;
using System.Globalization;
using System.Threading.Channels;

namespace LaptopServer.Infrastructure.API.NovaPost
{
    public interface INovaPostDbService
    {
        Task<List<NpWarehouse>> GetWarehouses(string settlementRef, string? searchString = null, CancellationToken ct = default);
        Task SaveCounterparty(NpCounterpartyRes counterparty, CancellationToken ct = default);
        Task<ErrorOr<NpInternetDocReq>> FillInternetDoc(Guid orderId, CancellationToken ct = default);
        Task ReadChannelWarehouse(ChannelReader<NpWarehouse> channelReader, CancellationToken ct = default);
        Task SaveInternetDoc(NpInternetDocRes internetDoc, Guid orderId, CancellationToken ct = default);
    }

    public class NovaPostDbService(LaptopsDBContext dbContext, IServiceProvider serviceProvider) : INovaPostDbService
    {
        private static string FormatPhoneNumber(string? phoneNumber)
        {
            if (string.IsNullOrWhiteSpace(phoneNumber))
                return string.Empty;

            var digits = new string(phoneNumber.Where(char.IsDigit).ToArray());

            if (digits.Length == 9)
                digits = $"380{digits}";
            else if (digits.Length == 10 && digits.StartsWith("0"))
                digits = $"38{digits}";
            else if (digits.Length > 12)
            {
                var countryCodeIndex = digits.IndexOf("380", StringComparison.Ordinal);
                if (countryCodeIndex >= 0 && digits.Length - countryCodeIndex >= 12)
                    digits = digits.Substring(countryCodeIndex, 12);
            }

            return digits;
        }

        public async Task<ErrorOr<NpInternetDocReq>> FillInternetDoc(Guid orderId, CancellationToken ct = default)
        {
            var orderData = await dbContext.Orders
                .AsNoTracking()
                .Where(ord => ord.Id == orderId)
                .Select(ord => new
                {
                    ord.TotalPrice,
                    ord.NpRecipientRef,
                    ord.NpContactRecipientRef,
                    ord.DeliveryCityRef,
                    ord.DeliveryWarehouseRef,
                    PhoneNumber = ord.CustomerInfo.PhoneNumber,
                    TotalVolume = ord.OrderItems.Sum(item => item.VolumeGeneral * item.Quantity),
                    TotalWeight = ord.OrderItems.Sum(item => item.Weight * item.Quantity)
                })
                .FirstOrDefaultAsync(ct);

            if (orderData == null)
                return Error.NotFound(code: "OrderNotFound", description: "Order for internet document not found");

            var formattedPhone = FormatPhoneNumber(orderData.PhoneNumber);
            if (formattedPhone.Length != 12 || !formattedPhone.StartsWith("380", StringComparison.Ordinal))
                return Error.Validation(code: "InvalidPhoneNumber", description: $"Invalid phone number format for order '{orderId}'.");

            return new NpInternetDocReq
            {
                VolumeGeneral = orderData.TotalVolume <= 0? "0.0000": orderData.TotalVolume.ToString("0.0000", CultureInfo.InvariantCulture),

                Weight = orderData.TotalWeight <= 0? "0.00": orderData.TotalWeight.ToString("0.00", CultureInfo.InvariantCulture),

                Cost = Math.Ceiling(orderData.TotalPrice).ToString(CultureInfo.InvariantCulture),

                Recipient = orderData.NpRecipientRef!,
                ContactRecipient = orderData.NpContactRecipientRef!,
                RecipientsPhone = formattedPhone,
                CityRecipient = orderData.DeliveryCityRef!,
                RecipientAddress = orderData.DeliveryWarehouseRef!
            };
        }
        public async Task SaveCounterparty(NpCounterpartyRes counterparty, CancellationToken ct = default)
        {
            var contactRecipientRef = counterparty.ContactPerson?.Data?.FirstOrDefault()?.Ref;

            if (string.IsNullOrEmpty(contactRecipientRef))
                return;

            await dbContext.Orders
                .Where(ord => ord.Id == counterparty.OrderId)
                .ExecuteUpdateAsync(set => set
                    .SetProperty(ord => ord.NpRecipientRef, counterparty.Ref)
                    .SetProperty(ord => ord.NpContactRecipientRef, contactRecipientRef), ct);
        }
        public async Task SaveInternetDoc(NpInternetDocRes internetDoc, Guid orderId, CancellationToken ct = default)
        {
            await dbContext.Orders
                .Where(ord => ord.Id == orderId && ord.TrackingDocRef == null)
                .ExecuteUpdateAsync(set => set
                    .SetProperty(ord => ord.TrackingDocRef, internetDoc.Ref)
                    .SetProperty(ord => ord.TrackingDocNum, internetDoc.IntDocNumber), ct);
        }
        public async Task<List<NpWarehouse>> GetWarehouses(string settlementRef, string? searchString = null, CancellationToken ct = default)
        {
            try
            {
                var query = dbContext.NpWarehouses.Where(w => w.SettlementRef == settlementRef);

                if (!string.IsNullOrWhiteSpace(searchString) && searchString != "all")
                {
                    query = query.Where(w =>
                        w.Ref.Contains(searchString) ||
                        (w.Description != null && w.Description.Contains(searchString)));
                }

                var warehouses = await query.ToListAsync(ct);

                var data = warehouses.Select(e => new NpWarehouse
                {
                    Ref = e.Ref,
                    SettlementRef = e.SettlementRef,
                    Description = e.Description,
                    TypeOfWarehouseRef = e.TypeOfWarehouseRef
                }).ToList();

                return data;
            }
            catch
            {
                return new List<NpWarehouse>();
            }
        }
        public async Task ReadChannelWarehouse(ChannelReader<NpWarehouse> channelReader, CancellationToken ct = default)
        {
            const int batchSize = 5000;
            var buffer = new List<Entities.NpWarehousesEntity>(batchSize);
            var bulkConfig = new BulkConfig
            {
                BatchSize = batchSize,
                SetOutputIdentity = false,
                UpdateByProperties = [nameof(Entities.NpWarehousesEntity.Ref)],
                CalculateStats = false
            };

            await foreach (var data in channelReader.ReadAllAsync(ct))
            {
                buffer.Add(Mappers.NovaPostMapper.ToWarehouseEntity(data));

                if (buffer.Count >= batchSize)
                {
                    await dbContext.BulkInsertOrUpdateAsync(buffer, bulkConfig: bulkConfig, cancellationToken: ct);
                    buffer.Clear();
                }
            }

            if (buffer.Count > 0)
            {
                await dbContext.BulkInsertOrUpdateAsync(buffer, bulkConfig: bulkConfig, cancellationToken: ct);
            }
        }
    }
}

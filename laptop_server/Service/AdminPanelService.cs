using ErrorOr;
using LaptopServer.DB;
using LaptopServer.DTO;
using LaptopServer.Mappers;
using Microsoft.EntityFrameworkCore;

namespace LaptopServer.Service
{
    public interface IAdminPanelService
    {
        Task<ErrorOr<LaptopAdminDTO>> AddLaptop(LaptopAdminDTO laptop, CancellationToken ct = default);
        Task<ErrorOr<Updated>> UpdateLaptop(LaptopAdminDTO laptop, CancellationToken ct = default);
        Task<ErrorOr<Deleted>> DeleteLaptop(Guid id, CancellationToken ct = default);
        Task<IReadOnlyList<OrderStatsDTO>> GetOrderStats(int days, CancellationToken ct = default);
    }
    public class AdminPanelService(LaptopsDBContext dbContext) : IAdminPanelService
    {
        public async Task<ErrorOr<LaptopAdminDTO>> AddLaptop(LaptopAdminDTO laptop, CancellationToken ct = default)
        {
            if (laptop.Id == Guid.Empty)
                laptop.Id = Guid.NewGuid();

            var entity = laptop.ToEntity();
            dbContext.Add(entity);
            await dbContext.SaveChangesAsync(ct);
            return laptop;
        }
        public async Task<ErrorOr<Updated>> UpdateLaptop(LaptopAdminDTO laptop, CancellationToken ct = default)
        {
            var exLaptop = await dbContext.Laptops.FindAsync([laptop.Id], ct);
            if (exLaptop == null)
                return Error.NotFound(code: "LaptopNotFound");

            exLaptop.Name = laptop.Name;
            exLaptop.Price = laptop.Price;
            exLaptop.Img = laptop.Img;
            exLaptop.CPU = laptop.CPU;
            exLaptop.RAM = laptop.RAM;
            exLaptop.GPU = laptop.GPU;

            await dbContext.SaveChangesAsync(ct);
            return Result.Updated;
        }
        public async Task<ErrorOr<Deleted>> DeleteLaptop(Guid id, CancellationToken ct = default)
        {
            var deletedCount = await dbContext.Laptops.Where(l => l.Id == id).ExecuteDeleteAsync(ct);
            if (deletedCount == 0)
                return Error.NotFound(code: "LaptopNotFound");

            return Result.Deleted;
        }
        public async Task<IReadOnlyList<OrderStatsDTO>> GetOrderStats(int days, CancellationToken ct = default)
        {
            var startDay = DateTime.UtcNow.AddDays(-days).Date;
            var groupedData = await dbContext.Orders
                .AsNoTracking()
                .Where(o => o.CreatedAt >= startDay)
                .GroupBy(o => o.CreatedAt.Date)
                .Select(g => new
                {
                    Date = g.Key,
                    Quantity = g.Count(),
                    Sum = g.Sum(o => o.TotalPrice)
                })
                .OrderBy(s => s.Date)
                .ToListAsync(ct);

            return groupedData
                .Select(o => new OrderStatsDTO(o.Date, o.Quantity, o.Sum))
                .ToList();
        }
    }
}

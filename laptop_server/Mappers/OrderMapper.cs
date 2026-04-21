using LaptopServer.DTO;
using LaptopServer.Entities;
using Riok.Mapperly.Abstractions;

namespace LaptopServer.Mappers;

[Mapper]
public static partial class OrderMapper
{
    public static partial IQueryable<OrderDTO> ToOrder(this IQueryable<OrderEntity> query);

    [MapProperty(nameof(OrderEntity.CustomerInfo), nameof(OrderDTO.CustomerInfo))]
    public static partial OrderDTO ToDto(this OrderEntity entity);
    [MapperIgnoreTarget(nameof(OrderEntity.Id))]
    [MapperIgnoreTarget(nameof(OrderEntity.OrderItems))]
    [MapperIgnoreTarget(nameof(OrderEntity.TotalPrice))]
    [MapperIgnoreTarget(nameof(OrderEntity.PaymentId))]
    [MapperIgnoreTarget(nameof(OrderEntity.PaymentUrl))]
    [MapperIgnoreTarget(nameof(OrderEntity.PaymentStatus))]
    [MapperIgnoreTarget(nameof(OrderEntity.NpRecipientRef))]
    [MapperIgnoreTarget(nameof(OrderEntity.NpContactRecipientRef))]
    [MapperIgnoreTarget(nameof(OrderEntity.TrackingDocRef))]
    [MapperIgnoreTarget(nameof(OrderEntity.TrackingDocNum))]
    [MapProperty(nameof(CreateOrderDTO.CustomerInfo), nameof(OrderEntity.CustomerInfo))]
    public static partial OrderEntity ToEntity(this CreateOrderDTO creatingOrder);
    public static partial IQueryable<CustomerInfoDTO> ToCustomerInfoDto(this IQueryable<OrderEntity.CustomerInfoType> customer);
    public static partial OrderEntity.CustomerInfoType ToCustomerInfoEntity(this CustomerInfoDTO dto);

    public static partial List<OrderItemEntity> ToOrderItems(this IEnumerable<CartItemDTO> items);

    [MapperIgnoreTarget(nameof(OrderItemEntity.Id))]
    [MapperIgnoreTarget(nameof(OrderItemEntity.OrderId))]
    [MapperIgnoreTarget(nameof(OrderItemEntity.Order))]
    public static partial OrderItemEntity MapItem(this CartItemDTO item);

    public static OrderEntity ToOrderEntity(this CreateOrderDTO creatingOrder, CartDTO cart, Guid orderId)
    {
        var order = creatingOrder.ToEntity();

        order.Id = orderId;
        order.TotalPrice = cart.GrandTotal;
        order.OrderItems = cart.Items.ToOrderItems();

        foreach (var item in order.OrderItems)
        {
            item.OrderId = orderId;
        }

        return order;
    }
}
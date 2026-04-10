using LaptopServer.DTO;
using LaptopServer.Entities;
using Riok.Mapperly.Abstractions;

[Mapper]
public static partial class OrderMapper
{
    public static partial IQueryable<OrderDTO> ToOrder(this IQueryable<OrderEntity> entity);
    public static partial OrderDTO ToDto(OrderEntity entity);

    public static OrderEntity ToOrderEntity(CreateOrderDTO creatingOrder, CartDTO cart, Guid orderId)
    {
        var order = Map(creatingOrder);

        order.Id = orderId;
        order.TotalPrice = cart.GrandTotal;
        order.OrderItems = ToOrderItems(cart.Items);

        return order;
    }
    private static partial List<OrderItemEntity> ToOrderItems(List<CartItemDTO> items);
    private static partial OrderItemEntity MapItem(CartItemDTO item);

    [MapperIgnoreTarget(nameof(OrderEntity.Id))]
    [MapperIgnoreTarget(nameof(OrderEntity.OrderItems))]
    [MapperIgnoreTarget(nameof(OrderEntity.TotalPrice))]
    [MapperIgnoreTarget(nameof(OrderEntity.PaymentId))]
    [MapperIgnoreTarget(nameof(OrderEntity.PaymentUrl))]
    [MapperIgnoreTarget(nameof(OrderEntity.PaymentStatus))]
    [MapperIgnoreTarget(nameof(OrderEntity.CreatedAt))]
    private static partial OrderEntity Map(CreateOrderDTO creatingOrder);
}
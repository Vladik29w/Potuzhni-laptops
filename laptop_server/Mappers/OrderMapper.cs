using LaptopServer.Entities;
using LaptopServer.DTO;
using Riok.Mapperly.Abstractions;

namespace LaptopServer.Mappers
{
    [Mapper]
    public static partial class OrderMapper
    {
        public static partial IQueryable<OrderDTO> ToOrder(this IQueryable<OrderEntity> entity);
    }
}

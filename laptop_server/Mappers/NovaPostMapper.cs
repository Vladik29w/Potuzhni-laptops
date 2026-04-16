using LaptopServer.DTO.NovaPost;
using LaptopServer.Entities;
using Riok.Mapperly.Abstractions;

namespace LaptopServer.Mappers
{
    [Mapper]
    public static partial class NovaPostMapper
    {
        [MapProperty(nameof(NpWarehouse.SettlementRef), nameof(NpWarehousesEntity.SettlementRef))]
        public static partial NpWarehousesEntity ToWarehouseEntity(this NpWarehouse entity);
    }
}

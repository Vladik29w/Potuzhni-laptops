using LaptopServer.DTO;
using LaptopServer.Entities;
using Riok.Mapperly;
using Riok.Mapperly.Abstractions;

namespace LaptopServer.Mappers
{
    [Mapper]
    public static partial class LaptopMapper
    {
        public static partial IQueryable<LaptopMainDTO> ToMain(this IQueryable<LaptopEntity> query);
        public static partial IQueryable<LaptopDetailsDTO> ToDetails(this IQueryable<LaptopEntity> entity);
        public static partial IQueryable<LaptopAdminDTO> ToAdmin(this IQueryable<LaptopEntity> entity);
        public static partial LaptopEntity ToEntity(this LaptopAdminDTO entity);
    }
}

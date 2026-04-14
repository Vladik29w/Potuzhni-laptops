using LaptopServer.DB;
using LaptopServer.DTO;
using LaptopServer.Mappers;
using Microsoft.EntityFrameworkCore;

namespace LaptopServer.Service
{
    public interface ILaptopService
    {
        Task<IReadOnlyList<LaptopMainDTO>> GetAllLaptops(CancellationToken ct = default);
        Task<LaptopDetailsDTO?> GetById(Guid id, CancellationToken ct = default);
        Task<IReadOnlyList<LaptopAdminDTO>> GetLaptopsAdmin(CancellationToken ct = default);
    }

    public class LaptopService(LaptopsDBContext dbContext) : ILaptopService
    {
        public async Task<IReadOnlyList<LaptopMainDTO>> GetAllLaptops(CancellationToken ct = default)
        {
            return await dbContext.Laptops
                .AsNoTracking()
                .ToMain()
                .ToListAsync(ct);
        }
        public async Task<LaptopDetailsDTO?> GetById(Guid id, CancellationToken ct = default)
        {
            return await dbContext.Laptops
                .AsNoTracking()
                .Where(l => l.Id == id)
                .ToDetails()
                .FirstOrDefaultAsync(ct);
        }
        public async Task<IReadOnlyList<LaptopAdminDTO>> GetLaptopsAdmin(CancellationToken ct = default)
        {
            return await dbContext.Laptops
              .AsNoTracking()
              .ToAdmin()
              .ToListAsync(ct);
        }
    }
}

using LaptopServer.DB;
using LaptopServer.DTO;
using Microsoft.EntityFrameworkCore;
using LaptopServer.Mappers;

namespace LaptopServer.Service
{
    public interface ILaptopService
    {
        Task<IEnumerable<LaptopMainDTO>> GetAllLaptops();
        Task<LaptopDetailsDTO?> GetById(string id);
        Task<IEnumerable<LaptopAdminDTO>> GetLaptopsAdmin();
    }

    public class LaptopService : ILaptopService
    {
        private readonly LaptopsDBContext _dbContext;

        public LaptopService(LaptopsDBContext dbContext)
        {
            _dbContext = dbContext;
        }
        public async Task<IEnumerable<LaptopMainDTO>> GetAllLaptops()
        {
            return await _dbContext.Laptops
                .AsNoTracking()
                .ToMain()
                .ToListAsync();
        }
        public async Task<LaptopDetailsDTO?> GetById(string id)
        {
            ArgumentException.ThrowIfNullOrWhiteSpace(id, nameof(id));

            return await _dbContext.Laptops
                .AsNoTracking()
                .Where(l => l.Id == id)
                .ToDetails()
                .FirstOrDefaultAsync();
        }
        public async Task<IEnumerable<LaptopAdminDTO>> GetLaptopsAdmin()
        {
            return await _dbContext.Laptops
              .AsNoTracking()
              .ToAdmin()
              .ToListAsync();
        }
    }
}

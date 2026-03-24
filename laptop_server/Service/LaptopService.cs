using LaptopServer.DB;
using LaptopServer.DTO;
using Microsoft.EntityFrameworkCore;

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
            _dbContext = dbContext ?? throw new ArgumentNullException(nameof(dbContext));
        }

        public async Task<IEnumerable<LaptopMainDTO>> GetAllLaptops()
        {
            return await _dbContext.Laptops
               .AsNoTracking()
               .Select(laptop => new LaptopMainDTO
               {
                   Id = laptop.Id,
                   Name = laptop.Name,
                   Price = laptop.Price,
                   Img = laptop.Img
               })
               .ToListAsync();
        }

        public async Task<LaptopDetailsDTO?> GetById(string id)
        {
            ArgumentException.ThrowIfNullOrWhiteSpace(id, nameof(id));

            return await _dbContext.Laptops
                .AsNoTracking()
                .Where(l => l.Id == id)
                .Select(laptop => new LaptopDetailsDTO
                {
                    Id = laptop.Id,
                    Name = laptop.Name,
                    Price = laptop.Price,
                    Img = laptop.Img,
                    CPU = laptop.CPU,
                    RAM = laptop.RAM,
                    GPU = laptop.GPU
                })
                .FirstOrDefaultAsync();
        }
        public async Task<IEnumerable<LaptopAdminDTO>> GetLaptopsAdmin()
        {
            return await _dbContext.Laptops
              .AsNoTracking()
              .Select(laptop => new LaptopAdminDTO
              {
                  Id = laptop.Id,
                  Name = laptop.Name,
                  Price = laptop.Price,
                  Img = laptop.Img,
                  CPU = laptop.CPU,
                  RAM = laptop.RAM,
                  GPU = laptop.GPU
              })
              .ToListAsync();
        }
    }
}

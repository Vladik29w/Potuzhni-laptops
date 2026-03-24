 using ErrorOr;
using LaptopServer.DB;
using LaptopServer.DTO;
using LaptopServer.Entities;

namespace LaptopServer.Service
{
    public interface IAdminPanelService
    {
        Task<ErrorOr<LaptopDetailsDTO>> AddLaptop(LaptopDetailsDTO laptop);
        Task<ErrorOr<bool>> UpdateLaptop(LaptopDetailsDTO laptop);
        Task<ErrorOr<bool>> DeleteLaptop(string id);
    }
    public class AdminPanelService : IAdminPanelService
    {
        private readonly LaptopsDBContext _dbContext;
        public AdminPanelService(LaptopsDBContext dbContext)
        {
            _dbContext = dbContext;
        }
        public async Task<ErrorOr<LaptopDetailsDTO>> AddLaptop(LaptopDetailsDTO laptop)
        {
            var newLaptop = new LaptopEntity
            {
                Id = laptop.Id,
                Name = laptop.Name,
                Price = laptop.Price,
                Img = laptop.Img,
                CPU = laptop.CPU,
                RAM = laptop.RAM,
                GPU = laptop.GPU
            };
            _dbContext.Add(newLaptop);
            await _dbContext.SaveChangesAsync();
            return laptop;
        }
        public async Task<ErrorOr<bool>> UpdateLaptop(LaptopDetailsDTO laptop)
        {
            var exLaptop = await _dbContext.Laptops.FindAsync(laptop.Id);
            if (exLaptop == null)
                return Error.NotFound(code: "LaptopNotFound");

            exLaptop.Name = laptop.Name;
            exLaptop.Price = laptop.Price;
            exLaptop.Img = laptop.Img;
            exLaptop.CPU = laptop.CPU;
            exLaptop.RAM = laptop.RAM;
            exLaptop.GPU = laptop.GPU;

            await _dbContext.SaveChangesAsync();
            return true;
        }
        public async Task<ErrorOr<bool>> DeleteLaptop(string id)
        {
            var exLaptop = await _dbContext.Laptops.FindAsync(id);
            if (exLaptop == null)
                return Error.NotFound(code: "LaptopNotFound");

            _dbContext.Laptops.Remove(exLaptop);
            await _dbContext.SaveChangesAsync();
            return true;
        }
    }
}

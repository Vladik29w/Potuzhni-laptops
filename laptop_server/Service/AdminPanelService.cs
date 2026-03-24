 using ErrorOr;
using LaptopServer.DB;
using LaptopServer.DTO;
using LaptopServer.Entities;
using LaptopServer.Mappers;

namespace LaptopServer.Service
{
    public interface IAdminPanelService
    {
        Task<ErrorOr<LaptopAdminDTO>> AddLaptop(LaptopAdminDTO laptop);
        Task<ErrorOr<bool>> UpdateLaptop(LaptopAdminDTO laptop);
        Task<ErrorOr<bool>> DeleteLaptop(string id);
    }
    public class AdminPanelService : IAdminPanelService
    {
        private readonly LaptopsDBContext _dbContext;
        public AdminPanelService(LaptopsDBContext dbContext)
        {
            _dbContext = dbContext;
        }
        public async Task<ErrorOr<LaptopAdminDTO>> AddLaptop(LaptopAdminDTO laptop)
        {
            laptop.ToEntity();
            _dbContext.Add(laptop);
            await _dbContext.SaveChangesAsync();
            return laptop;
        }
        public async Task<ErrorOr<bool>> UpdateLaptop(LaptopAdminDTO laptop)
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

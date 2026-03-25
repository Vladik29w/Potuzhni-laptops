using ErrorOr;
using LaptopServer.DB;
using LaptopServer.DTO;
using LaptopServer.Mappers;
using Microsoft.EntityFrameworkCore;

namespace LaptopServer.Service
{
    public interface IAdminPanelService
    {
        Task<ErrorOr<LaptopAdminDTO>> AddLaptop(LaptopAdminDTO laptop);
        Task<ErrorOr<Updated>> UpdateLaptop(LaptopAdminDTO laptop);
        Task<ErrorOr<Deleted>> DeleteLaptop(string id);
        Task<List<OrderStatsDTO>> GetOrderStats(int days);
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
            var entity = laptop.ToEntity();
            _dbContext.Add(entity);
            await _dbContext.SaveChangesAsync();
            return laptop;
        }
        public async Task<ErrorOr<Updated>> UpdateLaptop(LaptopAdminDTO laptop)
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
            return Result.Updated;
        }
        public async Task<ErrorOr<Deleted>> DeleteLaptop(string id)
        {
            var exLaptop = await _dbContext.Laptops.FindAsync(id);
            if (exLaptop == null)
                return Error.NotFound(code: "LaptopNotFound");

            _dbContext.Laptops.Remove(exLaptop);
            await _dbContext.SaveChangesAsync();
            return Result.Deleted;
        }
        public async Task<List<OrderStatsDTO>> GetOrderStats(int days)
        {
            var startDay = DateTime.UtcNow.AddDays(-days).Date;
            var groupedData = await _dbContext.Orders
                .AsNoTracking()
                .Where(o => o.CreatedAt >= startDay)
                .GroupBy(o => o.CreatedAt.Date)
                .Select(g => new
                {
                    Date = g.Key,
                    Quantity = g.Count(),
                    Sum = g.Sum(o => o.TotalPrice)
                })
                .OrderBy(s => s.Date)
                .ToListAsync();

            return groupedData
                .Select(o => new OrderStatsDTO(o.Date, o.Quantity, o.Sum))
                .ToList();
        }
    }
}

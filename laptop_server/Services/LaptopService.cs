using LaptopServer.DB;
using LaptopServer.DTO;
using LaptopServer.Mappers;
using ErrorOr;
using Microsoft.EntityFrameworkCore;

namespace LaptopServer.Service
{
    public interface ILaptopService
    {
        Task<IReadOnlyList<LaptopMainDTO>> GetAllLaptops(CancellationToken ct = default);
        Task<LaptopDetailsDTO?> GetById(Guid id, CancellationToken ct = default);
        Task<IReadOnlyList<LaptopAdminDTO>> GetLaptopsAdmin(CancellationToken ct = default);
        Task<ErrorOr<LaptopAdminDTO>> AddLaptop(LaptopAdminDTO laptop, CancellationToken ct = default);
        Task<ErrorOr<Updated>> UpdateLaptop(LaptopAdminDTO laptop, CancellationToken ct = default);
        Task<ErrorOr<Deleted>> DeleteLaptop(Guid id, CancellationToken ct = default);
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

        public async Task<ErrorOr<LaptopAdminDTO>> AddLaptop(LaptopAdminDTO laptop, CancellationToken ct = default)
        {
            if (laptop.Id == Guid.Empty)
                laptop.Id = Guid.NewGuid();

            var entity = laptop.ToEntity();
            dbContext.Add(entity);
            await dbContext.SaveChangesAsync(ct);
            return laptop;
        }

        public async Task<ErrorOr<Updated>> UpdateLaptop(LaptopAdminDTO laptop, CancellationToken ct = default)
        {
            var exLaptop = await dbContext.Laptops.FindAsync([laptop.Id], ct);
            if (exLaptop == null)
                return Error.NotFound(code: "LaptopNotFound");

            exLaptop.Name = laptop.Name;
            exLaptop.Price = laptop.Price;
            exLaptop.Img = laptop.Img;
            exLaptop.CPU = laptop.CPU;
            exLaptop.RAM = laptop.RAM;
            exLaptop.GPU = laptop.GPU;

            await dbContext.SaveChangesAsync(ct);
            return Result.Updated;
        }

        public async Task<ErrorOr<Deleted>> DeleteLaptop(Guid id, CancellationToken ct = default)
        {
            var deletedCount = await dbContext.Laptops.Where(l => l.Id == id).ExecuteDeleteAsync(ct);
            if (deletedCount == 0)
                return Error.NotFound(code: "LaptopNotFound");

            return Result.Deleted;
        }
    }
}

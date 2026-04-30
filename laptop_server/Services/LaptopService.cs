using ErrorOr;
using LaptopServer.DB;
using LaptopServer.DTO;
using LaptopServer.Mappers;
using Microsoft.EntityFrameworkCore;

namespace LaptopServer.Service
{
    public interface ILaptopService
    {
        Task<PageDTO<LaptopMainDTO>> GetAllLaptops(int page, int pageSize, CancellationToken ct = default);
        Task<LaptopDetailsDTO?> GetById(Guid id, CancellationToken ct = default);
        Task<IReadOnlyList<LaptopAdminDTO>> GetLaptopsAdmin(CancellationToken ct = default);
        Task<ErrorOr<LaptopAdminDTO>> AddLaptop(LaptopAdminDTO laptop, CancellationToken ct = default);
        Task<ErrorOr<Updated>> UpdateLaptop(LaptopAdminDTO laptop, CancellationToken ct = default);
        Task<ErrorOr<Deleted>> DeleteLaptop(Guid id, CancellationToken ct = default);
    }

    public class LaptopService(LaptopsDBContext dbContext) : ILaptopService
    {
        public async Task<PageDTO<LaptopMainDTO>> GetAllLaptops(int page, int pageSize, CancellationToken ct = default)
        {
            var skip = (page - 1) * pageSize;
            var totalCount = await dbContext.Laptops.CountAsync(ct);
            var items = await dbContext.Laptops
                .AsNoTracking()
                .OrderBy(l => l.Price)
                .Skip(skip)
                .Take(pageSize)
                .ToMain()
                .ToListAsync(ct);

            return new PageDTO<LaptopMainDTO>
            {
                Items = items,
                Page = page,
                PageSize = pageSize,
                TotalCount = totalCount
            };
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
              .ToAdminList()
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

            LaptopMapper.ApplyUpdate(laptop, exLaptop);

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

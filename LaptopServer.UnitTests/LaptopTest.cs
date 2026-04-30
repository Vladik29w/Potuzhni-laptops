using FluentAssertions;
using ErrorOr;
using LaptopServer.DB;
using LaptopServer.DTO;
using LaptopServer.Entities;
using LaptopServer.Mappers;
using LaptopServer.Service;
using Microsoft.AspNetCore.Identity;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;

namespace LaptopServer.UnitTests
{
    public class LaptopTest : IDisposable
    {
        private readonly SqliteConnection _connection;
        private readonly LaptopsDBContext _dbContext;
        private readonly LaptopService _service;

        public LaptopTest()
        {
            _connection = new SqliteConnection("DataSource=:memory:");
            _connection.Open();

            var options = new DbContextOptionsBuilder<LaptopsDBContext>()
                .UseSqlite(_connection)
                .Options;

            _dbContext = new LaptopsDBContext(options);
            _dbContext.Database.EnsureCreated();
            _service = new LaptopService(_dbContext);
        }

        private LaptopEntity CreateTestLaptop(
            Guid? id = null,
            string name = "Test Laptop",
            decimal price = 999.99m,
            string? img = "test.jpg",
            string? cpu = "Intel i7",
            int ram = 16,
            string? gpu = "RTX 3060",
            string? diskSize = "512 GB",
            double? screenSize = 15.6,
            string? screenResolution = "1920x1080",
            int? screenRefresh = 144,
            int? battery = 100)
        {
            return new LaptopEntity
            {
                Id = id ?? Guid.NewGuid(),
                Name = name,
                Price = price,
                Img = img,
                CPU = cpu,
                RAM = ram,
                GPU = gpu,
                DiskSize = diskSize,
                ScreenSize = screenSize,
                ScreenResolution = screenResolution,
                ScreenRefresh = screenRefresh,
                Battery = battery
            };
        }

        private LaptopAdminDTO CreateTestLaptopAdminDTO(
            Guid? id = null,
            string name = "Test Laptop",
            decimal price = 999.99m,
            string? img = "test.jpg",
            string? cpu = "Intel i7",
            int ram = 16,
            string? gpu = "RTX 3060",
            string? diskSize = "512 GB",
            double? screenSize = 15.6,
            string? screenResolution = "1920x1080",
            int? screenRefresh = 144,
            int? battery = 100)
        {
            return new LaptopAdminDTO
            {
                Id = id ?? Guid.NewGuid(),
                Name = name,
                Price = price,
                Img = img,
                CPU = cpu,
                RAM = ram,
                GPU = gpu,
                DiskSize = diskSize,
                ScreenSize = screenSize,
                ScreenResolution = screenResolution,
                ScreenRefresh = screenRefresh,
                Battery = battery
            };
        }

        #region GetAllLaptops Tests

        [Fact]
        public async Task GetAllLaptops_WithEmptyDatabase_ReturnsEmptyList()
        {
            // Act
            var result = await _service.GetAllLaptops(page: 1, pageSize: 10);

            // Assert
            result.Items.Should().BeEmpty();
            result.TotalCount.Should().Be(0);
            result.Page.Should().Be(1);
            result.PageSize.Should().Be(10);
        }

        #endregion

        #region GetById Tests

        [Fact]
        public async Task GetById_WithValidId_ReturnsLaptopDetails()
        {
            // Arrange
            var laptopId = Guid.NewGuid();
            var laptop = CreateTestLaptop(
                id: laptopId,
                name: "Detailed Laptop",
                price: 1299.99m);

            await _dbContext.Laptops.AddAsync(laptop);
            await _dbContext.SaveChangesAsync();

            // Act
            var result = await _service.GetById(laptopId);

            // Assert
            result.Should().NotBeNull();
            result!.Id.Should().Be(laptopId);
            result.Name.Should().Be("Detailed Laptop");
            result.Price.Should().Be(1299.99m);
            result.Should().BeOfType<LaptopDetailsDTO>();
        }

        [Fact]
        public async Task GetById_WithInvalidId_ReturnsNull()
        {
            // Act
            var result = await _service.GetById(Guid.NewGuid());

            // Assert
            result.Should().BeNull();
        }

        [Fact]
        public async Task GetById_ReturnsDetailedInformation()
        {
            // Arrange
            var laptopId = Guid.NewGuid();
            var laptop = CreateTestLaptop(
                id: laptopId,
                cpu: "AMD Ryzen 7",
                ram: 32,
                gpu: "RTX 4080",
                screenSize: 17.3,
                battery: 150);

            await _dbContext.Laptops.AddAsync(laptop);
            await _dbContext.SaveChangesAsync();

            // Act
            var result = await _service.GetById(laptopId);

            // Assert
            result.Should().NotBeNull();
            result!.CPU.Should().Be("AMD Ryzen 7");
            result.RAM.Should().Be(32);
            result.GPU.Should().Be("RTX 4080");
            result.ScreenSize.Should().Be(17.3);
            result.Battery.Should().Be(150);
        }

        #endregion

        #region GetLaptopsAdmin Tests

        [Fact]
        public async Task GetLaptopsAdmin_WithMultipleLaptops_ReturnsAllAsAdminDTOs()
        {
            // Arrange
            var laptops = new[]
            {
                CreateTestLaptop(name: "Admin Laptop 1", price: 800),
                CreateTestLaptop(name: "Admin Laptop 2", price: 1200),
                CreateTestLaptop(name: "Admin Laptop 3", price: 1600)
            };

            await _dbContext.Laptops.AddRangeAsync(laptops);
            await _dbContext.SaveChangesAsync();

            // Act
            var result = await _service.GetLaptopsAdmin();

            // Assert
            result.Should().HaveCount(3);
            result.Should().AllSatisfy(item => item.Should().BeOfType<LaptopAdminDTO>());
            result.Select(l => l.Name).Should().Contain(new[] { "Admin Laptop 1", "Admin Laptop 2", "Admin Laptop 3" });
        }

        [Fact]
        public async Task GetLaptopsAdmin_WithEmptyDatabase_ReturnsEmptyList()
        {
            // Act
            var result = await _service.GetLaptopsAdmin();

            // Assert
            result.Should().BeEmpty();
        }

        [Fact]
        public async Task GetLaptopsAdmin_ReturnsReadOnlyList()
        {
            // Arrange
            var laptop = CreateTestLaptop();
            await _dbContext.Laptops.AddAsync(laptop);
            await _dbContext.SaveChangesAsync();

            // Act
            var result = await _service.GetLaptopsAdmin();

            // Assert
            result.Should().BeAssignableTo<IReadOnlyList<LaptopAdminDTO>>();
        }

        #endregion

        #region AddLaptop Tests

        [Fact]
        public async Task AddLaptop_WithValidLaptop_AddsToDatabase()
        {
            // Arrange
            var laptopDTO = CreateTestLaptopAdminDTO(id: Guid.Empty, name: "New Laptop");

            // Act
            var result = await _service.AddLaptop(laptopDTO);

            // Assert
            result.IsError.Should().BeFalse();
            result.Value.Should().BeOfType<LaptopAdminDTO>();
            result.Value.Name.Should().Be("New Laptop");

            var savedLaptop = await _dbContext.Laptops.FirstOrDefaultAsync(l => l.Name == "New Laptop");
            savedLaptop.Should().NotBeNull();
        }

        [Fact]
        public async Task AddLaptop_WithEmptyId_GeneratesNewId()
        {
            // Arrange
            var laptopDTO = CreateTestLaptopAdminDTO(id: Guid.Empty);

            // Act
            var result = await _service.AddLaptop(laptopDTO);

            // Assert
            result.IsError.Should().BeFalse();
            result.Value.Id.Should().NotBe(Guid.Empty);
        }

        [Fact]
        public async Task AddLaptop_WithProvidedId_UsesProvidedId()
        {
            // Arrange
            var providedId = Guid.NewGuid();
            var laptopDTO = CreateTestLaptopAdminDTO(id: providedId);

            // Act
            var result = await _service.AddLaptop(laptopDTO);

            // Assert
            result.IsError.Should().BeFalse();
            result.Value.Id.Should().Be(providedId);

            var savedLaptop = await _dbContext.Laptops.FindAsync(providedId);
            savedLaptop.Should().NotBeNull();
            savedLaptop!.Id.Should().Be(providedId);
        }

        [Fact]
        public async Task AddLaptop_PersistsAllProperties()
        {
            // Arrange
            var laptopDTO = CreateTestLaptopAdminDTO(
                name: "Full Specs Laptop",
                price: 1999.99m,
                cpu: "Intel i9",
                ram: 64,
                gpu: "RTX 4090");

            // Act
            var result = await _service.AddLaptop(laptopDTO);

            // Assert
            var savedLaptop = await _dbContext.Laptops.FirstOrDefaultAsync(l => l.Id == result.Value.Id);
            savedLaptop.Should().NotBeNull();
            savedLaptop!.Name.Should().Be("Full Specs Laptop");
            savedLaptop.Price.Should().Be(1999.99m);
            savedLaptop.CPU.Should().Be("Intel i9");
            savedLaptop.RAM.Should().Be(64);
            savedLaptop.GPU.Should().Be("RTX 4090");
        }

        #endregion

        #region UpdateLaptop Tests

        [Fact]
        public async Task UpdateLaptop_WithExistingLaptop_ReturnsUpdatedResult()
        {
            // Arrange
            var laptopId = Guid.NewGuid();
            var originalLaptop = CreateTestLaptop(id: laptopId, name: "Original", price: 1000);
            await _dbContext.Laptops.AddAsync(originalLaptop);
            await _dbContext.SaveChangesAsync();

            var updatedDTO = CreateTestLaptopAdminDTO(id: laptopId, name: "Updated", price: 1500);

            // Act
            var result = await _service.UpdateLaptop(updatedDTO);

            // Assert
            result.IsError.Should().BeFalse();
        }

        [Fact]
        public async Task UpdateLaptop_WithNonExistingLaptop_ReturnsNotFoundError()
        {
            // Arrange
            var nonExistentId = Guid.NewGuid();
            var laptopDTO = CreateTestLaptopAdminDTO(id: nonExistentId);

            // Act
            var result = await _service.UpdateLaptop(laptopDTO);

            // Assert
            result.IsError.Should().BeTrue();
            result.FirstError.Code.Should().Be("LaptopNotFound");
        }

        [Fact]
        public async Task UpdateLaptop_UpdatesExistingRecord()
        {
            // Arrange
            var laptopId = Guid.NewGuid();
            var originalLaptop = CreateTestLaptop(
                id: laptopId,
                name: "Old Name",
                price: 500.00m,
                cpu: "Old CPU");

            await _dbContext.Laptops.AddAsync(originalLaptop);
            await _dbContext.SaveChangesAsync();

            var updatedDTO = CreateTestLaptopAdminDTO(
                id: laptopId,
                name: "New Name",
                price: 1500.00m,
                cpu: "New CPU");

            // Act
            var result = await _service.UpdateLaptop(updatedDTO);

            // Assert
            result.IsError.Should().BeFalse();

            _dbContext.ChangeTracker.Clear();
            var updatedLaptop = await _dbContext.Laptops.FindAsync(laptopId);
            updatedLaptop.Should().NotBeNull();
            updatedLaptop!.Name.Should().Be("New Name");
            updatedLaptop.Price.Should().Be(1500.00m);
            updatedLaptop.CPU.Should().Be("New CPU");
        }

        #endregion

        #region DeleteLaptop Tests

        [Fact]
        public async Task DeleteLaptop_WithExistingId_ReturnsDeletedResult()
        {
            // Arrange
            var laptopId = Guid.NewGuid();
            var laptop = CreateTestLaptop(id: laptopId);
            await _dbContext.Laptops.AddAsync(laptop);
            await _dbContext.SaveChangesAsync();

            // Act
            var result = await _service.DeleteLaptop(laptopId);

            // Assert
            result.IsError.Should().BeFalse();
        }

        [Fact]
        public async Task DeleteLaptop_WithNonExistingId_ReturnsNotFoundError()
        {
            // Act
            var result = await _service.DeleteLaptop(Guid.NewGuid());

            // Assert
            result.IsError.Should().BeTrue();
            result.FirstError.Code.Should().Be("LaptopNotFound");
        }

        [Fact]
        public async Task DeleteLaptop_RemovesLaptopFromDatabase()
        {
            // Arrange
            var laptopId = Guid.NewGuid();
            var laptop = CreateTestLaptop(id: laptopId);
            await _dbContext.Laptops.AddAsync(laptop);
            await _dbContext.SaveChangesAsync();

            // Act
            var result = await _service.DeleteLaptop(laptopId);

            // Assert
            result.IsError.Should().BeFalse();

            // Refresh context to avoid tracking cache
            _dbContext.ChangeTracker.Clear();
            var deletedLaptop = await _dbContext.Laptops.FindAsync(laptopId);
            deletedLaptop.Should().BeNull();
        }

        [Fact]
        public async Task DeleteLaptop_WithMultipleLaptops_DeletesOnlySpecified()
        {
            // Arrange
            var laptopId1 = Guid.NewGuid();
            var laptopId2 = Guid.NewGuid();
            var laptop1 = CreateTestLaptop(id: laptopId1, name: "To Delete");
            var laptop2 = CreateTestLaptop(id: laptopId2, name: "To Keep");

            await _dbContext.Laptops.AddAsync(laptop1);
            await _dbContext.Laptops.AddAsync(laptop2);
            await _dbContext.SaveChangesAsync();

            // Act
            await _service.DeleteLaptop(laptopId1);

            // Assert
            var remainingCount = await _dbContext.Laptops.CountAsync();
            remainingCount.Should().Be(1);

            var remainingLaptop = await _dbContext.Laptops.FindAsync(laptopId2);
            remainingLaptop.Should().NotBeNull();
        }

        #endregion

        public void Dispose()
        {
            _dbContext?.Dispose();
            _connection?.Dispose();
        }
    }
}

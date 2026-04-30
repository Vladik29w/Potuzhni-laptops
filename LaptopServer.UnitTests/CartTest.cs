using FluentAssertions;
using LaptopServer.DB;
using LaptopServer.Entities;
using LaptopServer.Service;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;

namespace LaptopServer.Tests;

public class CartServiceTests : IDisposable
{
    private readonly LaptopsDBContext _context;
    private readonly CartService _service;
    private readonly SqliteConnection _connection;

    public CartServiceTests()
    {
        // Створюємо з'єднання з SQLite в пам'яті
        _connection = new SqliteConnection("Filename=:memory:");
        _connection.Open();

        var options = new DbContextOptionsBuilder<LaptopsDBContext>()
            .UseSqlite(_connection)
            .Options;

        _context = new LaptopsDBContext(options);
        _context.Database.EnsureCreated();

        _service = new CartService(_context);
    }

    public void Dispose()
    {
        _connection.Close();
        _context.Dispose();
    }

    [Fact]
    public async Task GetCart_WhenCartDoesNotExist_ShouldReturnEmptyCartDTO()
    {
        // Arrange
        var cartId = Guid.NewGuid();

        // Act
        var result = await _service.GetCart(cartId);

        // Assert
        result.CartId.Should().Be(cartId);
        result.Items.Should().BeEmpty();
        result.GrandTotal.Should().Be(0);
    }

    [Fact]
    public async Task AddToCart_WhenLaptopNotFound_ShouldReturnError()
    {
        // Arrange
        var cartId = Guid.NewGuid();
        var laptopId = Guid.NewGuid();

        // Act
        var result = await _service.AddToCart(cartId, laptopId);

        // Assert
        result.IsError.Should().BeTrue();
        result.FirstError.Code.Should().Be("LaptopNotFound");
    }

    [Fact]
    public async Task AddToCart_WhenSuccess_ShouldCreateCartAndAddItem()
    {
        // Arrange
        var laptop = new LaptopEntity { Id = Guid.NewGuid(), Name = "MacBook Air", Price = 1000 };
        _context.Laptops.Add(laptop);
        await _context.SaveChangesAsync();

        var cartId = Guid.NewGuid();

        // Act
        var result = await _service.AddToCart(cartId, laptop.Id);

        // Assert
        result.IsError.Should().BeFalse();
        result.Value.Items.Should().HaveCount(1);
        result.Value.Items.First().LaptopName.Should().Be("MacBook Air");
        result.Value.GrandTotal.Should().Be(1000);

        // Перевірка в базі
        var cartInDb = await _context.Carts.Include(c => c.CartItems).FirstOrDefaultAsync(c => c.Id == cartId);
        cartInDb.Should().NotBeNull();
        cartInDb!.CartItems.Should().HaveCount(1);
    }

    [Fact]
    public async Task AddToCart_WhenItemAlreadyInCart_ShouldIncrementQuantity()
    {
        // Arrange
        var laptop = new LaptopEntity { Id = Guid.NewGuid(), Name = "Dell XPS", Price = 1500 };
        _context.Laptops.Add(laptop);
        await _context.SaveChangesAsync();

        var cartId = Guid.NewGuid();

        // Act
        await _service.AddToCart(cartId, laptop.Id);
        var result = await _service.AddToCart(cartId, laptop.Id);

        // Assert
        result.Value.Items.First().Quantity.Should().Be(2);
        result.Value.GrandTotal.Should().Be(3000);
    }

    [Fact]
    public async Task RemoveFromCart_WhenQuantityMoreThanOne_ShouldDecrementQuantity()
    {
        // Arrange
        var laptop = new LaptopEntity { Id = Guid.NewGuid(), Name = "Asus ROG", Price = 2000 };
        _context.Laptops.Add(laptop);

        var cartId = Guid.NewGuid();
        var cart = new CartEntity { Id = cartId };
        cart.CartItems.Add(new CartItemEntity { LaptopId = laptop.Id, Quantity = 2 });
        _context.Carts.Add(cart);
        await _context.SaveChangesAsync();

        // Act
        var result = await _service.RemoveFromCart(cartId, laptop.Id);

        // Assert
        result.Value.Items.First().Quantity.Should().Be(1);
    }

    [Fact]
    public async Task RemoveFromCart_WhenQuantityIsOne_ShouldRemoveItem()
    {
        // Arrange
        var laptop = new LaptopEntity { Id = Guid.NewGuid(), Name = "HP Spectre", Price = 1200 };
        _context.Laptops.Add(laptop);

        var cartId = Guid.NewGuid();
        var cart = new CartEntity { Id = cartId };
        cart.CartItems.Add(new CartItemEntity { LaptopId = laptop.Id, Quantity = 1 });
        _context.Carts.Add(cart);
        await _context.SaveChangesAsync();

        // Act
        var result = await _service.RemoveFromCart(cartId, laptop.Id);

        // Assert
        result.Value.Items.Should().BeEmpty();
    }

    [Fact]
    public async Task ClearCart_ShouldRemoveAllItems_ButKeepCartEntity()
    {
        //Arrange
        var cartId = Guid.NewGuid();
        var laptopId = Guid.NewGuid();

        _context.Carts.Add(new CartEntity { Id = cartId });
        _context.Laptops.Add(new LaptopEntity { Id = laptopId, Name = "Gaming Pro 15", Price = 1500 });
        await _context.SaveChangesAsync();

        await _service.AddToCart(cartId, laptopId);

        var itemsBefore = await _context.CartItems.CountAsync(ci => ci.CartId == cartId);
        itemsBefore.Should().Be(1);

        //Act
        var result = await _service.ClearCart(cartId);

        //Assert
        result.Items.Should().BeEmpty();
        result.GrandTotal.Should().Be(0);

        var cartInDb = await _context.Carts.FindAsync(cartId);
        cartInDb.Should().NotBeNull();

        var itemsInDb = await _context.CartItems.AnyAsync(ci => ci.CartId == cartId);
        itemsInDb.Should().BeFalse();
    }
}
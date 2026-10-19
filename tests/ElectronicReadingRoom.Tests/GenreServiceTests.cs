using ElectronicReadingRoom.Core.Entities;
using ElectronicReadingRoom.Data;
using ElectronicReadingRoom.Data.Services;
using Microsoft.EntityFrameworkCore;

namespace ElectronicReadingRoom.Tests;

public class GenreServiceTests : IDisposable
{
    private readonly AppDbContext _db;
    private readonly GenreService _service;

    public GenreServiceTests()
    {
        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;
        _db = new AppDbContext(options);
        _service = new GenreService(_db);
    }

    [Fact]
    public async Task CreateAsync_NewGenre_CreatesGenre()
    {
        var genre = await _service.CreateAsync("Фантастика");

        Assert.NotNull(genre);
        Assert.Equal("Фантастика", genre.Name);
        Assert.True(genre.Id > 0);
    }

    [Fact]
    public async Task CreateAsync_DuplicateName_ThrowsException()
    {
        await _service.CreateAsync("Роман");

        await Assert.ThrowsAsync<InvalidOperationException>(() => _service.CreateAsync("Роман"));
    }

    [Fact]
    public async Task UpdateAsync_ChangeName_UpdatesGenre()
    {
        var genre = await _service.CreateAsync("Старый жанр");
        genre.Name = "Новый жанр";

        await _service.UpdateAsync(genre);

        var updated = await _service.GetByIdAsync(genre.Id);
        Assert.Equal("Новый жанр", updated!.Name);
    }

    [Fact]
    public async Task UpdateAsync_DuplicateName_ThrowsException()
    {
        await _service.CreateAsync("Детектив");
        var genre2 = await _service.CreateAsync("Триллер");

        genre2.Name = "Детектив";
        await Assert.ThrowsAsync<InvalidOperationException>(() => _service.UpdateAsync(genre2));
    }

    [Fact]
    public async Task DeleteAsync_ExistingGenre_RemovesGenre()
    {
        var genre = await _service.CreateAsync("Поэзия");

        await _service.DeleteAsync(genre.Id);

        var found = await _service.GetByIdAsync(genre.Id);
        Assert.Null(found);
    }

    [Fact]
    public async Task GetAllAsync_ReturnsAllGenresSorted()
    {
        await _service.CreateAsync("Драма");
        await _service.CreateAsync("Комедия");

        var all = await _service.GetAllAsync();

        Assert.Equal(2, all.Count);
        Assert.Equal("Драма", all[0].Name);
    }

    public void Dispose() => _db.Dispose();
}

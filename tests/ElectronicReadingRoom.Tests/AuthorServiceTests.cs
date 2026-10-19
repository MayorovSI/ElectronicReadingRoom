using ElectronicReadingRoom.Core.Entities;
using ElectronicReadingRoom.Data;
using ElectronicReadingRoom.Data.Services;
using Microsoft.EntityFrameworkCore;

namespace ElectronicReadingRoom.Tests;

public class AuthorServiceTests : IDisposable
{
    private readonly AppDbContext _db;
    private readonly AuthorService _service;

    public AuthorServiceTests()
    {
        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;
        _db = new AppDbContext(options);
        _service = new AuthorService(_db);
    }

    [Fact]
    public async Task GetOrCreateAsync_NewAuthor_CreatesAuthor()
    {
        var author = await _service.GetOrCreateAsync("Толстой Л.Н.");

        Assert.NotNull(author);
        Assert.Equal("Толстой Л.Н.", author.Name);
        Assert.True(author.Id > 0);
    }

    [Fact]
    public async Task GetOrCreateAsync_ExistingAuthor_ReturnsExisting()
    {
        await _service.GetOrCreateAsync("Пушкин А.С.");
        var second = await _service.GetOrCreateAsync("Пушкин А.С.");

        var all = await _db.Authors.Where(a => a.Name == "Пушкин А.С.").ToListAsync();
        Assert.Single(all);
        Assert.Equal(second.Id, all[0].Id);
    }

    [Fact]
    public async Task SearchAsync_MatchingQuery_ReturnsAuthors()
    {
        await _service.GetOrCreateAsync("Толстой Л.Н.");
        await _service.GetOrCreateAsync("Толстой А.К.");
        await _service.GetOrCreateAsync("Пушкин А.С.");

        var results = await _service.SearchAsync("Толстой");

        Assert.Equal(2, results.Count);
    }

    [Fact]
    public async Task SearchAsync_NoMatch_ReturnsEmptyList()
    {
        await _service.GetOrCreateAsync("Пушкин А.С.");

        var results = await _service.SearchAsync("Гоголь");

        Assert.Empty(results);
    }

    [Fact]
    public async Task GetAllAsync_ReturnsAllAuthorsSorted()
    {
        await _service.GetOrCreateAsync("Булгаков М.А.");
        await _service.GetOrCreateAsync("Ахматова А.А.");

        var all = await _service.GetAllAsync();

        Assert.Equal(2, all.Count);
        Assert.Equal("Ахматова А.А.", all[0].Name);
    }

    public void Dispose() => _db.Dispose();
}

using ElectronicReadingRoom.Core.Entities;
using ElectronicReadingRoom.Data;
using ElectronicReadingRoom.Data.Services;
using Microsoft.EntityFrameworkCore;

namespace ElectronicReadingRoom.Tests;

public class DocumentServiceTests : IDisposable
{
    private readonly AppDbContext _db;
    private readonly DocumentService _service;

    public DocumentServiceTests()
    {
        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;
        _db = new AppDbContext(options);
        _service = new DocumentService(_db, new FakeFileStorageService());
    }

    [Fact]
    public async Task CreateAsync_ValidDocument_SavesToDatabase()
    {
        var doc = new Document { Title = "Test Book", Year = 2024 };
        using var stream = new MemoryStream("test content"u8.ToArray());

        var result = await _service.CreateAsync(doc, stream, "test.pdf");

        Assert.NotNull(result);
        Assert.Equal("Test Book", result.Title);
        Assert.Equal(2024, result.Year);
        Assert.NotNull(result.File);
        Assert.Equal("pdf", result.File.Format);
    }

    [Fact]
    public async Task GetByIdAsync_ExistingDocument_ReturnsDocument()
    {
        var doc = new Document { Title = "Find Me", Year = 2023 };
        using var stream = new MemoryStream("data"u8.ToArray());
        var created = await _service.CreateAsync(doc, stream, "find.txt");

        var found = await _service.GetByIdAsync(created.Id);

        Assert.NotNull(found);
        Assert.Equal("Find Me", found.Title);
    }

    [Fact]
    public async Task GetByIdAsync_NonExistent_ReturnsNull()
    {
        var found = await _service.GetByIdAsync(999);
        Assert.Null(found);
    }

    [Fact]
    public async Task ListAsync_ReturnsPaginatedResults()
    {
        for (int i = 1; i <= 5; i++)
        {
            var doc = new Document { Title = $"Book {i}", Year = 2020 + i };
            using var stream = new MemoryStream("data"u8.ToArray());
            await _service.CreateAsync(doc, stream, "book.pdf");
        }

        var (items, total) = await _service.ListAsync(1, 3);

        Assert.Equal(3, items.Count);
        Assert.Equal(5, total);
    }

    [Fact]
    public async Task ListAsync_SearchByTitle_FiltersResults()
    {
        var doc1 = new Document { Title = "War and Peace", Year = 1869 };
        var doc2 = new Document { Title = "Crime and Punishment", Year = 1866 };
        using var s1 = new MemoryStream("d"u8.ToArray());
        using var s2 = new MemoryStream("d"u8.ToArray());
        await _service.CreateAsync(doc1, s1, "w.pdf");
        await _service.CreateAsync(doc2, s2, "c.pdf");

        var (items, _) = await _service.ListAsync(1, 20, search: "War");

        Assert.Single(items);
        Assert.Equal("War and Peace", items[0].Title);
    }

    [Fact]
    public async Task DeleteAsync_ExistingDocument_RemovesFromDatabase()
    {
        var doc = new Document { Title = "Delete Me", Year = 2024 };
        using var stream = new MemoryStream("data"u8.ToArray());
        var created = await _service.CreateAsync(doc, stream, "del.pdf");

        await _service.DeleteAsync(created.Id);

        var found = await _service.GetByIdAsync(created.Id);
        Assert.Null(found);
    }

    public void Dispose() => _db.Dispose();
}

internal class FakeFileStorageService : ElectronicReadingRoom.Core.Interfaces.IFileStorageService
{
    public Task<string> SaveAsync(Stream fileStream, string extension)
        => Task.FromResult($"./uploads/{Guid.NewGuid()}.{extension}");

    public Task<Stream> GetAsync(string path)
        => Task.FromResult<Stream>(new MemoryStream("fake"u8.ToArray()));

    public Task DeleteAsync(string path) => Task.CompletedTask;

    public string DetectFormat(string fileName)
        => System.IO.Path.GetExtension(fileName).ToLowerInvariant().TrimStart('.');

    public long GetFileSize(string path) => 1024;
}

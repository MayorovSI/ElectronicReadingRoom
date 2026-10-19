using ElectronicReadingRoom.Core.Entities;
using ElectronicReadingRoom.Core.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace ElectronicReadingRoom.Data.Services;

public class DocumentService : IDocumentService
{
    private readonly AppDbContext _db;
    private readonly IFileStorageService _fileStorage;

    public DocumentService(AppDbContext db, IFileStorageService fileStorage)
    {
        _db = db;
        _fileStorage = fileStorage;
    }

    public async Task<Document?> GetByIdAsync(int id)
    {
        return await _db.Documents
            .Include(d => d.File)
            .Include(d => d.DocumentAuthors).ThenInclude(da => da.Author)
            .Include(d => d.DocumentGenres).ThenInclude(dg => dg.Genre)
            .FirstOrDefaultAsync(d => d.Id == id);
    }

    public async Task<(List<Document> Items, int TotalCount)> ListAsync(
        int page, int pageSize, string? search = null, int? genreId = null, string? sortBy = null)
    {
        var query = _db.Documents
            .Include(d => d.File)
            .Include(d => d.DocumentAuthors).ThenInclude(da => da.Author)
            .Include(d => d.DocumentGenres).ThenInclude(dg => dg.Genre)
            .AsQueryable();

        if (!string.IsNullOrWhiteSpace(search))
        {
            query = query.Where(d =>
                d.Title.Contains(search) ||
                d.DocumentAuthors.Any(da => da.Author.Name.Contains(search)) ||
                d.Year.ToString().Contains(search));
        }

        if (genreId.HasValue)
        {
            query = query.Where(d => d.DocumentGenres.Any(dg => dg.GenreId == genreId.Value));
        }

        query = sortBy?.ToLowerInvariant() switch
        {
            "year" => query.OrderByDescending(d => d.Year),
            "author" => query.OrderBy(d => d.DocumentAuthors.Select(da => da.Author.Name).FirstOrDefault()),
            _ => query.OrderBy(d => d.Title)
        };

        var total = await query.CountAsync();
        var items = await query.Skip((page - 1) * pageSize).Take(pageSize).ToListAsync();

        return (items, total);
    }

    public async Task<Document> CreateAsync(Document document, Stream fileStream, string fileName)
    {
        var format = _fileStorage.DetectFormat(fileName);
        var filePath = await _fileStorage.SaveAsync(fileStream, format);
        var fileSize = _fileStorage.GetFileSize(filePath);

        document.CreatedAt = DateTime.UtcNow;
        document.File = new DocumentFile
        {
            Path = filePath,
            Format = format,
            Size = fileSize
        };

        _db.Documents.Add(document);
        await _db.SaveChangesAsync();
        return document;
    }

    public async Task UpdateAsync(Document document)
    {
        var existing = await _db.Documents
            .Include(d => d.DocumentAuthors)
            .Include(d => d.DocumentGenres)
            .FirstOrDefaultAsync(d => d.Id == document.Id)
            ?? throw new KeyNotFoundException($"Document with id {document.Id} not found");

        existing.Title = document.Title;
        existing.Year = document.Year;
        existing.Publisher = document.Publisher;
        existing.Annotation = document.Annotation;

        _db.DocumentAuthors.RemoveRange(existing.DocumentAuthors);
        _db.DocumentGenres.RemoveRange(existing.DocumentGenres);

        existing.DocumentAuthors = document.DocumentAuthors;
        existing.DocumentGenres = document.DocumentGenres;

        await _db.SaveChangesAsync();
    }

    public async Task ReplaceFileAsync(int documentId, Stream fileStream, string fileName)
    {
        var doc = await _db.Documents.Include(d => d.File)
            .FirstOrDefaultAsync(d => d.Id == documentId)
            ?? throw new KeyNotFoundException($"Document with id {documentId} not found");

        if (doc.File is not null)
            await _fileStorage.DeleteAsync(doc.File.Path);

        var format = _fileStorage.DetectFormat(fileName);
        var filePath = await _fileStorage.SaveAsync(fileStream, format);
        var fileSize = _fileStorage.GetFileSize(filePath);

        doc.File!.Path = filePath;
        doc.File.Format = format;
        doc.File.Size = fileSize;

        await _db.SaveChangesAsync();
    }

    public async Task DeleteAsync(int id)
    {
        var doc = await _db.Documents
            .Include(d => d.File)
            .FirstOrDefaultAsync(d => d.Id == id)
            ?? throw new KeyNotFoundException($"Document with id {id} not found");

        if (doc.File is not null)
            await _fileStorage.DeleteAsync(doc.File.Path);

        _db.Documents.Remove(doc);
        await _db.SaveChangesAsync();
    }
}

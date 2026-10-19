using ElectronicReadingRoom.Core.Entities;

namespace ElectronicReadingRoom.Core.Interfaces;

public interface IDocumentService
{
    Task<Document?> GetByIdAsync(int id);
    Task<(List<Document> Items, int TotalCount)> ListAsync(int page, int pageSize, string? search = null, int? genreId = null, string? sortBy = null);
    Task<Document> CreateAsync(Document document, Stream fileStream, string fileName);
    Task UpdateAsync(Document document);
    Task ReplaceFileAsync(int documentId, Stream fileStream, string fileName);
    Task DeleteAsync(int id);
}

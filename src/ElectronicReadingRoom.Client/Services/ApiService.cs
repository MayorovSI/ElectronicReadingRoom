using System.Net.Http.Json;
using ElectronicReadingRoom.Core.Entities;

namespace ElectronicReadingRoom.Client.Services;

public class ApiService
{
    private readonly HttpClient _http;

    public ApiService(HttpClient http)
    {
        _http = http;
    }

    // Documents
    public async Task<DocumentListResult> GetDocumentsAsync(int page = 1, int pageSize = 20,
        string? search = null, int? genreId = null, string? sortBy = null)
    {
        var query = $"?page={page}&pageSize={pageSize}";
        if (search is not null) query += $"&search={Uri.EscapeDataString(search)}";
        if (genreId is not null) query += $"&genreId={genreId}";
        if (sortBy is not null) query += $"&sortBy={sortBy}";

        var response = await _http.GetFromJsonAsync<DocumentListResult>($"api/documents{query}");
        return response!;
    }

    public async Task<Document?> GetDocumentAsync(int id)
    {
        return await _http.GetFromJsonAsync<Document>($"api/documents/{id}");
    }

    public async Task DeleteDocumentAsync(int id)
    {
        await _http.DeleteAsync($"api/documents/{id}");
    }

    public async Task UpdateDocumentAsync(Document document)
    {
        await _http.PutAsJsonAsync($"api/documents/{document.Id}", document);
    }

    // Authors
    public async Task<List<Author>> SearchAuthorsAsync(string query)
    {
        return await _http.GetFromJsonAsync<List<Author>>($"api/authors/search?q={Uri.EscapeDataString(query)}") ?? [];
    }

    public async Task<List<Author>> GetAllAuthorsAsync()
    {
        return await _http.GetFromJsonAsync<List<Author>>("api/authors") ?? [];
    }

    // Genres
    public async Task<List<Genre>> GetAllGenresAsync()
    {
        return await _http.GetFromJsonAsync<List<Genre>>("api/genres") ?? [];
    }

    public string GetDownloadUrl(int documentId) => $"api/documents/{documentId}/download";

    public async Task CreateGenreAsync(string name)
    {
        await _http.PostAsJsonAsync("api/genres", new { Name = name });
    }

    public async Task UpdateGenreAsync(int id, string name)
    {
        await _http.PutAsJsonAsync($"api/genres/{id}", new { Name = name });
    }

    public async Task DeleteGenreAsync(int id)
    {
        await _http.DeleteAsync($"api/genres/{id}");
    }
}

public record DocumentListResult(List<Document> Items, int Total, int Page, int PageSize);

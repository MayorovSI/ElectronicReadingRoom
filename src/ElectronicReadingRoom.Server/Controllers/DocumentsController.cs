using ElectronicReadingRoom.Core.Entities;
using ElectronicReadingRoom.Core.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace ElectronicReadingRoom.Server.Controllers;

[ApiController]
[Route("api/[controller]")]
public class DocumentsController : ControllerBase
{
    private readonly IDocumentService _documentService;
    private readonly IFileStorageService _fileStorage;

    public DocumentsController(IDocumentService documentService, IFileStorageService fileStorage)
    {
        _documentService = documentService;
        _fileStorage = fileStorage;
    }

    [HttpGet]
    public async Task<IActionResult> List(
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 20,
        [FromQuery] string? search = null,
        [FromQuery] int? genreId = null,
        [FromQuery] string? sortBy = null)
    {
        var (items, total) = await _documentService.ListAsync(page, pageSize, search, genreId, sortBy);
        return Ok(new { items, total, page, pageSize });
    }

    [HttpGet("{id}")]
    public async Task<IActionResult> GetById(int id)
    {
        var doc = await _documentService.GetByIdAsync(id);
        return doc is null ? NotFound() : Ok(doc);
    }

    [HttpPost]
    public async Task<IActionResult> Create([FromForm] string title, [FromForm] int year,
        [FromForm] string? publisher, [FromForm] string? annotation,
        [FromForm] string authorNames, [FromForm] string? genreIds,
        IFormFile file)
    {
        var document = new Document
        {
            Title = title,
            Year = year,
            Publisher = publisher,
            Annotation = annotation
        };

        var authorNameList = authorNames.Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);
        foreach (var name in authorNameList)
            document.DocumentAuthors.Add(new DocumentAuthor { Author = new Author { Name = name } });

        if (!string.IsNullOrWhiteSpace(genreIds))
        {
            foreach (var gid in genreIds.Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries))
            {
                if (int.TryParse(gid, out var id))
                    document.DocumentGenres.Add(new DocumentGenre { GenreId = id });
            }
        }

        using var stream = file.OpenReadStream();
        var created = await _documentService.CreateAsync(document, stream, file.FileName);
        return CreatedAtAction(nameof(GetById), new { id = created.Id }, created);
    }

    [HttpPut("{id}")]
    public async Task<IActionResult> Update(int id, [FromBody] Document document)
    {
        if (id != document.Id) return BadRequest();
        await _documentService.UpdateAsync(document);
        return NoContent();
    }

    [HttpPut("{id}/file")]
    public async Task<IActionResult> ReplaceFile(int id, IFormFile file)
    {
        using var stream = file.OpenReadStream();
        await _documentService.ReplaceFileAsync(id, stream, file.FileName);
        return NoContent();
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> Delete(int id)
    {
        await _documentService.DeleteAsync(id);
        return NoContent();
    }

    [HttpGet("{id}/download")]
    public async Task<IActionResult> Download(int id)
    {
        var doc = await _documentService.GetByIdAsync(id);
        if (doc?.File is null) return NotFound();

        var path = doc.File.Path;
        if (!System.IO.File.Exists(path)) return NotFound();

        var ext = doc.File.Format;
        var fileName = $"{doc.Title}.{ext}";
        var contentType = GetContentType(ext);

        var stream = new FileStream(path, FileMode.Open, FileAccess.Read);
        return File(stream, contentType, fileName);
    }

    private static string GetContentType(string format) => format.ToLowerInvariant() switch
    {
        "pdf" => "application/pdf",
        "epub" => "application/epub+zip",
        "djvu" => "image/vnd.djvu",
        "txt" => "text/plain; charset=utf-8",
        "docx" => "application/vnd.openxmlformats-officedocument.wordprocessingml.document",
        _ => "application/octet-stream"
    };
}

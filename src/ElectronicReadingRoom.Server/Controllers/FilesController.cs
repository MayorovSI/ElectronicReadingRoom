using ElectronicReadingRoom.Core.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace ElectronicReadingRoom.Server.Controllers;

[ApiController]
[Route("api/[controller]")]
public class FilesController : ControllerBase
{
    private readonly IFileStorageService _fileStorage;

    public FilesController(IFileStorageService fileStorage)
    {
        _fileStorage = fileStorage;
    }

    [HttpGet("{*path}")]
    public async Task<IActionResult> GetFile(string path)
    {
        try
        {
            var stream = await _fileStorage.GetAsync(path);
            var contentType = GetContentType(path);
            return File(stream, contentType);
        }
        catch (FileNotFoundException)
        {
            return NotFound();
        }
    }

    private static string GetContentType(string path)
    {
        var ext = Path.GetExtension(path).ToLowerInvariant();
        return ext switch
        {
            ".pdf" => "application/pdf",
            ".epub" => "application/epub+zip",
            ".djvu" => "image/vnd.djvu",
            ".txt" => "text/plain",
            ".docx" => "application/vnd.openxmlformats-officedocument.wordprocessingml.document",
            _ => "application/octet-stream"
        };
    }
}

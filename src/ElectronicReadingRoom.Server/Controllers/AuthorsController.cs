using ElectronicReadingRoom.Core.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace ElectronicReadingRoom.Server.Controllers;

[ApiController]
[Route("api/[controller]")]
public class AuthorsController : ControllerBase
{
    private readonly IAuthorService _authorService;

    public AuthorsController(IAuthorService authorService)
    {
        _authorService = authorService;
    }

    [HttpGet]
    public async Task<IActionResult> GetAll()
    {
        var authors = await _authorService.GetAllAsync();
        return Ok(authors);
    }

    [HttpGet("search")]
    public async Task<IActionResult> Search([FromQuery] string q)
    {
        var authors = await _authorService.SearchAsync(q);
        return Ok(authors);
    }

    [HttpPost]
    public async Task<IActionResult> Create([FromBody] CreateAuthorRequest request)
    {
        var author = await _authorService.GetOrCreateAsync(request.Name);
        return Ok(author);
    }
}

public record CreateAuthorRequest(string Name);

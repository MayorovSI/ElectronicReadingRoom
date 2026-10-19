using ElectronicReadingRoom.Core.Entities;
using ElectronicReadingRoom.Core.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace ElectronicReadingRoom.Server.Controllers;

[ApiController]
[Route("api/[controller]")]
public class GenresController : ControllerBase
{
    private readonly IGenreService _genreService;

    public GenresController(IGenreService genreService)
    {
        _genreService = genreService;
    }

    [HttpGet]
    public async Task<IActionResult> GetAll()
    {
        var genres = await _genreService.GetAllAsync();
        return Ok(genres);
    }

    [HttpGet("{id}")]
    public async Task<IActionResult> GetById(int id)
    {
        var genre = await _genreService.GetByIdAsync(id);
        return genre is null ? NotFound() : Ok(genre);
    }

    [HttpPost]
    public async Task<IActionResult> Create([FromBody] CreateGenreRequest request)
    {
        var genre = await _genreService.CreateAsync(request.Name);
        return CreatedAtAction(nameof(GetById), new { id = genre.Id }, genre);
    }

    [HttpPut("{id}")]
    public async Task<IActionResult> Update(int id, [FromBody] UpdateGenreRequest request)
    {
        await _genreService.UpdateAsync(new Genre { Id = id, Name = request.Name });
        return NoContent();
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> Delete(int id)
    {
        await _genreService.DeleteAsync(id);
        return NoContent();
    }
}

public record CreateGenreRequest(string Name);
public record UpdateGenreRequest(string Name);

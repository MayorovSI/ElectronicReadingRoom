using ElectronicReadingRoom.Core.Entities;
using ElectronicReadingRoom.Core.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace ElectronicReadingRoom.Data.Services;

public class GenreService : IGenreService
{
    private readonly AppDbContext _db;

    public GenreService(AppDbContext db)
    {
        _db = db;
    }

    public async Task<Genre?> GetByIdAsync(int id)
    {
        return await _db.Genres.FindAsync(id);
    }

    public async Task<List<Genre>> GetAllAsync()
    {
        return await _db.Genres.OrderBy(g => g.Name).ToListAsync();
    }

    public async Task<Genre> CreateAsync(string name)
    {
        var existing = await _db.Genres.FirstOrDefaultAsync(g => g.Name == name);
        if (existing is not null)
            throw new InvalidOperationException($"Genre '{name}' already exists");

        var genre = new Genre { Name = name };
        _db.Genres.Add(genre);
        await _db.SaveChangesAsync();
        return genre;
    }

    public async Task UpdateAsync(Genre genre)
    {
        var existing = await _db.Genres.FindAsync(genre.Id)
            ?? throw new KeyNotFoundException($"Genre with id {genre.Id} not found");

        var duplicate = await _db.Genres.AnyAsync(g => g.Name == genre.Name && g.Id != genre.Id);
        if (duplicate)
            throw new InvalidOperationException($"Genre '{genre.Name}' already exists");

        existing.Name = genre.Name;
        await _db.SaveChangesAsync();
    }

    public async Task DeleteAsync(int id)
    {
        var genre = await _db.Genres.FindAsync(id)
            ?? throw new KeyNotFoundException($"Genre with id {id} not found");

        _db.Genres.Remove(genre);
        await _db.SaveChangesAsync();
    }
}

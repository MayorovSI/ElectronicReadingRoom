using ElectronicReadingRoom.Core.Entities;
using ElectronicReadingRoom.Core.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace ElectronicReadingRoom.Data.Services;

public class AuthorService : IAuthorService
{
    private readonly AppDbContext _db;

    public AuthorService(AppDbContext db)
    {
        _db = db;
    }

    public async Task<Author?> GetByIdAsync(int id)
    {
        return await _db.Authors.FindAsync(id);
    }

    public async Task<List<Author>> SearchAsync(string query)
    {
        return await _db.Authors
            .Where(a => a.Name.Contains(query))
            .OrderBy(a => a.Name)
            .Take(20)
            .ToListAsync();
    }

    public async Task<Author> GetOrCreateAsync(string name)
    {
        var existing = await _db.Authors.FirstOrDefaultAsync(a => a.Name == name);
        if (existing is not null)
            return existing;

        var author = new Author { Name = name };
        _db.Authors.Add(author);
        await _db.SaveChangesAsync();
        return author;
    }

    public async Task<List<Author>> GetAllAsync()
    {
        return await _db.Authors.OrderBy(a => a.Name).ToListAsync();
    }
}

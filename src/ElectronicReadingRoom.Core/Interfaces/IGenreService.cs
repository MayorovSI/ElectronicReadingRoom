using ElectronicReadingRoom.Core.Entities;

namespace ElectronicReadingRoom.Core.Interfaces;

public interface IGenreService
{
    Task<Genre?> GetByIdAsync(int id);
    Task<List<Genre>> GetAllAsync();
    Task<Genre> CreateAsync(string name);
    Task UpdateAsync(Genre genre);
    Task DeleteAsync(int id);
}

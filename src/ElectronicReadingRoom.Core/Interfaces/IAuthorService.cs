using ElectronicReadingRoom.Core.Entities;

namespace ElectronicReadingRoom.Core.Interfaces;

public interface IAuthorService
{
    Task<Author?> GetByIdAsync(int id);
    Task<List<Author>> SearchAsync(string query);
    Task<Author> GetOrCreateAsync(string name);
    Task<List<Author>> GetAllAsync();
}

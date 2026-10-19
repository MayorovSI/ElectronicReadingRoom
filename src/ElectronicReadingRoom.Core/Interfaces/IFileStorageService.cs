namespace ElectronicReadingRoom.Core.Interfaces;

public interface IFileStorageService
{
    Task<string> SaveAsync(Stream fileStream, string extension);
    Task<Stream> GetAsync(string path);
    Task DeleteAsync(string path);
    string DetectFormat(string fileName);
    long GetFileSize(string path);
}

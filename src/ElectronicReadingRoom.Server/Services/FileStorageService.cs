using ElectronicReadingRoom.Core.Interfaces;

namespace ElectronicReadingRoom.Server.Services;

public class FileStorageService : IFileStorageService
{
    private readonly string _uploadDir;

    public FileStorageService(IWebHostEnvironment env)
    {
        _uploadDir = Path.Combine(env.ContentRootPath, "uploads");
        Directory.CreateDirectory(_uploadDir);
    }

    public async Task<string> SaveAsync(Stream fileStream, string extension)
    {
        var fileName = $"{Guid.NewGuid()}.{extension.TrimStart('.')}";
        var filePath = Path.Combine(_uploadDir, fileName);

        using var fs = new FileStream(filePath, FileMode.Create);
        await fileStream.CopyToAsync(fs);

        return filePath;
    }

    public Task<Stream> GetAsync(string path)
    {
        if (!File.Exists(path))
            throw new FileNotFoundException("File not found", path);

        var stream = new FileStream(path, FileMode.Open, FileAccess.Read);
        return Task.FromResult<Stream>(stream);
    }

    public Task DeleteAsync(string path)
    {
        if (File.Exists(path))
            File.Delete(path);

        return Task.CompletedTask;
    }

    public string DetectFormat(string fileName)
    {
        var ext = Path.GetExtension(fileName).ToLowerInvariant();
        return ext switch
        {
            ".pdf" => "pdf",
            ".epub" => "epub",
            ".djvu" => "djvu",
            ".txt" => "txt",
            ".docx" => "docx",
            _ => throw new NotSupportedException($"Format '{ext}' is not supported")
        };
    }

    public long GetFileSize(string path)
    {
        if (!File.Exists(path))
            return 0;

        var info = new FileInfo(path);
        return info.Length;
    }
}

namespace ElectronicReadingRoom.Core.Entities;

public class Document
{
    public int Id { get; set; }
    public string Title { get; set; } = string.Empty;
    public int Year { get; set; }
    public string? Publisher { get; set; }
    public string? Annotation { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public DocumentFile? File { get; set; }
    public List<DocumentAuthor> DocumentAuthors { get; set; } = [];
    public List<DocumentGenre> DocumentGenres { get; set; } = [];
}

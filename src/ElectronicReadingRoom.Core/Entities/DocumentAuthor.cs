using System.Text.Json.Serialization;

namespace ElectronicReadingRoom.Core.Entities;

public class DocumentAuthor
{
    public int DocumentId { get; set; }
    public int AuthorId { get; set; }

    [JsonIgnore]
    public Document? Document { get; set; }
    public Author Author { get; set; } = null!;
}

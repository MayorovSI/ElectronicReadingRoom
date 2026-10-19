using System.Text.Json.Serialization;

namespace ElectronicReadingRoom.Core.Entities;

public class DocumentFile
{
    public int Id { get; set; }
    public int DocumentId { get; set; }

    [JsonIgnore]
    public string Path { get; set; } = string.Empty;

    public string Format { get; set; } = string.Empty;
    public long Size { get; set; }

    [JsonIgnore]
    public Document Document { get; set; } = null!;
}

using System.Text.Json.Serialization;

namespace ElectronicReadingRoom.Core.Entities;

public class Author
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;

    [JsonIgnore]
    public List<DocumentAuthor> DocumentAuthors { get; set; } = [];
}

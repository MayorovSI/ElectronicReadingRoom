using System.Text.Json.Serialization;

namespace ElectronicReadingRoom.Core.Entities;

public class Genre
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;

    [JsonIgnore]
    public List<DocumentGenre> DocumentGenres { get; set; } = [];
}

using System.Text.Json.Serialization;

namespace ElectronicReadingRoom.Core.Entities;

public class DocumentGenre
{
    public int DocumentId { get; set; }
    public int GenreId { get; set; }

    [JsonIgnore]
    public Document? Document { get; set; }
    public Genre? Genre { get; set; }
}

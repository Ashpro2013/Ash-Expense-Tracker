namespace AshproApp.Models;

public sealed class AlbumImage
{
    public string Id { get; set; } = string.Empty;
    public int UserId { get; set; }
    public string Category { get; set; } = "Personal";
    public string Caption { get; set; } = string.Empty;
    public string FileName { get; set; } = string.Empty;
    public DateTime CreatedAtUtc { get; set; } = DateTime.UtcNow;
}

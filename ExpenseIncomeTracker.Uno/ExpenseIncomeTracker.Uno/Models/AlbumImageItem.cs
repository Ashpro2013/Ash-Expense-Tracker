namespace ExpenseIncomeTracker.Uno.Models;

public sealed class AlbumImageItem
{
    public string Id { get; set; } = Guid.NewGuid().ToString("N");
    public string UserEmail { get; set; } = string.Empty;
    public string FileName { get; set; } = string.Empty;
    public string OriginalName { get; set; } = string.Empty;
    public string SourceType { get; set; } = "Gallery";
    public DateTime AddedOn { get; set; } = DateTime.Now;
    public string ImageUri => $"ms-appdata:///local/album/{Uri.EscapeDataString(FileName)}";
}

namespace ExpenseIncomeTracker.Uno.Models;

public sealed class PasswordDirectoryEntry
{
    public string Id { get; set; } = Guid.NewGuid().ToString("N");
    public string UserEmail { get; set; } = string.Empty;
    public string Title { get; set; } = string.Empty;
    public string Username { get; set; } = string.Empty;
    public string Password { get; set; } = string.Empty;
    public string Notes { get; set; } = string.Empty;

    public string MaskedPassword
        => string.IsNullOrEmpty(Password)
            ? string.Empty
            : new string('*', Math.Min(Password.Length, 12));
}

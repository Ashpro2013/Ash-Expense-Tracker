namespace ExpenseIncomeTracker.Uno.Models;

public sealed class AddressBookEntry
{
    public string Id { get; set; } = Guid.NewGuid().ToString("N");
    public string UserEmail { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string Address { get; set; } = string.Empty;
    public string PhoneNo { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string Remarks { get; set; } = string.Empty;
}

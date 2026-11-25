namespace StoreSystem.Models;

public class Customer
{
    public int Id { get; set; }
    public string FullName { get; set; } = string.Empty;
    public string Phone { get; set; } = string.Empty;

    public LoyaltyCard? LoyaltyCard { get; set; }
    public List<Order> Orders { get; set; } = new();
}

namespace StoreSystem.Models;

public class LoyaltyCard
{
    public int Id { get; set; }
    public string CardNumber { get; set; } = string.Empty;
    public decimal DiscountPercent { get; set; }

    public int CustomerId { get; set; }
    public Customer? Customer { get; set; }
}

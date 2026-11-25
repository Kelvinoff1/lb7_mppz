namespace StoreSystem.Models;

public class Payment
{
    public int Id { get; set; }
    public decimal Amount { get; set; }
    public DateTime PaymentDate { get; set; }
    public PaymentMethod Method { get; set; }

    public int OrderId { get; set; }
    public Order? Order { get; set; }
}

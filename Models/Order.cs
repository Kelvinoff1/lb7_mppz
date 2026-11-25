using StoreSystem.Models.States;

namespace StoreSystem.Models;

public class Order
{
    public int Id { get; set; }
    public DateTime OrderDate { get; set; }

    private OrderState _state = new NewOrderState();

    public OrderStatus Status => _state.Status;

    public int CustomerId { get; set; }
    public Customer? Customer { get; set; }

    public int EmployeeId { get; set; }
    public Employee? Employee { get; set; }

    public List<OrderItem> Items { get; set; } = new();

    public Payment? Payment { get; set; }

    public decimal Subtotal => Items.Sum(i => i.Total);

    public decimal DiscountPercent => Customer?.LoyaltyCard?.DiscountPercent ?? 0m;

    public decimal Total => Math.Round(Subtotal * (1 - DiscountPercent / 100m), 2);

    public void EnsureEditable() => _state.EnsureEditable(this);

    public void RegisterPayment(Payment payment) => _state.Pay(this, payment);

    public void Complete() => _state.Complete(this);

    public void Cancel() => _state.Cancel(this);

    internal void ChangeState(OrderState next)
    {
        _state = next;
        next.OnEnter(this);
    }

    internal void RestoreStatus(OrderStatus status) => _state = OrderState.FromStatus(status);
}

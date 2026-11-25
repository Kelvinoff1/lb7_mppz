namespace StoreSystem.Models.States;

public abstract class OrderState
{

    public abstract OrderStatus Status { get; }

    public abstract string DisplayName { get; }

    public virtual void OnEnter(Order order)
    {
    }

    public virtual void EnsureEditable(Order order) =>
        throw new InvalidOperationException(
            $"Замовлення #{order.Id} у стані «{DisplayName}», змінювати його не можна (BR-04).");

    public virtual void Pay(Order order, Payment payment) =>
        throw new InvalidOperationException(
            $"Замовлення #{order.Id} у стані «{DisplayName}», оплатити його не можна.");

    public virtual void Complete(Order order) =>
        throw new InvalidOperationException(
            $"Замовлення #{order.Id} у стані «{DisplayName}», видати його не можна.");

    public virtual void Cancel(Order order) =>
        throw new InvalidOperationException(
            $"Замовлення #{order.Id} у стані «{DisplayName}», скасувати його не можна.");

    internal static OrderState FromStatus(OrderStatus status) => status switch
    {
        OrderStatus.New => new NewOrderState(),
        OrderStatus.Paid => new PaidOrderState(),
        OrderStatus.Completed => new CompletedOrderState(),
        OrderStatus.Cancelled => new CancelledOrderState(),
        _ => throw new InvalidOperationException($"Невідомий статус замовлення: {status}.")
    };
}

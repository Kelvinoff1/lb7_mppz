namespace StoreSystem.Models.States;

public class CancelledOrderState : OrderState
{
    public override OrderStatus Status => OrderStatus.Cancelled;

    public override string DisplayName => "Скасоване";

    public override void OnEnter(Order order)
    {
        foreach (var item in order.Items)
            item.Product?.IncreaseStock(item.Quantity);
    }

    public override void Pay(Order order, Payment payment) =>
        throw new InvalidOperationException($"Замовлення #{order.Id} скасовано, оплатити його не можна.");

    public override void Complete(Order order) =>
        throw new InvalidOperationException($"Замовлення #{order.Id} скасовано, видати його не можна.");

    public override void Cancel(Order order) =>
        throw new InvalidOperationException($"Замовлення #{order.Id} вже скасовано.");
}

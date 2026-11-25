namespace StoreSystem.Models.States;

public class CompletedOrderState : OrderState
{
    public override OrderStatus Status => OrderStatus.Completed;

    public override string DisplayName => "Видане";

    public override void Pay(Order order, Payment payment) =>
        throw new InvalidOperationException($"Замовлення #{order.Id} вже оплачено.");

    public override void Complete(Order order) =>
        throw new InvalidOperationException($"Замовлення #{order.Id} вже видано.");

    public override void Cancel(Order order) =>
        throw new InvalidOperationException(
            $"Замовлення #{order.Id} вже видано покупцю, скасувати його не можна.");
}

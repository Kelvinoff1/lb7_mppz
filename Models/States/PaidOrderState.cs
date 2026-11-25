namespace StoreSystem.Models.States;

public class PaidOrderState : OrderState
{
    public override OrderStatus Status => OrderStatus.Paid;

    public override string DisplayName => "Оплачене";

    public override void Pay(Order order, Payment payment) =>
        throw new InvalidOperationException($"Замовлення #{order.Id} вже оплачено.");

    public override void Complete(Order order) => order.ChangeState(new CompletedOrderState());

    public override void Cancel(Order order) =>
        throw new InvalidOperationException(
            $"Замовлення #{order.Id} вже оплачено, скасувати його не можна.");
}

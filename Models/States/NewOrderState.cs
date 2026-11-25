namespace StoreSystem.Models.States;

public class NewOrderState : OrderState
{
    public override OrderStatus Status => OrderStatus.New;

    public override string DisplayName => "Нове";

    public override void EnsureEditable(Order order)
    {
    }

    public override void Pay(Order order, Payment payment)
    {
        if (order.Items.Count == 0)
            throw new InvalidOperationException($"Замовлення #{order.Id} порожнє.");
        if (order.Payment != null)
            throw new InvalidOperationException($"Замовлення #{order.Id} вже має оплату.");

        order.Payment = payment;
        payment.Order = order;
        order.ChangeState(new PaidOrderState());
    }

    public override void Complete(Order order) =>
        throw new InvalidOperationException(
            $"Замовлення #{order.Id} ще не оплачено, видати його не можна.");

    public override void Cancel(Order order) => order.ChangeState(new CancelledOrderState());
}

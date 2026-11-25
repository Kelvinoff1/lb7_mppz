using StoreSystem.Abstractions;
using StoreSystem.Models;

namespace StoreSystem.UseCases;

public record CheckoutResult(Order Order, PaymentReceipt Receipt);

public class CheckoutOrderUseCase
{
    private readonly CreateOrderUseCase _createOrder;
    private readonly PayOrderUseCase _payOrder;
    private readonly CancelOrderUseCase _cancelOrder;
    private readonly IAuditLog _log;

    public CheckoutOrderUseCase(CreateOrderUseCase createOrder, PayOrderUseCase payOrder,
        CancelOrderUseCase cancelOrder, IAuditLog log)
    {
        _createOrder = createOrder;
        _payOrder = payOrder;
        _cancelOrder = cancelOrder;
        _log = log;
    }

    public CheckoutResult Execute(int customerId, int employeeId, DateTime date,
        IReadOnlyCollection<(int ProductId, int Quantity)> items, PaymentMethod method)
    {

        var order = _createOrder.Execute(customerId, employeeId, date, items);

        try
        {

            var receipt = _payOrder.Execute(order.Id, method, date);
            _log.Info($"UC-12: покупку за замовленням #{order.Id} оформлено");
            return new CheckoutResult(order, receipt);
        }
        catch (InvalidOperationException)
        {

            _cancelOrder.Execute(order.Id);
            throw;
        }
    }
}

using System.Globalization;
using StoreSystem.Abstractions;
using StoreSystem.Models;

namespace StoreSystem.UseCases;

public class PayOrderUseCase
{
    private readonly IStoreRepository _repository;
    private readonly ApplyLoyaltyDiscountUseCase _applyDiscount;
    private readonly IAuditLog _log;

    public PayOrderUseCase(IStoreRepository repository, ApplyLoyaltyDiscountUseCase applyDiscount, IAuditLog log)
    {
        _repository = repository;
        _applyDiscount = applyDiscount;
        _log = log;
    }

    public PaymentReceipt Execute(int orderId, PaymentMethod method, DateTime date)
    {
        var order = _repository.GetOrderById(orderId)
            ?? throw new InvalidOperationException($"Замовлення з Id={orderId} не знайдено.");

        if (!Enum.IsDefined(method))
            throw new InvalidOperationException($"Невідомий спосіб оплати: {(int)method}.");

        decimal discount = _applyDiscount.Execute(order);

        var payment = new Payment
        {
            Id = _repository.NextPaymentId(),
            OrderId = orderId,
            Amount = order.Total,
            PaymentDate = date,
            Method = method
        };
        _repository.AddPayment(payment);

        _log.Info($"UC-07: оплачено замовлення #{orderId}, сума {payment.Amount.ToString("F2", CultureInfo.InvariantCulture)} грн ({method}), знижка {discount.ToString("F2", CultureInfo.InvariantCulture)} грн");
        return new PaymentReceipt(payment, order.Subtotal, discount);
    }
}

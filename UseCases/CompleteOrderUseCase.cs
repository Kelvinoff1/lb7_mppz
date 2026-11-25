using StoreSystem.Abstractions;
using StoreSystem.Models;

namespace StoreSystem.UseCases;

public class CompleteOrderUseCase
{
    private readonly IStoreRepository _repository;
    private readonly IAuditLog _log;

    public CompleteOrderUseCase(IStoreRepository repository, IAuditLog log)
    {
        _repository = repository;
        _log = log;
    }

    public Order Execute(int orderId)
    {
        var order = _repository.GetOrderById(orderId)
            ?? throw new InvalidOperationException($"Замовлення з Id={orderId} не знайдено.");

        order.Complete(); 

        _log.Info($"UC-10: замовлення #{orderId} видано покупцю (стан: Видане)");
        return order;
    }
}

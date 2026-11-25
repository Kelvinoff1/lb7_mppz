using StoreSystem.Abstractions;
using StoreSystem.Models;

namespace StoreSystem.UseCases;

public class CreateOrderUseCase
{
    private readonly IStoreRepository _repository;
    private readonly CheckProductStockUseCase _checkStock;
    private readonly AddProductToOrderUseCase _addProduct;
    private readonly IAuditLog _log;

    public CreateOrderUseCase(IStoreRepository repository, CheckProductStockUseCase checkStock,
        AddProductToOrderUseCase addProduct, IAuditLog log)
    {
        _repository = repository;
        _checkStock = checkStock;
        _addProduct = addProduct;
        _log = log;
    }

    public Order Execute(int customerId, int employeeId, DateTime date,
        IReadOnlyCollection<(int ProductId, int Quantity)> items)
    {

        if (items.Count == 0)
            throw new InvalidOperationException("Замовлення має містити хоча б один товар.");

        foreach (var group in items.GroupBy(i => i.ProductId))
            _checkStock.Execute(group.Key, group.Sum(i => i.Quantity));

        var order = new Order
        {
            Id = _repository.NextOrderId(),
            OrderDate = date,
            CustomerId = customerId,
            EmployeeId = employeeId
        };
        _repository.AddOrder(order);

        foreach (var (productId, quantity) in items)
            _addProduct.Execute(order.Id, productId, quantity); 

        _log.Info($"UC-05: створено замовлення #{order.Id} (покупець #{customerId}, співробітник #{employeeId}, позицій: {order.Items.Count})");
        return order;
    }
}

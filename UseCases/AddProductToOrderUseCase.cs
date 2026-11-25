using StoreSystem.Abstractions;
using StoreSystem.Models;
using StoreSystem.Rules;

namespace StoreSystem.UseCases;

public class AddProductToOrderUseCase
{
    private readonly IStoreRepository _repository;
    private readonly CheckProductStockUseCase _checkStock;
    private readonly IAuditLog _log;

    public AddProductToOrderUseCase(IStoreRepository repository, CheckProductStockUseCase checkStock, IAuditLog log)
    {
        _repository = repository;
        _checkStock = checkStock;
        _log = log;
    }

    public OrderItem Execute(int orderId, int productId, int quantity)
    {
        var order = _repository.GetOrderById(orderId)
            ?? throw new InvalidOperationException($"Замовлення з Id={orderId} не знайдено.");

        order.EnsureEditable();

        BusinessRules.ValidateQuantity(quantity);
        var existing = order.Items.FirstOrDefault(i => i.ProductId == productId);
        if (existing != null)
            BusinessRules.ValidateQuantity(existing.Quantity + quantity);

        _checkStock.Execute(productId, quantity);

        var product = _repository.GetProductById(productId)!;
        product.DecreaseStock(quantity); 

        OrderItem item;
        if (existing != null)
        {
            existing.Quantity += quantity;
            item = existing;
        }
        else
        {
            item = new OrderItem
            {
                Id = _repository.NextOrderItemId(),
                OrderId = orderId,
                ProductId = productId,
                Quantity = quantity
            };
            _repository.AddOrderItem(item);
        }

        _log.Info($"UC-06: до замовлення #{orderId} додано товар #{productId} x{quantity}");
        return item;
    }
}

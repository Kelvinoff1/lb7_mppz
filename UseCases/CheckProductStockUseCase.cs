using StoreSystem.Abstractions;

namespace StoreSystem.UseCases;

public class CheckProductStockUseCase
{
    private readonly IStoreRepository _repository;

    public CheckProductStockUseCase(IStoreRepository repository)
    {
        _repository = repository;
    }

    public void Execute(int productId, int quantity)
    {
        var product = _repository.GetProductById(productId)
            ?? throw new InvalidOperationException($"Товар з Id={productId} не знайдено.");

        if (!product.HasStock(quantity))
            throw new InvalidOperationException(
                $"Недостатньо товару \"{product.Title}\" на складі (потрібно {quantity}, доступно {product.StockQuantity}).");
    }
}

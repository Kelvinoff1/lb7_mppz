using StoreSystem.Abstractions;
using StoreSystem.Models;

namespace StoreSystem.UseCases;

public class ViewProductSuppliersUseCase
{
    private readonly IStoreRepository _repository;

    public ViewProductSuppliersUseCase(IStoreRepository repository)
    {
        _repository = repository;
    }

    public IReadOnlyList<Supplier> Execute(int productId)
    {
        var product = _repository.GetProductById(productId)
            ?? throw new InvalidOperationException($"Товар з Id={productId} не знайдено.");

        return product.Suppliers.ToList();
    }
}

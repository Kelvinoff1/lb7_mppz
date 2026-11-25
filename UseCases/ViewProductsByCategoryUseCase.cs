using StoreSystem.Abstractions;
using StoreSystem.Models;

namespace StoreSystem.UseCases;

public class ViewProductsByCategoryUseCase
{
    private readonly IStoreRepository _repository;

    public ViewProductsByCategoryUseCase(IStoreRepository repository)
    {
        _repository = repository;
    }

    public IReadOnlyList<Product> Execute(int categoryId)
    {
        var category = _repository.GetCategoryById(categoryId)
            ?? throw new InvalidOperationException($"Категорію з Id={categoryId} не знайдено.");

        return category.Products;
    }
}

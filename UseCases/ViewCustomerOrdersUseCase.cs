using StoreSystem.Abstractions;
using StoreSystem.Models;

namespace StoreSystem.UseCases;

public class ViewCustomerOrdersUseCase
{
    private readonly IStoreRepository _repository;

    public ViewCustomerOrdersUseCase(IStoreRepository repository)
    {
        _repository = repository;
    }

    public IReadOnlyList<Order> Execute(int customerId)
    {
        var customer = _repository.GetCustomerById(customerId)
            ?? throw new InvalidOperationException($"Покупця з Id={customerId} не знайдено.");

        return customer.Orders.OrderBy(o => o.OrderDate).ToList();
    }
}

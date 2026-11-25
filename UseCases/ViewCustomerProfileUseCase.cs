using StoreSystem.Abstractions;
using StoreSystem.Models;

namespace StoreSystem.UseCases;

public class ViewCustomerProfileUseCase
{
    private readonly IStoreRepository _repository;

    public ViewCustomerProfileUseCase(IStoreRepository repository)
    {
        _repository = repository;
    }

    public Customer Execute(int customerId) =>
        _repository.GetCustomerById(customerId)
            ?? throw new InvalidOperationException($"Покупця з Id={customerId} не знайдено.");
}

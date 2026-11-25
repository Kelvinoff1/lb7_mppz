using StoreSystem.Models;

namespace StoreSystem.UseCases;

public class ApplyLoyaltyDiscountUseCase
{

    public decimal Execute(Order order)
    {
        if (order.Customer?.LoyaltyCard is null)
            return 0m;

        return order.Subtotal - order.Total;
    }
}

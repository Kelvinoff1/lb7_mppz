namespace StoreSystem.Rules;

public static class BusinessRules
{

    public const decimal MaxDiscountPercent = 30m;

    public const int MaxQuantityPerItem = 100;

    public static void ValidateDiscount(decimal percent)
    {
        if (percent < 0 || percent > MaxDiscountPercent)
            throw new InvalidOperationException(
                $"BR-01: знижка має бути від 0 до {MaxDiscountPercent}% (вказано {percent}%).");
    }

    public static void ValidateQuantity(int quantity)
    {
        if (quantity < 1)
            throw new InvalidOperationException("BR-02: кількість товару має бути не менше 1.");
        if (quantity > MaxQuantityPerItem)
            throw new InvalidOperationException(
                $"BR-02: в одній позиції замовлення може бути не більше {MaxQuantityPerItem} одиниць товару.");
    }
}

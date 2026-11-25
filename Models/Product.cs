namespace StoreSystem.Models;

public class Product
{
    public int Id { get; set; }
    public string Title { get; set; } = string.Empty;
    public decimal Price { get; set; }
    public int StockQuantity { get; set; }

    public int CategoryId { get; set; }
    public Category? Category { get; set; }

    public List<OrderItem> OrderItems { get; set; } = new();
    public List<ProductSupplier> ProductSuppliers { get; set; } = new();

    public IEnumerable<Supplier> Suppliers =>
        ProductSuppliers.Where(ps => ps.Supplier != null).Select(ps => ps.Supplier!);

    public bool HasStock(int quantity) => quantity > 0 && StockQuantity >= quantity;

    public void DecreaseStock(int quantity)
    {
        if (!HasStock(quantity))
            throw new InvalidOperationException(
                $"Недостатньо товару \"{Title}\" на складі (потрібно {quantity}, доступно {StockQuantity}).");

        StockQuantity -= quantity;
    }

    public void IncreaseStock(int quantity)
    {
        if (quantity < 1)
            throw new InvalidOperationException("Кількість товару для повернення на склад має бути не менше 1.");

        StockQuantity += quantity;
    }
}

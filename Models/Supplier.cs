namespace StoreSystem.Models;

public class Supplier
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Phone { get; set; } = string.Empty;

    public List<ProductSupplier> ProductSuppliers { get; set; } = new();

    public IEnumerable<Product> Products =>
        ProductSuppliers.Where(ps => ps.Product != null).Select(ps => ps.Product!);
}

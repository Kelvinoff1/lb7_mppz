using System.Text.Encodings.Web;
using System.Text.Json;
using System.Text.Json.Serialization;
using StoreSystem.Abstractions;
using StoreSystem.Data;
using StoreSystem.Models;

namespace StoreSystem.Infrastructure;

public static class JsonStorage
{

    internal record CategoryDto(int Id, string Name);
    internal record ProductDto(int Id, string Title, decimal Price, int StockQuantity, int CategoryId);
    internal record SupplierDto(int Id, string Name, string Phone);
    internal record ProductSupplierDto(int Id, int ProductId, int SupplierId);
    internal record CustomerDto(int Id, string FullName, string Phone);
    internal record LoyaltyCardDto(int Id, string CardNumber, decimal DiscountPercent, int CustomerId);
    internal record EmployeeDto(int Id, string FullName, string Position);
    internal record OrderDto(int Id, DateTime OrderDate, int CustomerId, int EmployeeId, OrderStatus Status = OrderStatus.New);
    internal record OrderItemDto(int Id, int OrderId, int ProductId, int Quantity);
    internal record PaymentDto(int Id, decimal Amount, DateTime PaymentDate, PaymentMethod Method, int OrderId);

    internal record SnapshotDto(
        List<CategoryDto> Categories, List<ProductDto> Products, List<SupplierDto> Suppliers,
        List<ProductSupplierDto> ProductSuppliers, List<CustomerDto> Customers,
        List<LoyaltyCardDto> LoyaltyCards, List<EmployeeDto> Employees,
        List<OrderDto> Orders, List<OrderItemDto> OrderItems, List<PaymentDto> Payments);

    private static readonly JsonSerializerOptions Options = new()
    {
        WriteIndented = true,
        Encoder = JavaScriptEncoder.UnsafeRelaxedJsonEscaping, 
        Converters = { new JsonStringEnumConverter() }
    };

    public static void Save(IStoreRepository repo, string path)
    {
        var snapshot = new SnapshotDto(
            repo.Categories.Select(c => new CategoryDto(c.Id, c.Name)).ToList(),
            repo.Products.Select(p => new ProductDto(p.Id, p.Title, p.Price, p.StockQuantity, p.CategoryId)).ToList(),
            repo.Suppliers.Select(s => new SupplierDto(s.Id, s.Name, s.Phone)).ToList(),
            repo.ProductSuppliers.Select(l => new ProductSupplierDto(l.Id, l.ProductId, l.SupplierId)).ToList(),
            repo.Customers.Select(c => new CustomerDto(c.Id, c.FullName, c.Phone)).ToList(),
            repo.LoyaltyCards.Select(c => new LoyaltyCardDto(c.Id, c.CardNumber, c.DiscountPercent, c.CustomerId)).ToList(),
            repo.Employees.Select(e => new EmployeeDto(e.Id, e.FullName, e.Position)).ToList(),
            repo.Orders.Select(o => new OrderDto(o.Id, o.OrderDate, o.CustomerId, o.EmployeeId, o.Status)).ToList(),
            repo.OrderItems.Select(i => new OrderItemDto(i.Id, i.OrderId, i.ProductId, i.Quantity)).ToList(),
            repo.Payments.Select(p => new PaymentDto(p.Id, p.Amount, p.PaymentDate, p.Method, p.OrderId)).ToList());

        string tempPath = path + ".tmp";
        File.WriteAllText(tempPath, JsonSerializer.Serialize(snapshot, Options));
        File.Move(tempPath, path, overwrite: true);
    }

    public static StoreRepository Load(string path)
    {
        var s = JsonSerializer.Deserialize<SnapshotDto>(File.ReadAllText(path), Options)
            ?? throw new InvalidDataException("Файл даних порожній або пошкоджений.");

        var repo = new StoreRepository();

        foreach (var c in s.Categories) repo.AddCategory(new Category { Id = c.Id, Name = c.Name });
        foreach (var p in s.Products)
            repo.AddProduct(new Product { Id = p.Id, Title = p.Title, Price = p.Price, StockQuantity = p.StockQuantity, CategoryId = p.CategoryId });
        foreach (var x in s.Suppliers) repo.AddSupplier(new Supplier { Id = x.Id, Name = x.Name, Phone = x.Phone });
        foreach (var l in s.ProductSuppliers)
            repo.AddProductSupplier(new ProductSupplier { Id = l.Id, ProductId = l.ProductId, SupplierId = l.SupplierId });
        foreach (var c in s.Customers) repo.AddCustomer(new Customer { Id = c.Id, FullName = c.FullName, Phone = c.Phone });
        foreach (var c in s.LoyaltyCards)
            repo.AddLoyaltyCard(new LoyaltyCard { Id = c.Id, CardNumber = c.CardNumber, DiscountPercent = c.DiscountPercent, CustomerId = c.CustomerId });
        foreach (var e in s.Employees) repo.AddEmployee(new Employee { Id = e.Id, FullName = e.FullName, Position = e.Position });
        foreach (var o in s.Orders)
            repo.AddOrder(new Order { Id = o.Id, OrderDate = o.OrderDate, CustomerId = o.CustomerId, EmployeeId = o.EmployeeId });
        foreach (var i in s.OrderItems)
            repo.AddOrderItem(new OrderItem { Id = i.Id, OrderId = i.OrderId, ProductId = i.ProductId, Quantity = i.Quantity });
        foreach (var p in s.Payments) 
            repo.AddPayment(new Payment { Id = p.Id, Amount = p.Amount, PaymentDate = p.PaymentDate, Method = p.Method, OrderId = p.OrderId });

        foreach (var o in s.Orders.Where(x => x.Status is OrderStatus.Completed or OrderStatus.Cancelled))
        {
            var order = repo.GetOrderById(o.Id)!;
            if (o.Status == OrderStatus.Completed && order.Payment is null)
                throw new InvalidDataException($"Замовлення #{o.Id} має статус «Видане», але оплати немає.");
            order.RestoreStatus(o.Status);
        }

        return repo;
    }
}

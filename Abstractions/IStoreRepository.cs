using StoreSystem.Models;

namespace StoreSystem.Abstractions;

public interface IStoreRepository
{
    IReadOnlyList<Category> Categories { get; }
    IReadOnlyList<Product> Products { get; }
    IReadOnlyList<Supplier> Suppliers { get; }
    IReadOnlyList<ProductSupplier> ProductSuppliers { get; }
    IReadOnlyList<Customer> Customers { get; }
    IReadOnlyList<LoyaltyCard> LoyaltyCards { get; }
    IReadOnlyList<Employee> Employees { get; }
    IReadOnlyList<Order> Orders { get; }
    IReadOnlyList<OrderItem> OrderItems { get; }
    IReadOnlyList<Payment> Payments { get; }

    int NextOrderId();
    int NextOrderItemId();
    int NextPaymentId();

    void AddCategory(Category category);
    void AddProduct(Product product);
    void AddSupplier(Supplier supplier);
    void AddProductSupplier(ProductSupplier link);
    void AddCustomer(Customer customer);
    void AddLoyaltyCard(LoyaltyCard card);
    void AddEmployee(Employee employee);
    void AddOrder(Order order);
    void AddOrderItem(OrderItem item);
    void AddPayment(Payment payment);

    Category? GetCategoryById(int id);
    Product? GetProductById(int id);
    Supplier? GetSupplierById(int id);
    Customer? GetCustomerById(int id);
    Employee? GetEmployeeById(int id);
    Order? GetOrderById(int id);
}

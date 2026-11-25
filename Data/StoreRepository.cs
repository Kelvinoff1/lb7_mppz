using StoreSystem.Abstractions;
using StoreSystem.Models;
using StoreSystem.Rules;

namespace StoreSystem.Data;

public class StoreRepository : IStoreRepository
{
    private readonly List<Category> _categories = new();
    private readonly List<Product> _products = new();
    private readonly List<Supplier> _suppliers = new();
    private readonly List<ProductSupplier> _productSuppliers = new();
    private readonly List<Customer> _customers = new();
    private readonly List<LoyaltyCard> _loyaltyCards = new();
    private readonly List<Employee> _employees = new();
    private readonly List<Order> _orders = new();
    private readonly List<OrderItem> _orderItems = new();
    private readonly List<Payment> _payments = new();

    public IReadOnlyList<Category> Categories => _categories;
    public IReadOnlyList<Product> Products => _products;
    public IReadOnlyList<Supplier> Suppliers => _suppliers;
    public IReadOnlyList<ProductSupplier> ProductSuppliers => _productSuppliers;
    public IReadOnlyList<Customer> Customers => _customers;
    public IReadOnlyList<LoyaltyCard> LoyaltyCards => _loyaltyCards;
    public IReadOnlyList<Employee> Employees => _employees;
    public IReadOnlyList<Order> Orders => _orders;
    public IReadOnlyList<OrderItem> OrderItems => _orderItems;
    public IReadOnlyList<Payment> Payments => _payments;

    public int NextOrderId() => _orders.Count == 0 ? 1 : _orders.Max(o => o.Id) + 1;
    public int NextOrderItemId() => _orderItems.Count == 0 ? 1 : _orderItems.Max(i => i.Id) + 1;
    public int NextPaymentId() => _payments.Count == 0 ? 1 : _payments.Max(p => p.Id) + 1;

    public void AddCategory(Category category) => _categories.Add(category);

    public void AddProduct(Product product)
    {
        var category = GetCategoryById(product.CategoryId)
            ?? throw new InvalidOperationException($"Категорію з Id={product.CategoryId} не знайдено.");

        product.Category = category;
        category.Products.Add(product);
        _products.Add(product);
    }

    public void AddSupplier(Supplier supplier) => _suppliers.Add(supplier);

    public void AddProductSupplier(ProductSupplier link)
    {
        var product = GetProductById(link.ProductId)
            ?? throw new InvalidOperationException($"Товар з Id={link.ProductId} не знайдено.");
        var supplier = GetSupplierById(link.SupplierId)
            ?? throw new InvalidOperationException($"Постачальника з Id={link.SupplierId} не знайдено.");

        link.Product = product;
        link.Supplier = supplier;
        product.ProductSuppliers.Add(link);
        supplier.ProductSuppliers.Add(link);
        _productSuppliers.Add(link);
    }

    public void AddCustomer(Customer customer) => _customers.Add(customer);

    public void AddLoyaltyCard(LoyaltyCard card)
    {
        BusinessRules.ValidateDiscount(card.DiscountPercent); 

        var customer = GetCustomerById(card.CustomerId)
            ?? throw new InvalidOperationException($"Покупця з Id={card.CustomerId} не знайдено.");

        if (customer.LoyaltyCard != null)
            throw new InvalidOperationException($"У покупця {customer.FullName} вже є картка лояльності.");

        card.Customer = customer;
        customer.LoyaltyCard = card;
        _loyaltyCards.Add(card);
    }

    public void AddEmployee(Employee employee) => _employees.Add(employee);

    public void AddOrder(Order order)
    {
        var customer = GetCustomerById(order.CustomerId)
            ?? throw new InvalidOperationException($"Покупця з Id={order.CustomerId} не знайдено.");
        var employee = GetEmployeeById(order.EmployeeId)
            ?? throw new InvalidOperationException($"Співробітника з Id={order.EmployeeId} не знайдено.");

        order.Customer = customer;
        order.Employee = employee;
        customer.Orders.Add(order);
        employee.Orders.Add(order);
        _orders.Add(order);
    }

    public void AddOrderItem(OrderItem item)
    {
        var order = GetOrderById(item.OrderId)
            ?? throw new InvalidOperationException($"Замовлення з Id={item.OrderId} не знайдено.");
        var product = GetProductById(item.ProductId)
            ?? throw new InvalidOperationException($"Товар з Id={item.ProductId} не знайдено.");

        item.Order = order;
        item.Product = product;
        order.Items.Add(item);
        product.OrderItems.Add(item);
        _orderItems.Add(item);
    }

    public void AddPayment(Payment payment)
    {
        var order = GetOrderById(payment.OrderId)
            ?? throw new InvalidOperationException($"Замовлення з Id={payment.OrderId} не знайдено.");

        order.RegisterPayment(payment);
        _payments.Add(payment);
    }

    public Category? GetCategoryById(int id) => _categories.FirstOrDefault(c => c.Id == id);
    public Product? GetProductById(int id) => _products.FirstOrDefault(p => p.Id == id);
    public Supplier? GetSupplierById(int id) => _suppliers.FirstOrDefault(s => s.Id == id);
    public Customer? GetCustomerById(int id) => _customers.FirstOrDefault(c => c.Id == id);
    public Employee? GetEmployeeById(int id) => _employees.FirstOrDefault(e => e.Id == id);
    public Order? GetOrderById(int id) => _orders.FirstOrDefault(o => o.Id == id);

    public static StoreRepository CreateWithSeedData()
    {
        var repo = new StoreRepository();

        repo.AddCategory(new Category { Id = 1, Name = "Кава" });
        repo.AddCategory(new Category { Id = 2, Name = "Чай" });
        repo.AddCategory(new Category { Id = 3, Name = "Ковбаси" });
        repo.AddCategory(new Category { Id = 4, Name = "М'ясо" });

        repo.AddProduct(new Product { Id = 1, Title = "Раф", Price = 20, StockQuantity = 50, CategoryId = 1 });
        repo.AddProduct(new Product { Id = 2, Title = "Чорний", Price = 10, StockQuantity = 40, CategoryId = 2 });
        repo.AddProduct(new Product { Id = 3, Title = "Салямі", Price = 1590, StockQuantity = 15, CategoryId = 3 });
        repo.AddProduct(new Product { Id = 4, Title = "Курка", Price = 13232, StockQuantity = 10, CategoryId = 4 });

        repo.AddSupplier(new Supplier { Id = 1, Name = "ТОВ Колоніал", Phone = "+380441112233" });
        repo.AddSupplier(new Supplier { Id = 2, Name = "М'ясна гільдія", Phone = "+380442223344" });
        repo.AddSupplier(new Supplier { Id = 3, Name = "Смак України", Phone = "+380443334455" });

        repo.AddProductSupplier(new ProductSupplier { Id = 1, ProductId = 1, SupplierId = 1 });
        repo.AddProductSupplier(new ProductSupplier { Id = 2, ProductId = 2, SupplierId = 1 });
        repo.AddProductSupplier(new ProductSupplier { Id = 3, ProductId = 3, SupplierId = 2 });
        repo.AddProductSupplier(new ProductSupplier { Id = 4, ProductId = 4, SupplierId = 2 });
        repo.AddProductSupplier(new ProductSupplier { Id = 5, ProductId = 2, SupplierId = 3 });
        repo.AddProductSupplier(new ProductSupplier { Id = 6, ProductId = 3, SupplierId = 3 });

        repo.AddCustomer(new Customer { Id = 1, FullName = "Іван Петренко", Phone = "+380501112233" });
        repo.AddCustomer(new Customer { Id = 2, FullName = "Олена Коваль", Phone = "+380672223344" });
        repo.AddCustomer(new Customer { Id = 3, FullName = "Андрій Шевченко", Phone = "+380933334455" });

        repo.AddLoyaltyCard(new LoyaltyCard { Id = 1, CardNumber = "CARD-0001", DiscountPercent = 5, CustomerId = 1 });
        repo.AddLoyaltyCard(new LoyaltyCard { Id = 2, CardNumber = "CARD-0002", DiscountPercent = 10, CustomerId = 2 });

        repo.AddEmployee(new Employee { Id = 1, FullName = "Марія Іваненко", Position = "Касир" });
        repo.AddEmployee(new Employee { Id = 2, FullName = "Петро Сидоренко", Position = "Продавець-консультант" });

        repo.AddOrder(new Order { Id = 1, OrderDate = new DateTime(2026, 11, 10), CustomerId = 1, EmployeeId = 1 });
        repo.AddOrder(new Order { Id = 2, OrderDate = new DateTime(2026, 11, 12), CustomerId = 2, EmployeeId = 2 });
        repo.AddOrder(new Order { Id = 3, OrderDate = new DateTime(2026, 11, 14), CustomerId = 1, EmployeeId = 1 });
        repo.AddOrder(new Order { Id = 4, OrderDate = new DateTime(2026, 11, 15), CustomerId = 3, EmployeeId = 2 });

        repo.AddOrderItem(new OrderItem { Id = 1, OrderId = 1, ProductId = 1, Quantity = 2 });
        repo.AddOrderItem(new OrderItem { Id = 2, OrderId = 1, ProductId = 3, Quantity = 1 });
        repo.AddOrderItem(new OrderItem { Id = 3, OrderId = 2, ProductId = 2, Quantity = 3 });
        repo.AddOrderItem(new OrderItem { Id = 4, OrderId = 3, ProductId = 4, Quantity = 1 });
        repo.AddOrderItem(new OrderItem { Id = 5, OrderId = 3, ProductId = 1, Quantity = 1 });
        repo.AddOrderItem(new OrderItem { Id = 6, OrderId = 4, ProductId = 2, Quantity = 2 });

        repo.AddPayment(new Payment { Id = 1, OrderId = 1, Amount = repo.GetOrderById(1)!.Total,
            PaymentDate = new DateTime(2026, 11, 10), Method = PaymentMethod.Card });
        repo.AddPayment(new Payment { Id = 2, OrderId = 2, Amount = repo.GetOrderById(2)!.Total,
            PaymentDate = new DateTime(2026, 11, 12), Method = PaymentMethod.Cash });
        repo.AddPayment(new Payment { Id = 3, OrderId = 3, Amount = repo.GetOrderById(3)!.Total,
            PaymentDate = new DateTime(2026, 11, 14), Method = PaymentMethod.Card });

        return repo;
    }
}

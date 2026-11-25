using StoreSystem.Abstractions;
using StoreSystem.Models;

namespace StoreSystem.Services;

public class StoreInfoService
{
    private readonly IStoreRepository _repository;

    public StoreInfoService(IStoreRepository repository)
    {
        _repository = repository;
    }

    public void PrintCategoriesWithProducts()
    {
        PrintHeader("Категорії та товари (один-до-багатьох)");
        foreach (var category in _repository.Categories)
        {
            Console.WriteLine($"[{category.Id}] {category.Name}");
            foreach (var product in category.Products)
                Console.WriteLine($"    - {product.Title}: {product.Price:N2} грн, на складі: {product.StockQuantity}");
        }
    }

    public void PrintProductsWithSuppliers()
    {
        PrintHeader("Товари та постачальники (багато-до-багатьох)");
        foreach (var product in _repository.Products)
        {
            string suppliers = string.Join(", ", product.Suppliers.Select(s => s.Name));
            Console.WriteLine($"{product.Title} <- {suppliers}");
        }
    }

    public void PrintCustomersWithCards()
    {
        PrintHeader("Покупці та картки лояльності (один-до-одного)");
        foreach (var customer in _repository.Customers)
        {
            string card = customer.LoyaltyCard is null
                ? "картки немає"
                : $"{customer.LoyaltyCard.CardNumber} (знижка {customer.LoyaltyCard.DiscountPercent}%)";
            Console.WriteLine($"[{customer.Id}] {customer.FullName}, {customer.Phone} -> {card}");
        }
    }

    public void PrintEmployeesWithOrders()
    {
        PrintHeader("Співробітники та їхні замовлення (один-до-багатьох)");
        foreach (var employee in _repository.Employees)
        {
            string orders = employee.Orders.Count == 0
                ? "немає замовлень"
                : string.Join(", ", employee.Orders.Select(o => $"#{o.Id}"));
            Console.WriteLine($"[{employee.Id}] {employee.FullName} ({employee.Position}) -> {orders}");
        }
    }

    public void PrintOrders()
    {
        PrintHeader("Замовлення");
        foreach (var order in _repository.Orders)
            PrintOrder(order);
    }

    public void PrintOrder(Order order)
    {
        Console.WriteLine($"Замовлення #{order.Id} від {order.OrderDate:dd.MM.yyyy}, " +
                          $"покупець: {order.Customer?.FullName}, обслужив: {order.Employee?.FullName}");
        foreach (var item in order.Items)
            Console.WriteLine($"    - {item.Product?.Title} x{item.Quantity} = {item.Total:N2} грн");

        Console.WriteLine($"    Сума: {order.Subtotal:N2} грн, знижка: {order.DiscountPercent}%, до сплати: {order.Total:N2} грн");
        Console.WriteLine($"    Статус: {StatusToText(order.Status)}" +
            (order.Payment is null
                ? ", оплати немає"
                : $", оплата: {order.Payment.Amount:N2} грн ({MethodToText(order.Payment.Method)}, {order.Payment.PaymentDate:dd.MM.yyyy})"));
    }

    public void PrintAll()
    {
        PrintCategoriesWithProducts();
        PrintProductsWithSuppliers();
        PrintCustomersWithCards();
        PrintEmployeesWithOrders();
        PrintOrders();
    }

    public static void PrintHeader(string title)
    {
        Console.WriteLine();
        Console.WriteLine($"===== {title} =====");
    }

    private static string StatusToText(OrderStatus status) => status switch
    {
        OrderStatus.New => "нове",
        OrderStatus.Paid => "оплачене",
        OrderStatus.Completed => "видане",
        OrderStatus.Cancelled => "скасоване",
        _ => status.ToString()
    };

    private static string MethodToText(PaymentMethod method) => method switch
    {
        PaymentMethod.Cash => "готівка",
        PaymentMethod.Card => "картка",
        _ => method.ToString()
    };
}

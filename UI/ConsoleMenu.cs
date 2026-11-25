using StoreSystem.Abstractions;
using StoreSystem.Models;
using StoreSystem.Services;
using StoreSystem.UseCases;

namespace StoreSystem.UI;

public class InputClosedException : Exception { }

public class ConsoleMenu
{
    private readonly IStoreRepository _repository;
    private readonly StoreInfoService _info;
    private readonly UseCaseCatalog _useCases;
    private readonly IAuditLog _log;
    private readonly Action _save;

    public ConsoleMenu(IStoreRepository repository, StoreInfoService info, UseCaseCatalog useCases,
        IAuditLog log, Action save)
    {
        _repository = repository;
        _info = info;
        _useCases = useCases;
        _log = log;
        _save = save; 
    }

    public void Run()
    {
        try
        {
            while (true)
            {
                Console.WriteLine();
                Console.WriteLine("=== Система автоматизації магазину ===");
                Console.WriteLine("1 – Покупець");
                Console.WriteLine("2 – Співробітник");
                Console.WriteLine("0 – Вихід");

                switch (ReadInt("Оберіть роль"))
                {
                    case 1: CustomerSession(); break;
                    case 2: EmployeeSession(); break;
                    case 0: return;
                    default: Console.WriteLine("Невірний вибір."); break;
                }
            }
        }
        catch (InputClosedException)
        {
            Console.WriteLine();
            Console.WriteLine("Введення завершено.");
        }
    }

    private void CustomerSession()
    {
        Console.WriteLine("Покупці:");
        foreach (var c in _repository.Customers)
            Console.WriteLine($"  {c.Id}. {c.FullName}");

        var customer = _repository.GetCustomerById(ReadInt("Ваш Id"));
        if (customer is null)
        {
            Console.WriteLine("Покупця не знайдено.");
            return;
        }

        while (true)
        {
            Console.WriteLine();
            Console.WriteLine($"--- Покупець: {customer.FullName} ---");
            Console.WriteLine("1 – Переглянути товари за категорією");
            Console.WriteLine("2 – Переглянути мої замовлення");
            Console.WriteLine("3 – Переглянути мій профіль");
            Console.WriteLine("0 – Назад");

            switch (ReadInt("Ваш вибір"))
            {
                case 1:
                    Safe(() =>
                    {
                        PrintCategories();
                        PrintProducts(_useCases.ViewProductsByCategory.Execute(ReadInt("Id категорії")));
                    });
                    break;
                case 2:
                    Safe(() =>
                    {
                        var orders = _useCases.ViewCustomerOrders.Execute(customer.Id);
                        if (orders.Count == 0) Console.WriteLine("Замовлень немає.");
                        foreach (var order in orders) _info.PrintOrder(order);
                    });
                    break;
                case 3:
                    Safe(() => PrintProfile(_useCases.ViewCustomerProfile.Execute(customer.Id)));
                    break;
                case 0: return;
                default: Console.WriteLine("Невірний вибір."); break;
            }
        }
    }

    private void EmployeeSession()
    {
        Console.WriteLine("Співробітники:");
        foreach (var e in _repository.Employees)
            Console.WriteLine($"  {e.Id}. {e.FullName} ({e.Position})");

        var employee = _repository.GetEmployeeById(ReadInt("Ваш Id"));
        if (employee is null)
        {
            Console.WriteLine("Співробітника не знайдено.");
            return;
        }

        while (true)
        {
            Console.WriteLine();
            Console.WriteLine($"--- Співробітник: {employee.FullName} ---");
            Console.WriteLine("1 – Переглянути постачальників товару");
            Console.WriteLine("2 – Створити замовлення");
            Console.WriteLine("3 – Оплатити замовлення");
            Console.WriteLine("4 – Видати оплачене замовлення");
            Console.WriteLine("5 – Скасувати нове замовлення");
            Console.WriteLine("6 – Оформити покупку (створити замовлення й оплатити)");
            Console.WriteLine("0 – Назад");

            switch (ReadInt("Ваш вибір"))
            {
                case 1: Safe(ShowSuppliers); break;
                case 2: Safe(() => CreateOrder(employee)); break;
                case 3: Safe(PayOrder); break;
                case 4: Safe(CompleteOrder); break;
                case 5: Safe(CancelOrder); break;
                case 6: Safe(() => Checkout(employee)); break;
                case 0: return;
                default: Console.WriteLine("Невірний вибір."); break;
            }
        }
    }

    private void ShowSuppliers()
    {
        PrintAllProducts();
        var suppliers = _useCases.ViewProductSuppliers.Execute(ReadInt("Id товару"));
        foreach (var s in suppliers)
            Console.WriteLine($"  - {s.Name}, {s.Phone}");
    }

    private void CreateOrder(Employee employee)
    {
        Console.WriteLine("Покупці:");
        foreach (var c in _repository.Customers)
            Console.WriteLine($"  {c.Id}. {c.FullName}");
        int customerId = ReadInt("Id покупця");

        PrintAllProducts();
        var items = new List<(int ProductId, int Quantity)>();
        while (true)
        {
            int productId = ReadInt("Id товару (0 – завершити)");
            if (productId == 0) break;
            int quantity = ReadInt("Кількість");
            items.Add((productId, quantity));
        }

        var order = _useCases.CreateOrder.Execute(customerId, employee.Id, DateTime.Today, items);
        Console.WriteLine("Замовлення створено:");
        _info.PrintOrder(order);
        _save();
    }

    private void PayOrder()
    {
        var unpaid = _repository.Orders.Where(o => o.Status == OrderStatus.New).ToList();
        if (unpaid.Count == 0)
        {
            Console.WriteLine("Неоплачених замовлень немає.");
            return;
        }

        Console.WriteLine("Неоплачені замовлення:");
        foreach (var o in unpaid) _info.PrintOrder(o);

        int orderId = ReadInt("Id замовлення");
        int methodChoice = ReadInt("Спосіб оплати (1 – готівка, 2 – картка)");
        var method = methodChoice == 1 ? PaymentMethod.Cash : PaymentMethod.Card;

        var receipt = _useCases.PayOrder.Execute(orderId, method, DateTime.Today);
        PrintReceipt(receipt);
        _save();
    }

    private void Checkout(Employee employee)
    {
        Console.WriteLine("Покупці:");
        foreach (var c in _repository.Customers)
            Console.WriteLine($"  {c.Id}. {c.FullName}");
        int customerId = ReadInt("Id покупця");

        PrintAllProducts();
        var items = new List<(int ProductId, int Quantity)>();
        while (true)
        {
            int productId = ReadInt("Id товару (0 – завершити)");
            if (productId == 0) break;
            items.Add((productId, ReadInt("Кількість")));
        }

        int methodChoice = ReadInt("Спосіб оплати (1 – готівка, 2 – картка)");
        var method = methodChoice == 1 ? PaymentMethod.Cash : PaymentMethod.Card;

        try
        {
            var result = _useCases.CheckoutOrder.Execute(customerId, employee.Id, DateTime.Today, items, method);
            _info.PrintOrder(result.Order);
            PrintReceipt(result.Receipt);
        }
        finally
        {
            _save(); 
        }
    }

    private void CompleteOrder()
    {
        var paid = _repository.Orders.Where(o => o.Status == OrderStatus.Paid).ToList();
        if (paid.Count == 0)
        {
            Console.WriteLine("Оплачених замовлень, готових до видачі, немає.");
            return;
        }

        Console.WriteLine("Оплачені замовлення:");
        foreach (var o in paid) _info.PrintOrder(o);

        var order = _useCases.CompleteOrder.Execute(ReadInt("Id замовлення"));
        Console.WriteLine($"Замовлення #{order.Id} видано покупцю.");
        _save();
    }

    private void CancelOrder()
    {
        var active = _repository.Orders.Where(o => o.Status == OrderStatus.New).ToList();
        if (active.Count == 0)
        {
            Console.WriteLine("Нових замовлень, які можна скасувати, немає.");
            return;
        }

        Console.WriteLine("Нові замовлення:");
        foreach (var o in active) _info.PrintOrder(o);

        var order = _useCases.CancelOrder.Execute(ReadInt("Id замовлення"));
        Console.WriteLine($"Замовлення #{order.Id} скасовано, товари повернено на склад.");
        _save();
    }

    public static void PrintReceipt(PaymentReceipt receipt)
    {
        Console.WriteLine($"Оплата замовлення #{receipt.Payment.OrderId} виконана.");
        Console.WriteLine($"    Сума: {receipt.Subtotal:N2} грн");
        Console.WriteLine($"    Знижка: {receipt.Discount:N2} грн");
        Console.WriteLine($"    Сплачено: {receipt.Payment.Amount:N2} грн");
    }

    public static void PrintProducts(IEnumerable<Product> products)
    {
        foreach (var p in products)
            Console.WriteLine($"  - {p.Title}: {p.Price:N2} грн, на складі: {p.StockQuantity}");
    }

    public static void PrintProfile(Customer customer)
    {
        Console.WriteLine($"Покупець: {customer.FullName}, телефон: {customer.Phone}");
        Console.WriteLine(customer.LoyaltyCard is null
            ? "Картка лояльності: немає"
            : $"Картка лояльності: {customer.LoyaltyCard.CardNumber}, знижка {customer.LoyaltyCard.DiscountPercent}%");
        Console.WriteLine($"Кількість замовлень: {customer.Orders.Count}");
    }

    private void PrintCategories()
    {
        Console.WriteLine("Категорії:");
        foreach (var c in _repository.Categories)
            Console.WriteLine($"  {c.Id}. {c.Name}");
    }

    private void PrintAllProducts()
    {
        Console.WriteLine("Товари:");
        foreach (var p in _repository.Products)
            Console.WriteLine($"  {p.Id}. {p.Title} ({p.Price:N2} грн, на складі: {p.StockQuantity})");
    }

    private void Safe(Action action)
    {
        try
        {
            action();
        }
        catch (InvalidOperationException ex)
        {
            Console.WriteLine($"Помилка: {ex.Message}");
            _log.Error(ex.Message);
        }
    }

    private static int ReadInt(string prompt)
    {
        while (true)
        {
            Console.Write($"{prompt}: ");
            var line = Console.ReadLine();
            if (line is null) throw new InputClosedException();
            if (int.TryParse(line.Trim(), out int value)) return value;
            Console.WriteLine("Введіть ціле число.");
        }
    }
}

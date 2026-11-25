using StoreSystem.Abstractions;
using StoreSystem.Models;
using StoreSystem.Rules;
using StoreSystem.Services;
using StoreSystem.UseCases;

namespace StoreSystem.UI;

public static class DemoScenario
{
    public static void Run(IStoreRepository repository, StoreInfoService info, UseCaseCatalog uc, IAuditLog log)
    {
        StoreInfoService.PrintHeader("UC-01. Переглянути товари за категорією (категорія: Кава)");
        ConsoleMenu.PrintProducts(uc.ViewProductsByCategory.Execute(1));

        StoreInfoService.PrintHeader("UC-03. Переглянути профіль покупця (Id = 1)");
        ConsoleMenu.PrintProfile(uc.ViewCustomerProfile.Execute(1));

        StoreInfoService.PrintHeader("UC-05, UC-06, UC-09. Створити замовлення (Раф x3, Салямі x1)");
        var newOrder = uc.CreateOrder.Execute(customerId: 2, employeeId: 1, date: new DateTime(2026, 11, 16),
            items: new List<(int, int)> { (1, 3), (3, 1) });
        info.PrintOrder(newOrder);
        Console.WriteLine($"Залишок товару \"Раф\" на складі: {repository.GetProductById(1)!.StockQuantity}");

        StoreInfoService.PrintHeader("BR-03, UC-09. Спроба замовити 100 курок (на складі 10)");
        TryRun(log, () => uc.CheckProductStock.Execute(productId: 4, quantity: 100));

        StoreInfoService.PrintHeader($"BR-02. Спроба додати 101 одиницю товару (ліміт {BusinessRules.MaxQuantityPerItem})");
        TryRun(log, () => uc.AddProductToOrder.Execute(orderId: 4, productId: 2, quantity: 101));

        StoreInfoService.PrintHeader($"BR-01. Спроба видати картку зі знижкою 50% (ліміт {BusinessRules.MaxDiscountPercent}%)");
        TryRun(log, () => repository.AddLoyaltyCard(new LoyaltyCard
            { Id = 3, CardNumber = "CARD-0003", DiscountPercent = 50, CustomerId = 3 }));

        StoreInfoService.PrintHeader("UC-07, UC-08. Оплатити замовлення (знижка за карткою лояльності)");
        var receipt = uc.PayOrder.Execute(newOrder.Id, PaymentMethod.Card, new DateTime(2026, 11, 16));
        ConsoleMenu.PrintReceipt(receipt);

        StoreInfoService.PrintHeader("BR-04. Спроба змінити оплачене замовлення");
        TryRun(log, () => uc.AddProductToOrder.Execute(newOrder.Id, productId: 2, quantity: 1));

        StoreInfoService.PrintHeader("UC-10. Видати оплачене замовлення (стан: Оплачене → Видане)");
        info.PrintOrder(uc.CompleteOrder.Execute(newOrder.Id));

        StoreInfoService.PrintHeader("Стани. Спроба скасувати або повторно видати видане замовлення");
        TryRun(log, () => uc.CancelOrder.Execute(newOrder.Id));
        TryRun(log, () => uc.CompleteOrder.Execute(newOrder.Id));

        StoreInfoService.PrintHeader("Стани. Спроба видати неоплачене замовлення #4 (стан: Нове)");
        TryRun(log, () => uc.CompleteOrder.Execute(orderId: 4));

        StoreInfoService.PrintHeader("UC-11. Скасувати нове замовлення (стан: Нове → Скасоване, товари повертаються на склад)");
        var temp = uc.CreateOrder.Execute(customerId: 3, employeeId: 2, date: new DateTime(2026, 11, 17),
            items: new List<(int, int)> { (1, 5) });
        Console.WriteLine($"Залишок товару \"Раф\" після створення замовлення #{temp.Id}: {repository.GetProductById(1)!.StockQuantity}");
        info.PrintOrder(uc.CancelOrder.Execute(temp.Id));
        Console.WriteLine($"Залишок товару \"Раф\" після скасування: {repository.GetProductById(1)!.StockQuantity}");

        StoreInfoService.PrintHeader("Стани. Спроба оплатити або змінити скасоване замовлення");
        TryRun(log, () => uc.PayOrder.Execute(temp.Id, PaymentMethod.Cash, new DateTime(2026, 11, 17)));
        TryRun(log, () => uc.AddProductToOrder.Execute(temp.Id, productId: 2, quantity: 1));

        StoreInfoService.PrintHeader("UC-12. Оформити покупку (Чорний x4, готівка): створення замовлення й оплата");
        var checkout = uc.CheckoutOrder.Execute(customerId: 1, employeeId: 2, date: new DateTime(2026, 11, 18),
            items: new List<(int, int)> { (2, 4) }, method: PaymentMethod.Cash);
        info.PrintOrder(checkout.Order);
        ConsoleMenu.PrintReceipt(checkout.Receipt);

        StoreInfoService.PrintHeader("UC-12. Оплата не виконана (невідомий спосіб оплати): замовлення скасовується");
        Console.WriteLine($"Залишок товару \"Раф\" до спроби: {repository.GetProductById(1)!.StockQuantity}");
        TryRun(log, () => uc.CheckoutOrder.Execute(customerId: 1, employeeId: 2, date: new DateTime(2026, 11, 18),
            items: new List<(int, int)> { (1, 2) }, method: (PaymentMethod)99));
        Console.WriteLine($"Залишок товару \"Раф\" після спроби: {repository.GetProductById(1)!.StockQuantity}");
        Console.WriteLine($"Статус останнього замовлення: {repository.Orders[^1].Status}");

        StoreInfoService.PrintHeader("UC-12. Товару недостатньо (Курка x100): замовлення не створюється");
        int ordersBefore = repository.Orders.Count;
        TryRun(log, () => uc.CheckoutOrder.Execute(customerId: 1, employeeId: 2, date: new DateTime(2026, 11, 18),
            items: new List<(int, int)> { (4, 100) }, method: PaymentMethod.Cash));
        Console.WriteLine($"Кількість замовлень до і після: {ordersBefore} і {repository.Orders.Count}");

        StoreInfoService.PrintHeader($"S-02. Журнал аудиту ({Path.GetFileName(log.Location)})");
        foreach (var line in log.ReadAll())
            Console.WriteLine(line);
    }

    private static void TryRun(IAuditLog log, Action action)
    {
        try
        {
            action();
        }
        catch (InvalidOperationException ex)
        {
            Console.WriteLine($"Помилка: {ex.Message}");
            log.Error(ex.Message);
        }
    }
}

namespace StoreSystem.Models;

public class Employee
{
    public int Id { get; set; }
    public string FullName { get; set; } = string.Empty;
    public string Position { get; set; } = string.Empty;

    public List<Order> Orders { get; set; } = new();
}

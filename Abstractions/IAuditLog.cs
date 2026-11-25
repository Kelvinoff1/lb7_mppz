namespace StoreSystem.Abstractions;

public interface IAuditLog
{
    string Location { get; }
    void Info(string message);
    void Error(string message);
    IReadOnlyList<string> ReadAll();
}

using System.Globalization;
using StoreSystem.Abstractions;

namespace StoreSystem.Infrastructure;

public class FileAuditLog : IAuditLog
{
    private readonly string _path;
    private readonly object _lock = new();

    public FileAuditLog(string path)
    {
        _path = Path.GetFullPath(path);
        var dir = Path.GetDirectoryName(_path);
        if (!string.IsNullOrEmpty(dir)) Directory.CreateDirectory(dir);
    }

    public string Location => _path;

    public void Info(string message) => Write("INFO", message);
    public void Error(string message) => Write("ERROR", message);

    public IReadOnlyList<string> ReadAll() =>
        File.Exists(_path) ? File.ReadAllLines(_path) : Array.Empty<string>();

    private void Write(string level, string message)
    {
        string time = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss", CultureInfo.InvariantCulture);
        string line = $"{time} | {level,-5} | {message}";
        lock (_lock)
        {

            try { File.AppendAllText(_path, line + Environment.NewLine); }
            catch (IOException) { }
            catch (UnauthorizedAccessException) { }
        }
    }
}

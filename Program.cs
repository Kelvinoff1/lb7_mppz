using System.Globalization;
using System.Text;
using System.Text.Json;
using StoreSystem.Data;
using StoreSystem.Infrastructure;
using StoreSystem.Services;
using StoreSystem.UI;
using StoreSystem.UseCases;

Console.OutputEncoding = Encoding.UTF8;
Console.InputEncoding = Encoding.UTF8;

try
{
    var culture = new CultureInfo("uk-UA");
    CultureInfo.CurrentCulture = culture;
    CultureInfo.DefaultThreadCurrentCulture = culture;
}
catch (CultureNotFoundException)
{

}

bool demo = args.Contains("demo");

string logPath = demo ? "audit-demo.log" : "audit.log";
if (demo && File.Exists(logPath)) File.Delete(logPath);
var log = new FileAuditLog(logPath);

if (demo)
{
    var repository = StoreRepository.CreateWithSeedData();
    var info = new StoreInfoService(repository);
    DemoScenario.Run(repository, info, new UseCaseCatalog(repository, log), log);
    return;
}

const string dataPath = "store-data.json";
StoreRepository repo;
if (File.Exists(dataPath))
{
    try
    {
        repo = JsonStorage.Load(dataPath);
        log.Info($"Дані завантажено з файлу {dataPath}");
    }
    catch (Exception ex) when (ex is JsonException or IOException or InvalidDataException or InvalidOperationException)
    {

        log.Error($"Не вдалося завантажити {dataPath}: {ex.Message}");
        File.Copy(dataPath, dataPath + ".bak", overwrite: true);
        Console.WriteLine($"Файл даних пошкоджений (копію збережено як {dataPath}.bak). Використано початкові дані.");
        repo = StoreRepository.CreateWithSeedData();
    }
}
else
{
    repo = StoreRepository.CreateWithSeedData();
    JsonStorage.Save(repo, dataPath);
    log.Info($"Створено початкові дані та файл {dataPath}");
}

void Save()
{
    JsonStorage.Save(repo, dataPath);
    log.Info($"Дані збережено у файл {dataPath}");
}

var infoService = new StoreInfoService(repo);
new ConsoleMenu(repo, infoService, new UseCaseCatalog(repo, log), log, Save).Run();

using Game.ConsoleClient.Security;

Console.OutputEncoding = System.Text.Encoding.UTF8;
Console.WriteLine("=================================================");
Console.WriteLine("  Tactics Game Engine — Захищений консольний клієнт");
Console.WriteLine("  (Лабораторна робота №9: Протидія реверс-інжинірингу)");
Console.WriteLine("=================================================");

// 1. Перевірка наявності відладчика (Anti-Debugging)
Console.Write("[1] Перевірка середовища на наявність налагоджувача... ");
if (AntiDebugGuard.DetectDebugger())
{
    Console.ForegroundColor = ConsoleColor.Red;
    Console.WriteLine("ВИЯВЛЕНО ВІДЛАДЧИК!");
    Console.ResetColor();
    Console.WriteLine("Помилка безпеки: виконання заблоковано через виявлення інструментів динамічного аналізу.");
    return;
}
Console.ForegroundColor = ConsoleColor.Green;
Console.WriteLine("OK (Безпечно)");
Console.ResetColor();

// 2. Динамічне розкриття прихованих рядків (String Obfuscation)
Console.Write("[2] Динамічне формування ендпоінтів сервера... ");
var apiUrl = ObfuscatedStrings.GetApiUrl();
var model = ObfuscatedStrings.GetModelName();
var feature = ObfuscatedStrings.GetRequiredFeature();
Console.ForegroundColor = ConsoleColor.Green;
Console.WriteLine("OK");
Console.ResetColor();
Console.WriteLine($"    -> API URL: {apiUrl}");
Console.WriteLine($"    -> Очікувана модель ліцензії: {model}");
Console.WriteLine($"    -> Обов'язковий ігровий модуль: {feature}");

// 3. Завантаження ліцензії та перевірка через обфускований автомат станів
Console.Write("[3] Завантаження та валідація ліцензії (Control Flow Flattening)... ");
var baseDir = AppContext.BaseDirectory;
var candidatePaths = new[]
{
    Path.Combine(baseDir, "license.json"),
    Path.Combine(baseDir, "..", "..", "..", "..", "..", "license", "license.json"),
    Path.Combine(Directory.GetCurrentDirectory(), "license", "license.json"),
    Path.Combine(Directory.GetCurrentDirectory(), "..", "license", "license.json")
};

string? licensePath = null;
string? sigPath = null;

foreach (var cp in candidatePaths)
{
    var sp = Path.ChangeExtension(cp, ".sig");
    if (File.Exists(cp) && File.Exists(sp))
    {
        licensePath = Path.GetFullPath(cp);
        sigPath = Path.GetFullPath(sp);
        break;
    }
}

if (licensePath != null && sigPath != null)
{
    var licenseJson = File.ReadAllText(licensePath);
    var signature = File.ReadAllBytes(sigPath);

    var isValid = ObfuscatedLicenseValidator.ValidateClientLicense(licenseJson, signature);

    if (isValid)
    {
        Console.ForegroundColor = ConsoleColor.Green;
        Console.WriteLine("УСПІХ (Дійсна)");
        Console.ResetColor();
        Console.WriteLine("    -> Цифровий підпис RSA-PSS SHA-256 підтверджено.");
        Console.WriteLine("    -> Усі обов'язкові компоненти ліцензії Freeware активовані.");
        Console.WriteLine("\nІгровий сеанс успішно ініціалізовано. Бойовий рушій 5х5 готовий до роботи!");
    }
    else
    {
        Console.ForegroundColor = ConsoleColor.Red;
        Console.WriteLine("ВІДМОВА!");
        Console.ResetColor();
        Console.WriteLine("Помилка: ліцензія була модифікована або пошкоджена. Запуск скасовано.");
    }
}
else
{
    Console.ForegroundColor = ConsoleColor.Yellow;
    Console.WriteLine("ПОПЕРЕДЖЕННЯ: Файли ліцензії не знайдено.");
    Console.ResetColor();
}

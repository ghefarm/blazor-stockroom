using System.Management.Automation.Language;

// Parses PowerShell scripts without running them and prints every
// syntax error. Used to check the scripts in deploy/.
var failed = 0;
foreach (var path in args)
{
    Parser.ParseFile(Path.GetFullPath(path), out _, out var errors);
    foreach (var error in errors)
    {
        var line = error.Extent.StartLineNumber;
        Console.WriteLine($"{path}({line}): {error.Message}");
        failed++;
    }

    if (errors.Length == 0)
    {
        Console.WriteLine($"{path}: OK");
    }
}

return failed == 0 ? 0 : 1;

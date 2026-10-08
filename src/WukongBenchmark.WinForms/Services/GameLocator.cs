using Microsoft.Win32;

namespace WukongBenchmark.WinForms.Services;

public sealed class GameLocator : IGameLocator
{
    private const string UninstallRegistryPath =
        @"HKEY_LOCAL_MACHINE\SOFTWARE\Microsoft\Windows\CurrentVersion\Uninstall\Steam App 3132990";

    public string? TryFindGameFolder()
    {
        var registryPath = Registry.GetValue(UninstallRegistryPath, "InstallLocation", null) as string;
        if (!string.IsNullOrWhiteSpace(registryPath) && Directory.Exists(registryPath))
        {
            return Path.GetFullPath(registryPath).TrimEnd(Path.DirectorySeparatorChar);
        }

        foreach (var view in new[] { RegistryView.Registry64, RegistryView.Registry32 })
        {
            try
            {
                using var baseKey = RegistryKey.OpenBaseKey(RegistryHive.LocalMachine, view);
                using var key = baseKey.OpenSubKey(
                    @"SOFTWARE\Microsoft\Windows\CurrentVersion\Uninstall\Steam App 3132990");
                var value = key?.GetValue("InstallLocation") as string;
                if (!string.IsNullOrWhiteSpace(value) && Directory.Exists(value))
                {
                    return Path.GetFullPath(value).TrimEnd(Path.DirectorySeparatorChar);
                }
            }
            catch (Exception) when (view == RegistryView.Registry32 || view == RegistryView.Registry64)
            {
                // Не все среды Windows позволяют открыть обе представления реестра.
            }
        }

        return null;
    }

    public string FindAndValidateGameFolder(string? providedPath)
    {
        var gameFolder = string.IsNullOrWhiteSpace(providedPath)
            ? TryFindGameFolder()
            : Path.GetFullPath(providedPath.Trim()).TrimEnd(Path.DirectorySeparatorChar);

        if (string.IsNullOrWhiteSpace(gameFolder))
        {
            throw new InvalidOperationException(
                "Не нашли Benchmark Tool. Укажите папку установки вручную.");
        }

        if (!Directory.Exists(gameFolder))
        {
            throw new DirectoryNotFoundException($"Папка не существует: {gameFolder}");
        }

        if (!File.Exists(Path.Combine(gameFolder, "b1_benchmark.exe")))
        {
            throw new InvalidOperationException("Проверьте папку Benchmark Tool: b1_benchmark.exe не найден.");
        }

        return gameFolder;
    }

    public string FindSteamFolder()
    {
        var steamFolder = Registry.GetValue(
            @"HKEY_CURRENT_USER\Software\Valve\Steam",
            "SteamPath",
            null) as string;

        if (string.IsNullOrWhiteSpace(steamFolder))
        {
            steamFolder = Path.Combine(
                Environment.GetFolderPath(Environment.SpecialFolder.ProgramFilesX86),
                "Steam");
        }

        var steamExe = Path.Combine(steamFolder, "steam.exe");
        if (!File.Exists(steamExe))
        {
            throw new FileNotFoundException("Не найден steam.exe.", steamExe);
        }

        return Path.GetFullPath(steamFolder).TrimEnd(Path.DirectorySeparatorChar);
    }
}

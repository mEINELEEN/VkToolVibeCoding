using System.Diagnostics;
using System.Text.Json;
using WukongBenchmark.WinForms.Models;

namespace WukongBenchmark.WinForms.Services;

public sealed class SteamBenchmarkRunner : IBenchmarkRunner
{
    private static readonly TimeSpan ResultTimeout = TimeSpan.FromMinutes(5);
    private const int AppId = 3132990;
    private static readonly string ResultFolder =
        Path.Combine(Path.GetTempPath(), "b1", "BenchMarkHistory", "Tool");

    public IReadOnlyList<Process> GetRunningBenchmarkProcesses()
    {
        var processes = new List<Process>();
        var names = new[] { "b1_benchmark", "b1-Win64-Shipping" };

        foreach (var name in names)
        {
            processes.AddRange(Process.GetProcessesByName(name));
        }

        return processes;
    }

    public JsonElement Run(string steamFolder, BenchmarkProfile profile, CancellationToken cancellationToken)
    {
        var oldFiles = Directory.Exists(ResultFolder)
            ? new HashSet<string>(Directory.GetFiles(ResultFolder), StringComparer.OrdinalIgnoreCase)
            : new HashSet<string>(StringComparer.OrdinalIgnoreCase);

        var startedAt = DateTime.Now;
        Process? launch = null;

        try
        {
            var launchInfo = new ProcessStartInfo(Path.Combine(steamFolder, "steam.exe"))
            {
                UseShellExecute = false,
                WorkingDirectory = steamFolder
            };

            launchInfo.ArgumentList.Add("-applaunch");
            launchInfo.ArgumentList.Add(AppId.ToString());
            launchInfo.ArgumentList.Add("-benchmark");
            launchInfo.ArgumentList.Add("-windowed");
            launchInfo.ArgumentList.Add("-forceres");
            launchInfo.ArgumentList.Add($"-ResX={profile.Width}");
            launchInfo.ArgumentList.Add($"-ResY={profile.Height}");

            launch = Process.Start(launchInfo)
                ?? throw new InvalidOperationException("Не удалось запустить Steam.");

            while (DateTime.Now - startedAt < ResultTimeout)
            {
                cancellationToken.ThrowIfCancellationRequested();

                if (Directory.Exists(ResultFolder))
                {
                    foreach (var file in Directory.GetFiles(ResultFolder))
                    {
                        if (oldFiles.Contains(file))
                        {
                            continue;
                        }

                        try
                        {
                            using var json = JsonDocument.Parse(File.ReadAllText(file));
                            var result = json.RootElement.Clone();
                            CheckSettings(profile, result);
                            return result;
                        }
                        catch (JsonException)
                        {
                            // Игра может ещё дописывать JSON.
                        }
                        catch (IOException)
                        {
                            // Файл временно занят игрой.
                        }
                    }
                }

                Thread.Sleep(1000);
            }

            throw new TimeoutException($"{profile.Name}: за 5 минут результат не появился.");
        }
        finally
        {
            KillStartedBenchmarkProcesses(startedAt);

            if (!cancellationToken.IsCancellationRequested)
            {
                Thread.Sleep(5000);
            }

            launch?.Dispose();
        }
    }

    private void KillStartedBenchmarkProcesses(DateTime startedAt)
    {
        foreach (var process in GetRunningBenchmarkProcesses())
        {
            using (process)
            {
                try
                {
                    if (!process.HasExited && process.StartTime >= startedAt)
                    {
                        process.Kill(entireProcessTree: true);
                        process.WaitForExit(10_000);
                    }
                }
                catch (InvalidOperationException)
                {
                    // Процесс успел завершиться между проверками.
                }
                catch (System.ComponentModel.Win32Exception)
                {
                    // Процесс мог завершиться или стать недоступным для завершения.
                }
            }
        }
    }

    private static void CheckSettings(BenchmarkProfile profile, JsonElement result)
    {
        var resolution = $"{profile.Width} × {profile.Height}";
        if (result.GetProperty("ScreenResolution").GetString() != resolution)
        {
            throw new InvalidOperationException(
                $"{profile.Name}: игра применила другое разрешение.");
        }

        var expected = new Dictionary<string, int>
        {
            ["ImageQuality"] = profile.RenderPercent,
            ["ViewDistance"] = 5,
            ["Rtx"] = profile.RayTracing,
            ["Dlss"] = 3,
            ["InsertFrame"] = 0,
            ["MotionBlur"] = profile.MotionBlur
        };

        var qualityFields = new[]
        {
            "AntiAliasing", "PostProcessing", "ShadowQuality",
            "TextureQuality", "MaterialQuality", "VegetationQuality"
        };

        foreach (var field in qualityFields)
        {
            expected[field] = profile.Quality;
        }

        foreach (var setting in expected)
        {
            var actual = result.GetProperty(setting.Key).GetInt32();

            if (profile.Name == "GPU" && setting.Key == "Rtx" && actual == 0)
            {
                continue;
            }

            if (actual != setting.Value)
            {
                throw new InvalidOperationException(
                    $"{profile.Name}: игра применила другие настройки ({setting.Key}).");
            }
        }
    }
}

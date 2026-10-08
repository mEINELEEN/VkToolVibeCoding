using System.Text;
using WukongBenchmark.WinForms.Models;

namespace WukongBenchmark.WinForms.Services;

public sealed class BenchmarkService : IBenchmarkService
{
    private readonly IGameLocator _gameLocator;
    private readonly IGameConfigService _configService;
    private readonly IBenchmarkRunner _runner;
    private readonly IWindowsDisplayService _displayService;
    private readonly IBenchmarkReportFormatter _formatter;

    public BenchmarkService(
        IGameLocator gameLocator,
        IGameConfigService configService,
        IBenchmarkRunner runner,
        IWindowsDisplayService displayService,
        IBenchmarkReportFormatter formatter)
    {
        _gameLocator = gameLocator;
        _configService = configService;
        _runner = runner;
        _displayService = displayService;
        _formatter = formatter;
    }

    public BenchmarkReport Run(
        string? providedGameFolder,
        IProgress<string> log,
        IProgress<int> progress,
        CancellationToken cancellationToken)
    {
        var gameFolder = _gameLocator.FindAndValidateGameFolder(providedGameFolder);
        log.Report($"Benchmark Tool: {gameFolder}");

        var steamFolder = _gameLocator.FindSteamFolder();
        log.Report($"Steam: {steamFolder}");

        var running = _runner.GetRunningBenchmarkProcesses();
        var runningCount = running.Count;
        foreach (var process in running)
        {
            process.Dispose();
        }

        if (runningCount > 0)
        {
            throw new InvalidOperationException("Закройте Benchmark Tool перед запуском.");
        }

        var iniFolder = Path.Combine(gameFolder, "b1", "Saved", "Config", "Windows");
        var iniPath = Path.Combine(iniFolder, "GameUserSettings.ini");
        var hadConfig = File.Exists(iniPath);
        var originalBytes = hadConfig ? File.ReadAllBytes(iniPath) : Array.Empty<byte>();
        var report = new StringBuilder();
        var reportPath = Path.GetFullPath("report.txt");

        var screen = _displayService.GetPrimaryScreenSize();
        var profiles = new[]
        {
            new BenchmarkProfile
            {
                Name = "CPU",
                Width = 1280,
                Height = 720,
                RenderPercent = 25,
                Quality = 1,
                RayTracing = 0,
                MotionBlur = 0
            },
            new BenchmarkProfile
            {
                Name = "GPU",
                Width = screen.Width,
                Height = screen.Height,
                RenderPercent = 100,
                Quality = 5,
                RayTracing = 3,
                MotionBlur = 2
            }
        };

        try
        {
            Directory.CreateDirectory(iniFolder);

            for (var i = 0; i < profiles.Length; i++)
            {
                cancellationToken.ThrowIfCancellationRequested();

                var profile = profiles[i];
                var startProgress = i * 50;
                var endProgress = (i + 1) * 50;

                File.WriteAllText(iniPath, _configService.Create(profile));
                log.Report($"Запуск {profile.Name}-теста...");
                progress.Report(startProgress + 5);

                var result = _runner.Run(steamFolder, profile, cancellationToken);
                progress.Report(startProgress + 30);

                if (profile.Name == "CPU")
                {
                    var hardware = _formatter.FormatHardware(result);
                    report.AppendLine(hardware);
                    log.Report(hardware.TrimEnd());
                }

                var text = _formatter.FormatResult(profile, result);
                report.AppendLine(text);
                log.Report(text.TrimEnd());

                File.WriteAllText(reportPath, report.ToString());
                progress.Report(endProgress);
            }

            return new BenchmarkReport
            {
                Text = report.ToString(),
                FilePath = reportPath
            };
        }
        finally
        {
            if (hadConfig)
            {
                File.WriteAllBytes(iniPath, originalBytes);
            }
            else if (File.Exists(iniPath))
            {
                File.Delete(iniPath);
            }

            log.Report("Исходный GameUserSettings.ini восстановлен.");
        }
    }
}

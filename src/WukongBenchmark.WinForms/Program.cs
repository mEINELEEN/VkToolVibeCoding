using WukongBenchmark.WinForms.Presenters;
using WukongBenchmark.WinForms.Services;
using WukongBenchmark.WinForms.Views;

namespace WukongBenchmark.WinForms;

internal static class Program
{
    [STAThread]
    private static void Main(string[] args)
    {
        ApplicationConfiguration.Initialize();

        if (args.Length > 1)
        {
            MessageBox.Show(
                "Запуск: WukongBenchmark.WinForms.exe [\"папка Black Myth Wukong Benchmark Tool\"]",
                "Wukong Benchmark",
                MessageBoxButtons.OK,
                MessageBoxIcon.Information);
            return;
        }

        string? initialFolder = args.Length == 1
            ? Path.GetFullPath(args[0]).TrimEnd(Path.DirectorySeparatorChar)
            : null;

        var view = new MainForm(initialFolder);
        var gameLocator = new GameLocator();
        var configService = new GameConfigService();
        var runner = new SteamBenchmarkRunner();
        var displayService = new WindowsDisplayService();
        var reportFormatter = new BenchmarkReportFormatter();
        var benchmarkService = new BenchmarkService(
            gameLocator,
            configService,
            runner,
            displayService,
            reportFormatter);

        _ = new MainFormPresenter(view, benchmarkService, gameLocator);

        Application.Run(view);
    }
}

using WukongBenchmark.WinForms.Models;

namespace WukongBenchmark.WinForms.Services;

public interface IBenchmarkService
{
    BenchmarkReport Run(
        string? providedGameFolder,
        IProgress<string> log,
        IProgress<int> progress,
        CancellationToken cancellationToken);
}

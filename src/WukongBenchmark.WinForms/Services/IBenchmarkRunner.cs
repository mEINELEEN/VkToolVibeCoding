using System.Diagnostics;
using System.Text.Json;
using WukongBenchmark.WinForms.Models;

namespace WukongBenchmark.WinForms.Services;

public interface IBenchmarkRunner
{
    IReadOnlyList<Process> GetRunningBenchmarkProcesses();
    JsonElement Run(string steamFolder, BenchmarkProfile profile, CancellationToken cancellationToken);
}

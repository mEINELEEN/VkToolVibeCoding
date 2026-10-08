using WukongBenchmark.WinForms.Models;

namespace WukongBenchmark.WinForms.Services;

public interface IGameConfigService
{
    string Create(BenchmarkProfile profile);
}

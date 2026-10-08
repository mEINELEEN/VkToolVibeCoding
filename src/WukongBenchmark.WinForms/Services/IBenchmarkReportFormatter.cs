using System.Text.Json;
using WukongBenchmark.WinForms.Models;

namespace WukongBenchmark.WinForms.Services;

public interface IBenchmarkReportFormatter
{
    string FormatHardware(JsonElement result);
    string FormatResult(BenchmarkProfile profile, JsonElement result);
}

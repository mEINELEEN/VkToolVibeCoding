using System.Text.Json;
using WukongBenchmark.WinForms.Models;

namespace WukongBenchmark.WinForms.Services;

public sealed class BenchmarkReportFormatter : IBenchmarkReportFormatter
{
    public string FormatHardware(JsonElement result)
    {
        return $"CPU: {result.GetProperty("CPUModel")}\n" +
               $"GPU: {result.GetProperty("GPUModel")}\n" +
               $"RAM: {result.GetProperty("SysMem")}, VRAM: {result.GetProperty("VideoMemSize")}\n" +
               $"ОС: {result.GetProperty("SysVer")}, Benchmark Tool: {result.GetProperty("GameVer")}\n";
    }

    public string FormatResult(BenchmarkProfile profile, JsonElement result)
    {
        double cpuTime = 0;
        double gpuTime = 0;
        var count = 0;

        foreach (var record in result.GetProperty("Records").EnumerateArray())
        {
            cpuTime += record.GetProperty("CPUFrameTime").GetDouble();
            gpuTime += record.GetProperty("GPUFrameTime").GetDouble();
            count++;
        }

        if (count == 0)
        {
            throw new InvalidOperationException("В результате нет измерений времени кадра.");
        }

        cpuTime /= count;
        gpuTime /= count;

        var quality = GetQualityName(result.GetProperty("ShadowQuality").GetInt32());
        var distance = GetQualityName(result.GetProperty("ViewDistance").GetInt32());
        var rt = result.GetProperty("Rtx").GetInt32();
        var motionBlur = GetMotionBlurName(result.GetProperty("MotionBlur").GetInt32());
        var rtText = rt > 0 ? $"уровень {rt}" : "выключен";

        var rtNote = profile.Name == "GPU" && profile.RayTracing > 0 && rt == 0
            ? "Full Ray Tracing не применился, GPU-тест выполнен без него.\n"
            : string.Empty;

        return $"{profile.Name}-тест\n" +
               $"Настройки: {result.GetProperty("ScreenResolution")}, " +
               $"рендер {result.GetProperty("ImageQuality")}%, {quality}, дальность {distance}, RT {rtText}.\n" +
               rtNote +
               $"TSR; Motion Blur {motionBlur}; генерация кадров и VSync выключены; лимита FPS нет.\n" +
               $"FPS: средний {result.GetProperty("FPSAvg")}, " +
               $"минимум {result.GetProperty("FPSMin")}, максимум {result.GetProperty("FPSMax")}.\n" +
               $"Среднее время на кадр: CPU {cpuTime:F2} мс, GPU {gpuTime:F2} мс.\n" +
               $"Загрузка: CPU {result.GetProperty("CPUAvg")}%, GPU {result.GetProperty("GPUAvg")}%\n" +
               $"Использовано VRAM: {result.GetProperty("VideoMem").GetDouble():F2} GB.\n";
    }

    private static string GetQualityName(int value) => value switch
    {
        1 => "Low",
        2 => "Medium",
        3 => "High",
        4 => "Very High",
        5 => "Cinematic",
        _ => $"Unknown ({value})"
    };

    private static string GetMotionBlurName(int value) => value switch
    {
        0 => "выключен",
        1 => "Weak",
        2 => "Strong",
        _ => $"Unknown ({value})"
    };
}

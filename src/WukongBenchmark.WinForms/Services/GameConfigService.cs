using System.Globalization;
using WukongBenchmark.WinForms.Models;

namespace WukongBenchmark.WinForms.Services;

public sealed class GameConfigService : IGameConfigService
{
    private const string TemplateFileName = "GameUserSettings.template.ini";

    public string Create(BenchmarkProfile profile)
    {
        var templatePath = Path.Combine(AppContext.BaseDirectory, TemplateFileName);
        if (!File.Exists(templatePath))
        {
            throw new FileNotFoundException(
                $"Не найден шаблон конфигурации {TemplateFileName}. Поместите его рядом с программой.",
                templatePath);
        }

        var template = File.ReadAllText(templatePath);
        var renderWidth = profile.Width * profile.RenderPercent / 100;
        var renderHeight = profile.Height * profile.RenderPercent / 100;
        var engineQuality = profile.Quality - 1;
        var rayTracingEnabled = profile.RayTracing > 0 ? "1" : "0";

        return template
            .Replace("{Width}", profile.Width.ToString(CultureInfo.InvariantCulture))
            .Replace("{Height}", profile.Height.ToString(CultureInfo.InvariantCulture))
            .Replace("{RenderWidth}", renderWidth.ToString(CultureInfo.InvariantCulture))
            .Replace("{RenderHeight}", renderHeight.ToString(CultureInfo.InvariantCulture))
            .Replace("{RenderPercent}", profile.RenderPercent.ToString(CultureInfo.InvariantCulture))
            .Replace("{Quality}", profile.Quality.ToString(CultureInfo.InvariantCulture))
            .Replace("{EngineQuality}", engineQuality.ToString(CultureInfo.InvariantCulture))
            .Replace("{MotionBlur}", profile.MotionBlur.ToString(CultureInfo.InvariantCulture))
            .Replace("{RayTracingEnabled}", rayTracingEnabled)
            .Replace("{RayTracing}", profile.RayTracing.ToString(CultureInfo.InvariantCulture));
    }
}

namespace WukongBenchmark.WinForms.Models;

public sealed class BenchmarkProfile
{
    public string Name { get; init; } = string.Empty;
    public int Width { get; init; }
    public int Height { get; init; }
    public int RenderPercent { get; init; }
    public int Quality { get; init; }
    public int RayTracing { get; init; }
    public int MotionBlur { get; init; }
}

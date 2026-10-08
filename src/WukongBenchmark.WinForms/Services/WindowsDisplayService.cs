using System.Runtime.InteropServices;

namespace WukongBenchmark.WinForms.Services;

public sealed class WindowsDisplayService : IWindowsDisplayService
{
    [DllImport("user32.dll")]
    private static extern int GetSystemMetrics(int index);

    [DllImport("user32.dll")]
    private static extern bool SetProcessDPIAware();

    public (int Width, int Height) GetPrimaryScreenSize()
    {
        SetProcessDPIAware();
        return (GetSystemMetrics(0), GetSystemMetrics(1));
    }
}

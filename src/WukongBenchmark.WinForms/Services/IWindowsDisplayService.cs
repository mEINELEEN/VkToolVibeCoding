namespace WukongBenchmark.WinForms.Services;

public interface IWindowsDisplayService
{
    (int Width, int Height) GetPrimaryScreenSize();
}

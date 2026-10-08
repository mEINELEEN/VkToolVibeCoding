namespace WukongBenchmark.WinForms.Views;

public sealed class CloseRequestedEventArgs : EventArgs
{
    public bool Cancel { get; set; }
}

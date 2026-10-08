namespace WukongBenchmark.WinForms.Views;

public interface IBenchmarkView
{
    string GameFolder { get; set; }

    event EventHandler? StartRequested;
    event EventHandler? CancelRequested;
    event EventHandler<CloseRequestedEventArgs>? CloseRequested;

    void SetBusy(bool busy);
    void SetProgress(int percent);
    void SetStatus(string status);
    void AppendLog(string text);
    void ShowError(string message);
    void ShowInfo(string message);
    void CloseView();
    bool ConfirmCloseWhileRunning();
}

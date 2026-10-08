using WukongBenchmark.WinForms.Services;
using WukongBenchmark.WinForms.Views;

namespace WukongBenchmark.WinForms.Presenters;

public sealed class MainFormPresenter
{
    private readonly IBenchmarkView _view;
    private readonly IBenchmarkService _benchmarkService;
    private readonly IGameLocator _gameLocator;
    private CancellationTokenSource? _cancellation;
    private bool _isRunning;
    private bool _closeRequested;

    public MainFormPresenter(
        IBenchmarkView view,
        IBenchmarkService benchmarkService,
        IGameLocator gameLocator)
    {
        _view = view;
        _benchmarkService = benchmarkService;
        _gameLocator = gameLocator;

        _view.StartRequested += ViewOnStartRequested;
        _view.CancelRequested += ViewOnCancelRequested;
        _view.CloseRequested += ViewOnCloseRequested;

        if (string.IsNullOrWhiteSpace(_view.GameFolder))
        {
            var detected = _gameLocator.TryFindGameFolder();
            if (!string.IsNullOrWhiteSpace(detected))
            {
                _view.GameFolder = detected;
                _view.AppendLog($"Найдена папка Benchmark Tool: {detected}");
            }
            else
            {
                _view.AppendLog("Папка Benchmark Tool не найдена автоматически. Укажите её вручную.");
            }
        }
    }

    private async void ViewOnStartRequested(object? sender, EventArgs e)
    {
        if (_isRunning)
        {
            return;
        }

        var gameFolder = _view.GameFolder;
        if (string.IsNullOrWhiteSpace(gameFolder))
        {
            _view.ShowError("Укажите папку Black Myth Wukong Benchmark Tool.");
            return;
        }

        _closeRequested = false;
        _isRunning = true;
        _cancellation = new CancellationTokenSource();
        _view.SetBusy(true);
        _view.SetProgress(0);
        _view.SetStatus("Запуск…");
        _view.AppendLog("=== Начало benchmark ===");

        var logProgress = new Progress<string>(_view.AppendLog);
        var percentProgress = new Progress<int>(value =>
        {
            _view.SetProgress(value);
            _view.SetStatus($"Выполнение: {value}%");
        });

        try
        {
            var report = await Task.Run(
                () => _benchmarkService.Run(
                    gameFolder,
                    logProgress,
                    percentProgress,
                    _cancellation.Token),
                _cancellation.Token);

            _view.SetProgress(100);
            _view.SetStatus("Готово");
            _view.AppendLog("=== Benchmark завершён ===");
            _view.ShowInfo($"Отчёт сохранён:\n{report.FilePath}");
        }
        catch (OperationCanceledException)
        {
            _view.SetStatus("Отменено");
            _view.AppendLog("Запуск отменён. Конфигурация восстановлена.");
        }
        catch (Exception ex)
        {
            _view.SetStatus("Ошибка");
            _view.AppendLog($"Ошибка: {ex.Message}");
            _view.ShowError(ex.Message);
        }
        finally
        {
            _cancellation.Dispose();
            _cancellation = null;
            _isRunning = false;
            _view.SetBusy(false);

            if (_closeRequested)
            {
                _view.CloseView();
            }
        }
    }

    private void ViewOnCancelRequested(object? sender, EventArgs e)
    {
        _cancellation?.Cancel();
        _view.SetStatus("Отмена…");
        _view.AppendLog("Запрошена отмена. Завершаем текущий проход и восстанавливаем конфиг…");
    }

    private void ViewOnCloseRequested(object? sender, CloseRequestedEventArgs e)
    {
        if (!_isRunning)
        {
            return;
        }

        if (!_view.ConfirmCloseWhileRunning())
        {
            e.Cancel = true;
            return;
        }

        _closeRequested = true;
        _cancellation?.Cancel();
        e.Cancel = true;
        _view.SetStatus("Остановка перед закрытием…");
        _view.AppendLog("Закрытие запрошено. Останавливаем benchmark и восстанавливаем конфиг…");
    }
}

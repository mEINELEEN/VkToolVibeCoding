# Перенос исходного проекта в MVP

| Исходник | MVP-порт |
|---|---|
| `Program.cs` — точка входа и orchestration | `Program.cs` + `Presenters/MainFormPresenter.cs` + `Services/BenchmarkService.cs` |
| `Profile.cs` | `Models/BenchmarkProfile.cs` |
| `GameConfig.cs` | `Services/IGameConfigService.cs` + `Services/GameConfigService.cs` |
| `Benchmark.cs` — процессы, Steam, JSON, проверки | `Services/IBenchmarkRunner.cs` + `Services/SteamBenchmarkRunner.cs` |
| `FormatHardware` / `FormatResult` | `Services/IBenchmarkReportFormatter.cs` + `Services/BenchmarkReportFormatter.cs` |
| `GetGameFolder` + Steam registry lookup | `Services/IGameLocator.cs` + `Services/GameLocator.cs` |
| `GetSystemMetrics` / `SetProcessDPIAware` | `Services/IWindowsDisplayService.cs` + `Services/WindowsDisplayService.cs` |
| Console UI | `Views/IBenchmarkView.cs` + `Views/MainForm.cs` |

## Что является Presenter-логикой

`MainFormPresenter` отвечает за:

- реакцию на Start / Cancel / Close;
- создание и освобождение `CancellationTokenSource`;
- состояние View (`busy`, progress, status);
- передачу `IProgress<T>` сервисному слою;
- обработку `OperationCanceledException` и ошибок;
- безопасное завершение формы после восстановления конфигурации.

## Что не находится во View

`MainForm` не знает:

- где находится Steam;
- какой App ID использовать;
- какие параметры имеют CPU/GPU профили;
- где лежит JSON benchmark-а;
- как проверять настройки;
- как восстанавливать `GameUserSettings.ini`.

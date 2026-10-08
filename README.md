# WukongBenchmark.WinForms

Порт исходного `WukongRunner` на **C# / .NET 8.0 / Windows Forms** с архитектурой **MVP**.

Репозиторий-источник: https://github.com/raydenoir/task-1-wukong

## Архитектура

```text
WukongBenchmark.WinForms
├── Models
│   ├── BenchmarkProfile.cs
│   ├── BenchmarkRun.cs
│   └── BenchmarkReport.cs
├── Views
│   ├── IBenchmarkView.cs
│   ├── CloseRequestedEventArgs.cs
│   └── MainForm.cs
├── Presenters
│   └── MainFormPresenter.cs
├── Services
│   ├── GameLocator.cs
│   ├── GameConfigService.cs
│   ├── SteamBenchmarkRunner.cs
│   ├── WindowsDisplayService.cs
│   ├── BenchmarkReportFormatter.cs
│   └── BenchmarkService.cs
└── Program.cs
```

### MVP

- `MainForm` отвечает только за WinForms UI и пользовательские события.
- `MainFormPresenter` связывает View и сервис, управляет `CancellationTokenSource` и состоянием UI.
- `BenchmarkService` содержит сценарий всего benchmark-процесса.
- `SteamBenchmarkRunner` занимается Steam, процессами, ожиданием JSON и проверкой настроек.
- `GameConfigService` генерирует `GameUserSettings.ini` из шаблона.
- `BenchmarkReportFormatter` форматирует железо и результаты.
- Интерфейсы сервисов позволяют заменить инфраструктуру на заглушки при тестировании.

## Что сохранено из исходного проекта

1. Поиск Benchmark Tool через `Steam App 3132990` в реестре и ручной выбор папки.
2. Поиск Steam через реестр с fallback на `Program Files (x86)\\Steam`.
3. Проверка `b1_benchmark.exe`.
4. Проверка, что benchmark уже не запущен.
5. Резервная копия `GameUserSettings.ini` с восстановлением/удалением в `finally`.
6. Два прохода:
   - CPU: `1280x720`, 25%, Low, RT off, Motion Blur off.
   - GPU: разрешение основного экрана, 100%, Cinematic, RT level 3, Motion Blur Strong.
7. Запуск через `steam.exe -applaunch 3132990 -benchmark -windowed -forceres`.
8. Ожидание нового JSON в `%TEMP%\\b1\\BenchMarkHistory\\Tool`.
9. Проверка применённых игрой настроек.
10. Завершение процессов benchmark после прохода.
11. Таймаут 5 минут на проход.
12. `report.txt` сохраняется после каждого завершённого прохода.
13. Отмена через кнопку `Отмена` или закрытие формы; при закрытии форма не завершает приложение до окончания очистки и восстановления конфигурации.

## Сборка

На Windows с установленным .NET 8 SDK:

```powershell
dotnet restore
dotnet build -c Release
```

Публикация self-contained x64:

```powershell
dotnet publish .\src\WukongBenchmark.WinForms\WukongBenchmark.WinForms.csproj `
  -c Release -r win-x64 --self-contained true `
  -p:PublishSingleFile=true `
  -p:IncludeNativeLibrariesForSelfExtract=true `
  -p:DebugType=None `
  -o .\dist\win-x64
```

`GameUserSettings.template.ini` копируется рядом с опубликованным EXE автоматически.

## Запуск

Без аргумента папка определяется через реестр:

```powershell
.\WukongBenchmark.WinForms.exe
```

Можно передать путь вручную:

```powershell
.\WukongBenchmark.WinForms.exe "D:\SteamLibrary\steamapps\common\Black Myth Wukong Benchmark Tool"
```

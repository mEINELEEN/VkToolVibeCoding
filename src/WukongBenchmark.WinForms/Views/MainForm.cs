using System.Drawing;
using System.Windows.Forms;

namespace WukongBenchmark.WinForms.Views;

public sealed class MainForm : Form, IBenchmarkView
{
    private readonly TextBox _gameFolderTextBox;
    private readonly Button _browseButton;
    private readonly Button _startButton;
    private readonly Button _cancelButton;
    private readonly ProgressBar _progressBar;
    private readonly Label _statusLabel;
    private readonly RichTextBox _logBox;

    public MainForm(string? initialFolder)
    {
        Text = "Black Myth: Wukong — Benchmark Runner";
        StartPosition = FormStartPosition.CenterScreen;
        MinimumSize = new Size(900, 600);
        Size = new Size(1000, 680);
        Font = new Font("Segoe UI", 9F);
        AutoScaleMode = AutoScaleMode.Dpi;

        _gameFolderTextBox = new TextBox
        {
            Dock = DockStyle.Fill,
            PlaceholderText = "Папка Black Myth Wukong Benchmark Tool"
        };

        if (!string.IsNullOrWhiteSpace(initialFolder))
        {
            _gameFolderTextBox.Text = initialFolder;
        }

        _browseButton = new Button
        {
            Text = "Обзор…",
            Dock = DockStyle.Fill,
            Width = 100
        };
        _browseButton.Click += BrowseButtonOnClick;

        _startButton = new Button
        {
            Text = "Запустить benchmark",
            AutoSize = true,
            Padding = new Padding(12, 5, 12, 5)
        };
        _startButton.Click += (_, _) => StartRequested?.Invoke(this, EventArgs.Empty);

        _cancelButton = new Button
        {
            Text = "Отмена",
            AutoSize = true,
            Padding = new Padding(12, 5, 12, 5),
            Enabled = false
        };
        _cancelButton.Click += (_, _) => CancelRequested?.Invoke(this, EventArgs.Empty);

        _progressBar = new ProgressBar
        {
            Dock = DockStyle.Fill,
            Minimum = 0,
            Maximum = 100
        };

        _statusLabel = new Label
        {
            AutoSize = true,
            Text = "Готово"
        };

        _logBox = new RichTextBox
        {
            Dock = DockStyle.Fill,
            ReadOnly = true,
            DetectUrls = false,
            WordWrap = false,
            BackColor = SystemColors.Window,
            BorderStyle = BorderStyle.FixedSingle
        };

        BuildLayout();
    }

    public string GameFolder
    {
        get => _gameFolderTextBox.Text.Trim();
        set => _gameFolderTextBox.Text = value;
    }

    public event EventHandler? StartRequested;
    public event EventHandler? CancelRequested;
    public event EventHandler<CloseRequestedEventArgs>? CloseRequested;

    public void SetBusy(bool busy)
    {
        _gameFolderTextBox.ReadOnly = busy;
        _browseButton.Enabled = !busy;
        _startButton.Enabled = !busy;
        _cancelButton.Enabled = busy;
    }

    public void SetProgress(int percent)
    {
        _progressBar.Value = Math.Clamp(percent, _progressBar.Minimum, _progressBar.Maximum);
    }

    public void SetStatus(string status)
    {
        _statusLabel.Text = status;
    }

    public void AppendLog(string text)
    {
        if (_logBox.TextLength > 0 && !_logBox.Text.EndsWith(Environment.NewLine))
        {
            _logBox.AppendText(Environment.NewLine);
        }

        _logBox.AppendText(text);
        _logBox.ScrollToCaret();
    }

    public void ShowError(string message)
    {
        MessageBox.Show(this, message, "Wukong Benchmark", MessageBoxButtons.OK, MessageBoxIcon.Error);
    }

    public void ShowInfo(string message)
    {
        MessageBox.Show(this, message, "Wukong Benchmark", MessageBoxButtons.OK, MessageBoxIcon.Information);
    }

    public void CloseView()
    {
        Close();
    }

    public bool ConfirmCloseWhileRunning()
    {
        return MessageBox.Show(
            this,
            "Benchmark ещё выполняется. Закрыть приложение и остановить текущий проход?",
            "Wukong Benchmark",
            MessageBoxButtons.YesNo,
            MessageBoxIcon.Warning) == DialogResult.Yes;
    }

    protected override void OnFormClosing(FormClosingEventArgs e)
    {
        var args = new CloseRequestedEventArgs();
        CloseRequested?.Invoke(this, args);
        e.Cancel = args.Cancel;

        if (!e.Cancel)
        {
            base.OnFormClosing(e);
        }
    }

    private void BrowseButtonOnClick(object? sender, EventArgs e)
    {
        using var dialog = new FolderBrowserDialog
        {
            Description = "Выберите папку Black Myth Wukong Benchmark Tool",
            UseDescriptionForTitle = true,
            ShowNewFolderButton = false
        };

        if (Directory.Exists(GameFolder))
        {
            dialog.InitialDirectory = GameFolder;
        }

        if (dialog.ShowDialog(this) == DialogResult.OK)
        {
            GameFolder = dialog.SelectedPath;
        }
    }

    private void BuildLayout()
    {
        var root = new TableLayoutPanel
        {
            Dock = DockStyle.Fill,
            Padding = new Padding(12),
            ColumnCount = 1,
            RowCount = 5
        };
        root.RowStyles.Add(new RowStyle(SizeType.AutoSize));
        root.RowStyles.Add(new RowStyle(SizeType.AutoSize));
        root.RowStyles.Add(new RowStyle(SizeType.AutoSize));
        root.RowStyles.Add(new RowStyle(SizeType.AutoSize));
        root.RowStyles.Add(new RowStyle(SizeType.Percent, 100F));
        Controls.Add(root);

        var title = new Label
        {
            AutoSize = true,
            Text = "Автоматический CPU/GPU benchmark",
            Font = new Font(Font, FontStyle.Bold),
            Margin = new Padding(0, 0, 0, 8)
        };
        root.Controls.Add(title, 0, 0);

        var folderLayout = new TableLayoutPanel
        {
            Dock = DockStyle.Fill,
            ColumnCount = 2,
            RowCount = 1,
            AutoSize = true
        };
        folderLayout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100F));
        folderLayout.ColumnStyles.Add(new ColumnStyle(SizeType.AutoSize));
        folderLayout.Controls.Add(_gameFolderTextBox, 0, 0);
        folderLayout.Controls.Add(_browseButton, 1, 0);
        root.Controls.Add(folderLayout, 0, 1);

        var actionLayout = new FlowLayoutPanel
        {
            Dock = DockStyle.Fill,
            AutoSize = true,
            WrapContents = false,
            Margin = new Padding(0, 8, 0, 8)
        };
        actionLayout.Controls.Add(_startButton);
        actionLayout.Controls.Add(_cancelButton);
        root.Controls.Add(actionLayout, 0, 2);

        var statusLayout = new TableLayoutPanel
        {
            Dock = DockStyle.Fill,
            ColumnCount = 2,
            RowCount = 1,
            AutoSize = true,
            Margin = new Padding(0, 0, 0, 8)
        };
        statusLayout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100F));
        statusLayout.ColumnStyles.Add(new ColumnStyle(SizeType.AutoSize));
        statusLayout.Controls.Add(_progressBar, 0, 0);
        statusLayout.Controls.Add(_statusLabel, 1, 0);
        root.Controls.Add(statusLayout, 0, 3);

        var logGroup = new GroupBox
        {
            Dock = DockStyle.Fill,
            Text = "Вывод"
        };
        logGroup.Controls.Add(_logBox);
        root.Controls.Add(logGroup, 0, 4);
    }
}

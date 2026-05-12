using System.Collections.ObjectModel;
using BBDown.GUI.Models;
using BBDown.GUI.Services;

namespace BBDown.GUI.ViewModels;

public sealed class MainWindowViewModel : ViewModelBase, IAsyncDisposable
{
    private readonly BBDownApiClient apiClient;
    private readonly BBDownBackendHost backendHost;
    private CancellationTokenSource? pollingCts;
    private bool initialized;
    private bool isBusy;
    private string url = string.Empty;
    private string workDir = string.Empty;
    private string cookie = string.Empty;
    private string accessToken = string.Empty;
    private string selectPage = string.Empty;
    private string encodingPriority = string.Empty;
    private string dfnPriority = string.Empty;
    private string ffmpegPath = string.Empty;
    private string mp4boxPath = string.Empty;
    private string aria2cPath = string.Empty;
    private string aria2cArgs = string.Empty;
    private string selectedApiMode = "WEB";
    private string backendState = "启动中";
    private string statusMessage = "正在准备内置下载服务...";
    private bool useMP4box;
    private bool useAria2c;
    private bool multiThread = true;
    private bool videoOnly;
    private bool audioOnly;
    private bool debug;
    private bool skipMux;
    private bool skipSubtitle;
    private bool skipCover;
    private bool forceHttp = true;
    private bool downloadDanmaku;
    private bool skipAi = true;
    private bool forceReplaceHost = true;

    public MainWindowViewModel(BBDownApiClient apiClient, BBDownBackendHost backendHost)
    {
        this.apiClient = apiClient;
        this.backendHost = backendHost;
        SubmitTaskCommand = new AsyncCommand(SubmitTaskAsync, () => initialized && !string.IsNullOrWhiteSpace(Url) && !string.IsNullOrWhiteSpace(WorkDir));
        RefreshCommand = new AsyncCommand(RefreshTasksAsync, () => initialized);
        WorkDir = GetDefaultWorkDir();
    }

    public IReadOnlyList<string> ApiModes { get; } = ["WEB", "TV", "APP", "INTL"];

    public ObservableCollection<DownloadTaskItemViewModel> RunningTasks { get; } = [];

    public ObservableCollection<DownloadTaskItemViewModel> FinishedTasks { get; } = [];

    public AsyncCommand SubmitTaskCommand { get; }

    public AsyncCommand RefreshCommand { get; }

    public string Url
    {
        get => url;
        set
        {
            if (!SetProperty(ref url, value)) return;
            SubmitTaskCommand.RaiseCanExecuteChanged();
        }
    }

    public string WorkDir
    {
        get => workDir;
        set
        {
            if (!SetProperty(ref workDir, value)) return;
            OnPropertyChanged(nameof(WorkDirDisplay));
            OnPropertyChanged(nameof(HasWorkDir));
            SubmitTaskCommand?.RaiseCanExecuteChanged();
        }
    }

    public string Cookie
    {
        get => cookie;
        set => SetProperty(ref cookie, value);
    }

    public string AccessToken
    {
        get => accessToken;
        set => SetProperty(ref accessToken, value);
    }

    public string SelectPage
    {
        get => selectPage;
        set => SetProperty(ref selectPage, value);
    }

    public string EncodingPriority
    {
        get => encodingPriority;
        set => SetProperty(ref encodingPriority, value);
    }

    public string DfnPriority
    {
        get => dfnPriority;
        set => SetProperty(ref dfnPriority, value);
    }

    public string FFmpegPath
    {
        get => ffmpegPath;
        set => SetProperty(ref ffmpegPath, value);
    }

    public string Mp4boxPath
    {
        get => mp4boxPath;
        set => SetProperty(ref mp4boxPath, value);
    }

    public string Aria2cPath
    {
        get => aria2cPath;
        set => SetProperty(ref aria2cPath, value);
    }

    public string Aria2cArgs
    {
        get => aria2cArgs;
        set => SetProperty(ref aria2cArgs, value);
    }

    public string SelectedApiMode
    {
        get => selectedApiMode;
        set => SetProperty(ref selectedApiMode, value);
    }

    public string BackendState
    {
        get => backendState;
        set => SetProperty(ref backendState, value);
    }

    public string StatusMessage
    {
        get => statusMessage;
        set => SetProperty(ref statusMessage, value);
    }

    public bool UseMP4box
    {
        get => useMP4box;
        set => SetProperty(ref useMP4box, value);
    }

    public bool UseAria2c
    {
        get => useAria2c;
        set => SetProperty(ref useAria2c, value);
    }

    public bool MultiThread
    {
        get => multiThread;
        set => SetProperty(ref multiThread, value);
    }

    public bool VideoOnly
    {
        get => videoOnly;
        set
        {
            if (!SetProperty(ref videoOnly, value)) return;
            if (value) AudioOnly = false;
        }
    }

    public bool AudioOnly
    {
        get => audioOnly;
        set
        {
            if (!SetProperty(ref audioOnly, value)) return;
            if (value) VideoOnly = false;
        }
    }

    public bool Debug
    {
        get => debug;
        set => SetProperty(ref debug, value);
    }

    public bool SkipMux
    {
        get => skipMux;
        set => SetProperty(ref skipMux, value);
    }

    public bool SkipSubtitle
    {
        get => skipSubtitle;
        set => SetProperty(ref skipSubtitle, value);
    }

    public bool SkipCover
    {
        get => skipCover;
        set => SetProperty(ref skipCover, value);
    }

    public bool ForceHttp
    {
        get => forceHttp;
        set => SetProperty(ref forceHttp, value);
    }

    public bool DownloadDanmaku
    {
        get => downloadDanmaku;
        set => SetProperty(ref downloadDanmaku, value);
    }

    public bool SkipAi
    {
        get => skipAi;
        set => SetProperty(ref skipAi, value);
    }

    public bool ForceReplaceHost
    {
        get => forceReplaceHost;
        set => SetProperty(ref forceReplaceHost, value);
    }

    public bool IsBusy
    {
        get => isBusy;
        private set => SetProperty(ref isBusy, value);
    }

    public string WorkDirDisplay => string.IsNullOrWhiteSpace(WorkDir) ? "尚未选择输出目录" : WorkDir;

    public bool HasWorkDir => !string.IsNullOrWhiteSpace(WorkDir);

    public string RunningCount => RunningTasks.Count.ToString("00");

    public string FinishedCount => FinishedTasks.Count.ToString("00");

    public bool NoRunningTasks => RunningTasks.Count == 0;

    public bool NoFinishedTasks => FinishedTasks.Count == 0;

    public async Task InitializeAsync()
    {
        if (initialized) return;

        try
        {
            await apiClient.StartAsync();
            initialized = true;
            pollingCts = new CancellationTokenSource();
            BackendState = "后端已就绪";
            StatusMessage = $"内置服务 {backendHost.BaseUri}";
            SubmitTaskCommand.RaiseCanExecuteChanged();
            RefreshCommand.RaiseCanExecuteChanged();
            _ = PollTasksAsync(pollingCts.Token);
            await RefreshTasksAsync();
        }
        catch (Exception ex)
        {
            BackendState = "启动失败";
            StatusMessage = ex.Message;
        }
    }

    public async ValueTask DisposeAsync()
    {
        pollingCts?.Cancel();
        await backendHost.DisposeAsync();
    }

    private async Task SubmitTaskAsync()
    {
        try
        {
            IsBusy = true;
            StatusMessage = "正在加入下载队列...";
            await apiClient.AddTaskAsync(BuildRequest());
            StatusMessage = "任务已加入队列";
            await RefreshTasksAsync();
        }
        catch (Exception ex)
        {
            StatusMessage = ex.Message;
        }
        finally
        {
            IsBusy = false;
        }
    }

    private async Task RefreshTasksAsync()
    {
        try
        {
            var result = await apiClient.GetTasksAsync();
            ReplaceTasks(RunningTasks, result.Running
                .OrderByDescending(task => task.TaskCreateTime)
                .Select(task => new DownloadTaskItemViewModel(task)));
            ReplaceTasks(FinishedTasks, result.Finished
                .OrderByDescending(task => task.TaskFinishTime ?? task.TaskCreateTime)
                .Select(task => new DownloadTaskItemViewModel(task)));
            NotifyTaskCollectionChanges();
        }
        catch (Exception ex)
        {
            StatusMessage = ex.Message;
        }
    }

    private async Task PollTasksAsync(CancellationToken cancellationToken)
    {
        while (!cancellationToken.IsCancellationRequested)
        {
            await RefreshTasksAsync();
            try
            {
                await Task.Delay(TimeSpan.FromSeconds(2), cancellationToken);
            }
            catch (OperationCanceledException)
            {
                return;
            }
        }
    }

    private DownloadTaskRequest BuildRequest()
    {
        return new DownloadTaskRequest
        {
            Url = Url.Trim(),
            WorkDir = WorkDir.Trim(),
            Cookie = Cookie.Trim(),
            AccessToken = AccessToken.Trim(),
            SelectPage = SelectPage.Trim(),
            EncodingPriority = EmptyToNull(EncodingPriority),
            DfnPriority = EmptyToNull(DfnPriority),
            FFmpegPath = FFmpegPath.Trim(),
            Mp4boxPath = Mp4boxPath.Trim(),
            Aria2cPath = Aria2cPath.Trim(),
            Aria2cArgs = Aria2cArgs.Trim(),
            UseTvApi = SelectedApiMode == "TV",
            UseAppApi = SelectedApiMode == "APP",
            UseIntlApi = SelectedApiMode == "INTL",
            UseMP4box = UseMP4box,
            UseAria2c = UseAria2c,
            MultiThread = MultiThread,
            VideoOnly = VideoOnly,
            AudioOnly = AudioOnly,
            Debug = Debug,
            SkipMux = SkipMux,
            SkipSubtitle = SkipSubtitle,
            SkipCover = SkipCover,
            ForceHttp = ForceHttp,
            DownloadDanmaku = DownloadDanmaku,
            SkipAi = SkipAi,
            ForceReplaceHost = ForceReplaceHost
        };
    }

    private static string? EmptyToNull(string value)
    {
        return string.IsNullOrWhiteSpace(value) ? null : value.Trim();
    }

    private static string GetDefaultWorkDir()
    {
        var userHome = Environment.GetFolderPath(Environment.SpecialFolder.UserProfile);
        if (!string.IsNullOrWhiteSpace(userHome))
        {
            var downloadsPath = Path.Combine(userHome, "Downloads");
            if (Directory.Exists(downloadsPath))
            {
                return downloadsPath;
            }
        }

        return Environment.CurrentDirectory;
    }

    private void NotifyTaskCollectionChanges()
    {
        OnPropertyChanged(nameof(RunningCount));
        OnPropertyChanged(nameof(FinishedCount));
        OnPropertyChanged(nameof(NoRunningTasks));
        OnPropertyChanged(nameof(NoFinishedTasks));
    }

    private static void ReplaceTasks(ObservableCollection<DownloadTaskItemViewModel> target, IEnumerable<DownloadTaskItemViewModel> items)
    {
        target.Clear();
        foreach (var item in items)
        {
            target.Add(item);
        }
    }
}

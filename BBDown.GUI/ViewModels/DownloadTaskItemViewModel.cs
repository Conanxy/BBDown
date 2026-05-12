using System.Diagnostics;
using System.IO;
using BBDown.GUI.Models;

namespace BBDown.GUI.ViewModels;

public sealed class DownloadTaskItemViewModel
{
    private readonly string? primarySavePath;

    public DownloadTaskItemViewModel(DownloadTaskResponse source)
    {
        Title = string.IsNullOrWhiteSpace(source.Title) ? source.Aid : source.Title!;
        Url = source.Url;
        ProgressPercent = Math.Round(source.Progress * 100d, 2);
        StatusText = source.TaskFinishTime is null
            ? "下载中"
            : source.IsSuccessful ? "已完成" : "失败";
        ErrorMessage = source.ErrorMessage ?? string.Empty;
        SavePathsText = string.Join(Environment.NewLine, source.SavePaths);
        HasError = !string.IsNullOrWhiteSpace(ErrorMessage);
        HasSavePaths = source.SavePaths.Count > 0;
        primarySavePath = source.SavePaths.FirstOrDefault();
        OpenLocationText = OperatingSystem.IsMacOS() ? "在 Finder 中打开" : OperatingSystem.IsWindows() ? "在文件夹中打开" : "打开位置";
        OpenSavedPathCommand = new AsyncCommand(OpenSavedPathAsync, () => HasSavePaths);
        DetailText = BuildDetailText(source);
    }

    public string Title { get; }

    public string Url { get; }

    public double ProgressPercent { get; }

    public string StatusText { get; }

    public string DetailText { get; }

    public string ErrorMessage { get; }

    public bool HasError { get; }

    public string SavePathsText { get; }

    public bool HasSavePaths { get; }

    public string OpenLocationText { get; }

    public AsyncCommand OpenSavedPathCommand { get; }

    private Task OpenSavedPathAsync()
    {
        if (string.IsNullOrWhiteSpace(primarySavePath)) return Task.CompletedTask;

        var targetPath = ResolveExistingTarget(primarySavePath);
        if (string.IsNullOrWhiteSpace(targetPath)) return Task.CompletedTask;

        if (OperatingSystem.IsMacOS())
        {
            if (File.Exists(targetPath))
            {
                Process.Start(new ProcessStartInfo("open")
                {
                    RedirectStandardOutput = false,
                    RedirectStandardError = false
                }.AddArguments("-R", targetPath));
            }
            else
            {
                Process.Start(new ProcessStartInfo("open")
                {
                    RedirectStandardOutput = false,
                    RedirectStandardError = false
                }.AddArguments(targetPath));
            }
            return Task.CompletedTask;
        }

        if (OperatingSystem.IsWindows())
        {
            if (File.Exists(targetPath))
            {
                Process.Start(new ProcessStartInfo("explorer.exe", $"/select,\"{targetPath}\""));
            }
            else
            {
                Process.Start(new ProcessStartInfo("explorer.exe", $"\"{targetPath}\""));
            }
            return Task.CompletedTask;
        }

        Process.Start(new ProcessStartInfo("xdg-open")
        {
            RedirectStandardOutput = false,
            RedirectStandardError = false
        }.AddArguments(targetPath));
        return Task.CompletedTask;
    }

    private static string BuildDetailText(DownloadTaskResponse source)
    {
        List<string> parts =
        [
            source.Aid,
            $"进度 {Math.Round(source.Progress * 100d, 2):0.00}%",
            $"已下 {FormatBytes(source.TotalDownloadedBytes)}",
            $"速度 {FormatBytes(source.DownloadSpeed)}/s",
            $"创建 {FormatUnixTime(source.TaskCreateTime)}"
        ];

        if (source.TaskFinishTime is long finishedAt)
        {
            parts.Add($"结束 {FormatUnixTime(finishedAt)}");
        }

        return string.Join("  ·  ", parts);
    }

    private static string FormatUnixTime(long timestamp)
    {
        return DateTimeOffset.FromUnixTimeSeconds(timestamp).LocalDateTime.ToString("yyyy-MM-dd HH:mm:ss");
    }

    private static string FormatBytes(double bytes)
    {
        string[] units = ["B", "KB", "MB", "GB", "TB"];
        int index = 0;
        while (bytes >= 1024d && index < units.Length - 1)
        {
            bytes /= 1024d;
            index++;
        }

        return $"{bytes:0.##} {units[index]}";
    }

    private static string? ResolveExistingTarget(string path)
    {
        if (File.Exists(path) || Directory.Exists(path))
        {
            return path;
        }

        var parent = Path.GetDirectoryName(path);
        return !string.IsNullOrWhiteSpace(parent) && Directory.Exists(parent) ? parent : null;
    }
}

internal static class ProcessStartInfoExtensions
{
    public static ProcessStartInfo AddArguments(this ProcessStartInfo startInfo, params string[] arguments)
    {
        foreach (var argument in arguments)
        {
            startInfo.ArgumentList.Add(argument);
        }

        return startInfo;
    }
}

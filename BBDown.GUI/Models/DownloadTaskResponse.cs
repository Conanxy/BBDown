namespace BBDown.GUI.Models;

public sealed class DownloadTaskResponse
{
    public string Aid { get; set; } = string.Empty;

    public string Url { get; set; } = string.Empty;

    public long TaskCreateTime { get; set; }

    public string? Title { get; set; }

    public string? Pic { get; set; }

    public long? VideoPubTime { get; set; }

    public long? TaskFinishTime { get; set; }

    public double Progress { get; set; }

    public double DownloadSpeed { get; set; }

    public double TotalDownloadedBytes { get; set; }

    public bool IsSuccessful { get; set; }

    public string? ErrorMessage { get; set; }

    public List<string> SavePaths { get; set; } = [];
}

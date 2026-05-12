namespace BBDown.GUI.Models;

public sealed class DownloadTaskRequest
{
    public string Url { get; set; } = string.Empty;

    public bool UseTvApi { get; set; }

    public bool UseAppApi { get; set; }

    public bool UseIntlApi { get; set; }

    public bool UseMP4box { get; set; }

    public string? EncodingPriority { get; set; }

    public string? DfnPriority { get; set; }

    public bool UseAria2c { get; set; }

    public bool MultiThread { get; set; } = true;

    public bool VideoOnly { get; set; }

    public bool AudioOnly { get; set; }

    public bool Debug { get; set; }

    public bool SkipMux { get; set; }

    public bool SkipSubtitle { get; set; }

    public bool SkipCover { get; set; }

    public bool ForceHttp { get; set; } = true;

    public bool DownloadDanmaku { get; set; }

    public bool SkipAi { get; set; } = true;

    public string SelectPage { get; set; } = string.Empty;

    public string Cookie { get; set; } = string.Empty;

    public string AccessToken { get; set; } = string.Empty;

    public string Aria2cArgs { get; set; } = string.Empty;

    public string WorkDir { get; set; } = string.Empty;

    public string FFmpegPath { get; set; } = string.Empty;

    public string Mp4boxPath { get; set; } = string.Empty;

    public string Aria2cPath { get; set; } = string.Empty;

    public bool ForceReplaceHost { get; set; } = true;
}

namespace BBDown.GUI.Models;

public sealed class DownloadTaskCollectionResponse
{
    public List<DownloadTaskResponse> Running { get; set; } = [];

    public List<DownloadTaskResponse> Finished { get; set; } = [];
}

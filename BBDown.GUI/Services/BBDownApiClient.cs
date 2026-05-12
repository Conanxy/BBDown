using System.Net.Http.Json;
using System.Text.Json;
using BBDown.GUI.Models;

namespace BBDown.GUI.Services;

public sealed class BBDownApiClient
{
    private readonly BBDownBackendHost backendHost;
    private readonly JsonSerializerOptions jsonOptions = new()
    {
        PropertyNamingPolicy = null,
        PropertyNameCaseInsensitive = true
    };

    private HttpClient? httpClient;

    public BBDownApiClient(BBDownBackendHost backendHost)
    {
        this.backendHost = backendHost;
    }

    public async Task StartAsync(CancellationToken cancellationToken = default)
    {
        await backendHost.StartAsync(cancellationToken);
        httpClient ??= new HttpClient
        {
            BaseAddress = backendHost.BaseUri
        };
    }

    public async Task<DownloadTaskCollectionResponse> GetTasksAsync(CancellationToken cancellationToken = default)
    {
        var client = httpClient ?? throw new InvalidOperationException("后端尚未启动");
        var result = await client.GetFromJsonAsync<DownloadTaskCollectionResponse>("get-tasks", jsonOptions, cancellationToken);
        return result ?? new DownloadTaskCollectionResponse();
    }

    public async Task AddTaskAsync(DownloadTaskRequest request, CancellationToken cancellationToken = default)
    {
        var client = httpClient ?? throw new InvalidOperationException("后端尚未启动");
        using var response = await client.PostAsync("add-task", JsonContent.Create(request, options: jsonOptions), cancellationToken);
        if (response.IsSuccessStatusCode) return;

        var message = await response.Content.ReadAsStringAsync(cancellationToken);
        throw new InvalidOperationException(string.IsNullOrWhiteSpace(message) ? $"提交任务失败: {(int)response.StatusCode}" : message);
    }
}

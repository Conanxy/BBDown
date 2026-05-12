using System.Net;
using System.Net.Sockets;
using BBDown;

namespace BBDown.GUI.Services;

public sealed class BBDownBackendHost : IAsyncDisposable
{
    private readonly BBDownApiServer server = new();
    private bool started;

    public Uri? BaseUri { get; private set; }

    public async Task StartAsync(CancellationToken cancellationToken = default)
    {
        if (started) return;

        var port = GetAvailablePort();
        BaseUri = new Uri($"http://127.0.0.1:{port}/");
        server.SetUpServer();
        await server.StartAsync(BaseUri.ToString().TrimEnd('/'), cancellationToken);
        started = true;
    }

    public async ValueTask DisposeAsync()
    {
        if (!started) return;

        await server.StopAsync();
        started = false;
    }

    private static int GetAvailablePort()
    {
        TcpListener listener = new(IPAddress.Loopback, 0);
        listener.Start();
        var port = ((IPEndPoint)listener.LocalEndpoint).Port;
        listener.Stop();
        return port;
    }
}

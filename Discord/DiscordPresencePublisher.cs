
using DiscordRPC;
using DiscordRPC.Logging;

namespace UniversalRPC.Discord;

public sealed class DiscordPresencePublisher : IDisposable
{
    private readonly string _applicationId;
    private DiscordRpcClient? _client;
    private bool _isConnected;
    private bool _isDisposed;

    public DiscordPresencePublisher(string applicationId)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(applicationId);

        _applicationId = applicationId;
    }

    public void Connect()
    {
        ObjectDisposedException.ThrowIf(_isDisposed, this);

        if (_client is not null)
            throw new InvalidOperationException(
                "Discord client has already been initialized.");

        var client = new DiscordRpcClient(_applicationId)
        {
            Logger = new ConsoleLogger
            {
                Level = LogLevel.Warning
            }
        };

        client.OnReady += (_, e) =>
        {
            _isConnected = true;
            Console.WriteLine(
                $"Connected to Discord. User: {e.User.Username}");
        };

        client.OnConnectionFailed += (_, _) =>
        {
            _isConnected = false;
            Console.WriteLine("Failed to connect to Discord.");
        };

        client.OnClose += (_, _) =>
        {
            _isConnected = false;
            Console.WriteLine("Discord connection closed.");
        };

        _client = client;
        client.Initialize();
    }

    public void Publish(RichPresence presence)
    {
        ArgumentNullException.ThrowIfNull(presence);
        ObjectDisposedException.ThrowIf(_isDisposed, this);

        if (!_isConnected || _client is null)
            throw new InvalidOperationException(
                "Discord is not connected.");

        _client.SetPresence(presence);
    }

    public void Clear()
    {
        ObjectDisposedException.ThrowIf(_isDisposed, this);

        if (!_isConnected || _client is null)
            return;

        _client.ClearPresence();
    }

    public void Dispose()
    {
        if (_isDisposed)
            return;

        _isDisposed = true;
        _isConnected = false;

        _client?.Dispose();
        _client = null;
    }
}

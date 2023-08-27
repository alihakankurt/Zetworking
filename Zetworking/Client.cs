using System.Net.Sockets;
using Zetworking.Enums;

namespace Zetworking;

public sealed class Client
{
    private readonly Socket _socket;

    public ClientState State { get; private set; }

    public Client()
    {
        _socket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
    }

    public async ValueTask ConnectAsync(string host, int port, CancellationToken cancellationToken = default)
    {
        if (State is not ClientState.Disconnected)
            throw new InvalidOperationException("Client is already connected or connecting.");

        State = ClientState.Connecting;
        Console.WriteLine($"Connecting to {host}:{port}...");

        try
        {
            await _socket.ConnectAsync(host, port, cancellationToken);
        }
        catch
        {
            State = ClientState.Disconnected;
            Console.WriteLine("Failed to connect.");
            throw;
        }

        State = ClientState.Connected;
        Console.WriteLine("Connected to server.");
    }

    public async ValueTask DisconectAsync(CancellationToken cancellationToken = default)
    {
        if (State is not ClientState.Connected)
            throw new InvalidOperationException("Client is not connected.");

        State = ClientState.Disconnecting;
        Console.WriteLine("Disconnecting from server...");

        try
        {
            _socket.Shutdown(SocketShutdown.Both);
            await _socket.DisconnectAsync(reuseSocket: true, cancellationToken);
        }
        catch
        {
            State = ClientState.Connected;
            Console.WriteLine("Failed to disconnect.");
            throw;
        }

        State = ClientState.Disconnected;
        Console.WriteLine("Disconnected from server.");
    }
}

using System.Net.Sockets;
using Zetworking.Enums;

namespace Zetworking;

public sealed class Client
{
    private readonly Socket _socket;
    private readonly CancellationTokenSource _cts;

    public ClientState State { get; private set; }

    public Action<object, Type>? OnPacketReceived { get; set; }

    public Client()
    {
        _socket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
        _cts = new CancellationTokenSource();
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
            _ = Task.Run(StartReceivingAsync);
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

    public async ValueTask SendAsync(object packet, CancellationToken cancellationToken = default)
    {
        if (State is not ClientState.Connected)
            throw new InvalidOperationException("Client is not connected.");

        byte[] data = PacketCollection.Prepare(packet);

        Console.WriteLine($"Sending {data.Length} bytes to server...");

        try
        {
            await _socket.SendAsync(data, SocketFlags.None, cancellationToken);
        }
        catch
        {
            Console.WriteLine("Failed to send bytes to server.");
            throw;
        }

        Console.WriteLine("Sent bytes to server.");
    }

    private async Task StartReceivingAsync()
    {
        if (State is not ClientState.Connected)
            throw new InvalidOperationException("Client is not connected.");

        Console.WriteLine("Starting to receive bytes from server...");

        var sizeBuffer = new byte[2];

        while (!_cts.IsCancellationRequested)
        {
            try
            {
                int length = await _socket.ReceiveAsync(sizeBuffer.AsMemory(0), SocketFlags.None, _cts.Token);
                if (length != 2)
                    continue;

                ushort size = BitConverter.ToUInt16(sizeBuffer, 0);
                var buffer = new byte[size];

                length = await _socket.ReceiveAsync(buffer.AsMemory(0), SocketFlags.None, _cts.Token);
                if (length != size)
                    continue;

                object packet = PacketCollection.Resolve(buffer, out Type packetType);
                OnPacketReceived?.Invoke(packet, packetType);
            }
            catch
            {
                Console.WriteLine("Failed to receive bytes from server.");
                throw;
            }
        }
    }
}

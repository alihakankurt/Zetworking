using System.Net;
using System.Net.Sockets;
using Zetworking.Enums;

namespace Zetworking;

public sealed class Server
{
    private readonly Socket _socket;
    private readonly CancellationTokenSource _cts;
    private Socket? _clientSocket;

    public ServerState State { get; private set; }

    public Action<object, Type>? OnPacketReceived { get; set; }

    public Server()
    {
        _socket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
        _cts = new CancellationTokenSource();
    }

    public void Start(int port)
    {
        if (State is not ServerState.Stopped)
            throw new InvalidOperationException("Server is already started or starting.");

        State = ServerState.Starting;
        Console.WriteLine($"Starting server on port {port}...");

        try
        {
            _socket.Bind(new IPEndPoint(IPAddress.Any, port));
            _socket.Listen(16);
            _socket.BeginAccept(HandleConnectionRequest, null);
            _ = Task.Run(StartReceivingAsync);
        }
        catch
        {
            State = ServerState.Stopped;
            Console.WriteLine("Failed to start server.");
            throw;
        }

        State = ServerState.Running;
        Console.WriteLine($"Server is running on port {port}.");
    }

    public void Stop()
    {
        if (State is not ServerState.Running)
            throw new InvalidOperationException("Server is not running.");

        State = ServerState.Stopping;
        Console.WriteLine("Stopping server...");

        try
        {
            _socket.Close();
        }
        catch
        {
            State = ServerState.Running;
            Console.WriteLine("Failed to stop server.");
            throw;
        }

        State = ServerState.Stopped;
        Console.WriteLine("Server stopped.");
    }

    public async ValueTask SendAsync(object packet, CancellationToken cancellationToken = default)
    {
        if (State is not ServerState.Running)
            throw new InvalidOperationException("Server is not running.");

        if (_clientSocket is null)
            return;

        byte[] data = PacketCollection.Prepare(packet);

        Console.WriteLine($"Sending {data.Length} bytes to client...");

        try
        {
            await _clientSocket!.SendAsync(data, SocketFlags.None, cancellationToken);
        }
        catch
        {
            Console.WriteLine("Failed to send bytes to client.");
            throw;
        }

        Console.WriteLine("Sent bytes to client.");
    }

    private void HandleConnectionRequest(IAsyncResult result)
    {
        _clientSocket = _socket.EndAccept(result);
        Console.WriteLine($"Connection established from {_clientSocket.RemoteEndPoint}.");
    }

    private async Task StartReceivingAsync()
    {
        if (State is not ServerState.Running)
            throw new InvalidOperationException("Server is not running.");

        Console.WriteLine("Starting to receive bytes from client...");

        var sizeBuffer = new byte[2];

        while (!_cts.IsCancellationRequested)
        {
            if (_clientSocket is null)
                continue;

            try
            {
                int length = await _clientSocket.ReceiveAsync(sizeBuffer.AsMemory(0), SocketFlags.None, _cts.Token);
                if (length != 2)
                    continue;

                ushort size = BitConverter.ToUInt16(sizeBuffer, 0);
                var buffer = new byte[size];

                length = await _clientSocket.ReceiveAsync(buffer.AsMemory(0), SocketFlags.None, _cts.Token);
                if (length != size)
                    continue;

                object packet = PacketCollection.Resolve(buffer, out Type packetType);
                OnPacketReceived?.Invoke(packet, packetType);
            }
            catch
            {
                Console.WriteLine("Failed to receive bytes from client.");
                throw;
            }
        }
    }
}

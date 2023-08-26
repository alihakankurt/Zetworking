using System.Net;
using System.Net.Sockets;
using Zetworking.Enums;

namespace Zetworking;

public sealed class Server
{
    private readonly Socket _socket;
    private Socket? _clientSocket;

    public ServerState State { get; private set; }

    public Server()
    {
        _socket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
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
        }
        catch
        {
            State = ServerState.Stopped;
            Console.WriteLine("Failed to start server.");
            return;
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

    private void HandleConnectionRequest(IAsyncResult result)
    {
        _clientSocket = _socket.EndAccept(result);
        Console.WriteLine($"Connection established from {_clientSocket.RemoteEndPoint}.");
    }
}

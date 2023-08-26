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

    public void Connect(string host, int port)
    {
        if (State is not ClientState.Disconnected)
            throw new InvalidOperationException("Client is already connected or connecting.");

        State = ClientState.Connecting;
        Console.WriteLine($"Connecting to {host}:{port}...");

        try
        {
            _socket.Connect(host, port);
        }
        catch
        {
            State = ClientState.Disconnected;
            Console.WriteLine("Failed to connect.");
            return;
        }

        State = ClientState.Connected;
        Console.WriteLine("Connected to server.");
    }

    public void Disconect()
    {
        if (State is not ClientState.Connected)
            throw new InvalidOperationException("Client is not connected.");

        State = ClientState.Disconnecting;
        Console.WriteLine("Disconnecting from server...");

        try
        {
            _socket.Shutdown(SocketShutdown.Both);
            _socket.Close();
        }
        catch
        {
            State = ClientState.Connected;
            Console.WriteLine("Failed to disconnect.");
            return;
        }

        State = ClientState.Disconnected;
        Console.WriteLine("Disconnected from server.");
    }
}

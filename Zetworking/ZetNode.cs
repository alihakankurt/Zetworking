using System;
using System.Buffers;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using System.Threading.Tasks;

namespace Zetworking;

/// <summary>
/// Represents a node in the zet network. It uses UDP as the underlying protocol.
/// </summary>
public sealed class ZetNode : IDisposable
{
    private const int DelayInterval = 10;
    private const int BufferSize = 1 << 16;

    private readonly Socket _socket;

    private Task? _receiveTask;
    private CancellationTokenSource? _cancellationTokenSource;

    private bool _disposed;

    /// <summary>
    /// Gets the state of the node.
    /// </summary>
    public ZetNodeState State { get; private set; }

    /// <summary>
    /// Gets the local end point of the connection.
    /// </summary>
    public EndPoint LocalEndPoint => _socket.LocalEndPoint ?? throw new InvalidOperationException("The node is not started.");

    /// <summary>
    /// Gets or sets the callback that is invoked when a packet is received.
    /// The parameters are the received packet and the type of the packet, respectively.
    /// </summary>
    public Action<IZetPacket, Type>? OnPacketReceived { get; set; }

    /// <summary>
    /// Initializes a new instance of the <see cref="ZetNode"/> class.
    /// </summary>
    public ZetNode()
    {
        _socket = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp);
        State = ZetNodeState.Stopped;
    }

    /// <inheritdoc />
    ~ZetNode()
    {
        Dispose(disposing: false);
    }

    /// <summary>
    /// Disposes the resources used by the node.
    /// </summary>
    public void Dispose()
    {
        Dispose(disposing: true);
        GC.SuppressFinalize(this);
    }

    /// <summary>
    /// Starts the node to receive packets on the specified port.
    /// </summary>
    /// <param name="port">The port number to bind to. If not specified, a random port is used.</param>
    /// <exception cref="InvalidOperationException" />
    public void Start(int port = 0)
    {
        ObjectDisposedException.ThrowIf(_disposed, this);

        if (State is not ZetNodeState.Stopped)
            throw new InvalidOperationException($"{nameof(ZetNode)} is already started or starting up.");

        State = ZetNodeState.Starting;

        try
        {
            _socket.Bind(new IPEndPoint(IPAddress.Any, port));
            _receiveTask = Task.Run(StartReceivingAsync);
        }
        catch
        {
            State = ZetNodeState.Stopped;
            throw;
        }

        State = ZetNodeState.Running;
    }

    /// <summary>
    /// Stops the node from receiving packets.
    /// </summary>
    /// <exception cref="InvalidOperationException" />
    public void Stop()
    {
        if (State is not ZetNodeState.Running)
            throw new InvalidOperationException($"{nameof(ZetNode)} is not running.");

        State = ZetNodeState.Stopping;

        try
        {
            _cancellationTokenSource?.Cancel();
            _cancellationTokenSource?.Dispose();
            _receiveTask?.Wait();
        }
        catch
        {
            State = ZetNodeState.Running;
            throw;
        }

        _cancellationTokenSource = null;
        _receiveTask = null;
        State = ZetNodeState.Stopped;
    }

    /// <summary>
    /// Sends the specified packet to the specified end point.
    /// </summary>
    /// <param name="packet">The packet to send.</param>
    /// <param name="endPoint">The remote end point to send the packet to.</param>
    /// <param name="cancellationToken">The optional cancellation token to cancel the operation.</param>
    /// <returns>Whether the packet was sent successfully or not.</returns>
    /// <exception cref="InvalidOperationException" />
    public async ValueTask<bool> SendAsync(IZetPacket packet, EndPoint endPoint, CancellationToken cancellationToken = default)
    {
        if (State is not ZetNodeState.Running)
            throw new InvalidOperationException($"{nameof(ZetNode)} is not running.");

        byte[] data = ZetPacketCollection.Prepare(packet);
        return await _socket.SendToAsync(data, SocketFlags.None, endPoint, cancellationToken) == data.Length;
    }

    private void Dispose(bool disposing)
    {
        if (_disposed)
            return;

        if (disposing)
        {
            Stop();
            _socket.Dispose();
        }

        _disposed = true;
    }

    private async Task StartReceivingAsync()
    {
        if (State is not ZetNodeState.Running)
            throw new InvalidOperationException($"{nameof(ZetNode)} is not running.");

        EndPoint remoteEndPoint = new IPEndPoint(IPAddress.Any, 0);
        byte[] buffer = ArrayPool<byte>.Shared.Rent(BufferSize);

        _cancellationTokenSource = new CancellationTokenSource();
        while (!_cancellationTokenSource!.IsCancellationRequested)
        {
            try
            {
                if (_socket.Available == 0)
                {
                    await Task.Delay(DelayInterval);
                    continue;
                }

                SocketReceiveFromResult result = await _socket.ReceiveFromAsync(buffer, SocketFlags.None, remoteEndPoint, _cancellationTokenSource.Token);
                IZetPacket packet = ZetPacketCollection.Resolve(buffer, out Type packetType);
                OnPacketReceived?.Invoke(packet, packetType);
            }
            catch (SocketException ex) when (ex.SocketErrorCode is SocketError.Interrupted or SocketError.OperationAborted)
            {
                break;
            }
            catch (OperationCanceledException)
            {
                break;
            }
            catch
            {
                continue;
            }
        }

        ArrayPool<byte>.Shared.Return(buffer);
    }
}

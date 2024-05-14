using System.Net;
using Zetworking;

IPAddress ipAddress = Dns.GetHostEntry(Dns.GetHostName()).AddressList[0];
Console.WriteLine($"IP Address: {ipAddress}");
const int port = 51721;

ZetPacketCollection.Register(typeof(MessagePacket));

using var server = new ZetNode
{
    OnPacketReceived = static (packet, type) =>
    {
        if (type == typeof(MessagePacket))
        {
            var messagePacket = (MessagePacket)packet;
            Console.WriteLine($"[{messagePacket.CreatedAt.ToLongTimeString()}] {messagePacket.Message}");
        }
    }
};

using var client = new ZetNode();

server.Start(port);
client.Start();

var endPoint = new IPEndPoint(ipAddress, port);

while (true)
{
    var message = Console.ReadLine();
    if (string.IsNullOrEmpty(message))
        break;

    await client.SendAsync(MessagePacket.Create(message), endPoint);
}

public sealed class MessagePacket : IZetPacket
{
    public string Message { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }

    public static MessagePacket Create(string message)
    {
        return new MessagePacket { Message = message, CreatedAt = DateTime.Now };
    }
}

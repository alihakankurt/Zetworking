using System.Text;
using Zetworking;

const string host = "127.0.0.1";
const int port = 51721;

PacketCollection.Register<MessagePacket>();

var server = new Server
{
    OnPacketReceived = static (packet, type) =>
    {
        if (type == typeof(MessagePacket))
        {
            var messagePacket = (MessagePacket)packet;
            Console.WriteLine($"[{messagePacket.CreatedAt.ToLongTimeString()}] Client: {messagePacket.Message}");
        }
    }
};
server.Start(port);

var client = new Client()
{
    OnPacketReceived = static (packet, type) =>
    {
        if (type == typeof(MessagePacket))
        {
            var messagePacket = (MessagePacket)packet;
            Console.WriteLine($"[{messagePacket.CreatedAt.ToLongTimeString()}] Server: {messagePacket.Message}");
        }
    }
};
await client.ConnectAsync(host, port);

_ = Console.ReadLine();

await client.SendAsync(MessagePacket.Create("Hello, dear server!"));

_ = Console.ReadLine();

await server.SendAsync(MessagePacket.Create("Hi, my lovely client!"));

_ = Console.ReadLine();

await client.DisconectAsync();

server.Stop();

class MessagePacket
{
    public string Message { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }

    public static MessagePacket Create(string message)
    {
        return new MessagePacket { Message = message, CreatedAt = DateTime.Now };
    }
}

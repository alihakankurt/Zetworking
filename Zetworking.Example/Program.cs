using System.Text;
using Zetworking;

const string host = "127.0.0.1";
const int port = 51721;

var server = new Server
{
    OnBytesReceived = static (memory) =>
    {
        var message = Encoding.UTF8.GetString(memory.Span);
        Console.WriteLine($"Client: {message}");
    }
};
server.Start(port);

var client = new Client()
{
    OnBytesReceived = static (memory) =>
    {
        var message = Encoding.UTF8.GetString(memory.Span);
        Console.WriteLine($"Server: {message}");
    }
};
await client.ConnectAsync(host, port);

_ = Console.ReadLine();

await client.SendAsync(Encoding.UTF8.GetBytes("Hello, server!"));

_ = Console.ReadLine();

await client.SendAsync(Encoding.UTF8.GetBytes("Hello, client!"));

_ = Console.ReadLine();

await client.DisconectAsync();

server.Stop();

using Zetworking;

const string host = "127.0.0.1";
const int port = 51721;

var server = new Server();
server.Start(port);

var client = new Client();
await client.ConnectAsync(host, port);

_ = Console.ReadLine();

await client.DisconectAsync();

await client.ConnectAsync(host, port);

_ = Console.ReadLine();

await client.DisconectAsync();

server.Stop();

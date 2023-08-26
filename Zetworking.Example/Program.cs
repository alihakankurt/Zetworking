using Zetworking;

const string host = "127.0.0.1";
const int port = 51721;

var server = new Server();
server.Start(port);

var client = new Client();
client.Connect(host, port);

_ = Console.ReadLine();

client.Disconect();
server.Stop();


using System.Net;
using System.Net.Sockets;
using System.Text;

var port = 5000;

var server = new TcpListener(IPAddress.Loopback, port); // IPv4 127.0.0.1 IPv6 ::1
server.Start();

Console.WriteLine($"Server started on port {port}");

while (true)
{
    var client = server.AcceptTcpClient();
    Console.WriteLine("Client connected!!!");

    try
    {
        var stream = client.GetStream();

        var buffer = new byte[1024];

        stream.Read(buffer);

        var msg = Encoding.UTF8.GetString(buffer);

        Console.WriteLine("Message from client: " + msg);

        buffer = Encoding.UTF8.GetBytes(msg.ToUpper());
        stream.Write(buffer);
    }
    catch { }



}

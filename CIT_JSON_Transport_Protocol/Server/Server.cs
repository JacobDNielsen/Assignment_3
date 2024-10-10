using System.Net.Sockets;
using System.Net;
using System.Text;
using System.Text.Json;


public class Server
{
    private readonly int _port;

    public Server(int port)
    {
        _port = port;
    }
    public void Run()
    {


        var server = new TcpListener(IPAddress.Loopback, _port); // IPv4 127.0.0.1 IPv6 ::1

        server.Start();

        Console.WriteLine($"Server started on port {_port}");

        while (true)
        {
            var client = server.AcceptTcpClient();
            Console.WriteLine("Client connected!!!");

            Task.Run(() => HandleClient(client)); // Vi laver en ny thread for hver client, så vi kan håndtere flere clients på samme tid. Nødvendigt, da vi i test environment kører tests på flere threads, og denne tillader altså server at kører med flere threads. 





        }
    }
    private void HandleClient(TcpClient client)
    {
        try
        {
            var stream = client.GetStream();

            string msg = ReadFromStream(stream);

            Console.WriteLine("Message from client: " + msg);

            if (msg == "{}") 
            {
                var response = new Response
                {
                    Status = "missing method, missing date"
                };
                var json = ToJson(response);
                WriteToStream(stream, json);
            }
            else
            {
                var request = FromJson(msg);
                if (request == null)
                {
                    return; //Could possibly use better error handling here!
                }
                string[] validMethods = ["create", "read", "update", "delete", "echo"];

                if (!validMethods.Contains(request.Method))
                {
                    var response = new Response
                    {
                        Status = "illegal method"
                    };
                    var json = ToJson(response);
                    WriteToStream(stream, json);
                }

                else if (request.Path == null)
                {
                    var response = new Response
                    {
                        Status = "missing resource"
                    };
                    var json = ToJson(response);
                    WriteToStream(stream, json);
                }
            }
        }
        catch { }
    }
    private string ReadFromStream(NetworkStream stream)
    {
        var buffer = new byte[1024];
        var readCount = stream.Read(buffer);
        return Encoding.UTF8.GetString(buffer, 0, readCount); //Hvis der fx. er 10 bytes, så læser vi kun de 10 bytes. Vi tager altså fra pos0 til readcount. 
                                                              // Hvis vi bare gjorde: return Encoding.UTF8.GetString(buffer);, så vil vi tage hele bufferen med og sætte den til en string
    }

    private void WriteToStream(NetworkStream stream, string msg)
    {
        var buffer = Encoding.UTF8.GetBytes(msg);
        stream.Write(buffer);
    }

    public static string ToJson(Response response)
    {
        return JsonSerializer.Serialize(response, new JsonSerializerOptions { PropertyNamingPolicy = JsonNamingPolicy.CamelCase });
    }

    public static Request? FromJson(string element)
    {
        return JsonSerializer.Deserialize<Request>(element, new JsonSerializerOptions { PropertyNamingPolicy = JsonNamingPolicy.CamelCase });
    }
}


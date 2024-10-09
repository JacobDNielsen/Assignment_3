
using System.Linq.Expressions;
using System.Net;
using System.Net.Sockets;
using System.Text;
using static System.Runtime.InteropServices.JavaScript.JSType;
using System.Text.Json;
using System.Xml.Linq;

class ServerProgram
{
    public class Response
    {
        public string? Status { get; set; }
        public string? Body { get; set; }
    }

    static void Main(string[] args)
    {


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

                var buffer = new byte[2048];

                stream.Read(buffer);

                var msg = Encoding.UTF8.GetString(buffer);

                var msgProcessor = ProcessRequest(msg);

                Console.WriteLine("Message from client: " + msg);

                var msgProcessorToJson = JsonSerializer.Serialize(msgProcessor, new JsonSerializerOptions { PropertyNamingPolicy = JsonNamingPolicy.CamelCase });

                buffer = Encoding.UTF8.GetBytes(msgProcessorToJson);
                stream.Write(buffer);
                Console.WriteLine("test" + msgProcessorToJson);
            }
            catch { }



        }


    }

    public static Response ProcessRequest(string msg)
    {
        var response = new Response();
        //  “create”, “read”, “update”, “delete”, “echo”

        try
        {
            string testMsg = "{\"status\": \"missing method\", \"body\": \"yahoo\"}";

            response = JsonSerializer.Deserialize<Response>(testMsg, new JsonSerializerOptions { PropertyNamingPolicy = JsonNamingPolicy.CamelCase });
            Console.WriteLine("status er " + response.Status);
            Console.WriteLine("body er " + response.Body);
            if (response.Status == null)
            {
                response.Status = "missing method";
            }
            if (response.Body == null)
            {
                response.Body = "missing body";
            }
            return response;

        }
        catch
        {
            string testMsg = "{\"status\": \"missing method\", \"body\": \"yahoo\"}";


            response = JsonSerializer.Deserialize<Response>(testMsg, new JsonSerializerOptions { PropertyNamingPolicy = JsonNamingPolicy.CamelCase });

            response.Status = "missing method";
            response.Body = "war";
            return response;


        }


    }



}


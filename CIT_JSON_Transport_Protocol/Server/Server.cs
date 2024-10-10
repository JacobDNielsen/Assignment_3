﻿using System.Net.Sockets;
using System.Net;
using System.Text;
using System.Text.Json;
using System.Text.RegularExpressions;

public class Server
{
    Category category = new Category();

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

            var regexItem = new Regex("^[0-9]*$");
            var regexItemForID = new Regex(@"/(\d+)$");
            var response = new Response();

            if (msg == "{}")
            {
                response.Status = "missing method, missing date";

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
                string[] validMethodsForBody = ["create", "update", "echo"];
                string[] validMethodsRequiresJsonBody = ["create", "update"];
                string[] validMethodsRequireID = ["read", "update", "delete"];

                if (!validMethods.Contains(request.Method))
                {
                    response.Status = "illegal method";

                    var json = ToJson(response);
                    WriteToStream(stream, json);
                }
                else if (request.Path == null && request.Method != "echo")
                {
                    response.Status = "missing resource";

                    var json = ToJson(response);
                    WriteToStream(stream, json);
                }
                else if (!regexItem.IsMatch(request.Date.ToString()))
                {
                    response.Status = "illegal date";

                    var json = ToJson(response);
                    WriteToStream(stream, json);
                }
                else if (validMethodsForBody.Contains(request.Method) && request.Body == null)
                {
                    response.Status = "missing body";
                    var json = ToJson(response);
                    WriteToStream(stream, json);
                }
                else if (validMethodsRequiresJsonBody.Contains(request.Method))
                {
                    try
                    {
                        JsonDocument.Parse(request.Body); // do something if it is ok
                    }
                    catch
                    {
                        response.Status = "illegal body";

                        var json = ToJson(response);
                        WriteToStream(stream, json);
                    }
                }
                else if (request.Method == "echo")
                {
                    response.Body = request.Body;
                    var json = ToJson(response);
                    WriteToStream(stream, json);
                    Console.WriteLine("Body test: " + response.Body);
                }
                else if (request.Method == "read" && !regexItemForID.IsMatch(request.Path)) // should match on if there is numbers and /
                {
                    Console.WriteLine(  "Called read and categories, no id");
                    response.Status = "4 Bad Request";

                    var json = ToJson(response);
                    WriteToStream(stream, json);
                }
                if (request.Method == "read" && regexItemForID.IsMatch(request.Path)) //If path is not xxxx/number
                {
                    response.Status = "1 Ok";
                    response.Body = category.GetCategories();

                    var json = ToJson(response);
                    WriteToStream(stream, json);
                }
                if (request.Method == "read" && regexItemForID.IsMatch(request.Path))
                {
                    string[] pathArray = request.Path.Split('/');
                    string value = pathArray[^1]; // "1"
                    Console.WriteLine(value);


                    response.Status = "1 Ok";
                    response.Body = category.GetCategoryByID(Int32.Parse(value));

                    var json = ToJson(response);
                    WriteToStream(stream, json);
                }
                if (request.Method == "create" && (request.Path != "/api/categories" || request.Path != "/api/categories/"))
                {
                    response.Status = "4 Bad Request";

                    var json = ToJson(response);
                    WriteToStream(stream, json);
                }

                if (validMethodsRequireID.Contains(request.Method))
                {
                    int id = -1;
                    if (!regexItemForID.IsMatch(request.Path))
                    {
                        response.Status = "4 Bad Request";

                        var json = ToJson(response);
                        WriteToStream(stream, json);
                    }
                    else
                    {
                        id = Int32.Parse(request.Path);
                        // send this to the category dict
                    }
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


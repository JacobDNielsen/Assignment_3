using System.Net.Sockets;
using System.Net;
using System.Text;
using System.Text.Json;
using System.Text.RegularExpressions;
using static System.Runtime.InteropServices.JavaScript.JSType;

public class Server
{
    CategoryList category = new CategoryList();
    Regex regexOnlyDigit = new Regex("^[0-9]*$");
    Regex regexItemForID = new Regex(@"/(\d+)$");
    Regex regexValidPathReadAll = new Regex(@"^/api/categories/?$");
    Regex regexDigitAtEndOfString = new Regex("[0-9]*$");

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

            var response = new Response();
            var request = FromJson(msg);
            string[] validMethods = ["create", "read", "update", "delete", "echo"];

            if (request == null)
            {
                response.Status = "Request was in bad format";
                var json = ToJson(response);
                WriteToStream(stream, json);
                return;
            }

            bool failed = false;

            if (request.Date == null)
            {
                response.Status += "missing date";
                failed = true;
            }
            if (request.Method == null)
            {
                response.Status += "missing method";
                failed = true;
            }
            else if (!validMethods.Contains(request.Method))
            {
                response.Status += "illegal method";
                failed = true;
            }

            if (failed)
            {
                var json = ToJson(response);
                WriteToStream(stream, json);
                return;
            }

            // can collect similar ones and easily see which ones trigger what response
            switch (request)
            {
                case { Method: "read", Date: string, Path: null }:
                case { Method: "update", Date: string, Path: null }:
                case { Method: "create", Date: string, Path: null }:
                case { Method: "delete", Date: string, Path: null }:
                    WriteToStream(stream, MissingResource(response));
                    break;
                case { Method: "echo", Date: string, Path: string, Body: null }:
                case { Method: "create", Date: string, Path: string, Body: null }:
                case { Method: "update", Date: string, Path: string, Body: null }:
                    WriteToStream(stream, MissingBody(response));
                    break;
            }

            // switch for each crud method + echo
            switch (request.Method)
            {
                case "create":
                    WriteToStream(stream, ProccessCreateRequest(request, response));
                    break;
                case "read":
                    WriteToStream(stream, ProccessReadRequest(request, response));
                    break;
                case "update":
                    WriteToStream(stream, ProccessUpdateRequest(request, response));
                    break;
                case "delete":
                    WriteToStream(stream, ProccessDeleteRequest(request, response));
                    break;
                case "echo":
                    WriteToStream(stream, ProccessEchoRequest(request, response));
                    break;
            }
        }
        catch { }
    }

    // CREATE 
    public string ProccessCreateRequest(Request request, Response response)
    {
        //if (!HasPath(request)) return MissingResource(response);
        //if (!HasBody(request)) return MissingBody(response);

        if (regexItemForID.IsMatch(request.Path)) return BadRequest(response);

        if (HasBody(request))
        {
            Category cat = FromJsonC(request.Body);
            if (category.CreateCategory(cat))
            {
                response.Body = ToJson(cat);
                return Ok(response);
            }
            else
            {
                response.Status = "Could not create new category";
                return ToJson(response);
            }
        }
        return ToJson(response);
    }
    // READ
    public string ProccessReadRequest(Request request, Response response)
    {
        //if (!HasPath(request)) return MissingResource(response);

        if (regexValidPathReadAll.IsMatch(request.Path))
        {
            response.Body = category.GetCategories();
            return Ok(response);
        }
        if (regexDigitAtEndOfString.IsMatch(request.Path))
        {
            string[] pathArray = request.Path.Split('/');
            int value = 0;
            try
            {
                value = Int32.Parse(pathArray[^1]);
            }
            catch
            {
                return BadRequest(response);
            }

            if (category.GetCategoryCount() > value)
            {
                response.Body = category.GetCategoryByID(value);
                return Ok(response);
            }
            else
                return NotFound(response);
        }
        if (!regexDigitAtEndOfString.IsMatch(request.Path))
        {
            return BadRequest(response);
        }
        return ToJson(response);
    }
    // UPDATE
    public string ProccessUpdateRequest(Request request, Response response)
    {
        //if (!HasPath(request)) return MissingResource(response);
        //if (!HasBody(request)) return MissingBody(response);

        if (!regexOnlyDigit.IsMatch(request.Date.ToString()))
        {
            response.Status += "illegal date";
            return ToJson(response);
        }
        try
        {
            JsonDocument.Parse(request.Body);
        }
        catch
        {
            response.Status += "illegal body,";
            return ToJson(response);
        }
        if (HasPath(request) && regexItemForID.IsMatch(request.Path))
        {
            Category cat = FromJsonC(request.Body);

            if (category.UpdateCategoryById(PathToInt(request.Path), cat))
            {
                response.Status = "3 updated";
                return ToJson(response);
            }
            else return NotFound(response);
        }
        else
            return BadRequest(response);
    }
    // DELETE
    public string ProccessDeleteRequest(Request request, Response response)
    {
        //if (!HasPath(request)) return MissingResource(response);

        if (!regexItemForID.IsMatch(request.Path)) return BadRequest(response);

        if (HasPath(request) && regexItemForID.IsMatch(request.Path))
        {
            if (category.DeleteCategory(PathToInt(request.Path))) return Ok(response);
            else
                return NotFound(response);
        }
        return ToJson(response);
    }
    // ECHO 
    public string ProccessEchoRequest(Request request, Response response)
    {
        //if (!HasBody(request)) return MissingBody(response);
        response.Body = request.Body;
        return ToJson(response);
    }

    public bool HasPath(Request request) { return request.Path != null; }

    public bool HasBody(Request request) { return request.Body != null; }
    public string Ok(Response response)
    {
        response.Status = "1 Ok";
        return ToJson(response);
    }
    public string BadRequest(Response response)
    {
        response.Status = "4 bad request";
        return ToJson(response);
    }
    public string NotFound(Response response)
    {
        response.Status = "5 not found";
        return ToJson(response);
    }
    public string MissingResource(Response response)
    {
        response.Status = "missing resource";
        return ToJson(response);
    }
    public string MissingBody(Response response)
    {
        response.Status = "missing body";
        return ToJson(response);
    }

    private string ReadFromStream(NetworkStream stream)
    {
        // Hvis der fx. er 10 bytes, så læser vi kun de 10 bytes. Vi tager altså fra pos0 til readcount. 
        // Hvis vi bare gjorde: return Encoding.UTF8.GetString(buffer);, så vil vi tage hele bufferen med og sætte den til en string
        var buffer = new byte[1024];
        var readCount = stream.Read(buffer);
        return Encoding.UTF8.GetString(buffer, 0, readCount);
    }

    private void WriteToStream(NetworkStream stream, string msg)
    {
        var buffer = Encoding.UTF8.GetBytes(msg);
        stream.Write(buffer);
    }

    public static string ToJson(object response)
    {
        return JsonSerializer.Serialize(response, new JsonSerializerOptions { PropertyNamingPolicy = JsonNamingPolicy.CamelCase });
    }

    public static Request? FromJson(string element)
    {
        return JsonSerializer.Deserialize<Request>(element, new JsonSerializerOptions { PropertyNamingPolicy = JsonNamingPolicy.CamelCase });
    }
    public static Category? FromJsonC(string element)
    {
        return JsonSerializer.Deserialize<Category>(element, new JsonSerializerOptions { PropertyNamingPolicy = JsonNamingPolicy.CamelCase });
    }

    public int PathToInt(string path)
    {
        return Int32.Parse(regexDigitAtEndOfString.Match(path).Value);
    }
}

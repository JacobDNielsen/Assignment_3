using System.Net.Sockets;
using System.Net;
using System.Text;
using System.Text.Json;
using System.Text.RegularExpressions;

public class Server
{
    CategoryList category = new CategoryList();

    Regex regexValidDate = new Regex("^[0-9]*$"); //Checks that string is only numbers. String can contain 0 to many numbers.
    Regex regexDigitAtEndOfString = new Regex(@"(?<=/)(\d+)$"); //Finds all digits after the last / in a string. 
    Regex regexValidPathReadAll = new Regex(@"^/api/categories/?$"); //Checks that string is exactly /api/categories or /api/categories/

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

            Task.Run(() => HandleClient(client)); // Queues up each client and run HandleClient on each client
        }
    }
    private void HandleClient(TcpClient client)
    {
        try //Try-Catch to catch any exceptions that might occur during the handling of the client
        {
            var stream = client.GetStream();
            string msg = ReadFromStream(stream);

            Console.WriteLine("Message from client: " + msg);

            var response = new Response();
            var request = FromJson<Request>(msg);

            if (IsValidRequest(request, response))
            {
                var jsonResponse = ToJson(response);
                WriteToStream(stream, jsonResponse);
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
                    return;
                case { Method: "echo", Date: string, Path: string, Body: null }:
                case { Method: "create", Date: string, Path: string, Body: null }:
                case { Method: "update", Date: string, Path: string, Body: null }:
                    WriteToStream(stream, MissingBody(response));
                    return; // return instead of break, don't need to run the other parts of the code once we write to stream
            }

            // switch for each crud method + echo
            switch (request.Method)
            {
                case "create":
                    WriteToStream(stream, ProccessCreateRequest(request, response));
                    return;
                case "read":
                    WriteToStream(stream, ProccessReadRequest(request, response));
                    return;
                case "update":
                    WriteToStream(stream, ProccessUpdateRequest(request, response));
                    return;
                case "delete":
                    WriteToStream(stream, ProccessDeleteRequest(request, response));
                    return;
                case "echo":
                    WriteToStream(stream, ProccessEchoRequest(request, response));
                    return;
            }
        }
        catch { }
    }

    // Method to check if request is valid. Sets response status and marks bool value accordingly.
    public bool IsValidRequest(Request request, Response response)
    {
        bool validRequest = false;
        string[] validMethods = ["create", "read", "update", "delete", "echo"];

        if (request == null)
        {
            response.Status = "Request was in bad format";
            return true;
        }

        if (request.Date == null)
        {
            response.Status += "missing date";
            validRequest = true;
        }
        if (request.Method == null)
        {
            response.Status += "missing method";
            validRequest = true;
        }
        else if (!validMethods.Contains(request.Method))
        {
            response.Status += "illegal method";
            validRequest = true;
        }
        return validRequest;
    }

    // CREATE 
    public string ProccessCreateRequest(Request request, Response response)
    {

        if (regexDigitAtEndOfString.IsMatch(request.Path)) return BadRequest(response);

        if (HasBody(request))
        {
            Category cat = FromJson<Category>(request.Body);
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

        if (regexValidPathReadAll.IsMatch(request.Path))
        {
            response.Body = category.GetCategories();
            return Ok(response);
        }
        if (regexDigitAtEndOfString.IsMatch(request.Path))
        {
            int value = 0;
            try
            {
                value = PathToInt(request.Path);
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

        if (!regexValidDate.IsMatch(request.Date.ToString()))
        {
            response.Status = "illegal date";
            return ToJson(response);
        }
        try
        {
            JsonDocument.Parse(request.Body);
        }
        catch
        {
            response.Status = "illegal body";
            return ToJson(response);
        }
        if (HasPath(request) && regexDigitAtEndOfString.IsMatch(request.Path))
        {
            Category category = FromJson<Category>(request.Body);

            if (this.category.UpdateCategoryById(PathToInt(request.Path), category))
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

        if (!regexDigitAtEndOfString.IsMatch(request.Path)) return BadRequest(response);

        if (HasPath(request) && regexDigitAtEndOfString.IsMatch(request.Path))
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
        response.Body = request.Body;
        return ToJson(response);
    }

    public bool HasPath(Request request) { return request.Path != null; } //Returns true if request has a path

    public bool HasBody(Request request) { return request.Body != null; } //Same as above, but for body

    //Below methods sets Response.Status to a specific status and returns the response as a JSON string.
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


    //Takes a string (which is the path) and returns the last digit in the string as an integer.
    public int PathToInt(string path)
    {
        try
        {
            return Int32.Parse(regexDigitAtEndOfString.Match(path).Value);
        }
        catch (InvalidCastException)
        {
            throw new Exception();
        }
    }

    private string ReadFromStream(NetworkStream stream)
    {
        var buffer = new byte[1024];
        var readCount = stream.Read(buffer); //readCount is the amount of bytes read from the stream
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

    public static T FromJson<T>(string element)
    {
        return JsonSerializer.Deserialize<T>(element, new JsonSerializerOptions { PropertyNamingPolicy = JsonNamingPolicy.CamelCase });
    }


}

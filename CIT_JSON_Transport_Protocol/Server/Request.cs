public class Request
{
    public string Method { get; set; }
    public string? Path { get; set; } //added posibility for null value
    public string Date { get; set; }
    public string Body { get; set; }
}
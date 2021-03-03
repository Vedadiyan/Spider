using System.Net;

namespace Spider.WebAPI.Abstraction
{
    public interface IActionResult
    {
        string ContentType { get; }
        HttpStatusCode StatusCode { get; }
        string Response();
    }
}
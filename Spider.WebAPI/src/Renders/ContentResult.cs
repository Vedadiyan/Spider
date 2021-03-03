using System;
using System.Net;
using Spider.WebAPI.Abstraction;

namespace Spider.WebAPI.Renders
{
    public class ContentResult : IActionResult
    {
        public string ContentType {get;}

        public HttpStatusCode StatusCode { get; set; }
        private readonly string content;
        public ContentResult(HttpStatusCode statusCode, string contentType, string content) {
            StatusCode = statusCode;
            ContentType = contentType;
            this.content = content;
        }
        public string Response()
        {
            return content;
        }
    }
}
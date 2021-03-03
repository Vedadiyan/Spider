using System;
using System.Net;
using Spider.WebAPI.Abstraction;

namespace Spider.WebAPI.Renders
{
    public class RedirectResult : IActionResult
    {
        public string ContentType => "";

        public HttpStatusCode StatusCode { get; set; }
        private readonly string url;
        public RedirectResult(string url) {
            StatusCode = HttpStatusCode.Redirect;
            this.url = url;
        }
        public string Response()
        {
            return url;
        }
    }
}
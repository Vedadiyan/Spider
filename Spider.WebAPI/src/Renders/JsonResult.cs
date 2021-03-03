using System;
using System.Net;
using Spider.WebAPI.Abstraction;

namespace Spider.WebAPI.Renders
{
    public class JsonResult : IActionResult
    {
        public string ContentType => "application/json";

        public HttpStatusCode StatusCode { get; set; }
        private readonly string jsonString;
        public JsonResult(string jsonString) {
            StatusCode = HttpStatusCode.OK;
            this.jsonString = jsonString;
        }
        public string Response()
        {
            return jsonString;
        }
    }
}
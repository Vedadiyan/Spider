using System;
using System.IO;
using System.Net;
using System.Threading.Tasks;
using Spider.ArcheType;

namespace Spider.WebAPI.Handlers
{
    public class ResponseHandler : IResponse
    {
        public HttpStatusCode HttpStatusCode { get; private set; }
        private WebHeaderCollection webHeaderCollection;
        public WebHeaderCollection WebHeaderCollection {
            get {
                return webHeaderCollection;
            }
            set {
                if(webHeaderCollection == null) {
                    webHeaderCollection = value;
                }
            }
        }

        public string ContentType { get; private set; }

        private readonly String response;

        public ResponseHandler(HttpStatusCode httpStatusCode, String contentType, String response)
        {
            HttpStatusCode = httpStatusCode;
            ContentType = contentType;
            if (HttpStatusCode != HttpStatusCode.Redirect)
            {
                this.response = response;
            }
            else {
                WebHeaderCollection = new WebHeaderCollection();
                WebHeaderCollection.Add("Location", response);
            }
        }

        public Stream RenderResponse()
        {
            StreamWriter streamWriter = new StreamWriter(new MemoryStream());
            streamWriter.Write(response);
            streamWriter.Flush();
            streamWriter.BaseStream.Seek(0, SeekOrigin.Begin);
            return streamWriter.BaseStream;
        }

        public async Task<Stream> RenderResponseAsync()
        {
            StreamWriter streamWriter = new StreamWriter(new MemoryStream());
            streamWriter.Write(response);
            await streamWriter.FlushAsync();
            streamWriter.BaseStream.Seek(0, SeekOrigin.Begin);
            return streamWriter.BaseStream;
        }
    }
}
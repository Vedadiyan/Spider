using System.IO;
using System.Net;
using System.Threading.Tasks;

namespace Spider.ArcheType
{
    public interface IResponse
    {
        HttpStatusCode HttpStatusCode { get; }
        WebHeaderCollection WebHeaderCollection { get; }
        string ContentType { get; }
        Task<Stream> RenderResponseAsync();
        Stream RenderResponse();
    }
}
using System;
using System.Net;
using System.Threading.Tasks;

namespace Spider.ArcheType {
    public interface IMiddleware {
        ValueTask<bool> OnRequest(IRequestContext context);
        ValueTask<bool> OnResponse(IResponse response);
        ValueTask<bool> OnError(HttpStatusCode statusCode, Exception exception);
    }
}
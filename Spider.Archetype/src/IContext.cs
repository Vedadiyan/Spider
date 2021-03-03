using System.Collections.Generic;
using System.IO;
using System.Net;

namespace Spider.ArcheType
{
    public interface IContext
    {
        Dictionary<string, object> Query { get; }
        Dictionary<string, object> RouteValues { get; }
        Stream Body { get; }
        bool BodyAvailable { get; }
        WebHeaderCollection Headers { get; }
        string Method { get; }
        bool IsWebSocket { get; }
        IWebSocket WebSocket { get; }
        IPEndPoint RemoteEndPoint { get; }
    }
}
using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Threading.Tasks;

namespace Spider.ArcheType
{
    public interface IRequestContext
    {
        IReadOnlyDictionary<string, object> Query { get; }
        IReadOnlyDictionary<string, object> RouteValues { get; }
        Stream Body { get; }
        bool BodyAvailable { get; }
        WebHeaderCollection Headers { get; }
        string Method { get; }
        bool IsWebSocket { get; }
        IWebSocket WebSocket { get; }
        IPEndPoint RemoteEndPoint { get; }
        string AbsolutePath { get; }
        Dictionary<string, object> Claims { get; }
        bool IsSecureConnection { get; }
        string UserHostAddress { get; }
        string UserAgent { get; }
        Uri Url { get; }
        TransportContext TransportContext { get; }
        string ServiceName { get; }
        Guid RequestTraceIdentifier { get; }
        string RawUrl { get; }
        Version ProtocolVersion { get; }
        bool KeepAlive { get; }
        bool IsWebSocketRequest { get; }
        string UserHostName { get; }
        string[] UserLanguages { get; }
        bool IsAuthenticated { get; }
        CookieCollection Cookies { get; }
        string ContentType { get; }
        long ContentLength64 { get; }
        Encoding ContentEncoding { get; }
        int ClientCertificateError { get; }
        string[] AcceptTypes { get; }
        bool IsLocal { get; }
        Task<X509Certificate2> GetClientCertificateAsync();
    }
}
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Threading.Tasks;
using Spider.ArcheType;
using Spider.Routing;

namespace Spider.Core.Web
{
    public class RequestContext : IRequestContext
    {
        private Dictionary<string, object> query { get; }
        private Dictionary<string, object> routeValues { get; }
        public Stream Body { get; }
        public WebHeaderCollection Headers { get; }
        public string Method { get; }
        public bool IsWebSocket { get; }
        public IWebSocket WebSocket { get; }
        public IPEndPoint RemoteEndPoint { get; }
        public bool BodyAvailable { get; }
        public string AbsolutePath { get; }
        public Dictionary<string, object> Claims { get; }
        public IReadOnlyDictionary<string, object> Query => query;
        public IReadOnlyDictionary<string, object> RouteValues => routeValues;
        public bool IsSecureConnection { get; }
        public string UserHostAddress { get; }
        public string UserAgent { get; }
        public Uri Url { get; }
        public TransportContext TransportContext { get; }
        public string ServiceName { get; }
        public Guid RequestTraceIdentifier { get; }
        public string RawUrl { get; }
        public Version ProtocolVersion { get; }
        public bool KeepAlive { get; }
        public bool IsWebSocketRequest { get; }
        public string UserHostName { get; }
        public string[] UserLanguages { get; }
        public bool IsAuthenticated { get; }
        public CookieCollection Cookies { get; }
        public string ContentType { get; }
        public long ContentLength64 { get; }
        public Encoding ContentEncoding { get; }
        public int ClientCertificateError { get; }
        public string[] AcceptTypes { get; }
        public bool IsLocal { get; }
        private readonly HttpListenerContext httpListenerContext;
        public RequestContext(Route route, HttpListenerContext httpListenerContext, bool strictRouting)
        {
            this.httpListenerContext = httpListenerContext;
            this.routeValues = new Dictionary<string, object>();
            Claims = new Dictionary<string, object>();
            string[] routeValues = httpListenerContext.Request.Url.AbsolutePath.TrimStart('/').Split('/');
            for (int i = 0; i < routeValues.Length; i++)
            {
                Parameter parameter = route.Parameters[i];
                if (parameter.ReadOnly)
                {
                    this.routeValues.Add(parameter.Alias ?? parameter.Name, routeValues[i]);
                }
                else
                {
                    this.routeValues.Add(parameter.Name, Convert.ChangeType(routeValues[i], parameter.TypeCode));
                }
            }
            query = new Dictionary<string, object>();
            foreach (var key in httpListenerContext.Request.QueryString.AllKeys)
            {
                Parameter queryParameter = route.Query?.FirstOrDefault(x => x.Name.Equals(key, StringComparison.OrdinalIgnoreCase)) ?? default;
                if (queryParameter.Name != null)
                {
                    if (queryParameter.IsArray)
                    {
                        string[] stringValues = httpListenerContext.Request.QueryString[key].Split(',');
                        object[] convertedValues = new object[stringValues.Length];
                        for (int i = 0; i < stringValues.Length; i++)
                        {
                            convertedValues[i] = Convert.ChangeType(stringValues[i], queryParameter.TypeCode);
                        }
                        query.Add(queryParameter.Name, convertedValues);
                    }
                    else
                    {
                        query.Add(queryParameter.Name, Convert.ChangeType(httpListenerContext.Request.QueryString[key], queryParameter.TypeCode));
                    }
                }
                else if (strictRouting)
                {
                    throw new Exception($"Unauthorized Route Parameter {key}");
                }
            }
            MemoryStream body = new MemoryStream();
            httpListenerContext.Request.InputStream.CopyTo(body);
            if (body.Length != 0)
            {
                BodyAvailable = true;
                Body = body;
                Body.Seek(0, SeekOrigin.Begin);
            }
            Method = httpListenerContext.Request.HttpMethod;
            Headers = new WebHeaderCollection();
            foreach (string key in httpListenerContext.Request.Headers.Keys)
            {
                Headers.Add(key, httpListenerContext.Request.Headers[key]);
            }
            IsWebSocket = httpListenerContext.Request.IsWebSocketRequest;
            WebSocket = new WebSocket(httpListenerContext);
            RemoteEndPoint = httpListenerContext.Request.RemoteEndPoint;
            AbsolutePath = httpListenerContext.Request.Url.AbsolutePath.TrimStart('/');
            IsSecureConnection = httpListenerContext.Request.IsSecureConnection;
            UserHostAddress = httpListenerContext.Request.UserHostAddress;
            UserAgent = httpListenerContext.Request.UserAgent;
            Url = httpListenerContext.Request.Url;
            TransportContext = httpListenerContext.Request.TransportContext; 
            ServiceName = httpListenerContext.Request.ServiceName;
            RawUrl = httpListenerContext.Request.RawUrl;
            ProtocolVersion = httpListenerContext.Request.ProtocolVersion;
            KeepAlive = httpListenerContext.Request.KeepAlive;
            IsWebSocketRequest = httpListenerContext.Request.IsWebSocketRequest;
            UserHostName = httpListenerContext.Request.UserHostName;
            UserLanguages = httpListenerContext.Request.UserLanguages;
            IsAuthenticated = httpListenerContext.Request.IsAuthenticated;
            Cookies = httpListenerContext.Request.Cookies;
            ContentType = httpListenerContext.Request.ContentType;
            ContentLength64 = httpListenerContext.Request.ContentLength64;
            ContentEncoding = httpListenerContext.Request.ContentEncoding;
            ClientCertificateError = httpListenerContext.Request.ClientCertificateError;
            AcceptTypes = httpListenerContext.Request.AcceptTypes;
            IsLocal = httpListenerContext.Request.IsLocal;

        }

        public Task<X509Certificate2> GetClientCertificateAsync()
        {
            return httpListenerContext.Request.GetClientCertificateAsync();
        }
    }

}
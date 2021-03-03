using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using Spider.ArcheType;
using Spider.Core.Routing;

namespace Spider.Core.Web
{
    public class Context : IContext
    {
        public Dictionary<string, object> Query { get; }
        public Dictionary<string, object> RouteValues { get; }
        public Stream Body { get; }
        public WebHeaderCollection Headers { get; }
        public string Method { get; }
        public bool IsWebSocket { get; }
        public IWebSocket WebSocket { get; }
        public IPEndPoint RemoteEndPoint { get; }
        public bool BodyAvailable { get; }
        public Context(Route route, HttpListenerContext httpListenerContext)
        {
            RouteValues = new Dictionary<string, object>();
            string[] routeValues = httpListenerContext.Request.Url.AbsolutePath.TrimStart('/').Split('/');
            for (int i = 0; i < routeValues.Length; i++)
            {
                Parameter parameter = route.Parameters[i];
                if (parameter.ReadOnly)
                {
                    RouteValues.Add(parameter.Alias ?? parameter.Name, routeValues[i]);
                }
                else
                {
                    RouteValues.Add(parameter.Name, Convert.ChangeType(routeValues[i], parameter.TypeCode));
                }
            }
            Query = new Dictionary<string, object>();
            foreach (var key in httpListenerContext.Request.QueryString.AllKeys)
            {
                Parameter queryParameter = route.Query.FirstOrDefault(x => x.Name.Equals(key, StringComparison.OrdinalIgnoreCase));
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
                        Query.Add(queryParameter.Name, convertedValues);
                    }
                    else
                    {
                        Query.Add(queryParameter.Name, Convert.ChangeType(httpListenerContext.Request.QueryString[key], queryParameter.TypeCode));
                    }
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
        }
    }

}
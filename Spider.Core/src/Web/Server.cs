using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Threading;
using System.Threading.Tasks;
using Spider.ArcheType;
using Spider.Core.Routing;

namespace Spider.Core.Web
{
    public class WebServer
    {
        private HttpListener httpListener;
        private bool isRunning;
        private List<IMiddleware> middleware;
        private bool cors_Enabled;
        private string cors_AllowedHeaders;
        private string cors_AccessOrigin;
        private string cores_Methods;
        private bool detailedError_Enable;
        public WebServer(string[] prefixes)
        {
            middleware = new List<IMiddleware>();
            httpListener = new HttpListener();
            foreach (var prefix in prefixes)
            {
                httpListener.Prefixes.Add(prefix);
            }
        }
        public WebServer UseMiddleware(IMiddleware middleware)
        {
            this.middleware.Add(middleware);
            return this;
        }
        public WebServer EnableCORS(string accessOrigin = null, string allowedHeaders = null, string methods = null)
        {
            cors_Enabled = true;
            cors_AccessOrigin = accessOrigin;
            cors_AllowedHeaders = allowedHeaders;
            cores_Methods = methods;
            return this;
        }
        public WebServer EnableDetailedError()
        {
            detailedError_Enable = true;
            return this;
        }
        public async Task Start(CancellationToken cancellationToken)
        {
            if (!isRunning)
            {
                cancellationToken.Register(() =>
                {
                    isRunning = false;
                    httpListener.Stop();
                    Console.WriteLine("Spider is Shutting Down");
                });
                isRunning = true;
                httpListener.Start();
                Console.WriteLine("Spider is Listening to: {0}", string.Join(";", httpListener.Prefixes));
                while (isRunning)
                {
                    try
                    {
                        HttpListenerContext httpListenerContext = await httpListener.GetContextAsync();
                        handleRequest(httpListenerContext);
                    }
                    catch
                    {
                        continue;
                    }
                }
            }
        }
        private void handleRequest(HttpListenerContext httpListenerContext)
        {
            Task.Run(async () =>
            {
                httpListenerContext.Response.AddHeader("Server", "Spider Integrated Web Server");
                httpListenerContext.Response.AddHeader("X-Powered-By", "Spider Platform by Centaurus ESS");
                if (cors_Enabled)
                {
                    httpListenerContext.Response.AddHeader("Access-Control-Allow-Origin", cors_AccessOrigin ?? "*");
                    httpListenerContext.Response.AddHeader("Access-Control-Allow-Headers", cors_AllowedHeaders ?? "Content-Type, Access-Control-Allow-Headers, Authorization, X-Requested-With");
                    httpListenerContext.Response.AddHeader("Access-Control-Allow-Methods", cores_Methods ?? "HEAD, GET, PUT, POST, DELETE");
                }
                try
                {
                    #region DEBUG
                    var test = Router.Routes;
                    #endregion
                    Route route = Router.GetRoute(httpListenerContext.Request.HttpMethod, httpListenerContext.Request.Url.AbsolutePath.TrimStart('/'));
                    if (route.Hash != null)
                    {
                        IContext context = new Context(route, httpListenerContext);
                        if (middleware.Count > 0)
                        {
                            foreach (var _middleware in middleware)
                            {
                                if (!(await _middleware.OnRequest(context)))
                                {
                                    return;
                                }
                            }
                        }
                        IResponse response = await route.RequestHandler.HandleRequest(context);
                        if (response != null)
                        {
                            if (response.HttpStatusCode != HttpStatusCode.Redirect)
                            {
                                httpListenerContext.Response.ContentType = response.ContentType;
                                httpListenerContext.Response.StatusCode = (int)response.HttpStatusCode;
                                if (response.WebHeaderCollection != null)
                                {
                                    foreach (string i in response.WebHeaderCollection)
                                    {
                                        httpListenerContext.Response.Headers[i] = response.WebHeaderCollection[i];
                                    }
                                }
                                if (!httpListenerContext.Request.IsWebSocketRequest)
                                {
                                    await (await response.RenderResponseAsync()).CopyToAsync(httpListenerContext.Response.OutputStream);
                                    httpListenerContext.Response.Close();
                                    if (middleware.Count > 0)
                                    {
                                        foreach (var _middleware in middleware)
                                        {
                                            if (!(await _middleware.OnResponse(response)))
                                            {
                                                break;
                                            }
                                        }
                                    }
                                }
                            }
                            else
                            {
                                httpListenerContext.Response.StatusCode = 302;
                                httpListenerContext.Response.Headers.Add("Location", response.WebHeaderCollection["Location"]);
                                httpListenerContext.Response.Close();
                            }
                        }
                    }
                    else
                    {
                        httpListenerContext.Response.StatusCode = (int)HttpStatusCode.NotFound;
                        httpListenerContext.Response.Close();
                        if (middleware.Count > 0)
                        {
                            foreach (var _middleware in middleware)
                            {
                                if (!(await _middleware.OnError(HttpStatusCode.NoContent, null)))
                                {
                                    break;
                                }
                            }
                        }
                    }
                }
                catch (Exception ex)
                {
                    httpListenerContext.Response.StatusCode = (int)HttpStatusCode.InternalServerError;
                    using (StreamWriter streamWriter = new StreamWriter(httpListenerContext.Response.OutputStream))
                    {
                        if (detailedError_Enable)
                        {
                            await streamWriter.WriteLineAsync($"Internal Server Error: {ex.Message}");
                        }
                        else
                        {
                            await streamWriter.WriteLineAsync("Spider was unable to handle this request. Further information cannot be disclosed.");
                        }
                        Console.WriteLine("Error: {0}\r\n\t{1}", ex.Message, ex.StackTrace);
                    }
                    httpListenerContext.Response.Close();
                    if (middleware.Count > 0)
                    {
                        foreach (var _middleware in middleware)
                        {
                            if (!(await _middleware.OnError(HttpStatusCode.InternalServerError, ex)))
                            {
                                break;
                            }
                        }
                    }
                }
            });
        }
    }
}
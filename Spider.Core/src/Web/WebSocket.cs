using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;
using System.Net;
using System.Net.WebSockets;
using System.Security.Principal;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Spider.ArcheType;

namespace Spider.Core.Web
{
    public class WebSocket : IWebSocket
    {
        public event EventHandler<MessageEventArgs> Message;
        public event EventHandler<EventArgs> Close;
        public NameValueCollection Headers { get; private set; }
        public CookieCollection Cookies { get; private set; }
        public bool IsAuthenticated { get; private set; }
        public bool IsLocal { get; private set; }
        public bool IsSecureConnection { get; private set; }
        public string Origin { get; private set; }
        public Uri RequestUri { get; private set; }
        public string SecWebSocketKey { get; private set; }
        public IEnumerable<string> SecWebSocketProtocols { get; private set; }
        public IPrincipal User { get; private set; }
        private string SecWebSocketVersion;
        private HttpListenerContext httpListenerContext;
        private HttpListenerWebSocketContext webSocketContext;
        private SemaphoreSlim semaphoreSlim;
        private Timer socketWatcher = null;
        public bool Closed { get; private set; }
        public WebSocket(HttpListenerContext httpListenerContext)
        {
            this.httpListenerContext = httpListenerContext;
            semaphoreSlim = new SemaphoreSlim(1);
        }

        public async Task AcceptAsync(string protocol, int bufferSize, TimeSpan keepAliveInterval)
        {
            webSocketContext = await this.httpListenerContext.AcceptWebSocketAsync(protocol, bufferSize, keepAliveInterval);
            Headers = webSocketContext.Headers;
            Cookies = webSocketContext.CookieCollection;
            IsAuthenticated = webSocketContext.IsAuthenticated;
            IsLocal = webSocketContext.IsLocal;
            IsSecureConnection = webSocketContext.IsSecureConnection;
            Origin = webSocketContext.Origin;
            RequestUri = webSocketContext.RequestUri;
            SecWebSocketKey = webSocketContext.SecWebSocketKey;
            SecWebSocketProtocols = webSocketContext.SecWebSocketProtocols;
            SecWebSocketVersion = webSocketContext.SecWebSocketVersion;
            User = webSocketContext.User;
            // socketWatcher = new Timer(async (state) =>
            // {
            //     var socket = ((System.Net.WebSockets.WebSocket)state);
            //     switch (socket.State)
            //     {
            //         case WebSocketState.Aborted:
            //         case WebSocketState.Closed:
            //         case WebSocketState.CloseReceived:
            //         case WebSocketState.CloseSent:
            //             Close?.Invoke(this, new EventArgs());
            //             socketWatcher.Change(Timeout.Infinite, Timeout.Infinite);
            //             return;
            //     }
            //     try
            //     {
            //         await semaphoreSlim.WaitAsync();
            //         await socket.SendAsync(new ArraySegment<byte>(new byte[] { 0, 0, 0 }), WebSocketMessageType.Binary, true, new CancellationToken());
            //         semaphoreSlim.Release();
            //     }
            //     catch (WebSocketException ex)
            //     {
            //         if (!checkWebSocketState(ex.WebSocketErrorCode))
            //         {
            //             return;
            //         }
            //     }
            // }, webSocketContext.WebSocket, 0, (int)keepAliveInterval.TotalMilliseconds);
        }

        public async Task PushAsync(byte[] data, CancellationToken cancellationToken)
        {
            await semaphoreSlim.WaitAsync();
            await webSocketContext.WebSocket.SendAsync(new ArraySegment<byte>(data), WebSocketMessageType.Binary, true, cancellationToken);
            semaphoreSlim.Release();
        }
        public async Task PushAsync(string data, Encoding encoding, CancellationToken cancellationToken)
        {
            await semaphoreSlim.WaitAsync();
            await webSocketContext.WebSocket.SendAsync(new ArraySegment<byte>(encoding.GetBytes(data)), WebSocketMessageType.Text, true, cancellationToken);
            semaphoreSlim.Release();
        }
        public Task RecieveAsync(int bufferSize, CancellationToken cancellationToken)
        {
            return Task.Run(async () =>
            {
                List<IEnumerable<byte>> message = new List<IEnumerable<byte>>();
                while (!cancellationToken.IsCancellationRequested && !Closed)
                {
                    try
                    {
                        var buffer = new ArraySegment<byte>(new byte[bufferSize]);
                        var result = await webSocketContext.WebSocket.ReceiveAsync(buffer, cancellationToken);
                        if (result != null)
                        {
                            if (result.EndOfMessage)
                            {
                                message.Add(buffer.Array.Take(result.Count));
                                Message?.Invoke(this, new MessageEventArgs(this, message));
                                message.Clear();
                            }
                            else
                            {
                                message.Add(buffer.Array.Take(result.Count));
                            }
                        }
                    }
                    catch (WebSocketException ex)
                    {
                        if (!checkWebSocketState(ex.WebSocketErrorCode))
                        {
                            return;
                        }
                    }
                    catch (Exception ex)
                    {
                        Close?.Invoke(this, new EventArgs());
                        Closed = true;
                        return;
                    }
                }
            }, cancellationToken);
        }
        public async Task CloseAsync(int closeStatus, string description, CancellationToken cancellationToken)
        {
            await webSocketContext.WebSocket.CloseAsync((WebSocketCloseStatus)closeStatus, description, cancellationToken);
            //socketWatcher.Change(Timeout.Infinite, Timeout.Infinite);
            Closed = true;
            Close?.Invoke(this, new EventArgs());
        }
        private bool checkWebSocketState(WebSocketError webSocketError)
        {
            switch (webSocketError)
            {
                case WebSocketError.ConnectionClosedPrematurely:
                case WebSocketError.Faulted:
                case WebSocketError.InvalidState:
                case WebSocketError.NativeError:
                case WebSocketError.NotAWebSocket:
                case WebSocketError.UnsupportedProtocol:
                case WebSocketError.UnsupportedVersion:
                    Close?.Invoke(this, new EventArgs());
                    socketWatcher?.Change(Timeout.Infinite, Timeout.Infinite);
                    Closed = true;
                    return false;
            }
            return true;
        }
    }
}
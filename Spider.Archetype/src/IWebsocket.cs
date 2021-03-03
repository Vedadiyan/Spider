using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;
using System.Net;
using System.Security.Principal;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Spider.ArcheType
{
    public interface IWebSocket
    {
        event EventHandler<MessageEventArgs> Message;
        event EventHandler<EventArgs> Close;
        NameValueCollection Headers { get; }
        CookieCollection Cookies { get; }
        bool IsAuthenticated { get; }
        bool IsLocal { get; }
        bool IsSecureConnection { get; }
        string Origin { get; }
        Uri RequestUri { get; }
        string SecWebSocketKey { get; }
        IEnumerable<string> SecWebSocketProtocols { get; }
        IPrincipal User { get; }
        Task AcceptAsync(string protocol, int bufferSize, TimeSpan keepAliveInterval);
        Task PushAsync(byte[] data, CancellationToken cancellationToken);
        Task PushAsync(string data, Encoding encoding, CancellationToken cancellationToken);
        Task RecieveAsync(int bufferSize, CancellationToken cancellationToken);
        Task CloseAsync(int closeStatus, string description, CancellationToken cancellationToken);
    }
    public class MessageEventArgs : EventArgs
    {
        public IEnumerable<byte> MessageBytes { get; }
        private List<IEnumerable<byte>> message;
        private IWebSocket webSocket;
        public MessageEventArgs(IWebSocket webSocket, List<IEnumerable<byte>> message)
        {
            var messageBytes = new List<byte>();
            this.message = message;
            this.webSocket = webSocket;
            foreach (var i in message)
            {
                messageBytes.AddRange(i);
            }
            MessageBytes = messageBytes;
        }
        public string ReadMessage(Encoding encoding)
        {
            StringBuilder sb = new StringBuilder();
            for (int i = 0; i < message.Count; i++)
            {
                string value = encoding.GetString(message[i].ToArray());
                sb.Append(value);
            }
            return sb.ToString();
        }
        public Task RespondAsync(byte[] data, CancellationToken cancellationToken) {
            return webSocket.PushAsync(data, cancellationToken);
        }
        public Task RespondAsync(string data, Encoding encoding, CancellationToken cancellationToken) {
            return webSocket.PushAsync(data, encoding, cancellationToken);
        }   
    }
}
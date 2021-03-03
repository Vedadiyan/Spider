using System;
using System.Net;
using System.Threading.Tasks;
using Spider.ArcheType;

namespace Spider.WebAPI.Annotations
{
    public delegate Task<Boolean> ExecutionDelegate(IContext context);
    public abstract class ExecAttribute : Attribute
    {
        public ExecutionScope ExecutionScope { get; }
        public int Order { get; set; }
        public ExecAttribute(ExecutionScope executionScope)
        {
            ExecutionScope = executionScope;
        }
        public abstract Task<Continuation> Run(IContext context);
        public Continuation Next()
        {
            Continuation continuation = new Continuation();
            continuation.Next();
            return continuation;
        }
        public Continuation Cancel()
        {
            Continuation continuation = new Continuation();
            continuation.Cancel();
            return continuation;
        }
        public Continuation Cancel(HttpStatusCode statusCode, String contentType, String content)
        {
            Continuation continuation = new Continuation();
            continuation.Cancel(statusCode, contentType, content);
            return continuation;
        }
        public Continuation Cancel(String url)
        {
            Continuation continuation = new Continuation();
            continuation.Cancel(url);
            return continuation;
        }
    }
    public class Continuation
    {
        internal ContinuationState ContinuationState { get; private set; }
        internal HttpStatusCode StatusCode { get; private set; }
        internal string ContentType { get; private set; }
        internal string Content { get; private set; }
        internal string Url { get; private set; }
        internal Continuation()
        {

        }
        internal void Next()
        {
            ContinuationState = ContinuationState.Continue;
        }
        internal void Cancel()
        {
            ContinuationState = ContinuationState.Cancel;
        }
        internal void Cancel(HttpStatusCode statusCode, string contentType, string content)
        {
            ContinuationState = ContinuationState.CancelWithError;
            StatusCode = statusCode;
            ContentType = contentType;
            Content = content;
        }
        internal void Cancel(string redirectionUrl)
        {
            ContinuationState = ContinuationState.CancelWithRedirection;
            Url = redirectionUrl;
        }
    }
    internal enum ContinuationState
    {
        Continue,
        Cancel,
        CancelWithError,
        CancelWithRedirection
    }
    public enum ExecutionScope
    {
        After,
        Before
    }
}
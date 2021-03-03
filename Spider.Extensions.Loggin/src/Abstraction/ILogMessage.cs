using System;

namespace Spider.Extensions.Logging.Abstraction
{
    public interface ILogMessage<T>
    {
        Type Source { get; }
        string Location { get; }
        string Message { get; }
        DateTime DateTime { get; }
    }
}

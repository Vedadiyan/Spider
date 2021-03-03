using System;

namespace Spider.Extensions.Logging.Abstraction
{
    public interface ILogger
    {
        void Verbose<T>(ILogMessage<T> verbose);
        void Information<T>(ILogMessage<T> information);
        void Debug<T>(ILogMessage<T> debug);
        void Warning<T>(ILogMessage<T> warning, string cause = null);
        void Error<T>(Exception exception, ILogMessage<T> error);
    }
}
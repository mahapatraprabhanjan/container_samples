using System;

namespace Identity.Api.Extensions
{
    public class SelectTryResult<TSource, TResult>
    {
        public TSource Source { get; set; }

        public TResult Result { get; set; }

        public Exception CaughtException { get; set; }

        internal SelectTryResult(TSource source, TResult result, Exception exception)
        {
            Source = source;
            Result = result;
            CaughtException = exception;
        }
    }
}

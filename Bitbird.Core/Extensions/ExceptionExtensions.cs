using System;
using System.Linq;

namespace Bitbird.Core.Extensions
{
    public static class ExceptionExtensions
    {
        public static bool TryGetInnerException<T>(this Exception exc, out T found) where T:Exception
        {
            var e = exc;

            do
            {
                if (e is T t)
                {
                    found = t;
                    return true;
                }

                e = e.InnerException;
            }
            while (e != null);

            found = null;
            return false;
        }
        public static Exception GetMostInnerException(this Exception exc)
        {
            var e = exc;

            while (e?.InnerException != null)
                e = e.InnerException;

            return e;
        }
        public static Exception[] GetAggregatedExceptions(this Exception exc)
        {
            if (!(exc is AggregateException aggExc))
                return new[] {exc};

            return aggExc.InnerExceptions.SelectMany(e => e.GetAggregatedExceptions()).ToArray();
        }
        public static Exception ReplaceApiErrorExceptionsWith(this Exception exc, Func<ApiErrorException, bool> predicate, Func<ApiErrorException, Exception> translationFunc)
        {
            var replaced = exc.GetAggregatedExceptions().Select(e =>
            {
                if (!(e is ApiErrorException aee) || !predicate(aee))
                    return e;

                try
                {
                    throw translationFunc(aee);
                }
                catch (Exception caught)
                {
                    return caught;
                }
            });

            return new AggregateException(replaced);
        }
        public static bool ContainsApiError(this Exception exc, ApiErrorType type)
        {
            return exc.GetAggregatedExceptions().OfType<ApiErrorException>().Any(aee => aee.ApiErrors.Any(apiError => apiError.Type == type));
        }
    }
}

using System;
using System.Threading;

namespace PrimeTracker.Browsers
{
    public partial class PrimeBrowser
    {
        static class SafeExecutor
        {
            public const int MaxAttempts = 5;
            public const int RetryTimeMS = 1000;

            public static T ExecuteFunc<T>(Func<T> func, int maxAttempts = MaxAttempts, int sleepTime = RetryTimeMS)
            {
                T result = default(T);

                int attempt = 0;

                while (attempt < MaxAttempts)
                {
                    try
                    {
                        result = func.Invoke();
                        break;
                    }
                    catch (Exception e)
                    {
                        attempt++;
                        Thread.Sleep(RetryTimeMS);
                    }
                }

                return result;
            }
            public static void ExecuteAction(Action action, string exceptionMessage, int maxAttempts = MaxAttempts, int sleepTime = RetryTimeMS)
            {
                int attempt = 0;

                while (attempt < maxAttempts)
                {
                    try
                    {
                        action.Invoke();
                        return;
                    }
                    catch (Exception e)
                    {
                        attempt++;
                        Thread.Sleep(RetryTimeMS);
                    }
                }

                throw new Exception(exceptionMessage);
            }
        }
    }
}

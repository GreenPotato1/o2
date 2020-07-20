using System;
using System.Threading;
using Com.O2Bionics.Utils.Properties;
using JetBrains.Annotations;
using log4net;

namespace Com.O2Bionics.Utils
{
    public static class RepeatHelper
    {
        public static T RunUntilSuccess<T>(
            [NotNull] this Func<T> func,
            int attempts,
            int sleepMs,
            [NotNull] ILog log,
            bool throwOnLastException = true)
        {
            CheckArgs(func, nameof(func), attempts, sleepMs, log);

            for (var i = 0; i < attempts; ++i)
            {
                if (0 < i)
                    Thread.Sleep(sleepMs);
                try
                {
                    var result = func();
                    return result;
                }
                catch (Exception e)
                {
                    if (throwOnLastException && i == attempts - 1)
                        throw;

                    log.Error($"Attempt {i} to run an action failed.", e);
                }
            }

            return default(T);
        }

        public static void RunUntilSuccess(
            [NotNull] this Action action,
            int attempts,
            int sleepMs,
            [NotNull] ILog log,
            bool throwOnLastException = true)
        {
            CheckArgs(action, nameof(action), attempts, sleepMs, log);

            RunUntilSuccess(
                () =>
                    {
                        action();
                        return false;
                    },
                attempts,
                sleepMs,
                log,
                throwOnLastException);
        }

        [AssertionMethod]
        private static void CheckArgs([NotNull] object o, [NotNull] string objectName, int attempts, int sleepMs, ILog log)
        {
            if (null == o)
                throw new ArgumentNullException(objectName);
            if (attempts <= 0)
                throw new ArgumentException(string.Format(Resources.ArgumentMustBePositive2, nameof(attempts), attempts));
            if (sleepMs <= 0)
                throw new ArgumentException(string.Format(Resources.ArgumentMustBePositive2, nameof(sleepMs), sleepMs));
            if (null == log)
                throw new ArgumentNullException(nameof(log));
        }
    }
}
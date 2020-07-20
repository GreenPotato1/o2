using System;

namespace Com.O2Bionics.Utils
{
    public static class Safe
    {
        public static T Do<T>(Func<T> func, Action<Exception> errorAction = null, T errorResult = default(T))
        {
            try
            {
                return func();
            }
            catch (Exception e)
            {
                errorAction?.Invoke(e);
                return errorResult;
            }
        }

        public static void Do(Action action, Action<Exception> errorAction = null)
        {
            Do(
                () =>
                    {
                        action();
                        return 0;
                    },
                errorAction);
        }
    }
}
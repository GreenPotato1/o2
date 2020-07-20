using System;

namespace Com.O2Bionics.Utils
{
    public interface ITcpServiceClient<out TService> : IDisposable
    {
        void Call(Action<TService> action);

        TResult Call<TResult>(Func<TService, TResult> func);
    }
}
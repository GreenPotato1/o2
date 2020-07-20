using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Com.O2Bionics.Utils;
using JetBrains.Annotations;

namespace Com.O2Bionics.ChatService
{
    public interface ISubscriberCollection<out TService>
    {
        void Load(IDataContext dc);

        void Add(IDataContext dc, Subscriber subscriber);
        void Remove(IDataContext dc, Subscriber subscriber);

        [CanBeNull]
        Task[] Publish(Action<TService> action);

        List<TResult> Call<TResult>(Func<TService, TResult> call);
    }
}
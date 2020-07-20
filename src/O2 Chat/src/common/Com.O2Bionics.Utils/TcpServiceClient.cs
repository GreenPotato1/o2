using System;
using System.ServiceModel;
using Com.O2Bionics.Utils.Network;
using JetBrains.Annotations;

namespace Com.O2Bionics.Utils
{
    public class TcpServiceClient<TService> : ITcpServiceClient<TService>
    {
        protected readonly ChannelFactory<TService> ChannelFactory;
        [CanBeNull] private readonly IHeaderContextFactory m_headerContextFactory;

        public TcpServiceClient(string host, int port, IHeaderContextFactory headerContextFactory = null)
            : this(new ChannelFactory<TService>(BindingFactory.CreateClientBinding(), CreateAddress(host, port)), headerContextFactory)
        {
        }

        protected TcpServiceClient(ChannelFactory<TService> factory, IHeaderContextFactory headerContextFactory = null)
        {
            ChannelFactory = factory;
            m_headerContextFactory = headerContextFactory;
        }

        public void Dispose()
        {
            var disposable = ChannelFactory as IDisposable;
            disposable?.Dispose();
        }

        public TResult Call<TResult>(Func<TService, TResult> func)
        {
            var channel = (IClientChannel)ChannelFactory.CreateChannel();
            var context = m_headerContextFactory?.Create(channel);
            var success = false;
            try
            {
                var result = func((TService)channel);
                channel.Close();
                success = true;
                return result;
            }
            finally
            {
                context?.Dispose();
                if (!success) channel.Abort();
            }
        }

        public void Call(Action<TService> action)
        {
            Call(
                s =>
                    {
                        action(s);
                        return 0;
                    }
            );
        }

        protected static EndpointAddress CreateAddress(string host, int port)
        {
            var uri = new Uri(string.Format("net.tcp://{0}:{1}/{2}", host, port, typeof(TService).Name));
            var address = new EndpointAddress(uri);
            return address;
        }
    }
}
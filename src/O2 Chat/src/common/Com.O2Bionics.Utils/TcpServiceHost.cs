using System;
using System.ServiceModel;
using log4net;

namespace Com.O2Bionics.Utils
{
    public abstract class TcpServiceHost : IDisposable
    {
        protected static readonly ILog Log = LogManager.GetLogger(typeof(TcpServiceHost));

        public abstract void Dispose();
    }

    public class TcpServiceHost<T> : TcpServiceHost where T : class
    {
        private readonly ServiceHost m_host;

        public readonly T Instance;

        public TcpServiceHost(T instance, int port)
        {
            if (instance == null)
                throw new ArgumentNullException("instance");
            if (port <= 0)
                throw new IndexOutOfRangeException("Port value can't be less or equal to 0 but is " + port);

            Instance = instance;
            var instanceTypeName = Instance.GetType().FullName;

            try
            {
                var baseAddress = new Uri(string.Format("net.tcp://localhost:{0}/{1}", port, typeof(T).Name));
                Log.InfoFormat("Open ServiceHost for {0} at {1}", instanceTypeName, baseAddress);
                var binding = BindingFactory.CreateServerBinding();

                m_host = new ServiceHost(Instance);
                m_host.AddServiceEndpoint(typeof(T), binding, baseAddress);
                m_host.Open();
            }
            catch (Exception e)
            {
                Log.Error($"Exception when creating ServiceHost for {instanceTypeName}.", e);
                throw;
            }
        }

        public override void Dispose()
        {
            var typeName = m_host.SingletonInstance.GetType().FullName;
            Log.InfoFormat("Closing ServiceHost for {0}", typeName);
            try
            {
                m_host.Close();
            }
            catch (Exception e)
            {
                Log.Error($"Exception when closing ServiceHost for {typeName}.", e);
            }
        }
    }
}
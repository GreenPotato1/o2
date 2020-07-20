using JetBrains.Annotations;
using Unity;
using Unity.Lifetime;

namespace Com.O2Bionics.Utils
{
    public static class GlobalContainer
    {
        [NotNull]
        private static IUnityContainer m_container = new UnityContainer();

        [NotNull]
        internal static IUnityContainer UnityContainer => m_container;

        public static void RegisterInstance<T>(T x)
        {
            m_container.RegisterInstance(x, new ContainerControlledLifetimeManager());
        }

        public static void RegisterType<T1, T2>(LifetimeManager lm = null) where T2 : T1
        {
            m_container.RegisterType<T1, T2>(lm ?? new ContainerControlledLifetimeManager());
        }

        public static T Resolve<T>()
        {
            return m_container.Resolve<T>();
        }

        public static void Clear()
        {
            var old = m_container;
            m_container = new UnityContainer();
            old.Dispose();
        }
    }
}
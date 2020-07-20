using System;
using JetBrains.Annotations;
using log4net;
using Topshelf;

namespace Com.O2Bionics.Utils
{
    public static class ServiceHelper
    {
        public static void Run<TService>(
            string serviceName,
            string serviceDescription,
            string serviceDisplayName,
            Func<ILog, TService> createService,
            [NotNull] Action setupLogging,
            Action<TService> startService = null,
            Action<TService> stopService = null)
            where TService : IDisposable
        {
            if (setupLogging == null) throw new ArgumentNullException(nameof(setupLogging));

            HostFactory.Run(
                x =>
                    {
                        x.Service<ServiceThreadControl<TService>>(
                            s =>
                                {
                                    s.ConstructUsing(
                                        name =>
                                            {
                                                setupLogging();
                                                return new ServiceThreadControl<TService>(
                                                    createService,
                                                    startService,
                                                    stopService);
                                            });
                                    s.WhenStarted((tc, hc) => tc.Start(hc));
                                    s.WhenStopped((tc, hc) => tc.Stop(hc));
                                });

                        x.RunAsLocalSystem();
                        x.StartAutomatically();
                        x.EnableShutdown();

                        x.SetServiceName(serviceName);
                        x.SetDescription(serviceDescription);
                        x.SetDisplayName(serviceDisplayName);

                        // x.EnableServiceRecovery(rc => rc.RestartService(1));
                    });
        }
    }
}
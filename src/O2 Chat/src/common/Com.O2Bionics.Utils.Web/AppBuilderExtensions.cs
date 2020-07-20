using System;
using System.Threading;
using JetBrains.Annotations;
using Microsoft.Owin.BuilderProperties;
using Owin;

namespace Com.O2Bionics.Utils.Web
{
    public static class AppBuilderExtensions
    {
        public static void OnAppDisposing([NotNull] this IAppBuilder app, [NotNull] Action action)
        {
            var token = new AppProperties(app.Properties).OnAppDisposing;
            if (token != CancellationToken.None) token.Register(action);
        }

        public static void ScheduleDisposing([NotNull] this IAppBuilder app)
        {
            app.OnAppDisposing(() => GlobalContainer.UnityContainer.Dispose());
        }
    }
}
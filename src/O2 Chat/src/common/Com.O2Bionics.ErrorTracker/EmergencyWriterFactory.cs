using System;
using System.IO;
using Com.O2Bionics.Utils;
using JetBrains.Annotations;
using log4net;

namespace Com.O2Bionics.ErrorTracker
{
    public static class EmergencyWriterFactory
    {
        [NotNull]
        public static IEmergencyWriter CreateAndRegister([NotNull] ErrorTrackerSettings settings, [NotNull] string applicationKind)
        {
            if (null == settings)
                throw new ArgumentNullException(nameof(settings));

            var logDirectory = settings.EmergencyLogDirectory;
            if (string.IsNullOrEmpty(logDirectory))
                throw new ArgumentException($"The {nameof(settings.EmergencyLogDirectory)} must be set in the settings file.");

            if (!Directory.Exists(logDirectory))
                Directory.CreateDirectory(logDirectory);
            RequireWritableDirectory(logDirectory, applicationKind);

            var result = Register(logDirectory, applicationKind);
            return result;
        }

        private static void RequireWritableDirectory(string logDirectory, string applicationKind)
        {
            try
            {
                var filename = Path.Combine(logDirectory, $"{applicationKind}.{Guid.NewGuid().ToString()}.Test");
                using (var fs = new FileStream(filename, FileMode.Create))
                {
                    fs.WriteByte(0x54);
                }

                File.Delete(filename);
                return;
            }
            catch (Exception e)
            {
                var error = $"The log directory '{logDirectory}' must be writable.";

                var log = LogManager.GetLogger(typeof(EmergencyWriterFactory));
                log.Error(error, e);
            }

            Environment.Exit(82);
        }

        private static IEmergencyWriter Register(string logDirectory, string applicationKind)
        {
            var result = new EmergencyWriter(logDirectory, applicationKind);
            GlobalContainer.RegisterInstance<IEmergencyWriter>(result);
            return result;
        }
    }
}
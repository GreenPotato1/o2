using System;
using System.Diagnostics;
using System.IO;
using JetBrains.Annotations;

namespace Com.O2Bionics.ErrorTracker
{
    [DebuggerDisplay("{m_directory} {m_applicationKind}")]
    public sealed class EmergencyWriter : IEmergencyWriter
    {
        private readonly string m_directory;
        private readonly string m_applicationKind;

        public EmergencyWriter([NotNull] string directory, [NotNull] string applicationKind)
        {
            if (string.IsNullOrEmpty(directory))
                throw new ArgumentNullException(nameof(directory));
            if (string.IsNullOrEmpty(applicationKind))
                throw new ArgumentNullException(nameof(applicationKind));
            m_directory = directory;
            m_applicationKind = applicationKind;
        }

        public void Report(string contents)
        {
            if (string.IsNullOrEmpty(contents))
                throw new ArgumentNullException(nameof(contents));

            try
            {
                var filename = Path.Combine(m_directory, m_applicationKind + "." + Guid.NewGuid().ToString("N") + ".txt");
                File.WriteAllText(filename, contents);
            }
            catch (Exception e)
            {
                try
                {
                    Trace.TraceError($"{nameof(EmergencyWriter)}: {e}");
                }
                catch
                {
//Ignore
                }
            }
        }
    }
}
using System;
using JetBrains.Annotations;

namespace Com.O2Bionics.Console
{
    public sealed class KibanaDeleteContext
    {
        private const int MaxErrors = 3;
        private const string BasePath = "/api/saved_objects?per_page=100&search=";
        public readonly string GetPath;
        public readonly object Lock = new object();

        public readonly int MaxDegree = Environment.ProcessorCount * 10;

        public long DeletedCount = 0;
        private int m_errorCount;

        public KibanaDeleteContext([NotNull] string index)
        {
            if (string.IsNullOrEmpty(index))
                throw new ArgumentNullException(nameof(index));

            GetPath = BasePath + index;
        }

        public bool CanRun => m_errorCount < MaxErrors;

        public void EncounterError()
        {
            ++m_errorCount;
        }
    }
}
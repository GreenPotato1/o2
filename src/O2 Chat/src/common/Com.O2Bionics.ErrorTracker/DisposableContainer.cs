using System;
using JetBrains.Annotations;

namespace Com.O2Bionics.ErrorTracker
{
    public sealed class DisposableContainer : IDisposable
    {
        private readonly IDisposable[] m_array;

        public DisposableContainer([NotNull] IDisposable[] array)
        {
            m_array = array;
        }

        public void Dispose()
        {
            // ReSharper disable once ForCanBeConvertedToForeach
            for (int i = 0; i < m_array.Length; i++)
            {
                m_array[i].Dispose();
            }
        }
    }
}
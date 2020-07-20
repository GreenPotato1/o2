using System.Collections.Generic;
using System.Threading;
using Com.O2Bionics.Utils;
using JetBrains.Annotations;

namespace Com.O2Bionics.ChatService.Impl.Storage
{
    public sealed class UnknownDomainCounter
    {
        [NotNull] public readonly ConcurrentHashSet<string> Names;

        //To determine when "Names" has exactly "m_maximumUnknownDomains" items.
        private int m_count;

        public int GetCount()
        {
            var result = Interlocked.Add(ref m_count, 0);
            return result;
        }

        public int IncrementCount()
        {
            var result = Interlocked.Increment(ref m_count);
            return result;
        }

        public UnknownDomainCounter() : this(new ConcurrentHashSet<string>())
        {
        }

        private UnknownDomainCounter([NotNull] ConcurrentHashSet<string> names)
        {
            Names = names;
            m_count = names.Count;
        }

        [NotNull]
        public static UnknownDomainCounter FromHashSet([CanBeNull] HashSet<string> names)
        {
            var result = new UnknownDomainCounter(new ConcurrentHashSet<string>(names));
            return result;
        }
    }
}
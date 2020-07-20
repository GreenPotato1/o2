using System.Diagnostics;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Com.O2Bionics.Utils;
using JetBrains.Annotations;

namespace Com.O2Bionics.PageTracker.Storage
{
    /// <summary>
    /// There can be holes in the Id distribution after restart e.g. 1,2,3,4,
    /// 11,12.
    /// </summary>
    public sealed class IdGenerator : IIdGenerator
    {
        private class Entry
        {
            public readonly SemaphoreSlim Lock = new SemaphoreSlim(1, 1);
            public Block Block;

            public Entry(long counter)
            {
                Block = new Block(1) { Counter = counter };
            }
        }

        private sealed class Block
        {
            private readonly ulong m_last;
            public long Counter = 1;

            public Block(ulong last)
            {
                m_last = last;
            }

            public ulong Current(long counter, ulong blockSize)
            {
                Debug.Assert(0 < counter);
                var result = m_last - blockSize + (ulong)counter;
                Debug.Assert(0 < result);
                Debug.Assert(result <= m_last);
                return result;
            }
        }

        private readonly IIdStorage m_idStorage;
        private readonly ulong m_blockSize;

        private readonly Entry[] m_scopes
            = new Entry[EnumHelper.Values<IdScope>().Cast<int>().Max() + 1];

        public IdGenerator([NotNull] IIdStorage idStorage)
        {
            m_idStorage = idStorage.NotNull(nameof(idStorage));
            m_blockSize = m_idStorage.BlockSize;

            foreach (var scope in EnumHelper.Values<IdScope>())
            {
                m_scopes[(int)scope] = new Entry((long)m_blockSize);
            }
        }

        public async Task<ulong> NewId(IdScope scope)
        {
            var entry = m_scopes[(int)scope];
            for (;;)
            {
                Thread.MemoryBarrier();
                var block = entry.Block;
                var counter = Interlocked.Increment(ref block.Counter);
                if ((ulong)counter <= m_blockSize)
                {
                    return block.Current(counter, m_blockSize);
                }

                Interlocked.Decrement(ref block.Counter);

                Block newBlock;
                await entry.Lock.WaitAsync(); // Allow 1 writer.
                try
                {
                    Thread.MemoryBarrier();
                    if (block != entry.Block)
                        continue; // Someone has already changed.

                    var last = await m_idStorage.Add(scope);

                    newBlock = new Block(last);
                    entry.Block = newBlock;
                }
                finally
                {
                    entry.Lock.Release();
                }

                return newBlock.Current(1L, m_blockSize);
            }
        }
    }
}
using System.Threading.Tasks;

namespace Com.O2Bionics.PageTracker.Storage
{
    public interface IIdStorage
    {
        /// <summary>
        /// Allocates next <see cref="BlockSize"/> block of ids and returns last
        /// the last id  in the block allocated. Block ids are [result -
        /// BlockSize + 1, result]
        /// </summary>
        Task<ulong> Add(IdScope scope);

        ulong BlockSize { get; }
    }
}
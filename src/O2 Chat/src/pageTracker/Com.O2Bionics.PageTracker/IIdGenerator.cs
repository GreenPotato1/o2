using System.Threading.Tasks;

namespace Com.O2Bionics.PageTracker
{
    public interface IIdGenerator
    {
        Task<ulong> NewId(IdScope scope);
    }
}
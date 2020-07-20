using JetBrains.Annotations;

namespace Com.O2Bionics.ErrorTracker
{
    public interface IEmergencyWriter
    {
        void Report([NotNull] string contents);
    }
}
using Com.O2Bionics.Utils.JsonSettings;
using JetBrains.Annotations;

namespace Com.O2Bionics.Console
{
    public interface ICommand
    {
        [NotNull]
        [ItemNotNull]
        string[] Names { get; }

        [Pure]
        [NotNull]
        string GetUsage([NotNull] JsonSettingsReader reader);

        void Run([NotNull] string commandName, [NotNull] JsonSettingsReader reader);
    }
}
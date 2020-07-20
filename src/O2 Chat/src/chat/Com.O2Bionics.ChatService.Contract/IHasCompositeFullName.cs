using System;
using JetBrains.Annotations;

namespace Com.O2Bionics.ChatService.Contract
{
    public interface IHasCompositeFullName
    {
        string FirstName { get; }
        string LastName { get; }
    }

    public static class CompositeFullNameHolderExtensions
    {
        [NotNull]
        public static string FullName([NotNull] this IHasCompositeFullName x)
        {
            if (x == null) throw new ArgumentNullException(nameof(x));

            return ((x.FirstName ?? "") + " " + (x.LastName ?? "")).Trim();
        }
    }
}
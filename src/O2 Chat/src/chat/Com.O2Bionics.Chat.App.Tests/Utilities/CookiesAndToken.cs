using System;
using JetBrains.Annotations;

namespace Com.O2Bionics.Chat.App.Tests.Utilities
{
    public sealed class CookiesAndToken
    {
        [NotNull] public readonly string AppCookie;
        [NotNull] public readonly string VerificationCookie;
        [NotNull] public readonly string VerificationToken;

        public CookiesAndToken(
            [NotNull] string appCookie,
            [NotNull] string verificationCookie,
            [NotNull] string verificationToken)
        {
            if (string.IsNullOrEmpty(appCookie))
                throw new ArgumentNullException(nameof(appCookie));
            if (string.IsNullOrEmpty(verificationCookie))
                throw new ArgumentNullException(nameof(verificationCookie));
            if (string.IsNullOrEmpty(verificationToken))
                throw new ArgumentNullException(nameof(verificationToken));

            AppCookie = appCookie;
            VerificationCookie = verificationCookie;
            VerificationToken = verificationToken;
        }
    }
}
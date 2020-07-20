using Com.O2Bionics.Utils.Web.Models;
using JetBrains.Annotations;

namespace Com.O2Bionics.Chat.App.Tests.Models
{
    public sealed class LoginViewModelWithToken : LoginViewModel
    {
        // ReSharper disable once InconsistentNaming
        public string __RequestVerificationToken { [UsedImplicitly] get; set; }
    }
}
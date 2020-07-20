namespace Com.O2Bionics.Utils.Web
{
    public static class LoginConstants
    {
        public const string LoginPath = "/Account/Login";
        public const string LogoutPath = "/Account/LogOut";
        public const string CookieName = "ca";

        //Anti forgery token name
        public const string TokenKey = "__RequestVerificationToken";
    }
}
namespace Com.O2Bionics.Tests.Common
{
    public static class TestConstants
    {
        public const string CustomerMainDomain = "net.customer";
        public static string CustomerDomains => CustomerMainDomain + ";" + "dev.net.customer";

        public const string ApplicationName = "Tests";
        public const uint CustomerId = 1;
        public const string CustomerIdString = "1";

        public const int FakeUserId = 89345701;
        public const string FakeUserName = "Peter Taram Param";

        public const string TestUserEmail1 = "agent1@test.o2bionics.com";
        public const string TestUserPassword1 = "p1";
        public const string TestUserFirstName1 = "Vasily";
        public const string TestUserFullName1 = "Vasily Pupkin";

        public const string TestUserEmail2 = "agent2@test.o2bionics.com";
        public const string TestUserPassword2 = "p2";
        public const string TestUserFirstName2 = "James";
        public const string ChatFramePathQuery = "/Client/chatframe.cshtml?cid=";

        public const string UserAgent =
            "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/64.0.3282.186 Safari/537.36";
    }
}
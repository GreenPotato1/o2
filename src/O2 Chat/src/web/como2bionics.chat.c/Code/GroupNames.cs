namespace Com.O2Bionics.ChatService.Web.Chat
{
    public static class GroupNames
    {
        public static string CustomerGroupName(uint customerId)
        {
            return "cg-" + customerId;
        }

        public static string PageGroupName(decimal skey)
        {
            return "pg-" + skey;
        }

        public static string VisitorGroupName(ulong id)
        {
            return "vg-" + id;
        }
    }
}
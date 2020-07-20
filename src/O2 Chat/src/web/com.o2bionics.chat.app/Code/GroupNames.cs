namespace Com.O2Bionics.ChatService.Web.Console
{
    public static class GroupNames
    {
        public static string CustomerGroupName(uint customerId)
        {
            return "customer-" + customerId;
        }

        public static string AgentGroupName(uint agentId)
        {
            return "agent-" + agentId;
        }

        public static string DepartmentGroupName(uint id)
        {
            return "department-" + id;
        }
    }
}
namespace Com.O2Bionics.ChatService.Web.Console.Models.AuditTrail
{
    public sealed class AuditTrailModel
    {
        public string Title { get; set; }

        public string FormKind { get; set; }

        public int MaxDays { get; set; }

        public override string ToString()
        {
            return $"Title={Title}, FormKind={FormKind}, MaxDays={MaxDays}";
        }
    }
}
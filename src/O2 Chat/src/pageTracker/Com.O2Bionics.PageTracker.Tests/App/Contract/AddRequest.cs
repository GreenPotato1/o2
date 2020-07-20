namespace Com.O2Bionics.PageTracker.Tests.App.Contract
{
    public sealed class AddRequest
    {
// ReSharper disable InconsistentNaming
        public uint cid { set; get; }
        public string ct { set; get; }
        public string tzde { set; get; }
        public int tzof { set; get; }
        public string u { set; get; }
        public ulong vid { set; get; }
    }
}
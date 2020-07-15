using Xamarin.Forms;

namespace PFRCenterGlobal.Helpers
{
    public class NavigationCommander
    {
        public static Page Current { get; internal set; }
        public static INavigation Navigation { get; internal set; }
        public static App App { get; internal set; }
    }
}

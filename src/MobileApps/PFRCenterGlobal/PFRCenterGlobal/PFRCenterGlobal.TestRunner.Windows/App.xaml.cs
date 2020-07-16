namespace PFRCenterGlobal.TestRunner.Windows
{
    /// <summary>
    /// Provides application-specific behavior to supplement the default Application class.
    /// </summary>
    sealed partial class App : RunnerApplication
    {
        protected override void OnLaunched(LaunchActivatedEventArgs e)
        {
            base.OnLaunched(e);
            Xamarin.Forms.Forms.Init(e);
        }

        protected override void OnInitializeRunner()
        {
            AddTestAssembly(typeof(PFRCenterGlobal.UnitTests.CatalogViewModelTests).GetTypeInfo().Assembly);
        }
    }
}

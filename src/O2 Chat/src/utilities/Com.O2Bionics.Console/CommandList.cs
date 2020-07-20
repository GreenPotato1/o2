namespace Com.O2Bionics.Console
{
    public static class CommandList
    {
        public static ICommand[] GetCommands()
        {
            return new ICommand[]
                {
                    new ErrorTrackCommand(),
                    new AuditCommand(),
                    new PageTrackCommand(),
                    new KibanaCommand()
                };
        }
    }
}
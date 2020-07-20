using log4net;

namespace Com.O2Bionics.Console
{
    public abstract class BaseCommand
    {
        private readonly ILog m_log;

        protected BaseCommand()
        {
            m_log = LogManager.GetLogger(GetType());
        }

        protected void WriteLine(string format, params object[] arguments)
        {
            m_log.InfoFormat(format, arguments);
        }
    }
}
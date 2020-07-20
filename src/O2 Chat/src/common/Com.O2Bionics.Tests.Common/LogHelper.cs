using System;
using log4net;
using log4net.Core;

namespace Com.O2Bionics.Tests.Common
{
    public static class LogHelper
    {
        private static readonly ILog m_log = LogManager.GetLogger(typeof(LogHelper));

        public static void WithLogLevel(Level level, Action action)
        {
            var currentLevel = ((log4net.Repository.Hierarchy.Hierarchy)LogManager.GetRepository()).Root.Level;
            var levelChanged = false;
            if (currentLevel != level)
            {
                m_log.InfoFormat("temporarily changing log level from {0} to {1}", currentLevel, level);
                SetLogLevel(level);
                levelChanged = true;
            }

            try
            {
                action();
            }
            finally
            {
                if (levelChanged)
                {
                    SetLogLevel(currentLevel);
                    m_log.DebugFormat("changing log level back to {0}", currentLevel);
                }
            }
        }

        private static void SetLogLevel(Level level)
        {
            ((log4net.Repository.Hierarchy.Hierarchy)LogManager.GetRepository()).Root.Level = level;
            ((log4net.Repository.Hierarchy.Hierarchy)LogManager.GetRepository()).RaiseConfigurationChanged(EventArgs.Empty);
        }
    }
}
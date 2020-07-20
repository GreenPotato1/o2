using System;
using Com.O2Bionics.ChatService.Impl;
using Com.O2Bionics.Utils;
using LinqToDB;
using NUnit.Framework;

namespace Com.O2Bionics.ChatService.Tests.WidgetLoadLimiter
{
    public class WidgetTestBase
    {
        protected readonly ChatDatabaseFactory DatabaseFactory;
        protected DateTime Date;

        protected WidgetTestBase()
        {
            var settings = new TestChatServiceSettings();
            DatabaseFactory = new ChatDatabaseFactory(settings);
        }

        [SetUp]
        public void Setup()
        {
            Date = DateTime.UtcNow.RemoveTime();
            ClearTable();
            ContinueSetup();
        }
        
        protected virtual void ContinueSetup()
        {
        }

        private void ClearTable()
        {
            using (var dataContext = DatabaseFactory.CreateContext())
            {
                dataContext.Db.WIDGET_LOAD.Delete();
                dataContext.Commit();
            }
        }
    }
}
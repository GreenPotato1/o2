using System;
using LinqToDB;

namespace Com.O2Bionics.ChatService.DataModel
{
    public static class SqlMethod
    {
        [Sql.Property("Oracle", "SYSTIMESTAMP", ServerSideOnly = true)]
        // ReSharper disable once InconsistentNaming
        public static DateTime SYSTIMESTAMP
        {
            get { return DateTime.Now; }
        }

        [Sql.Property("Oracle", "SYSDATE", ServerSideOnly = true)]
        // ReSharper disable once InconsistentNaming
        public static DateTime SYSDATE
        {
            get { return DateTime.Now; }
        }
    }
}
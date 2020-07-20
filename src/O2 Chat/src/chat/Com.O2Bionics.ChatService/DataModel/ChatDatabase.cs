using System;
using System.Collections.Generic;
using LinqToDB.DataProvider.Oracle;

namespace Com.O2Bionics.ChatService.DataModel
{
    public partial class ChatDatabase
    {
        public ChatDatabase(string connectionString) : base(new OracleDataProvider(), connectionString)
        {
        }

        public List<Action> OnCommitActions { get; } = new List<Action>();
    }
}
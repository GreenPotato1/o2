using System;
using System.Collections.Generic;
using System.Diagnostics;
using Com.O2Bionics.Console.Properties;
using Com.O2Bionics.Elastic;
using Com.O2Bionics.Utils.JsonSettings;
using JetBrains.Annotations;

namespace Com.O2Bionics.Console
{
    public abstract class IndexBaseCommand : BaseCommand
    {
        [NotNull] private readonly string m_create;

        [NotNull] private readonly string m_recreate;

        protected IndexBaseCommand([NotNull] string create, [NotNull] string recreate)
        {
            m_create = create;
            m_recreate = recreate;
        }

        public string[] Names => new[] { m_create, m_recreate };

        [NotNull]
        protected abstract EsConnectionSettings ElasticConnection { get; }

        [NotNull]
        protected abstract List<string> Indices { get; }

        [NotNull]
        protected IEsClient Client => new EsClient(ElasticConnection);

        public string GetUsage(JsonSettingsReader reader)
        {
            ReadSettings(reader);
            var result = string.Format(Resources.Usage5, Indices, ElasticConnection, Utilities.ExeName, m_create, m_recreate);
            return result;
        }

        public void Run(string commandName, JsonSettingsReader reader)
        {
            ReadSettings(reader);

            if (m_recreate == commandName)
            {
                foreach (var index in Indices) DeleteIndexImpl(ElasticConnection, index);
            }
            else if (m_create != commandName)
                throw new Exception($"Unknown command {commandName}.");

            CreateIndexImpl();
            reader.CheckIndexesUniquenessSafe();
        }

        protected abstract void ReadSettings(JsonSettingsReader reader);
        protected abstract void CreateIndex();

        private void DeleteIndexImpl([NotNull] EsConnectionSettings elasticConnection, [NotNull] string index)
        {
            var watch = Stopwatch.StartNew();
            WriteLine(Resources.StartDeletingIndex2, index, elasticConnection);
            Client.DeleteIndex(index);
            WriteLine(Resources.IndexDeletedInMs1, watch.ElapsedMilliseconds);
        }

        private void CreateIndexImpl()
        {
            var watch = Stopwatch.StartNew();
            WriteLine(Resources.StartCreatingIndex2, Indices, ElasticConnection);
            CreateIndex();
            WriteLine(Resources.IndexCreateadInMs1, watch.ElapsedMilliseconds);
        }
    }
}
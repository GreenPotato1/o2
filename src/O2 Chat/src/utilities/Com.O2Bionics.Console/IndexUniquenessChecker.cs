using System;
using Com.O2Bionics.AuditTrail.Contract.Names;
using Com.O2Bionics.AuditTrail.Contract.Settings;
using Com.O2Bionics.ChatService.Contract.AuditTrail;
using Com.O2Bionics.ErrorTracker;
using Com.O2Bionics.Utils.JsonSettings;
using JetBrains.Annotations;
using log4net;

namespace Com.O2Bionics.Console
{
    public static class IndexUniquenessChecker
    {
        private static readonly ILog m_log = LogManager.GetLogger(typeof(IndexUniquenessChecker));

        /// <summary>
        /// When deleting objects from Kibana, index prefix is used to search for existing names.
        /// In order not to accidentally delete objects of another dashboard, the index names must not be contained in each other.
        /// </summary>
        /// <param name="reader"></param>
        public static void CheckIndexesUniquenessSafe(this JsonSettingsReader reader)
        {
            try
            {
                var auditSettings = reader.ReadFromFile<AuditTrailServiceSettings>();
                var auditIndex = IndexNameFormatter.FormatWithValidation(auditSettings.Index.Name, ProductCodes.Chat);

                var errorSettings = reader.ReadFromFile<ErrorTrackerSettings>();
                var errorIndex = errorSettings.Index.Name;
                var error = IdentifierHelper.LowerCase(errorIndex);
                if (!string.IsNullOrEmpty(error))
                    throw new Exception($"Error Tracker index '{errorIndex}' is bad: {error}");

                if (auditIndex == errorIndex)
                    throw new Exception($"Elastic Search indexes must be the different '{auditIndex}'.");

                CheckIndex(auditIndex, errorIndex);
                CheckIndex(errorIndex, auditIndex);
            }
            catch (Exception e)
            {
                m_log.Error("Uniqueness check failed.", e);
            }
        }

        private static void CheckIndex([NotNull] string value0, [NotNull] string value1)
        {
            if (string.IsNullOrEmpty(value0))
                throw new ArgumentNullException(nameof(value0));
            if (string.IsNullOrEmpty(value1))
                throw new ArgumentNullException(nameof(value1));

            if (value0.Contains(value1))
                throw new Exception($"The index '{value0}' must not contain another index '{value1}'.");
        }
    }
}
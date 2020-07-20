using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Text;

namespace Com.O2Bionics.Utils.JsonSettings
{
    [Serializable]
    public class JsonSettingsErrorsException : Exception
    {
        private readonly string m_message;

        public string FileName { get; }
        public string Json { get; }
        public IReadOnlyCollection<string> Errors { get; }

        public JsonSettingsErrorsException(string message, string fileName, string json, List<string> errors, Exception inner)
            : base(message, inner)
        {
            m_message = message;
            FileName = fileName;
            Json = json;
            Errors = errors?.AsReadOnly() ?? new ReadOnlyCollection<string>(new List<string>());
        }

        public override string Message
        {
            get
            {
                var msb = new StringBuilder();
                msb.AppendLine(m_message);
                if (FileName != null) msb.AppendLine($"in file '{FileName}'");
                if (Json != null) msb.AppendLine($"json: [{Json}]");
                if (Errors != null && Errors.Count > 0)
                {
                    msb.AppendLine();
                    foreach (var e in Errors) msb.AppendLine(e);
                }
                return msb.ToString();
            }
        }
    }
}
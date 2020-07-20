using System;
using System.Collections.Generic;
using Com.O2Bionics.AuditTrail.Contract;
using Com.O2Bionics.ChatService.Contract;
using Com.O2Bionics.ChatService.Contract.AuditTrail;
using Com.O2Bionics.ChatService.Impl.AuditTrail.Names;
using JetBrains.Annotations;
using log4net;

namespace Com.O2Bionics.ChatService.Impl.AuditTrail
{
    public static class AuditEventExtensions
    {
        private static readonly string[] m_keys = { CustomFieldNames.ClientIp, CustomFieldNames.VisitorId };

        public static void SetExceptionAndStatus<T>([NotNull] this AuditEvent<T> auditEvent, [NotNull] Exception e)
        {
            string message;
            switch (e)
            {
                case CallResultException callResultException:
                    message = callResultException.Message;
                    var denied = null != callResultException.Status
                                 && CallResultStatusCode.AccessDenied == callResultException.Status.StatusCode;
                    auditEvent.Status = denied ? OperationStatus.AccessDeniedKey : OperationStatus.ValidationFailedKey;
                    break;
                case ValidationException validationException:
                    message = validationException.Message;
                    auditEvent.Status = OperationStatus.ValidationFailedKey;
                    break;
                default:
                    auditEvent.Status = OperationStatus.OperationFailedKey;
                    return;
            }

            if (string.IsNullOrEmpty(message))
                return;

            auditEvent.AddCustomValue(CustomFieldNames.ExceptionMessage, message);
        }

        public static void SetContextCustomValues<T>([NotNull] this AuditEvent<T> auditEvent)
        {
            var keys = m_keys;

            // ReSharper disable once ForCanBeConvertedToForeach
            for (var i = 0; i < keys.Length; i++)
                TrySetContextCustomValue(auditEvent, keys[i]);
        }

        private static string GetContextValue([NotNull] string stackKey)
        {
            var contextStack = LogicalThreadContext.Stacks[stackKey];
            if (null != contextStack && 0 < contextStack.Count)
            {
                var value = contextStack.ToString();
                if (!string.IsNullOrEmpty(value))
                    return value;
            }

            return null;
        }

        private static void TrySetContextCustomValue<T>([NotNull] this AuditEvent<T> auditEvent, [NotNull] string key)
        {
            var value = GetContextValue(key);
            if (string.IsNullOrEmpty(value))
                return;

            auditEvent.AddCustomValue(key, value);
        }

        public static void AddCustomValue<T>(this AuditEvent<T> ae, string key, string value)
        {
            if (ae.CustomValues == null)
                ae.CustomValues = new Dictionary<string, string>();
            ae.CustomValues[key] = value;
        }
    }
}
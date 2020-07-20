using System;
using System.Collections.Generic;
using Com.O2Bionics.AuditTrail.Client;
using Com.O2Bionics.AuditTrail.Contract;
using JetBrains.Annotations;
using NSubstitute;
using NUnit.Framework;

namespace Com.O2Bionics.ChatService.Tests.Mocks
{
    public sealed class AuditTrailClientSaveMock<TEventArg>
        where TEventArg : class
    {
        private readonly List<AuditEvent<TEventArg>> m_auditEvents = new List<AuditEvent<TEventArg>>();
        private readonly object m_lock = new object();

        public AuditTrailClientSaveMock([NotNull] IAuditTrailClient auditTrailClient)
        {
            if (null == auditTrailClient)
                throw new ArgumentNullException(nameof(auditTrailClient));

            auditTrailClient
                .WhenForAnyArgs(a => a.Save(Arg.Any<AuditEvent<TEventArg>>()))
                .Do(
                    args =>
                        {
                            var auditEvent = args.Arg<AuditEvent<TEventArg>>();
                            Assert.IsNotNull(auditEvent, nameof(auditEvent));
                            lock (m_lock)
                            {
                                m_auditEvents.Add(auditEvent);
                            }
                        });
        }

        public List<AuditEvent<TEventArg>> AuditEvents
        {
            get
            {
                lock (m_lock)
                {
                    var result = new List<AuditEvent<TEventArg>>(m_auditEvents);
                    return result;
                }
            }
        }

        public void Clear()
        {
            lock (m_lock)
            {
                m_auditEvents.Clear();
            }
        }
    }
}
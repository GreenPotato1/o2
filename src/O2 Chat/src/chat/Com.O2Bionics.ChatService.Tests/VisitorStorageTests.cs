using System;
using Com.O2Bionics.ChatService.Contract;
using Com.O2Bionics.ChatService.DataModel;
using Com.O2Bionics.ChatService.Impl.Storage;
using Com.O2Bionics.Tests.Common;
using FluentAssertions;
using NSubstitute;
using NUnit.Framework;

namespace Com.O2Bionics.ChatService.Tests
{
    [TestFixture]
    public class VisitorStorageTests
    {
        [Test]
        public void TestGet()
        {
            var now = DateTime.UtcNow;
            var nowProvider = new TestNowProvider(now);
            var serviceSettings = new ChatServiceSettings
                {
                    Cache = new ChatServiceCacheSettings { Visitor = 5 },
                };
            var databaseFactory = Substitute.For<IChatDatabaseFactory>();


            var storage = new VisitorStorage(nowProvider, databaseFactory, serviceSettings);

            databaseFactory.Query(Arg.Any<Func<ChatDatabase, VISITOR>>()).Returns(_ => null);
            storage.Get(1000000).Should().Be(null);
            databaseFactory.ReceivedCalls().Should().HaveCount(1);
            storage.Get(1000001).Should().Be(null);
            databaseFactory.ReceivedCalls().Should().HaveCount(2);


            var visitorId = 1000002u;
            var dbo = new VISITOR
                {
                    VISITOR_ID = visitorId,
                    CUSTOMER_ID = 2,
                    ADD_TIMESTAMP = now.AddHours(-2),
                    UPDATE_TIMESTAMP = now.AddHours(-3),
                    NAME = "name1",
                    EMAIL = "email1",
                    PHONE = "phone1",
                    MEDIA_SUPPORT = (sbyte)MediaSupport.Audio,
                };

            databaseFactory.ClearReceivedCalls();
            databaseFactory.Query(Arg.Any<Func<ChatDatabase, VISITOR>>()).Returns(_ => dbo);

            var r = storage.Get(visitorId);
            r.Should().NotBeNull();
            r.Should().BeEquivalentTo(
                new
                    {
                        CustomerId = dbo.CUSTOMER_ID,
                        Id = dbo.VISITOR_ID,
                        AddTimestampUtc = dbo.ADD_TIMESTAMP,
                        UpdateTimestampUtc = dbo.UPDATE_TIMESTAMP,
                        Name = dbo.NAME,
                        Email = dbo.EMAIL,
                        Phone = dbo.PHONE,
                        MediaSupport = dbo.MEDIA_SUPPORT.HasValue ? (MediaSupport?)(MediaSupport)dbo.MEDIA_SUPPORT : (MediaSupport?)null,
                        TranscriptMode = (VisitorSendTranscriptMode?)null,
                    });
            var r2 = storage.Get(visitorId);
            r2.Should().NotBeNull();
            r2.Should().BeEquivalentTo(r);
            databaseFactory.ReceivedCalls().Should().HaveCount(1);
        }
    }
}
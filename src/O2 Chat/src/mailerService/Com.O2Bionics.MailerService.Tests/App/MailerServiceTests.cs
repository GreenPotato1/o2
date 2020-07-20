using System;
using System.Net;
using System.Reflection;
using System.Threading.Tasks;
using Com.O2Bionics.ChatService.Contract.AuditTrail;
using Com.O2Bionics.ChatService.Contract.MailerService;
using Com.O2Bionics.MailerService.Client;
using Com.O2Bionics.MailerService.Client.Settings;
using Com.O2Bionics.MailerService.Contract;
using Com.O2Bionics.Tests.Common;
using Com.O2Bionics.Utils;
using Com.O2Bionics.Utils.JsonSettings;
using Com.O2Bionics.Utils.Network;
using NUnit.Framework;
using FluentAssertions;

namespace Com.O2Bionics.MailerService.Tests.App
{
    [TestFixture]
    public sealed class MailerServiceTests : IDisposable
    {
        public sealed class TestCase
        {
            public readonly MailRequest Request;
            public readonly SubjectBody Response;

            public override string ToString()
            {
                return Request?.TemplateId ?? string.Empty;
            }

            public TestCase(MailRequest request = null, SubjectBody response = null)
            {
                Request = request;
                Response = response;
            }
        }

        private static readonly DateTime m_now = new DateTime(2345, 6, 27, 0, 0, 0, DateTimeKind.Utc);
        private readonly MailerServiceClientSettings m_clientSettings = new JsonSettingsReader().ReadFromFile<MailerServiceClientSettings>();
        private readonly MailerServiceClient m_mailerServiceClient;

        public MailerServiceTests()
        {
            m_mailerServiceClient = new MailerServiceClient(m_clientSettings);
        }

        private static readonly TestCase[] m_testCases =
            {
                new TestCase(
                    new MailRequest
                        {
                            ProductCode = ProductCodes.Chat,
                            TemplateId = TemplateIds.ChatSessionTranscript,
                            To = TestConstants.TestUserEmail1,
                            TemplateModel = BuildSessionTranscriptModel(),
                        },
                    new SubjectBody
                        {
                            Subject = "Chat session transcript",
                            Body = Assembly.GetExecutingAssembly()
                                .ReadEmbeddedResource("Com.O2Bionics.MailerService.Tests.App.ExpectedMessageText.ChatSessionTranscript.txt")
                        }),
                new TestCase(
                    new MailRequest
                        {
                            ProductCode = ProductCodes.Chat,
                            TemplateId = TemplateIds.ResetPassword,
                            To = TestConstants.TestUserEmail1,
                            TemplateModel = "{\"Link\": \"https://aaa.bbb.ccc/ddd.TXT\" }",
                        },
                    new SubjectBody
                        {
                            Subject = "Password Reset",
                            Body = Assembly.GetExecutingAssembly()
                                .ReadEmbeddedResource("Com.O2Bionics.MailerService.Tests.App.ExpectedMessageText.ResetPassword.txt")
                        }),
            };

        public void Dispose()
        {
            m_mailerServiceClient.Dispose();
        }

        [Test]
        public async Task GenerateSubjectAndBodyTest([ValueSource(nameof(m_testCases))] TestCase testCase)
        {
            var actual = await m_mailerServiceClient.GenerateSubjectAndBody(testCase.Request);
            //System.IO.File.WriteAllText(@"C:\O2Bionics\body-actual.txt", actual?.Body);
            //System.IO.File.WriteAllText(@"C:\O2Bionics\body-expected.txt", testCase.Response.Body);
            actual.Should().BeEquivalentTo(testCase.Response, testCase.Request.TemplateId);
        }

        [Test]
        public async Task SendTest([ValueSource(nameof(m_testCases))] TestCase testCase)
        {
            var actual = await m_mailerServiceClient.Send(testCase.Request);
            actual.Should().BeEquivalentTo(null, testCase.Request.TemplateId);
        }

        [Test]
        public void GenerateBadProductCode()
        {
            var request = BadRequest();
            var ex = Assert.ThrowsAsync<PostException>(async () => await m_mailerServiceClient.GenerateSubjectAndBody(request));
            Assert.AreEqual((int)HttpStatusCode.BadRequest, ex.HttpCode, nameof(ex.HttpCode));
            var expected = ExpectedBadProductError();
            Assert.AreEqual(expected, ex.Message, nameof(ex.Message));
        }

        [Test]
        public async Task SendBadProductCode()
        {
            var request = BadRequest();
            var error = await m_mailerServiceClient.Send(request);
            var expected = ExpectedBadProductError();
            Assert.AreEqual(expected, error, "Send response");
        }

        private static MailRequest BadRequest() => new MailRequest
            {
                ProductCode = "bad product name",
                TemplateId = "NotExistingId",
                TemplateModel = "<script>console.log('Hello from BadRequest test.')</script>",
                To = "Peter The Great <peter@gre.at>"
            };

        private static string ExpectedBadProductError() => 
            $"{nameof(MailRequest.ProductCode)} {Utils.Properties.Resources.LowerOrUpperCaseError}";

        private static string BuildSessionTranscriptModel()
        {
            var o = new
                {
                    Messages = new[]
                        {
                            new
                                {
                                    TimestampUtc = m_now,
                                    Text = @"Some
long  
text 
yes1.",
                                    SenderClass = "agent",
                                    SenderName = "Peter",
                                },
                            new
                                {
                                    TimestampUtc = m_now,
                                    Text = @"Other 
text 
yes2.
<a href='http://test.com/asd?aa=bb&test>test</a>

#&<! &nbsp;
",
                                    SenderClass = "system",
                                    SenderName = "Piter",
                                },
                        },
                    VisitorTimezoneOffsetMinutes = 123
                };
            var result = o.JsonStringify2();
            return result;
        }
    }
}
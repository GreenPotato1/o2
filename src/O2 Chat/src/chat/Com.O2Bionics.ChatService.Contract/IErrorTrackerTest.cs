#if ERRORTRACKERTEST
using System;
using System.Runtime.Serialization;
using System.ServiceModel;
using Com.O2Bionics.Utils;
using Com.O2Bionics.Utils.Network;

namespace Com.O2Bionics.ChatService.Contract
{
    /// <summary>
    /// It is used to test the error logging by throwing an exception in the WCF
    /// server.
    /// </summary>
    [ServiceContract(Namespace = ServiceConstants.Namespace)]
    public interface IErrorTrackerTest
    {
        [OperationContract]
        [FaultContract(typeof(ErrorTrackerFault))]
        void TestThrowError(string message);
    }

    [DataContract(Namespace = ServiceConstants.Namespace)]
    public sealed class ErrorTrackerFault
    {
        [DataMember]
        public string Message { get; set; }

        public override string ToString()
        {
            return Message;
        }
    }

    public static class ErrorTrackerTestHelper
    {
        public static int RunTest<TService>(out string message)
            where TService : IErrorTrackerTest
        {
            var messageToSend = $"TestMessageAt_{DateTime.UtcNow}";
            int code;
            try
            {
                var service = GlobalContainer.Resolve<TcpServiceClient<TService>>();
                service.Call(s => s.TestThrowError(messageToSend));
                code = (int)System.Net.HttpStatusCode.ExpectationFailed;
                message = "An exception must have been thrown.";
            }
            catch (FaultException<ErrorTrackerFault> e)
            {
                if (messageToSend == e.Detail.Message)
                {
                    code = (int)System.Net.HttpStatusCode.OK;
                    message = e.Message + ' ' + e.Detail.Message;
                }
                else
                {
                    code = (int)System.Net.HttpStatusCode.PreconditionFailed;
                    message = $"Sent '{messageToSend}', but the server returned '{e.Detail.Message}'.";
                }
            }
            catch (Exception e)
            {
                code = (int)System.Net.HttpStatusCode.InternalServerError;
                message = e.ToString();
            }

            return code;
        }
    }
}
#endif
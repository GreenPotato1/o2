using System.ServiceModel;
using Com.O2Bionics.Utils.Network;

namespace Com.O2Bionics.ChatService.Contract
{
    [ServiceContract(Namespace = ServiceConstants.Namespace)]
    public interface IPingable
    {
        [OperationContract]
        void Ping();
    }
}
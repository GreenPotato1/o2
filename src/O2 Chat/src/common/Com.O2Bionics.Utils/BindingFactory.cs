using System;
using System.Net.Security;
using System.ServiceModel;
using System.ServiceModel.Channels;

namespace Com.O2Bionics.Utils
{
    internal static class BindingFactory
    {
        private static NetTcpBinding CreateBaseNetTcpBinding()
        {
            return new NetTcpBinding
                {
                    ReliableSession = new OptionalReliableSession { Enabled = false, },
                    Security = new NetTcpSecurity
                        {
                            Mode = SecurityMode.None,
                            Message = new MessageSecurityOverTcp { ClientCredentialType = MessageCredentialType.None, },
                            Transport = new TcpTransportSecurity
                                {
                                    ClientCredentialType = TcpClientCredentialType.None,
                                    ProtectionLevel = ProtectionLevel.None,
                                },
                        },
                    TransactionFlow = false,
                    PortSharingEnabled = false,
                    ReceiveTimeout = TimeSpan.FromSeconds(5),
                };
        }

        public static Binding CreateClientBinding()
        {
            var binding = CreateBaseNetTcpBinding();
            binding.MaxBufferSize = 2147483647;
            binding.MaxReceivedMessageSize = 2147483647;
            return binding;
        }

        public static Binding CreateServerBinding()
        {
            var binding = CreateBaseNetTcpBinding();
            binding.ListenBacklog = 2147483647;
            binding.MaxBufferPoolSize = 2147483647;
            binding.MaxBufferSize = 2147483647;
            binding.MaxConnections = 2147483647;
            binding.MaxReceivedMessageSize = 2147483647;
            return binding;
        }
    }
}
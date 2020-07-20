using System.Runtime.Serialization;
using Com.O2Bionics.Utils;
using JetBrains.Annotations;

namespace Com.O2Bionics.ChatService.Contract
{
    [DataContract]
    public sealed class CustomerEntry
    {
        [DataMember] [CanBeNull] public string[] Domains;

        [DataMember] [CanBeNull] public ConcurrentHashSet<string> UnknownDomains;

        [DataMember] public byte StatusFlags;

        #region Flags

        [IgnoreDataMember] private const int ActivePosition = 0;
        [IgnoreDataMember] private const int OverloadPosition = 1;
        [IgnoreDataMember] private const int ManyPosition = 2;

        [IgnoreDataMember]
        public bool Active
        {
            get => BitwiseUtilities.IsSet(StatusFlags, ActivePosition);
            set => StatusFlags = BitwiseUtilities.SetByte(StatusFlags, ActivePosition, value);
        }

        [IgnoreDataMember]
        public bool ViewCounterExceeded
        {
            get => BitwiseUtilities.IsSet(StatusFlags, OverloadPosition);
            set => StatusFlags = BitwiseUtilities.SetByte(StatusFlags, OverloadPosition, value);
        }

        [IgnoreDataMember]
        public bool UnknownDomainNumberExceeded
        {
            get => BitwiseUtilities.IsSet(StatusFlags, ManyPosition);
            set => StatusFlags = BitwiseUtilities.SetByte(StatusFlags, ManyPosition, value);
        }

        #endregion

        public override string ToString()
        {
            return
                $"{nameof(Active)}={Active}, {nameof(ViewCounterExceeded)}={ViewCounterExceeded}, {nameof(UnknownDomainNumberExceeded)}={UnknownDomainNumberExceeded}, {nameof(Domains)}={Domains.JoinAsString()}, {nameof(UnknownDomains)}={UnknownDomains?.Keys.JoinAsString()}";
        }
    }
}
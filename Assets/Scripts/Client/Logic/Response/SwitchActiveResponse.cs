using System;
using Unity.Netcode;

namespace Client.Logic.Response
{
    public class SwitchActiveResponse : BaseResponse, IEquatable<SwitchActiveResponse>
    {
        public string CharacterId;
        
        public SwitchActiveResponse() { }

        public SwitchActiveResponse(ulong id, string character) : base(id)
        {
            CharacterId = character;
        }
        
        public override async void Process()
        {
            await Player.characterZone.SwitchActiveCharacter(CharacterId);
            
            base.Process();
        }

        public override void NetworkSerialize<T>(BufferSerializer<T> serializer)
        {
            base.NetworkSerialize(serializer);

            serializer.SerializeValue(ref CharacterId);
        }

        public bool Equals(SwitchActiveResponse other)
        {
            if (ReferenceEquals(other, null) || !base.Equals(other))
                return false;

            return CharacterId.Equals(other.CharacterId);
        }
    }
}
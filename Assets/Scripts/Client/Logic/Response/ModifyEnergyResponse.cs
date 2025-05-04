using System;
using Unity.Netcode;

namespace Client.Logic.Response
{
    public class ModifyEnergyResponse : BaseResponse, IEquatable<ModifyEnergyResponse>
    {
        public int Amount;
        public string CharacterId;
        
        public ModifyEnergyResponse() { }

        public ModifyEnergyResponse(int amount, string character)
        {
            Amount = amount;
            CharacterId = character;
        }

        public override void Process()
        {
            Global.GetCharacter(CharacterId)?.ModifyEnergy(Amount);
            
            base.Process();
        }

        public override void NetworkSerialize<T>(BufferSerializer<T> serializer)
        {
            serializer.SerializeValue(ref Amount);
            serializer.SerializeValue(ref CharacterId);
        }

        public bool Equals(ModifyEnergyResponse other)
        {
            if (ReferenceEquals(other, null) || !base.Equals(other))
                return false;

            return Amount == other.Amount && CharacterId.Equals(other.CharacterId);
        }
    }
}
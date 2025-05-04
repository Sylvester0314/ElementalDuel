using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Server.GameLogic;
using Server.ResolveLogic;
using Shared.Handler;
using Shared.Misc;
using Unity.Netcode;

namespace Client.Logic.Response
{
    public enum ResourceOperationType
    {
        Append,
        Remove,
        ResetAll
    }
    
    public class ResourceResponse : BaseResponse, IEquatable<ResourceResponse>
    {
        public ResourceOperationType Operation;
     
        public int Energy;
        public int ArcaneEdict;
        public string CharacterId;
        public List<DiceLogic> Dices;
        
        public ResourceResponse()
        {
            Dices = DiceLogic.EmptyList;
        }

        public ResourceResponse(
            ulong id, ResourceOperationType operation, 
            List<DiceLogic> dices, int arcane, int energy
        ) : base(id)
        {
            Operation = operation;
            Dices = dices;
            Energy = energy;
            ArcaneEdict = arcane;
            CharacterId = string.Empty;
        }

        #region Factory Methods

        public static ResourceResponse AppendDices(ulong id, List<DiceLogic> dices)
            => new (id, ResourceOperationType.Append, dices, 0, 0);

        public static ResourceResponse RestoreArcaneEdict(ulong id)
            => new (id, ResourceOperationType.Append, DiceLogic.EmptyList, 1, 0);
        
        public static ResourceResponse Use(ResolveNode node, ICostHandler entity, List<string> ids)
        {
            var actual = entity.Cost.Actual;
            var (dices, arcane, energy) = node.RemoveResource(actual, ids);
            var characterId = node.State.ActiveCharacter.UniqueId;
            
            return new ResourceResponse(
                node.State.Id, ResourceOperationType.Remove, dices, arcane, energy
            ) { CharacterId = characterId };
        }

        #endregion

        public override async void Process()
        {
            var task = Operation switch
            {
                ResourceOperationType.Append    => Append(),
                ResourceOperationType.Remove    => Remove(),
                ResourceOperationType.ResetAll  => ResetAll(),
                _                               => throw new Exception("Invalid operation")
            };
            await task;
            
            base.Process();
        }
        
        public async Task Append()
        {
            if (!IsRequester)
                Global.indicator.oppoCountdown.diceCount.Count += Dices.Count;
            else
                Global.diceFunction.Append(Dices);
            
            await Task.CompletedTask;
        }
        
        public async Task Remove() 
        {
            if (!IsRequester)
            {
                Global.indicator.oppoCountdown.diceCount.Count -= Dices.Count;
                Global.Opponent.nameBar.legend.Remaining -= ArcaneEdict;
            }
            else
            {
                Global.diceFunction.Remove(Dices);
                Player.nameBar.legend.Remaining -= ArcaneEdict;
            }
            
            Global.GetCharacter(CharacterId)?.ModifyEnergy(-Energy);
            
            await Task.CompletedTask;
        }
        
        public async Task ResetAll() 
        {
            await Task.CompletedTask;
        }
        
        public override void NetworkSerialize<T>(BufferSerializer<T> serializer)
        {
            base.NetworkSerialize(serializer);
            
            serializer.SerializeValue(ref Operation);
            serializer.SerializeValue(ref Energy);
            serializer.SerializeValue(ref ArcaneEdict);
            serializer.SerializeValue(ref CharacterId);
            NetCodeMisc.SerializeList(serializer, ref Dices);
        }

        public bool Equals(ResourceResponse other)
        {
            if (ReferenceEquals(other, null) || !base.Equals(other))
                return false;

            return Operation == other.Operation &&
                   Energy == other.Energy &&
                   ArcaneEdict == other.ArcaneEdict &&
                   CharacterId == other.CharacterId &&
                   Dices.Equals(other.Dices);
        }
    }
}
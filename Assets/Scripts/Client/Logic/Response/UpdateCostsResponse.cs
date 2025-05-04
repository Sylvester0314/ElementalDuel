using System;
using System.Collections.Generic;
using System.Linq;
using Server.GameLogic;
using Shared.Classes;
using Shared.Handler;
using Shared.Misc;
using Unity.Netcode;
using UnityEngine;

namespace Client.Logic.Response
{
    public class UpdateCostsResponse : BaseResponse, IEquatable<UpdateCostsResponse>
    {
        public string Check;
        public Dictionary<string, CostMatchResult> Hands;
        public Dictionary<string, CostMatchResult> Skills;
        
        public UpdateCostsResponse()
        {
            Hands = new Dictionary<string, CostMatchResult>();
            Skills = new Dictionary<string, CostMatchResult>();
        }

        public UpdateCostsResponse(PlayerLogic logic) : base(logic.Id)
        {
            Check = Guid.NewGuid().ToString();
            
            var active = logic.ActiveCharacter;
            var skills = active.Skills.Values.Attach(logic.SwitchActive);

            Skills = skills.ToDictionary(
                skill => skill.Name,
                skill => new CostMatchResult(logic, skill)
            );

            Hands = logic.DeckCard.Hand.ToDictionary(
                pair => pair.Key.ToString(),
                pair => new CostMatchResult(logic, pair.Value)
            );
        }

        public override void Process()
        {
            Global.hand.cards.ForEach(card =>
            {
                var timestamp = card.timestamp.ToString();
                var result = Hands[timestamp];
                
                card.NetworkSynchronous(result);
            });
            
            Global.combatAction.NetworkSynchronous(Skills);
            
            base.Process();
        }

        public override void NetworkSerialize<T>(BufferSerializer<T> serializer)
        {
            base.NetworkSerialize(serializer);
            
            serializer.SerializeValue(ref Check);
            NetCodeMisc.SerializeDictionary(serializer, ref Hands);
            NetCodeMisc.SerializeDictionary(serializer, ref Skills);
        }

        public bool Equals(UpdateCostsResponse other)
        {
            if (ReferenceEquals(other, null) || !base.Equals(other))
                return false;

            return Check.Equals(other.Check);
        }
    }
}
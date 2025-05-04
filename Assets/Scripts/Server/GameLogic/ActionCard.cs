using System;
using System.Collections.Generic;
using System.Linq;
using Server.Logic.Event;
using Shared.Enums;
using Shared.Handler;
using Shared.Logic.Condition;
using Shared.Logic.Effect;
using Unity.Netcode;

namespace Server.GameLogic
{
    public class ActionCard : ICostHandler, IEventSource
    {
        public PlayerLogic PlayerLogic;
        public ActionCardAsset Asset;
        
        public int Timestamp;
        public ConditionLogic UseCondition;
        public EffectContainer Effects;
        
        public static List<ActionCard> EmptyList = Array.Empty<ActionCard>().ToList();

        public string Key => $"{Timestamp}_{EntityName}";
        public string EntityName => Asset.name;
        public string MainCardName => Asset.name.Split('-')[0];
        public bool CombatAction => Asset.Properties.Contains(Property.CardAction);
        public PlayerLogic Belongs => PlayerLogic;
        public List<string> BannerHint => Asset.BannerHint(PlayerLogic);
        public CostLogic Cost { get; private set; }
        public string UniqueId { get => Timestamp.ToString(); set {} }
        
        public ActionCard() { }
        
        public ActionCard(PlayerLogic logic, ActionCardAsset asset)
        {
            PlayerLogic = logic;
            Asset = asset;
            
            UseCondition = asset.useCondition;
            Cost = new CostLogic(asset.costs);
            Effects = new EffectContainer(asset.Effects);
        }

        // TODO 时点实现 - 行动牌费用计算
        public void CalculateActualCost()
        {
            
        }

        public bool EvaluateUsable()
        {
            var active = PlayerLogic.ActiveCharacter;
            var passive = PassiveEvent.Create(active);
            return UseCondition.Evaluate(passive,  EffectVariables.Empty);
        }

        public ActionCardInformation Convert()
            => new (EntityName, Timestamp);

        public ActionCard Clone(PlayerLogic logic)
            => new ()
            {
                PlayerLogic = logic,
                Asset = Asset,
                UseCondition = UseCondition,
                Effects = Effects,
                Timestamp = Timestamp,
                Cost = Cost.Clone()
            };
    }
    
    [Serializable]
    public class ActionCardInformation : INetworkSerializable, IEquatable<ActionCardInformation>
    {
        public string name;
        public int timestamp;
        
        public static List<ActionCardInformation> EmptyList = Array.Empty<ActionCardInformation>().ToList();
        
        public ActionCardInformation() { }

        public ActionCardInformation(string name, int timestamp)
        {
            this.name = name;
            this.timestamp = timestamp;
        }

        public void NetworkSerialize<T>(BufferSerializer<T> serializer) where T : IReaderWriter
        {
            serializer.SerializeValue(ref name);
            serializer.SerializeValue(ref timestamp);
        }

        public bool Equals(ActionCardInformation other)
        {
            if (ReferenceEquals(null, other)) 
                return false;
            
            return name == other.name && timestamp == other.timestamp;
        }
    }
}
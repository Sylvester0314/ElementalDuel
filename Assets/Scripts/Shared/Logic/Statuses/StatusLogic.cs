using System;
using System.Collections.Generic;
using System.Linq;
using Server.GameLogic;
using Server.Logic.Event;
using Server.ResolveLogic;
using Shared.Enums;
using Shared.Logic.Effect;

namespace Shared.Logic.Statuses
{
    public class StatusLogic
    {
        public string UniqueId;
        public int MaxCapacity;
        
        public StatusType Type;
        public PlayerLogic Logic;
        public List<Status> Statuses;

        public CharacterData Belongs;
        
        public StatusLogic() { }
        
        public StatusLogic(PlayerLogic logic, StatusType type, CharacterData character = null)
        {
            UniqueId = Guid.NewGuid().ToString();
            MaxCapacity = type is StatusType.Summon or StatusType.Support ? 4 : int.MaxValue;
            
            Type = type;
            Logic = logic;
            Statuses = new List<Status>();
            Belongs = character;
        }
        
        public List<BaseEvent> Process(BaseEvent e, Timing timing)
        {
            return Statuses
                .SelectMany(status => status.Trigger(e, timing))
                .ToList();
        }

        public Status Append(GenerateStatusEffect via)
        {
            var asset = via.statusAsset;
            if (asset.type is not StatusType.Support)
            {
                var existingStatus = Statuses.Find(status => status.Key == asset.name);
                if (existingStatus != null)
                {
                    existingStatus.Regenerate();
                    return existingStatus;
                }

                // Used to handle the status where talents
                // generate the same name but different effects
                var sameNameStatus = Statuses.Find(status => status.Name == asset.statusName);
                if (sameNameStatus != null)
                    Statuses.Remove(sameNameStatus);
            }

            var status = new Status(this, asset, via);
            Statuses.Add(status);
            
            return status;
        }

        public void Remove(string uniqueId)
            => Statuses.RemoveAll(status => status.UniqueId == uniqueId);

        public bool Contains(StatusCardAsset asset)
            => Statuses.Any(status => status.Key == asset.name);

        public StatusLogic Clone(PlayerLogic logic, CharacterData character = null)
        {
            var clone = new StatusLogic
            {
                UniqueId = UniqueId,
                MaxCapacity = MaxCapacity,
                Type = Type,
                Logic = logic,
                Belongs = character,
                Statuses = new List<Status>()
            };

            foreach (var status in Statuses)
                clone.Statuses.Add(status.Clone(clone));
            
            return clone;
        }
    }
}
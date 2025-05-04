using System;
using System.Collections.Generic;
using System.Linq;
using Client.Logic.Response;
using Shared.Enums;
using Shared.Misc;

namespace Server.GameLogic
{
    public class CharacterLogic
    {
        public PlayerLogic PlayerLogic;
        
        public List<CharacterData> Characters;
        public CharacterData Active;
        
        public List<CharacterData> AliveCharacters 
            => Characters.Where(character => character.IsAlive).ToList();
        
        public CharacterLogic() { }
        
        public CharacterLogic(PlayerLogic logic, List<CharacterAsset> characters)
        {
            PlayerLogic = logic;
            Characters = characters
                .Select((asset, i) => new CharacterData(this, asset, i))
                .ToList();
        }

        public ActionResponseWrapper[][] SwitchActive(string id)
        {
            // TODO 用 SwitchActiveEvent 来驱动
            Active = FindCharacter(id);
            Active.IsActive = true;

            var wrappers = new ActionResponseWrapper[3][];

            wrappers[0] = new ActionResponseWrapper[] { };
            
            var playerId = PlayerLogic.Id;
            var response = new SwitchActiveResponse(playerId, id);
            wrappers[1] = ActionResponseWrapper.Package(response);
            
            wrappers[2] = new ActionResponseWrapper[] { };
            
            return wrappers;
        }

        public CharacterData FindCharacter(string id)
            => Characters.Find(character => character.UniqueId == id);

        public HashSet<CostType> GetElementTypes(bool shouldAlive)
            => Characters
                .Where(character => !shouldAlive || character.IsAlive)
                .Select(character => character.Element)
                .ToHashSet();

        public List<CharacterData> GetTargetedCharacters(TargetType type, bool mustStandby)
        {
            Func<CharacterData, bool> filter = type switch
            {
                TargetType.Alive        => character => character.IsAlive,
                TargetType.Dead         => character => character.Defeated,
                _ => null
            };
            
            if (filter != null)
                return Characters
                    .Where(character => !mustStandby || !character.IsActive)
                    .Where(filter).ToList();

            Func<CharacterData, int> metric = type switch
            {
                TargetType.MostDamaged      => character => character.Damaged,
                TargetType.LeastDamaged     => character => -character.Damaged,
                TargetType.HighestHealth    => character => character.CurrentHealth,
                TargetType.LowestHealth     => character => -character.CurrentHealth,
                _                           => null
            };

            if (metric != null)
                return FindExtremum();
            
            return type switch
            {
                TargetType.Active           => Active.SingleList(),
                TargetType.Next             => Active.NextCharacter(mustStandby),
                TargetType.Previous         => Active.PreviousCharacter(mustStandby),
                TargetType.Adjacent         => Active.AdjacentCharacters(),
                _                           => new List<CharacterData>()
            };

            List<CharacterData> FindExtremum()
            {
                var extremum = int.MinValue;
                var character = Active;
                CharacterData result = null;
        
                do
                {
                    if (mustStandby && character.IsAlive)
                        continue;
                    
                    var value = metric(character);
                    if (value >= extremum)
                    {
                        extremum = value;
                        result = character;
                    }
                    character = character.NextCharacter(true).FirstOrDefault();
                } while (!character?.IsActive ?? false);
        
                return result?.SingleList() ?? CharacterData.EmptyList;
            }
        }

        public CharacterLogic Clone(PlayerLogic logic)
        {
            var clone = new CharacterLogic
            {
                PlayerLogic = logic,
                Characters = new List<CharacterData>()
            };

            Characters.ForEach(card =>
            {
                var character = card.Clone(clone);
                clone.Characters.Add(character);

                if (character.Index == Active.Index)
                    clone.Active = character;
            });

            return clone;
        }
    }
}
using System.Collections.Generic;
using System.Linq;
using Server.Logic.Event;
using Server.Managers;
using Server.ResolveLogic;
using Shared.Classes;
using Shared.Enums;
using Shared.Logic.Statuses;

namespace Server.GameLogic
{
    public class PlayerLogic
    {
        public ulong Id;
        public GameManager Game;
        public PlayerLogic Opponent;
        
        public ResourceLogic Resource;
        public DeckCardLogic DeckCard;
        public CharacterLogic CharacterLogic;
        public StatusLogic CombatStatuses;
        public StatusLogic SummonZone;
        public StatusLogic SupportZone;
        public SkillLogic SwitchActive;

        public Dictionary<string, CharacterData> CharactersMap;
        
        public CharacterData ActiveCharacter => CharacterLogic.Active;
        
        public PlayerLogic() { }

        public PlayerLogic(GameManager game, ulong id, DeckData deck)
        {
            Id = id;
            Game = game;
            
            Resource = new ResourceLogic(this, Game.Configuration);
            DeckCard = new DeckCardLogic(this, deck.actionCards);
            CharacterLogic = new CharacterLogic(this, deck.characters);
            CombatStatuses = new StatusLogic(this, StatusType.CombatStatus);
            SummonZone     = new StatusLogic(this, StatusType.Summon);
            SupportZone    = new StatusLogic(this, StatusType.Support);
            SwitchActive = SkillLogic.SwitchActive(this, deck.switchActive);
        }

        public void SetOpponent(PlayerLogic opponent)
        {
            Opponent = opponent;
            CharactersMap = CharacterLogic.Characters
                .Concat(Opponent.CharacterLogic.Characters)
                .ToDictionary(data => data.UniqueId, data => data);
        }

        public List<BaseEvent> CatchEvents(BaseEvent e, Timing timing, bool opponentAccessible = true)
        {
            var active = ActiveCharacter;
            var statusZones = new List<StatusLogic> { active.Statuses, CombatStatuses };

            var index = active.Index;
            var count = CharacterLogic.Characters.Count;
            for (var i = (index + 1) % count; i != index; i = (i + 1) % count)
                statusZones.Add(CharacterLogic.Characters[i].Statuses);

            statusZones.Add(SummonZone);
            statusZones.Add(SupportZone);

            var result = statusZones
                .SelectMany(zone => zone.Process(e, timing));
            
            if (opponentAccessible)
                result = result.Concat(Opponent.CatchEvents(e, timing, false));
            
            return result.ToList();
        }

        #region Misc
        
        public SkillLogic FindSkill(string characterId, string skillKey)
        {
            if (string.IsNullOrEmpty(characterId))
                return SwitchActive;
            
            var character = CharactersMap.GetValueOrDefault(characterId);
            if (character == null || !character.Skills.TryGetValue(skillKey, out var skill))
                return null;
            
            return skill;
        }

        public void MirrorSkill(SkillLogic origin, out SkillLogic skill)
        {
            skill = FindSkill(origin.Owner?.UniqueId ?? string.Empty, origin.Name);
        }

        public void MirrorCard(ActionCard origin, out ActionCard card)
        {
            card = DeckCard.Hand[origin.Timestamp];
        }

        public List<PlayerLogic> GetActiveDefeatedPlayers()
        {
            var players = new List<PlayerLogic>();
            
            if (!ActiveCharacter.IsAlive)
                players.Add(this);
            if (!Opponent.ActiveCharacter.IsAlive)
                players.Add(Opponent);

            return players;
        }
        
        public PlayerLogic Clone()
        {
            var clone = new PlayerLogic
            {
                Id = Id, 
                Game = Game
            };

            clone.SwitchActive = SwitchActive.Clone(null, clone);
            clone.CharacterLogic = CharacterLogic.Clone(clone);
            clone.CombatStatuses = CombatStatuses.Clone(clone);
            clone.DeckCard = DeckCard.Clone(clone);
            clone.Resource = Resource.Clone(clone);
            clone.SummonZone = SummonZone.Clone(clone);
            clone.SupportZone = SupportZone.Clone(clone);
            
            return clone;
        }

        #endregion
    }
}
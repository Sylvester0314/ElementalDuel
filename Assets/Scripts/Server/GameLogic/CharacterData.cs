using System;
using System.Collections.Generic;
using System.Linq;
using Shared.Enums;
using Shared.Handler;
using Shared.Logic.Statuses;
using Shared.Misc;
using SkillMap = System.Collections.Generic.Dictionary<
    string, 
    Server.GameLogic.SkillLogic
>;

namespace Server.GameLogic
{
    public class CharacterData : IEventTarget
    {
        public CharacterLogic Logic;

        public int Index;
        public string Name;
        public int MaxHealth;
        public int MaxEnergy;
        public CostType Element;
        
        public bool IsActive;
        public bool IsAlive;
        public int CurrentHealth;
        public int CurrentEnergy;
        public ElementalApplication Application;

        public StatusLogic Statuses;
        public SkillMap Skills;

        public string UniqueId { get; set; }
        public string EntityName => Name;
        public PlayerLogic Belongs => Logic.PlayerLogic;
        public int Damaged => MaxHealth - CurrentHealth;
        public bool Defeated => !IsAlive;

        public static List<CharacterData> EmptyList = Array.Empty<CharacterData>().ToList();

        public CharacterData() { }
        
        public CharacterData(CharacterLogic logic, CharacterAsset asset, int index)
        {
            Logic = logic;
            
            Index = index;
            UniqueId = Guid.NewGuid().ToString();
            Name = asset.characterName;
            MaxHealth = asset.baseMaxHealth;
            MaxEnergy = asset.baseMaxEnergy;
            Element = CostLogic.Map(asset.Properties);
            
            IsActive = false;
            IsAlive = true;
            CurrentHealth = MaxHealth;
            CurrentEnergy = 0;
            Application = ElementalApplication.None;

            Statuses = new StatusLogic(Belongs, StatusType.Status, this);
            Skills = asset.skillList
                .Select(skill => new SkillLogic(this, skill))
                .ToDictionary(skill => skill.Name, skill => skill);
        }

        public void ModifyEnergy(ref int amount)
        {
            amount = Math.Min(amount, MaxEnergy - CurrentEnergy);
            CurrentEnergy += amount;
        }

        public bool ModifyHealth(ref int amount, int sign)
        {
            var delta = amount * sign;
            if (CurrentHealth + delta > MaxHealth)
                amount = MaxHealth - CurrentHealth;
            
            CurrentHealth = Math.Max(CurrentHealth + delta, 0);
            return CurrentHealth == 0;
        }

        #region Relative Position

        private CharacterData NearByCharacter(int relative, bool mustStandby)
        {
            var count = Logic.Characters.Count;
            var index = (Index + relative + count) % count;

            CharacterData character = null;
            while (index != Index)
            {
                character = Logic.Characters[index];
                
                // If the next or previous character is not defeated,
                // return that character
                if (character.IsAlive)
                    return character;
                
                // Try to get the next or prev character instance
                index = (index + relative + count) % count;
            }

            // If there is no adjacent character that has not been defeated,
            // the result is returned based on whether to force the acquisition
            // of the standby character
            return mustStandby ? null : character;
        }

        public List<CharacterData> NextCharacter(bool mustStandby)
        {
            return NearByCharacter(1, mustStandby)?.SingleList() ?? EmptyList;
        }

        public List<CharacterData> PreviousCharacter(bool mustStandby)
        {
            return NearByCharacter(-1, mustStandby)?.SingleList() ?? EmptyList;
        }

        public List<CharacterData> AdjacentCharacters()
        {
            var set = new HashSet<CharacterData>();
            
            set.UnionWith(NextCharacter(true));
            set.UnionWith(PreviousCharacter(true));
            
            return set.ToList();
        }
        
        #endregion

        #region Misc

        public CharacterData Clone(CharacterLogic logic)
        {
            var clone = new CharacterData
            {
                Logic = logic,
                Index = Index,
                UniqueId = UniqueId,
                Name = Name,
                IsActive = IsActive,
                IsAlive = IsAlive,
                MaxHealth = MaxHealth,
                MaxEnergy = MaxEnergy,
                CurrentHealth = CurrentHealth,
                CurrentEnergy = CurrentEnergy,
                Application = Application,
                Element = Element,
                Skills = new SkillMap()
            };

            clone.Statuses = Statuses.Clone(logic.PlayerLogic, clone);
            
            foreach (var (key, skill) in Skills)
                clone.Skills.Add(key, skill.Clone(clone));
            
            return clone;
        }

        #endregion
    }
}
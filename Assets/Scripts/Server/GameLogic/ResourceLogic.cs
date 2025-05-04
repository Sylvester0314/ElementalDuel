using System;
using System.Collections.Generic;
using System.Linq;
using Shared.Classes;
using Shared.Enums;
using Shared.Misc;
using Unity.Netcode;
using GroupedDice = System.Collections.Generic.Dictionary<
    Shared.Enums.CostType, 
    System.Collections.Generic.List<Server.GameLogic.DiceLogic>
>;

namespace Server.GameLogic
{
    public enum DiceMode
    {
        Standard,
        SemiOmni,
        AllOmni
    }
    
    public class ResourceLogic
    {
        public PlayerLogic PlayerLogic;
        
        public int ArcaneEdict;
        public DiceMode Mode;
        public Dictionary<string, DiceLogic> Dices;
        
        public static List<CostType> DiceTypes = new ()
        {
            CostType.Cryo,      CostType.Hydro,
            CostType.Pyro,      CostType.Anemo,
            CostType.Electro,   CostType.Geo,
            CostType.Dendro,    CostType.Any
        };
        
        public HashSet<CostType> AliveElements => PlayerLogic.CharacterLogic.GetElementTypes(true);
        public HashSet<CostType> AllElements => PlayerLogic.CharacterLogic.GetElementTypes(false);
        
        public ResourceLogic() { }

        public ResourceLogic(PlayerLogic logic, RoomConfiguration configuration)
        {
            PlayerLogic = logic;
            ArcaneEdict = 1;
            Dices = new Dictionary<string, DiceLogic>();
    
            Mode = configuration.diceMode switch
            {
                "dice_mode_standard" => DiceMode.Standard,
                "dice_mode_all_omni" => DiceMode.AllOmni,
                _ => DiceMode.SemiOmni
            };
        }

        #region Operation

        public List<DiceLogic> Roll()
        {
            if (Mode == DiceMode.AllOmni)
                AllOmni();
            else if (Mode == DiceMode.SemiOmni)
                SemiOmni();
            else
                Standard();

            return Dices.Values.ToList();
        }
        
        public List<DiceLogic> Reroll(List<string> targets)
        {
            var dices = targets.Select(id => Dices[id]).ToList();

            foreach (var dice in dices)
            {
                var type = RandomType(false);
                dice.ModifyType(type);
            }
            
            return dices;
        }

        public void Append(CostType type)
        {
            var dice = new DiceLogic(this, type);
            Dices.Add(dice.Id, dice);
        }

        public void Append(List<DiceLogic> dices)
        {
            dices.ForEach(dice => Dices.Add(dice.Id, dice));
        }

        public List<DiceLogic> Remove(
            List<string> ids, List<CostUnion> unions, out int arcane, out int energy
        )
        {
            arcane = GetSpecialCost(unions, CostType.Legend).FirstOrDefault()?.count ?? 0;
            energy = GetSpecialCost(unions, CostType.Energy).FirstOrDefault()?.count ?? 0;

            ArcaneEdict -= arcane;
            PlayerLogic.ActiveCharacter.CurrentEnergy -= energy;

            return ids.Select(id => Dices.RemoveAndGet(id)).ToList();
        }

        public void Clear()
        {
            Dices.Clear();
        }

        private void AllOmni()
        {
            for (var i = 0; i < 8; i++)
                Append(CostType.Any);
        }

        private void SemiOmni()
        {
            for (var i = 0; i < 4; i++)
            {
                Append(CostType.Any);
                Append(RandomType(false));
            }
        }

        private void Standard()
        {
            for (var i = 0; i < 8; i++)
                Append(RandomType(false));
        }

        private CostType RandomType(bool isBasic)
        {
            var max = isBasic ? 7 : 8;
            var index = PlayerLogic.Game.Random.NextInt(max);
            return DiceTypes[index];
        }

        #endregion

        #region Match and Check
        
        public ResourceMatchedResult Match(CostLogic cost)
        {
            var result = new ResourceMatchedResult();
            
            var energyCost = GetSpecialCost(cost.Actual, CostType.Energy);
            if (energyCost.Count != 0)
            {
                var active = PlayerLogic.ActiveCharacter;
                if (active.CurrentEnergy < energyCost[0].count)
                    return new ResourceMatchedResult(MatchedResultType.InsufficientEnergy);

                result.energy = energyCost[0].count;
            }
            
            if (GetSpecialCost(cost.Actual, CostType.Legend).Count != 0)
            {
                if (PlayerLogic.Resource.ArcaneEdict >= 1)
                    return new ResourceMatchedResult(MatchedResultType.InsufficientLegend);

                result.legend = true;
            }

            MatchResourceCost(cost, Dices.Values.ToList(), AliveElements, ref result);
            
            return result;
        }

        public bool Check(CostLogic cost, List<string> selected)
        {
            var dices = selected
                .Select(id => Dices.GetValueOrDefault(id))
                .Where(dice => dice != null)
                .ToList();

            if (dices.Count != cost.GetTotalDicesCost(false))
                return false;
        
            var result = new ResourceMatchedResult();

            return MatchResourceCost(cost, dices, AliveElements, ref result);
        }

        #endregion

        #region Sub-determination

        private bool MatchResourceCost(
            CostLogic cost, List<DiceLogic> dices, HashSet<CostType> elements,
            ref ResourceMatchedResult result
        )
        {
            var grouped = dices
                .GroupBy(v => v.Type)
                .ToDictionary(
                    v => v.Key,
                    v => v.ToList()
                );

            var success = cost.Actual
                .Where(union => union.type is not (CostType.Energy or CostType.Legend))
                .All(union =>
                {
                    if (union.count == 0)
                        return true;

                    return union.type switch
                    {
                        CostType.Same => MatchSameDice(grouped, elements, union.count),
                        CostType.Diff => MatchDiffDice(dices, union.count),
                        _ => MatchSpecificTypeDice(grouped, union.type, union.count)
                    };
                });

            result.dices = dices
                .Where(dice => dice.Choosing)
                .Select(dice => dice.Id)
                .ToList();
            result.type = success 
                ? MatchedResultType.Successfully 
                : MatchedResultType.InsufficientDice;
        
            dices.ForEach(v => v.Choosing = false);
            
            return result.Success;
        }
        
        private static bool MatchSameDice(GroupedDice grouped, HashSet<CostType> elements, int required)
        {
            var sortedElements = new List<CostType>();
            for (var i = 0; i < 7; i++)
            {
                var type = (CostType)i;
                
                // Prioritize using dice of element types that do not exist in our character
                if (!elements.Contains(type) && grouped.Keys.Contains(type))
                    sortedElements.Add(type);
            }

            var aliveElementList = elements.Where(type => grouped.Keys.Contains(type)).ToList();
            sortedElements.AddRange(aliveElementList);

            // Take out the omni element
            var omni = GetDicesByType(grouped, CostType.Any);

            if (sortedElements.Count == 0)
            {
                if (omni.Count < required)
                    return false;

                for (var i = 0; i < required; i++)
                    omni[i].Choosing = true;
                return true;
            }
            
            // Two ways to obtain sets of dice of the same color
            // [1] If the number of dice of an element type is not less than the
            // required amount, then that type is preferred
            List<DiceLogic> selectedList = null;
            // [2] If it is less than the required amount, use omni element instead
            List<DiceLogic> alternativeList = null;
            var alternativeOmniUsed = 100;
            
            foreach (var type in sortedElements)
            {
                // Get the list of dice of the current type
                if (!grouped.TryGetValue(type, out var willSelectList))
                    continue;
                var count = willSelectList.Count;

                // Process [1]
                if (count >= required && selectedList == null)
                    selectedList = willSelectList;
                
                // Process [2], and only when the number of omni elements used in the current
                // type of element dice is less than the current solution, will replace result
                else if (count + omni.Count >= required && required - count < alternativeOmniUsed)
                {
                    alternativeList = willSelectList;
                    alternativeOmniUsed = required - count;
                }
            }

            // If there is a matching result for [1], return the result
            if (selectedList != null)
            {
                for (var i = 0; i < required; i++)
                    selectedList[i].Choosing = true;
                return true;
            }

            // If there is no matching result for [2], return a matching failure
            if (alternativeList == null)
                return false;

            for (var i = 0; i < alternativeOmniUsed; i++)
                omni[i].Choosing = true;
            for (var i = 0; i < required - alternativeOmniUsed; i++)
                alternativeList[i].Choosing = true;

            return true;
        }

        private static bool MatchDiffDice(List<DiceLogic> dices, int required)
        {
            var notChoosing = dices
                .OrderBy(dice => dice.Weight)
                .Where(dice => !dice.Choosing)
                .ToList();
            if (notChoosing.Count < required)
                return false;

            for (var i = 0; i < required; i++)
                notChoosing[i].Choosing = true;
            return true;
        }

        private static bool MatchSpecificTypeDice(GroupedDice grouped, CostType type, int required)
        {
            var current = GetDicesByType(grouped, type);
            var currentCount = current.Count;
            
            // Omni elements require additional process. If the current type is omni, 
            // then the value is 0
            var omni = GetDicesByType(grouped, CostType.Any);
            var omniCount = type == CostType.Any ? 0 : omni.Count;
        
            // If the total number of element dice that meet the criteria is lower than
            // the required number, return a matching failure
            if (currentCount + omniCount < required)
                return false;

            for (var i = 0; i < Math.Min(required, currentCount); i++)
                current[i].Choosing = true;
            for (var i = 0; i < required - currentCount; i++)
                omni[i].Choosing = true;

            return true;
        }
        
        #endregion

        #region Misc
        
        public static List<CostUnion> GetSpecialCost(List<CostUnion> unions, CostType type)
            => unions.Where(v => v.type == type).ToList();
        
        public static List<DiceLogic> GetDicesByType(GroupedDice grouped, CostType type)
            => grouped.TryGetValue(type, out var list)
                ? list.Where(v => !v.Choosing).ToList()
                : Array.Empty<DiceLogic>().ToList();

        public ResourceLogic Clone(PlayerLogic logic)
        {
            var clone = new ResourceLogic
            {
                PlayerLogic = logic,
                ArcaneEdict = ArcaneEdict,
                Mode = Mode,
                Dices = new Dictionary<string, DiceLogic>()
            };

            foreach (var (id, dice) in Dices)
                clone.Dices.Add(id, dice.Clone(clone));

            return clone;
        }

        public NetworkResource Networking()
            => new ()
            {
                ArcaneEdict = ArcaneEdict,
                Ids = Dices.Keys.ToList(),
                Types = Dices.Values
                    .Select(dice => dice.Type)
                    .Packing<CostType, Enum<CostType>>()
            };
        
        #endregion
    }
    
    public class NetworkResource : INetworkSerializable, IEquatable<NetworkResource>
    {
        public int ArcaneEdict;
        public List<string> Ids;
        public List<Enum<CostType>> Types;
        
        public void NetworkSerialize<T>(BufferSerializer<T> serializer) where T : IReaderWriter
        {
            serializer.SerializeValue(ref ArcaneEdict);
            
            NetCodeMisc.SerializeList(serializer, ref Ids);
            NetCodeMisc.SerializeList(serializer, ref Types);
        }

        public bool Equals(NetworkResource other)
        {
            if (ReferenceEquals(null, other))
                return false;

            return ArcaneEdict == other.ArcaneEdict &&
                   Ids.Equals(other.Ids) &&
                   Types.Equals(other.Types);
        } 
    }
}
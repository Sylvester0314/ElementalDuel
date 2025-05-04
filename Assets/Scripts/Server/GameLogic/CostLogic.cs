using System;
using System.Collections.Generic;
using System.Linq;
using Shared.Classes;
using Shared.Enums;
using Shared.Misc;
using Unity.Netcode;
using UnityEngine;

namespace Server.GameLogic
{
    public class CostLogic : INetworkSerializable, IEquatable<CostLogic>
    {
        public List<CostUnion> Original;
        public List<CostUnion> Actual;

        public CostLogic()
        {
            Original = new List<CostUnion>();
            Actual = new List<CostUnion>();
        }
        
        public CostLogic(List<CostUnion> costs)
        {
            Original = costs;
            
            Actual = new List<CostUnion>();
            foreach (var union in Original)
                Actual.Add(union.Clone());
        }

        #region Server Logic

        public int GetTotalCost(bool isOriginal = true)
        {
            var costs = isOriginal ? Original : Actual;
            return costs.Sum(cost => cost.count);
        }
        
        public int GetTotalDicesCost(bool isOriginal = true)
        {
            var costs = isOriginal ? Original : Actual;
            return costs
                .Where(cost => cost.type is not (CostType.Legend or CostType.Energy))
                .Sum(cost => cost.count);
        }

        public void ResetActualCost()
        {
            for (var i = 0; i < Original.Count; i++)
                Actual[i].count = Original[i].count;
        }

        #endregion
        
        #region Client Tools

        public static int MaxCostType = 3;
        public static readonly Color[] Colors =
        {
            new(88, 240, 129, 255),
            Color.white,
            new(255, 109, 109, 255)
        };
        
        public void RefreshCostDisplay(CostSetComponent component)
        {
            var nodes = component.costComponentList;
            var numberOfTypes = Original.Count;
            for (var i = 0; i < MaxCostType; i++)
            {
                var costNode = nodes[i];
                if (i >= numberOfTypes)
                    costNode.gameObject.SetActive(false);
                else
                {
                    var type = Original[i].Compare(Actual[i]);
                    var color = Colors[type + 1];
                    costNode.gameObject.SetActive(true);
                    costNode.SetInformation((Actual[i], color));
                }
            }
        }
        
        public override string ToString()
        {
            var pattern = ResourceLoader.GetLocalizedUIText("pay_dice_extend");
            var list = Actual
                .Where(cost => cost.count != 0)
                .Select(cost =>
                {
                    var count = cost.count;
                    var type = ResourceLoader.GetLocalizedUIText(
                        cost.type.ToString().ToLower() + "_name"
                    );
                    return pattern
                        .Replace("$[Number]", count.ToString())
                        .Replace("$[Sub]", type);
                })
                .ToList();
        
            return string.Join("+", list);
        }

        public bool PureDiceUsed()
        {
            return !Original.Any(union => union.type is CostType.Energy or CostType.Legend);
        }

        #endregion
        
        #region Misc

        public void NetworkSerialize<T>(BufferSerializer<T> serializer) where T : IReaderWriter
        {
            NetCodeMisc.SerializeList(serializer, ref Original);
            NetCodeMisc.SerializeList(serializer, ref Actual);
        }
        
        public bool Equals(CostLogic other)
        {
            if (ReferenceEquals(other, null))
                return false;
        
            return Original.Equals(other.Original) && Actual.Equals(other.Actual);
        }
        
        public static CostType Map(List<Property> properties)
            => properties
                .Select(property => property switch
                {
                    Property.ElementCryo => CostType.Cryo,
                    Property.ElementHydro => CostType.Hydro,
                    Property.ElementPyro => CostType.Pyro,
                    Property.ElementElectro => CostType.Electro,
                    Property.ElementAnemo => CostType.Anemo,
                    Property.ElementGeo => CostType.Geo,
                    Property.ElementDendro => CostType.Dendro,
                    _ => CostType.None
                })
                .FirstOrDefault(type => type != CostType.None);

        public CostLogic Clone()
        {
            var clone = new CostLogic
            {
                Original = Original,
                Actual = new List<CostUnion>()
            };

            foreach (var union in Actual)
                clone.Actual.Add(union.Clone());

            return clone;
        }

        #endregion
    }
}
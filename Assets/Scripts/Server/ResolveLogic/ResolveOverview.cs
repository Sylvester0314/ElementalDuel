using System;
using System.Collections.Generic;
using System.Linq;
using Server.GameLogic;
using Shared.Enums;
using Shared.Logic.Statuses;
using Shared.Misc;
using Unity.Netcode;

namespace Server.ResolveLogic
{
    public class ResolveOverview : INetworkSerializable, IEquatable<ResolveOverview>
    {
        public Dictionary<string, CharacterModification> Modifications;
        public Dictionary<string, StatusModification> StatusModifications;

        public ResolveOverview()
        {
            Modifications = new Dictionary<string, CharacterModification>();
            StatusModifications = new Dictionary<string, StatusModification>();
        }

        public ResolveOverview(PlayerLogic logic)
        {
            Modifications = logic.CharactersMap.ToDictionary(
                pair => pair.Key,
                pair => new CharacterModification(pair.Value.Application)
            );
            StatusModifications = new Dictionary<string, StatusModification>();
        }
        
        public void NetworkSerialize<T>(BufferSerializer<T> serializer) where T : IReaderWriter
        {
            NetCodeMisc.SerializeDictionary(serializer, ref Modifications);
            NetCodeMisc.SerializeDictionary(serializer, ref StatusModifications);
        }

        public bool Equals(ResolveOverview other)
        {
            if (ReferenceEquals(null, other))
                return false;

            return Modifications.Equals(other.Modifications) &&
                   StatusModifications.Equals(other.StatusModifications);
        }
    }
    
    public class CharacterModification : INetworkSerializable, IEquatable<CharacterModification>
    {
        public int HealthModified;
        public int EnergyModified;
        public bool DamageTook;
        public bool HealReceived;
        public bool Defeated;
        public bool SwitchedTarget;
        public bool AppliedNewApplication;
        public List<Enum<ElementalApplication>> Applications;

        public bool Modified;
        public ElementalApplication CurrentApplication;

        public CharacterModification()
        {
            Applications = new List<Enum<ElementalApplication>>();
        }

        public CharacterModification(ElementalApplication initial)
        {
            CurrentApplication = initial;
            Applications = new List<Enum<ElementalApplication>>();
        }

        public void AppendApplications(ElementalApplication first, ElementalApplication second)
        {
            Applications.Add(new Enum<ElementalApplication>(first));
            Applications.Add(new Enum<ElementalApplication>(second));
        }

        public void MergeInitialApplication()
        {
            if (Applications.Count == 0)
                Applications.Add(new Enum<ElementalApplication>(CurrentApplication));
        }
        
        public void NetworkSerialize<T>(BufferSerializer<T> serializer) where T : IReaderWriter
        {
            serializer.SerializeValue(ref HealthModified);
            serializer.SerializeValue(ref EnergyModified);
            serializer.SerializeValue(ref HealReceived);
            serializer.SerializeValue(ref DamageTook);
            serializer.SerializeValue(ref Defeated);
            serializer.SerializeValue(ref SwitchedTarget);
            serializer.SerializeValue(ref AppliedNewApplication);
            
            NetCodeMisc.SerializeList(serializer, ref Applications);
        }

        public bool Equals(CharacterModification other)
        {
            if (ReferenceEquals(null, other))
                return false;

            return HealthModified == other.HealthModified && 
                   EnergyModified == other.EnergyModified &&
                   DamageTook == other.DamageTook &&
                   HealReceived == other.HealReceived &&
                   Defeated == other.Defeated &&
                   SwitchedTarget == other.SwitchedTarget &&
                   AppliedNewApplication == other.AppliedNewApplication &&
                   Applications.Equals(other.Applications);
        }
    }

    public class StatusModification : INetworkSerializable, IEquatable<StatusModification>
    {
        public List<Status> Statuses;

        public StatusModification()
        {
            Statuses = new List<Status>();
        }
        
        public void NetworkSerialize<T>(BufferSerializer<T> serializer) where T : IReaderWriter
        {
            NetCodeMisc.SerializeList(serializer, ref Statuses);
        }

        public bool Equals(StatusModification other)
        {
            if (ReferenceEquals(null, other))
                return false;

            return Statuses.Equals(other.Statuses);
        }
    }
}
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Shared.Misc;
using Unity.Netcode;

namespace Shared.Classes
{
    [Serializable]
    public class RawDeckData: INetworkSerializable, IEquatable<RawDeckData>
    {
        public string uniqueId;
        public bool isUsing;
        public string deckName;
        public string basePreset;
        public List<string> characterCards = new ();
        public List<string> actionCards = new ();

        public async Task<DeckData> Parse()
        {
            var charsTask = await Task.WhenAll(
                characterCards.Select(
                    async name => name.Length == 0 ? null 
                        : await ResourceLoader.LoadSoAsset<CharacterAsset>(name)
                ));
            var cardsTask = await Task.WhenAll(
                actionCards.Select(
                    async name => await ResourceLoader.LoadSoAsset<ActionCardAsset>(name)
                ));
            var switchActive = await ResourceLoader.LoadSoAsset<SkillAsset>("10000");
            
            return new DeckData
            {
                UniqueId = new Guid(uniqueId),
                isUsing = isUsing,
                deckName = deckName,
                basePreset = basePreset,
                switchActive = switchActive,
                characters = charsTask.ToList(),
                actionCards = cardsTask.ToList()
            };
        }
        
        public void NetworkSerialize<T>(BufferSerializer<T> serializer) where T : IReaderWriter
        {
            serializer.SerializeValue(ref uniqueId);
            serializer.SerializeValue(ref isUsing);
            serializer.SerializeValue(ref deckName);
            serializer.SerializeValue(ref basePreset);
            
            NetCodeMisc.SerializeList(serializer, ref characterCards);
            NetCodeMisc.SerializeList(serializer, ref actionCards);
        }

        public bool Equals(RawDeckData other)
        {
            if (ReferenceEquals(null, other))
                return false;

            return uniqueId == other.uniqueId;
        }
    }
    
    [Serializable]
    public class DeckData
    {
        public Guid UniqueId;
        public bool isUsing;
        public string deckName;
        public string basePreset;
        public SkillAsset switchActive;
        public List<CharacterAsset> characters;
        public List<ActionCardAsset> actionCards;
        
        public RawDeckData ToRaw()
        {
            return new RawDeckData
            {
                uniqueId = UniqueId.ToString(),
                isUsing = isUsing,
                deckName = deckName,
                basePreset = basePreset,
                actionCards = actionCards.Select(card => card.name).ToList(),
                characterCards = characters.Select(
                    card => card?.name ?? string.Empty
                ).ToList()
            };
        }

        public DeckData Copy()
        {
            return new DeckData
            {
                UniqueId = Guid.NewGuid(),
                isUsing = false,
                deckName = deckName,
                basePreset = basePreset,
                switchActive = switchActive,
                characters = new List<CharacterAsset>(characters),
                actionCards = new List<ActionCardAsset>(actionCards)
            };
        }

        public bool CheckActionCards()
        {
            return actionCards.Count == 30;
        }

        public bool CheckCharacterCards()
        {
            return !characters.Contains(null);
        }

        public bool CheckInvalidActionCards()
        {
            return actionCards.All(card => card.IsValid(characters));
        }

        public bool CheckSaveFeasibility()
        {
            return CheckActionCards() &&
                   CheckCharacterCards() &&
                   CheckInvalidActionCards();
        }
    }
}
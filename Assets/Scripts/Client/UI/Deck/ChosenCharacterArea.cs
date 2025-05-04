using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class ChosenCharacterArea : MonoBehaviour
{
    public List<ChosenDeckCharacter> slots = new();

    public int Count
    {
        get
        {
            return slots.Count(slot => slot.Count != 0);
        }
    }
    
    public bool TryGetValue(string key, out ChosenDeckCharacter card)
    {
        card = slots.Find(character => character.Asset?.name == key);
        return card != null;
    }
    
    public void Append(CharacterAsset asset, int targetSlotIndex = -1)
    {
        if (asset == null)
            return;
        
        var hasAdded = TryGetValue(asset.name, out _);
        if (hasAdded)
        {
            BuildDeckContainer.Instance.DisplayHint("character_card_can_only_have_1");
            return;
        }
        
        var slot = targetSlotIndex != -1
            ? slots[targetSlotIndex]
            : slots
                .Where(slot => slot.Count == 0)
                .FirstOrDefault();

        if (slot == null)
        {
            BuildDeckContainer.Instance.DisplayHint("card_limited_exceeded");
            return;
        }
        
        slot.emptyStatus.SetActive(false);
        slot.avatar.gameObject.SetActive(true);
        slot.SetAvatarDisplay(asset);
    }
    
    public void SetChoosingCount(int count, CharacterAsset asset)
    {
        BuildDeckContainer.Instance.inventoryArea
            .SearchInventoryCard(asset.name)
            ?.SetChoosingCount(count, asset.name);
    }

    public void Clear()
    {
        slots
            .Where(character => character.Count != 0)
            .ToList()
            .ForEach(character => character.Count = 0);
    }

    public void RefreshCount()
    {
        BuildDeckContainer.Instance.characterCount.text = $"{Count}/3";
    }

    public List<CharacterAsset> ToList()
    {
        return slots
            .Select(chosen => chosen.Count == 0 ? null : chosen.Asset)
            .ToList();
    }
}
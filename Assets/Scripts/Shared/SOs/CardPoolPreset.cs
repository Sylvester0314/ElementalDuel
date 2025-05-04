using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "New Option", menuName = "Custom/New Preset")]
public class CardPoolPreset : ScriptableObject
{
    public List<CharacterAsset> characterCards;
    public List<ActionCardAsset> actionCards;
}
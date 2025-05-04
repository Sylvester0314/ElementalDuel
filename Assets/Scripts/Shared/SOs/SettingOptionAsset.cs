using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "New Option", menuName = "Custom/New Option")]
public class SettingOptionAsset : ScriptableObject
{
    public string titleEntry;
    public List<string> options;
}

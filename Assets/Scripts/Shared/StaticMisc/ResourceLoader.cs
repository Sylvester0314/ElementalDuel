using System;
using System.Collections.Generic;
using Shared.Enums;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.Localization;
using UnityEngine.ResourceManagement.AsyncOperations;

public static class ResourceLoader
{
    public static string GetCostSpritePath(CostType type, string suffix = "")
    {
        var typeString = type.ToString();
        return "Assets/Sources/UI/Costs/Cost_" + typeString + suffix + ".png";
    }

    public static async Task<Sprite> LoadSprite(string path)
    {
        var handle = Addressables.LoadAssetAsync<Sprite>(path);
        await handle.Task;
        if (handle.Status == AsyncOperationStatus.Succeeded)
            return handle.Result;
        else
        {
            Debug.LogError("Failed to load image at path: " + path);
            return null;
        }
    }
    
    public static async Task<Sprite> TryLoadSprite(string path)
    {
        var handle = Addressables.LoadAssetAsync<Sprite>(path);
    
        await handle.Task;
        return handle.Status == AsyncOperationStatus.Succeeded
            ? handle.Result : null;
    }

    private static readonly Dictionary<Type, string> AssetPathMap = new()
    {
        { typeof(ActionCardAsset),  "ActionCards" },
        { typeof(CharacterAsset),   "CharacterCards" },
        { typeof(StatusCardAsset),  "StatusCards" },
        { typeof(CardPoolPreset),   "CardPoolPreset" },
        { typeof(SkillAsset),       "Skills" }
    };
    
    public static async Task<T> LoadSoAsset<T>(string name) where T : ScriptableObject
    {
        var type = AssetPathMap.GetValueOrDefault(typeof(T), "");
        var path = $"Assets/SOAssets/{type}/{name}.asset";
        var handle = Addressables.LoadAssetAsync<T>(path);
        await handle.Task;
        
        if (handle.Status == AsyncOperationStatus.Succeeded)
            return handle.Result;
        else
        {
            Debug.LogError("Failed to load asset at path: " + path);
            return null;
        } 
    }
    
    public static string GetLocalizedValue(string reference, string entry)
    {
        var localizedString = new LocalizedString();
        localizedString.TableReference = reference;
        localizedString.TableEntryReference = entry;
        var value = localizedString.GetLocalizedString();
        return value;
    }

    public static string GetLocalizedUIText(string entry)
        => GetLocalizedValue("UIText", entry);
    
    public static string GetLocalizedCard(string entry)
        => GetLocalizedValue("CardDescription", entry);
}

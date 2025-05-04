using System;
using System.Threading.Tasks;
using DG.Tweening;
using Shared.Classes;
using Shared.Misc;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class RoomItem : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    public string id;
    
    [Header("Components")]
    public TextMeshProUGUI roomName;
    public TextMeshProUGUI playerCountText;
    public Image roomImage;
    public RectTransform gameBaseConfig;
    public RectTransform gameOptions;
    public LittleButton button;
    
    [Header("Prefabs References")]
    public TagItem tagPrefab;

    public async void Initialize(RoomInformation information, PlayerData owner, Action<string> callback)
    {
        id = information.roomId;
        
        var path = $"Assets/Sources/Avatars/Avatar_{owner.metadata.avatar}.png";
        roomName.text = owner.username;
        roomImage.sprite = await ResourceLoader.LoadSprite(path);
        
        playerCountText.text = $"{information.playerCount}/2";
        button.Callback = () =>
        {
            if (information.playerCount >= 2)
            {
                Handbook.Instance.popUps
                    .Create<PopUps>("pop_hint")
                    .AppendText("room_full_now")
                    .Display();
                return;
            }
            
            button.text.text = ResourceLoader.GetLocalizedUIText("loading_room");
            PlayerPrefs.SetInt("HandbookContainer", 1);
            PlayerPrefs.Save();
            callback.Invoke(id);
        };
        
        information.BaseConfigs.ForEach(pair =>
        {
            Instantiate(tagPrefab, gameBaseConfig, false)
                .Initialize(ParseTag(pair.Key, pair.Entry));
        });
        
        information.Options.ForEach(pair =>
        {
            Instantiate(tagPrefab, gameOptions, false)
                .Initialize(ParseTag(pair.Key, pair.Entry));
        });
        
        LayoutRebuilder.ForceRebuildLayoutImmediate(gameBaseConfig);
        LayoutRebuilder.ForceRebuildLayoutImmediate(gameOptions);
    }

    private ValueTuple<string, int, bool> ParseTag(string key, string value)
    {
        var colorIndex = key switch
        {
            "CardPoolPreset" or "GameMode" => 1,
            "DiceMode" => 2,
            "ContemplationTime" => 3,
            _ => 4
        };

        if (key != "ContemplationTime")
        {
            var entry = "rih_" + value;
            return (entry, colorIndex, true);
        }

        var text = value switch
        {
            "thinking_time_capped_70_plus_25" => "70+25s",
            "thinking_time_120_plus_10" => "120+10s",
            "thinking_time_20_plus_5" => "20+5s",
            "thinking_time_fixed_240" => "240s",
            _ => string.Empty
        };
        
        return (text, colorIndex, false);
    }
    
    public void OnPointerEnter(PointerEventData eventData)
    {
        transform
            .DOScale(Vector3.one * 1.025f, 0.15f)
            .SetEase(Ease.OutExpo);
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        transform
            .DOScale(Vector3.one, 0.15f)
            .SetEase(Ease.OutExpo);
    }
}

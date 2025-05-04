using DG.Tweening;
using Shared.Misc;
using TMPro;
using UnityEngine;
using UnityEngine.Localization.Components;
using UnityEngine.UI;

public class NameBar : MonoBehaviour
{
    public CanvasGroup canvas;
    public Image avatar;
    public Image background;
    public GameObject loserMask;
    public GameObject preparedIcon;
    public TextMeshProUGUI playerName;
    public LocalizeStringEvent statusEvent;

    [Header("Game Scene Components")] 
    public Legend legend;
    public RectTransform arrow;
    public Tween ArrowTween;
    
    public bool roomFirstLoad;

    [Header("References")] 
    public Sprite prince;
    
    public async void Initialize(PlayerData data)
    {
        var path = $"Assets/Sources/Avatars/Avatar_{data.metadata.avatar}.png";
        playerName.text = data.username;
        avatar.sprite = await ResourceLoader.LoadSprite(path);
        roomFirstLoad = true;
    }
    
    public void SetNoPlayerStyle()
    {
        avatar.sprite = prince;
        playerName.text = "NoName";
        statusEvent.SetEntry("namebar_status_waiting_player");
        preparedIcon.SetActive(false);
    }

    public void Fade(bool display, bool setActive = false, float duration = 0.25f)
    {
        var alpha = display ? 1 : 0;
        canvas
            .DOFade(alpha, duration)
            .SetEase(Ease.OutQuint)
            .OnComplete(() => 
            {
                if (setActive)
                    gameObject.SetActive(display);
            });
    }

    public void SetGameResult(bool isWinner)
    {
        loserMask.SetActive(!isWinner);
        statusEvent.SetEntry("namebar_game_over");
    }
}
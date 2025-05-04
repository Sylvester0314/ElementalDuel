using System;
using System.Collections.Generic;
using DG.Tweening;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using NetworkManager = Unity.Netcode.NetworkManager;

public class Handbook : MonoBehaviour
{
    public static Handbook Instance;
    public List<AbstractHandbookContainer> containers;

    [Header("In Game Data")] 
    public AbstractHandbookContainer choosingContainer;

    [Header("Components")] 
    public RectTransform rightButton;
    public Image buttonImage;
    public Action ButtonClickCallback;
    public PopUpsInjector popUps;
    public SceneLoader sceneLoader;

    [Header("Style Settings")] 
    public Color defaultColor;
    public Color clickingColor;
    public AnimationCurve curve;
    
    public const float Duration = 0.5f;
    private float _lastClickTime = -1;

    public void Awake()
    {
        if (Instance != null && Instance != this)
            Destroy(gameObject);
        else
            Instance = this;
    }
    
    public void Start()
    {
        var index = PlayerPrefs.GetInt("HandbookContainer", 0);
        containers[index].Open();
    }

    public void SwitchContainerDisplay(int index)
    {
        containers[index].Open();
    }

    #region Network

    public void LoginSuccessfully()
    {
        NetworkManager.Singleton.StartClient();
    }

    #endregion

    #region Interactive

    public void HideButton()
    {
        rightButton.DOAnchorPosX(-135, Duration).SetEase(Ease.OutExpo);
    }

    public void ShowButton()
    {
        rightButton.DOAnchorPosX(-20, Duration).SetEase(curve);
    }

    public void ClickButtonIconDown(BaseEventData eventData)
    {
        buttonImage.color = clickingColor;
    }

    public void ClickButtonIconUp(BaseEventData eventData)
    {
        buttonImage.color = defaultColor;
    }

    public void ClickButtonIconClick(BaseEventData eventData)
    {
        var current = Time.time;
        if (current - _lastClickTime < 0.6f)
            return;

        _lastClickTime = current;
        ButtonClickCallback?.Invoke();
    }

    #endregion
}
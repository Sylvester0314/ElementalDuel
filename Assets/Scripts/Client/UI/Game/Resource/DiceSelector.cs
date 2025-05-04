using DG.Tweening;
using Server.GameLogic;
using UnityEngine;

public class DiceSelector : MonoBehaviour
{
    public Transform dices;
    public DiceFunction function;

    [Header("Prefab References")]
    public LargeDice dicePrefab;

    [Header("In Game Data")]
    public LargeDice prevClickedDice;
    public int maxChoosing;
    public int currentChoosing;
    
    public LargeDice CreateDice(DiceLogic logic)
    {
        var instance = Instantiate(dicePrefab);
        instance.Initialize(logic.Type, this);
        return instance;
    }

    public bool TryAppendDice(bool isSelecting)
    {
        currentChoosing += !isSelecting ? 1 : -1;
        if (currentChoosing <= maxChoosing)
            return !isSelecting;

        foreach (var entity in function.DiceEntities)
            entity.SetSelectStatus(false);
        currentChoosing = 1;
        return true;
    }
    
    public void OpenBackground()
    {
        gameObject.SetActive(true);
        function.displayer.gameObject.SetActive(false);
        dices.localScale = Vector3.one * function.Count switch
        {
            > 12 and <= 16 => 0.8f,
            > 8 and <= 12 => 0.9f,
            _ => 1
        };
        transform.DOLocalMove(Vector3.zero, 0.25f).SetEase(Ease.OutExpo);
    }

    public void CloseBackground()
    {
        gameObject.SetActive(false);
        function.displayer.gameObject.SetActive(true);
        transform.localPosition = Vector3.right * 10.5f;
        function.displayer.normalBackground.Close();
    }

    public void SwitchToDisplayerExtraMode()
    {
        CloseBackground();
        prevClickedDice?.SetScale(1);
        prevClickedDice = null;
        
        function.displayer.gameObject.SetActive(true);
        function.displayer.normalBackground.Open();
    }
}

using Server.GameLogic;
using Shared.Enums;
using UnityEngine;

public class DiceDisplayer : MonoBehaviour
{
    public Canvas rootCanvas;
    public RectTransform checker;
    public DiceDisplayerHeader header;
    public Transform dices;
    public DiceFunction function;
    
    public ExtraBackground normalBackground;
    public ExtraBackground tuningBackground;
    public ExtraBackground tuningDisableBackground;
    
    [Header("Prefab References")]
    public SmallDice dicePrefab;
    
    public SmallDice CreateDice(DiceLogic logic)
    {
        var instance = Instantiate(dicePrefab);
        instance.Initialize(logic.Type);
        return instance;
    }
    
    public void SwitchToSelectorOnlyMode()
    {
        normalBackground.Close();
        function.selector.OpenBackground();
    }
}

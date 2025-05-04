using System.Threading.Tasks;
using Shared.Enums;
using UnityEngine;
using UnityEngine.EventSystems;

public class LargeDice : AbstractDice, 
    IPointerClickHandler, IPointerEnterHandler, 
    IPointerExitHandler, IPointerDownHandler
{
    public GameObject hover;
    public GameObject locking;
    public CanvasGroup canvas;

    [Header("In Game Data")]
    public bool isLocking;
    public bool interactable = true;
    public bool enterSelectable;

    private DiceSelector _selector;
    private DiceEntity _entity;

    public LargeDice SetEntity(DiceEntity entity)
    {
        _entity = entity;
        return this;
    }
    
    public async void Initialize(CostType type, DiceSelector s)
    {
        await SetStyle(type);
        _selector = s;
    }

    public void SetInteractable(bool value)
    {
        interactable = value;
    }

    public void SetEnterSelectable(bool value)
    {
        enterSelectable = value;
    }

    public async Task SetStyle(CostType type)
    {
        var path = ResourceLoader.GetCostSpritePath(type);
        var dice = await ResourceLoader.LoadSprite(path);
        background.sprite = dice;
        icon.gameObject.SetActive(type != CostType.None);

        if (type == CostType.None)
            return;
        
        var element = await ResourceLoader.LoadSprite(
            $"Assets/Sources/UI/Costs/Cost_Icon_{type.ToString()}.png"
        );
        icon.sprite = element;
    }

    public void SetScale(float scale)
    {
        transform.localScale = Vector3.one * scale;
    }
    
    public void OnPointerClick(PointerEventData eventData)
    {
        if (isLocking || !interactable || enterSelectable)
            return;

        // Only enabled during Reroll Phase
        if (_selector == null)
        {
            _entity.SwitchSelectStatus();
            return;
        }

        // If selecting this dice will result in a total number of dice
        // selected that is greater than the required number of dice,
        // clear the previous selection
        var tryResult = _selector.TryAppendDice(_entity.Selecting);
        _entity.SetSelectStatus(tryResult);
        _selector.prevClickedDice?.SetScale(1);
        
        if (_selector.function.tuning)
        {
            var tuningType = _entity.Selecting ? _entity.Logic.Type : CostType.None;
            _selector.function.global.prompt.tuning.SetOrigin(tuningType);
        }

        if (_entity.Selecting)
        {
            SetScale(1.08f);
            _selector.prevClickedDice = this;
        }
        
        select.SetActive(_entity.Selecting);
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        if (isLocking || !interactable)
            return;

        if (enterSelectable && Input.GetMouseButton(0))
        {
            var controller = FindObjectOfType<DiceReroll>();
            _entity.SetSelectStatus(controller.currentOperationStatus);
            return;
        }
        
        diceBase.SetActive(false);
        outline.SetActive(false);
        hover.SetActive(true);
    }

    public void OnPointerDown(PointerEventData eventData)
    {
        if (!interactable || !enterSelectable)
            return;

        var rerollController = FindObjectOfType<DiceReroll>();
        _entity.SwitchSelectStatus();
        rerollController.currentOperationStatus = _entity.Selecting;
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        if (isLocking || !interactable)
            return;
        
        if (!_entity.Selecting)
        {
            diceBase.SetActive(true);
            outline.SetActive(true);    
        }
        hover.SetActive(false);
    }
}

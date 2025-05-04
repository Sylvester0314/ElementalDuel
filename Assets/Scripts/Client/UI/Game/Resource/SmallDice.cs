using Shared.Enums;
using UnityEngine.UI;

public class SmallDice : AbstractDice
{
    public Image front;

    private DiceEntity _entity;
    
    public SmallDice SetEntity(DiceEntity entity)
    {
        _entity = entity;
        return this;
    }
    
    public async void Initialize(CostType type)
    {
        var path = ResourceLoader.GetCostSpritePath(type);
        var dice = await ResourceLoader.LoadSprite(path);
        front.sprite = dice;
        background.sprite = dice;

        var element = await ResourceLoader.LoadSprite(
            $"Assets/Sources/UI/Elements/Pure_{type.ToString()}.png"
        );
        icon.sprite = element;
    }
}
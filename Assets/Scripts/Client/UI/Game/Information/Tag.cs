using Shared.Enums;
using Shared.Misc;
using UnityEngine.UI;

public class Tag : AbstractInformationComponent
{
    public Image type;

    public override async void SetInformation<T>(T data)
    {
        if (data is not Property property)
            return;
        
        var path = $"Assets/Sources/UI/Tags/{property.ToSnakeCase()}.png";
        var sprite = await ResourceLoader.LoadSprite(path);
        
        if (gameObject != null && type != null)
            type.sprite = sprite;
    }
}

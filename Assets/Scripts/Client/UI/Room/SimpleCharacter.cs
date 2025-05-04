using UnityEngine;
using UnityEngine.UI;

public class SimpleCharacter : MonoBehaviour
{
    public Image cardFace;
    public Image cardFrame;

    public Sprite invisibleFace;
    
    public void SetCardFace(CharacterAsset asset)
    {
        cardFace.sprite = asset.cardImage;
    }
}
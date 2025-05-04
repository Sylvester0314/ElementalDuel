using DG.Tweening;
using UnityEngine;
using UnityEngine.UI;

public class DeckCharacterCard : MonoBehaviour
{
    public Image cardFace;
    public Image cardFrame;

    private readonly Vector3 _frontStart = new (26.5f, -5.5f, -6.5f);
    private readonly Vector3 _frontEnd = new (34, -10.5f, -7);
    
    private readonly Vector3 _middleStart = new (0, 0, 4.5f);
    private readonly Vector3 _middleEnd = new (-31, 0.5f, 8);
    
    private readonly Vector3 _backStart = new (-24.5f, 1, 10);
    private readonly Vector3 _backEnd = new (-2.5f, 48.5f, -6);

    private readonly Vector3 _pos = new (1, 1, 0);
    private readonly Vector3 _rot = new (0, 0, 1);
    
    public void Move(string pos, int value)
    {
        var target = (pos + value) switch
        {
            "f1" => _frontEnd,
            "f0" => _frontStart,
            "m1" => _middleEnd,
            "m0" => _middleStart,
            "b1" => _backEnd,
            "b0" => _backStart,
            _ => Vector3.zero
        };

        var position = Vector3.Scale(target, _pos);
        var rotation = Vector3.Scale(target, _rot);

        transform.DOLocalMove(position, DeckItem.Duration * 1.5f).SetEase(Ease.OutExpo);
        transform.DORotate(rotation, DeckItem.Duration * 1.5f).SetEase(Ease.OutExpo);
    }
}
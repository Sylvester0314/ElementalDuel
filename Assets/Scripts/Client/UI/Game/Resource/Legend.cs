using UnityEngine;
using UnityEngine.EventSystems;

public class Legend : MonoBehaviour, IPointerClickHandler
{
    public Hint hint;
    public GameObject availableIcon;

    public bool HasRemaining => _remaining > 0;

    public int Remaining
    {
        get => _remaining;
        set
        {
            _remaining = value;
            availableIcon.SetActive(HasRemaining);
        }
    }
    private int _remaining;

    public void OnPointerClick(PointerEventData eventData)
    {
        hint.Display(HasRemaining ? "not_used_legend" : "already_used_legend", 31.5f);
    }
}
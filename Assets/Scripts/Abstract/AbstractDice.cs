using UnityEngine;
using UnityEngine.UI;

public class AbstractDice : MonoBehaviour
{
    public GameObject diceBase;
    public GameObject outline;
    public GameObject select;
    public Image background;
    public Image icon;

    public void SetSelectStatus(bool status)
    {
        outline.gameObject.SetActive(!status);
        diceBase.gameObject.SetActive(!status);
        select.gameObject.SetActive(status);
    }
}
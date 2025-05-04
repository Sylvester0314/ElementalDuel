using DG.Tweening;
using UnityEngine;

public class PopUpsInjector : MonoBehaviour 
{
    public bool hasPopUps;
    public Transform wrapper;
    public GameObject popUpsPrefab;
    public UISizeFitter sizeFitter;
    
    private GameObject _instance;

    public T Create<T>(string titleEntry) where T : PopUps
    {
        if (hasPopUps)
            return null;
        hasPopUps = true;

        _instance = Instantiate(popUpsPrefab, wrapper, false);
        
        var popUps = _instance.GetComponent<T>();
        
        popUps.Initialize(titleEntry);
        popUps.sizeFitter.rootCanvas = sizeFitter.rootCanvas;
        popUps.confirmButton.Callback += ClosePopUps;

        if (popUps is AdvancedPopUps adv)
            adv.cancelButton.Callback += ClosePopUps;
        
        return popUps;
    }

    private void ClosePopUps()
    {
        hasPopUps = false;
        DOVirtual.DelayedCall(0.02f, () => Destroy(_instance));
    }
}
using UnityEngine;

[ExecuteAlways]
public class CardRotation : MonoBehaviour
{
    public GameObject cardFront;
    public GameObject cardBack;
    public Transform targetPoint;
    public Collider col;

    public bool forcedSet;
    public bool forcedValue;

    public bool debugMode;

    private bool _showingBack;

    private void Set(bool value)
    {
        _showingBack = value;
        cardBack.SetActive(_showingBack);
        cardFront.SetActive(!_showingBack);
    }

    public void ForceFront()
    {
        forcedSet = true;
        forcedValue = false;
    }
    
    public void ForceBack()
    {
        forcedSet = true;
        forcedValue = true;
    }
    
    public void Update()
    {
        if (forcedSet)
        {
            Set(forcedValue);
            return;
        }
        
        var origin = targetPoint.position;
        var direction = Vector3.up * 20f;
        
        var ray = new Ray(origin, direction);

        var thorough = col.Raycast(ray, out _, Mathf.Infinity);
        if (thorough == _showingBack)
            return;

        if (debugMode)
        {
            Debug.Log($"Status: {thorough}\n" +
                      $"Rotation: {transform.rotation.eulerAngles}\n" +
                      $"Local: {transform.localPosition}\n"+
                      $"Target Point: {targetPoint.position}\n" +
                      $"Col Point: {col.bounds.center}\n" +
                      $"Time: {Time.time}");
        }
        
        Set(thorough);
    }
}
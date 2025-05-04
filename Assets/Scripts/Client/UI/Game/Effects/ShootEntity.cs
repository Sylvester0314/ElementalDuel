using System;
using UnityEngine;

public class ShootEntity : MonoBehaviour
{
    public float speed;
    public ParticleSystem particle;
    public ParticleSystem hitPrefab;

    [HideInInspector]
    public Vector3 path;
    [HideInInspector]
    public Vector3 from;
    [HideInInspector]
    public Vector3 to;

    public Action OnComplete;
    
    public void Update()
    {
        transform.position += path.normalized * (speed * Time.deltaTime);
        
        var moved = transform.position - from;
        var progress = Vector3.Dot(moved, path) / path.sqrMagnitude;

        if (progress < 0.95f)
            return;
        
        OnComplete?.Invoke();
        
        var hitParticle = Instantiate(hitPrefab, to, Quaternion.identity, transform.parent);
        Destroy(hitParticle.gameObject, hitParticle.main.duration);
        Destroy(gameObject);
    }
}
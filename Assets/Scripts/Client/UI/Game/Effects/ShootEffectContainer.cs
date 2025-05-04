using System;
using System.Collections.Generic;
using Shared.Enums;
using UnityEngine;

public class ShootEffectContainer : MonoBehaviour
{
    public float duration;
    public List<ShootEntity> shootEffects;

    public Transform testStart;
    public CharacterCard character;
    
    public void Play(Transform source, CharacterCard target, Element type, Action onComplete = null)
    {
        var index = Mathf.Clamp((int)type, 0, shootEffects.Count - 1);
        
        var from = source.position;
        var to = target.transform.position;

        var path = to - from;
        var rotation = Quaternion.LookRotation(path) * Quaternion.Euler(0, -90, 0);

        var entity = Instantiate(shootEffects[index], from, rotation, transform);

        entity.path = path;
        entity.from = from;
        entity.to = to;
        entity.speed = path.magnitude / duration;
        entity.OnComplete = onComplete;
        
        entity.particle.Play();
    }
}
using System;
using System.Collections.Generic;
using Shared.Enums;
using UnityEngine;

public class EntityHitEffect : MonoBehaviour
{
    public ParticleSystem background;
    public ParticleSystem outer;
    public ParticleSystem inner;
    
    public List<Gradient> bgColors;
    public List<Gradient> outerColors;
    public List<Gradient> innerColors;

    public void Play(Element element)
    {
        var index = Math.Clamp((int)element, 0, 7);
        
        var c1 = background.colorOverLifetime;
        var c2 = outer.colorOverLifetime;
        var c3 = inner.colorOverLifetime;
        
        c1.color = new ParticleSystem.MinMaxGradient(bgColors[index]);
        c2.color = new ParticleSystem.MinMaxGradient(outerColors[index]);
        c3.color = new ParticleSystem.MinMaxGradient(innerColors[index]);
        
        inner.Play();
    }
}
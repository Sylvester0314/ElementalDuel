using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "New Path Animation", menuName = "Custom/New Path Animation")]
public class AnimationConfiguration : ScriptableObject
{
    public float duration;
    public float maxCorrected;
    public float maxOffset;
    public AnimationCurve curve;
    public List<AnimationWaypoint> path;
}
using System;
using System.Collections.Generic;
using Server.GameLogic;
using UnityEngine;

public class CostSetComponent : MonoBehaviour
{
    public Cost costPrefab;
    public Transform costs;
    public float scale = 1.0f;
    
    [HideInInspector]
    public List<Cost> costComponentList;
    
    public void InitializeCostList(string suffix, Action<List<Cost>> onComplete = null)
    {
        if (costComponentList?.Count != 0)
            return;
        
        costComponentList = new List<Cost>();
        for (var i = 0; i < CostLogic.MaxCostType; i++)
        {
            var instance = Instantiate(costPrefab);
            instance.Initialize(scale, suffix);
            instance.SetParent(costs);
            costComponentList.Add(instance);
        }
        
        onComplete?.Invoke(costComponentList);
    }
}
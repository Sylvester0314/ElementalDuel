using System.Collections.Generic;
using Shared.Enums;
using UnityEngine;

public interface IInitializableScriptableObject
{
    void Initialize(string fileName);
}

public interface ICardAsset
{
    public List<Property> Properties { get; }

    public string Name { get; }
}
using System;
using UnityEngine;

public enum UnityColor { Blue, Green, Red };

[Serializable]
public class UnityCharacter
{
    public UnityColor unityColor;

    public GameObject prefab;
}

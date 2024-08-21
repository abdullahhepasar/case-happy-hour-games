using System;
using UnityEngine;

[Serializable]
public class ResourcesData
{
    public VariableID id = VariableID.Wood1;
    [HideInInspector] public GameObject obj;
    [HideInInspector] public Resource resource;
}
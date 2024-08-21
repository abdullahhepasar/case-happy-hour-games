using System.Collections.Generic;
using UnityEngine;

public class Level : MonoBehaviour
{
    [HideInInspector] public LevelScriptable levelScriptable;

    public Transform PivotParent;

    public void Initialize(List<LevelScriptable.LevelObjects> levelData)
    {

    }
}

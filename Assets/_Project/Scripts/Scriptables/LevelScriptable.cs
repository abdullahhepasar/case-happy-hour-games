using System;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "Level", menuName = "Project/LevelData")]
public class LevelScriptable : ScriptableObject
{
    [Tooltip("Required for sorting")]
    public int LevelID = 0;
    public int OrderId = 0; // FOR RCW(Remote Config)
    public string LevelPrefabName = "Level";
    [Tooltip("Add offset relative to previous level")]
    public Vector3 SpawnPositionOffset = Vector3.zero;
    public Vector3 SpawnRotation = Vector3.zero;

    [HideInInspector] public Level LevelPrefab;

    [Serializable]
    public class LevelObjects
    {
        public VariableID objectId = VariableID.Empty;
    }

    public List<LevelObjects> levelObjects = new List<LevelObjects>();
}

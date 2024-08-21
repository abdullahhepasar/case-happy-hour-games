using UnityEngine;

[CreateAssetMenu(fileName = "LevelSettings", menuName = "Project/LevelSettings")]
public class LevelSettingsScriptable : ScriptableObject
{
    [Header("level Style")]
    public LevelVariation levelVariation;
    public LevelScriptable CurrentLevelData;
    public int CurrentLevelIndex;
    [HideInInspector] public int ActualCurrentLevelIndex;

    [Tooltip("if LevelVariation->Shifter, u must change")]
    [Range(1, 50)] public int LevelCreateCounter = 5;
}

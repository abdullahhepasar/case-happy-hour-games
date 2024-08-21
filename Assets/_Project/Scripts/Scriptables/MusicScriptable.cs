using UnityEngine;

[CreateAssetMenu(fileName = "Music", menuName = "Project/MusicData")]
public class MusicScriptable : ScriptableObject
{
    public string audioClipName;
    [Tooltip("Default Volume")]
    public float musicVolume = 1f;
    [Tooltip("Low Volume")]
    public float musicLowVolume;
}

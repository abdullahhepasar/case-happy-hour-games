using UnityEngine;

[CreateAssetMenu(fileName = "Sound", menuName = "Project/SoundData")]
public class SoundScriptable : ScriptableObject
{
    public SoundID soundID;
    public string audioClipName;
    public float volume = 1f;
    public bool loopActive = false;

    public float pitchDefault = 1f;
    public float pitchChange = .1f;
    public float pitchMax = 2f;
    public float pitchDelayDeactivateTime = 2f;
}

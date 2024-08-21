using UnityEngine;

[CreateAssetMenu(fileName = "Vibrate", menuName = "Project/VibrateData")]
public class VibrateScriptable : ScriptableObject
{
    public string VibrateName;
    public string ResourcesName;

    public VibrateIOSAdvancedID VIosID = VibrateIOSAdvancedID.None;
    [Tooltip("if Advanced Haptic Pattern")]
    public TextAsset AHAPFile;
}

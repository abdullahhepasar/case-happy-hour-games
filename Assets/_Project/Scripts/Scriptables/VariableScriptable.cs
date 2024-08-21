using UnityEngine;

[CreateAssetMenu(fileName = "Variable", menuName = "Project/VariableData")]
public class VariableScriptable : ScriptableObject
{
    public VariableID variableID;
    public int Value;

    public void GetValue()
    {
        if (PlayerPrefs.HasKey(variableID.ToString()))
            Value = PlayerPrefs.GetInt(variableID.ToString());
        else
            SetValue();
    }

    public void SetValue()
    {
        PlayerPrefs.SetInt(variableID.ToString(), Value);
    }
}

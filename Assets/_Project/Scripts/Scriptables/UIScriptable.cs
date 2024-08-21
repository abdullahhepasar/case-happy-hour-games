using UnityEngine;

[CreateAssetMenu(fileName = "UI", menuName = "Project/UIData")]
public class UIScriptable : ScriptableObject
{
    [Tooltip("ID")]
    public UIPrefabNames UIPrefabID = UIPrefabNames.None;
    public string Static_UIResourcesName;
    [Tooltip("If you have Dynamic UI ac and Enter Filename")]
    public bool DynamicCanvasActive = false;
    public string Dynamic_UIResourcesName;
    [Tooltip("Static_LayerSortOrder = 2 and you can set it to true to override dynamic UI in the desired state.")]
    public bool Dynamic_LayerSortOrder = false;
}

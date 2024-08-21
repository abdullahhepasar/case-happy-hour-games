using UnityEngine;

[CreateAssetMenu(fileName = "VFX", menuName = "Project/VFXData")]
public class VFXScriptable : ScriptableObject
{
    public string VFXName;
    public VFXID VFXId;
    public VFXCategory VFXCategory;
    public string ResourcesName;
    [Header("Automatically Destroy?")]
    public bool AutoDestroyActive;
    public float AutoDestroyTime;
    public Vector3 OffSetPos;
    public Vector3 OffSetRot;

    [Space(5f)]
    [Tooltip("if using Object Pool System, u can active")]
    public bool ObjectPoolActive = false;
    public int ObjectPoolInitialSize = 20;
    public GameObject ObjectPoolPrefab;
}

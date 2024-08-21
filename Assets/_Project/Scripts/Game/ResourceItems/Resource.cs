using UnityEngine;
using UnityEngine.Events;

public abstract class Resource : MonoBehaviour
{
    public VariableID resourceType;

    protected int _resourceID;
    public int ResourceID
    {
        get {  return _resourceID; }
        set { _resourceID = value; }
    }

    public int baseValue;

    protected int _currentValue;
    public int CurrentValue
    {
        get { return _currentValue; }
        set { _currentValue = value; }
    }

    public bool active;

    public GameObject meshPrefab;

    public Collider TriggerCollider;

    [HideInInspector] public SceneGenerator SceneGenerator;

    public UnityEvent eventResourceDone = new UnityEvent();

    public abstract void Initialize(SceneGenerator sg, int id);
    public abstract void ChangeValueResource(int value, bool updateRemote = true);
    public abstract void Reset();
}

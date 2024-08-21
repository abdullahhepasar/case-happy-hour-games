using UnityEngine;
using System.Collections.Generic;
using UnityEngine.Assertions;

public class ObjectPool : MonoBehaviour
{
    [HideInInspector] public GameObject prefab;
    [SerializeField] private int initialSize;

    private readonly Stack<GameObject> instances = new Stack<GameObject>();

    /// <summary>
    /// 
    /// </summary>
    /// <param name="pre">Prefab</param>
    /// <param name="count">Initial Size</param>
    public void SetStart(GameObject pre, int count)
    {
        Assert.IsNotNull(pre);

        this.prefab = pre;
        this.initialSize = count;

        for (int i = 0; i < initialSize; i++)
        {
            GameObject obj = CreateInstance();
            obj.SetActive(false);
            instances.Push(obj);
        }
    }

    /// <summary>
    /// Returns a new object from the pool
    /// </summary>
    public GameObject GetObject()
    {
        GameObject obj = instances.Count > 0 ? instances.Pop() : CreateInstance();
        obj.SetActive(true);
        return obj;
    }

    /// <summary>
    /// Returns the specified game object to the pool where it came from.
    /// </summary>
    /// <param name="obj">The object to return to its origin pool.</param>
    public void ReturnObject(GameObject obj)
    {
        PooledObject pooledObject = obj.GetComponent<PooledObject>();
        Assert.IsNotNull(pooledObject);
        Assert.IsTrue(pooledObject.pool == this);

        obj.SetActive(false);
        if (!instances.Contains(obj))
        {
            instances.Push(obj);
        }
    }

    /// <summary>
    /// Resets the object pool to its initial state.
    /// </summary>
    public void Reset()
    {
        List<GameObject> objectsToReturn = new List<GameObject>();
        foreach (PooledObject instance in transform.GetComponentsInChildren<PooledObject>())
        {
            if (instance.gameObject.activeSelf)
            {
                objectsToReturn.Add(instance.gameObject);
            }
        }
        foreach (GameObject instance in objectsToReturn)
        {
            ReturnObject(instance);
        }
    }

    /// <summary>
    /// Creates a new instance of the pooled object type.
    /// </summary>
    /// <returns>A new instance of the pooled object type.</returns>
    private GameObject CreateInstance()
    {
        GameObject obj = Instantiate(prefab);
        PooledObject pooledObject = obj.AddComponent<PooledObject>();
        pooledObject.pool = this;
        obj.transform.SetParent(transform);
        return obj;
    }
}

/// <summary>
/// Utility class to identify the pool of a pooled object.
/// </summary>
public class PooledObject : MonoBehaviour
{
    public ObjectPool pool;
}
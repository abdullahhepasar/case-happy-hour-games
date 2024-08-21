using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using GPUInstancer;
using UnityEngine.Assertions;

public class GPUInstancerController : MonoBehaviour
{
    public static GPUInstancerController Instance;

    public GPUInstancerPrefabManager GPUInstancerPrefabManager;

    [Serializable]
    public class GPUInstances
    {
        public VariableID ID;
        [Tooltip("Add GPUInstancerControllerPrefab to Prototypes. And Set Prefab's properties -> \n" +
            "Enable Runtime Modification = true, \n" +
            "Add / Remove Instances At Runtime = true, \n" +
            "Extra Buffer Size = 500, \n" +
            "Auto Add / Remove Instances = true, \n" +
            "Auto Update Transform Data = true")]
        public GPUInstancerPrefab prefab;
        public List<GPUInstancerPrefab> InstancePrefabs = new List<GPUInstancerPrefab>();
        public int Count;
    }

    public List<GPUInstances> GPUInstanceList = new List<GPUInstances>();

    private bool LOADED = false;

    void Awake()
    {
        //Check if instance already exists
        if (Instance == null)
        {
            //if not, set instance to this
            Instance = this;
        }
        //If instance already exists and it's not this:
        else if (Instance != this)
        {
            Destroy(gameObject);
        }

        //Sets this to not be destroyed when reloading scene
        DontDestroyOnLoad(gameObject);
    }

    public IEnumerator LoadDatas()
    {
        if (LOADED)
            yield break;

        yield return StartCoroutine(SetObjectPoling());

        LOADED = true;

        yield return null;
    }

    private IEnumerator SetObjectPoling()
    {
        for (int i = 0; i < GPUInstanceList.Count; i++)
        {
            for (int j = 0; j < GPUInstanceList[i].Count; j++)
            {
                CreateInstance(GPUInstanceList[i].ID, GPUInstanceList[i].prefab);
            }            
        }

        yield return null;
    }

    private Transform IsHave(Transform target, string name)
    {
        foreach (Transform item in target)
        {
            if (item.gameObject.name == name)
                return item;
        }

        Transform createTransform = new GameObject(name).transform;
        createTransform.SetParent(target);

        return createTransform;
    }

    /// <summary>
    /// Take object from pool
    /// </summary>
    /// <param name="ID">Object Type</param>
    /// <param name="ObjectActive">If the active or passive state of the object is desired</param>
    /// <returns></returns>
    public GameObject GetObject(VariableID ID, bool ObjectActive = true)
    {
        for (int i = 0; i < GPUInstanceList.Count; i++)
        {
            if (GPUInstanceList[i].ID == ID)
            {
                GPUInstancerPrefab temp = new GPUInstancerPrefab();

                if (GPUInstanceList[i].InstancePrefabs.Count > 0 )
                {
                    temp = GPUInstanceList[i].InstancePrefabs[GPUInstanceList[i].InstancePrefabs.Count - 1];
                    GPUInstanceList[i].InstancePrefabs.RemoveAt(GPUInstanceList[i].InstancePrefabs.Count - 1);
                }
                else
                    temp = CreateInstance(ID, GPUInstanceList[i].prefab, false);

                temp.gameObject.SetActive(ObjectActive);

                return temp.gameObject;
            }
        }

        return null;
    }

    /// <summary>
    /// Add the used object to the pool
    /// </summary>
    /// <param name="ID">Object Type</param>
    /// <param name="obj">Object Gameobject</param>
    /// <param name="lastPosition">If desired, a remote position can be selected in case of flicker.</param>
    public void ReturnObject(VariableID ID, GameObject obj, Vector3 lastPosition = new Vector3())
    {
        GPUInstancerPrefab temp = obj.GetComponent<GPUInstancerPrefab>();
        Assert.IsNotNull(temp);

        Transform IDParent = IsHave(this.transform, ID.ToString());

        temp.transform.SetParent(IDParent);
        temp.gameObject.SetActive(false);
        temp.transform.position = lastPosition;

        for (int i = 0; i < GPUInstanceList.Count; i++)
        {
            if (GPUInstanceList[i].ID == ID)
            {
                GPUInstanceList[i].InstancePrefabs.Add(temp);
            }
        }
    }

    public IEnumerator RemoveInstances(VariableID ID, bool AllRemove = false, float DestroyTimeOfEachObject = 0)
    {
        for (int i = 0; i < GPUInstanceList.Count; i++)
        {
            if (AllRemove)
            {
                int count = GPUInstanceList[i].InstancePrefabs.Count;

                for (int j = count; 0 < j ; j--)
                {
                    if (!GPUInstanceList[i].InstancePrefabs[j - 1].prefabPrototype.addRuntimeHandlerScript)
                        GPUInstancerAPI.RemovePrefabInstance(GPUInstancerPrefabManager, GPUInstanceList[i].InstancePrefabs[j - 1]);

                    Destroy(GPUInstanceList[i].InstancePrefabs[j - 1].gameObject);
                    GPUInstanceList[i].InstancePrefabs.RemoveAt(j - 1);

                    yield return new WaitForSeconds(DestroyTimeOfEachObject);
                }
            }
            else
            {
                if (GPUInstanceList[i].ID == ID)
                {
                    int count = GPUInstanceList[i].InstancePrefabs.Count;

                    for (int j = count; 0 < j; j--)
                    {
                        if (!GPUInstanceList[i].InstancePrefabs[j - 1].prefabPrototype.addRuntimeHandlerScript)
                            GPUInstancerAPI.RemovePrefabInstance(GPUInstancerPrefabManager, GPUInstanceList[i].InstancePrefabs[j - 1]);

                        Destroy(GPUInstanceList[i].InstancePrefabs[j - 1].gameObject);
                        GPUInstanceList[i].InstancePrefabs.RemoveAt(j - 1);

                        yield return new WaitForSeconds(DestroyTimeOfEachObject);
                    }
                }
            }
        }
    }

    private GPUInstancerPrefab CreateInstance(VariableID ID, GPUInstancerPrefab prefab, bool addList = true)
    {
        Transform IDParent = IsHave(this.transform, ID.ToString());
        GPUInstancerPrefab obj = new GPUInstancerPrefab();

        for (int i = 0; i < GPUInstanceList.Count; i++)
        {
            if (GPUInstanceList[i].ID == ID)
            {
                obj = Instantiate(prefab);
                obj.transform.SetParent(IDParent);

                if (!obj.prefabPrototype.addRuntimeHandlerScript)
                    GPUInstancerAPI.AddPrefabInstance(GPUInstancerPrefabManager, obj);

                if (addList)
                    GPUInstanceList[i].InstancePrefabs.Add(obj);

                obj.gameObject.SetActive(false);

                break;
            }
        }

        return obj;
    }

    #region TEST

    [HideInInspector] public List<GameObject> TESTLIST = new List<GameObject>();

    public void SetButton_LoadData()
    {
        LOADED = false;
        StartCoroutine(LoadDatas());
    }
    public void SetButton_GetObject()
    {
        GameObject test = GetObject(VariableID.MN);
        test.transform.SetParent(null);
        test.transform.localPosition = UnityEngine.Random.insideUnitSphere * 10;
        TESTLIST.Add(test);
    }

    public void SetButton_ReturnObject()
    {
        if (TESTLIST.Count > 0)
        {
            GameObject lastObj = TESTLIST[TESTLIST.Count - 1];
            ReturnObject(VariableID.MN, lastObj);
            TESTLIST.RemoveAt(TESTLIST.Count - 1);
        }
    }

    public void SetButton_RemoveInstances()
    {
        StartCoroutine(RemoveInstances(VariableID.MN, true, 0.00001f));
    }

    #endregion
}

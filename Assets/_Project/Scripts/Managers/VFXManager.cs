using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using Unity.VisualScripting;

public class VFXManager : MonoBehaviour
{
    public static VFXManager Instance;

    public List<VFXScriptable> VFXes = new List<VFXScriptable>();

    private List<GameObject> cacheVFX = new List<GameObject>(); //VFX created in the scene are kept as cache

    private string ResourceFolderName_VFXScriptable = "VFX/Scriptable/";
    private string ResourceFolderName_VFXPrefabs = "VFX/Prefabs/";

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

        yield return StartCoroutine(GetAndLoadVFXDatas());

        yield return StartCoroutine(SetObjectPoling());

        LOADED = true;

        yield return null;
    }

    private IEnumerator GetAndLoadVFXDatas()
    {
        VFXes = new List<VFXScriptable>();

        VFXScriptable[] tempLevelScriptable = Resources.LoadAll<VFXScriptable>(ResourceFolderName_VFXScriptable) as VFXScriptable[];
        tempLevelScriptable = tempLevelScriptable.OrderBy(e => e.VFXName).ToArray();

        for (int i = 0; i < tempLevelScriptable.Length; i++)
        {
            VFXes.Add(tempLevelScriptable[i]);
        }

        yield return null;
    }

    private IEnumerator SetObjectPoling()
    {
        for (int i = 0; i < VFXes.Count; i++)
        {
            if (VFXes[i].ObjectPoolActive)
            {
                Transform categoryParent = IsHave(this.transform, VFXes[i].VFXCategory.ToString());
                Transform IDParent = IsHave(categoryParent, VFXes[i].VFXId.ToString());

                IDParent.AddComponent<ObjectPool>();

                //CANCEL FOR NOW
                /*ResourceRequest resource = null;

                resource = Resources.LoadAsync(ResourceFolderName_VFXPrefabs + VFXes[i].ResourcesName, typeof(GameObject));
                while (!resource.isDone)
                {
                    yield return resource;
                }

                if (resource.asset == null)
                {
                    Debug.LogError("VFX Prefab is Null");
                    yield break;
                }

                GameObject tempObject = Instantiate(resource.asset) as GameObject;*/

                IDParent.GetComponent<ObjectPool>().SetStart(VFXes[i].ObjectPoolPrefab, VFXes[i].ObjectPoolInitialSize);
            }
        }

        yield return null;
    }

    private Transform IsHave(Transform target, string name)
    {
        foreach (Transform item in target)
        {
            if (item.gameObject.name.Contains(name))
                return item;
        }

        Transform createTransform = new GameObject(name).transform;
        createTransform.SetParent(target);

        return createTransform;
    }

    /// <summary>
    /// Start VFX on stage
    /// </summary>
    /// <param name="vFXID"></param>
    /// <param name="spawnPosition"></param>
    /// <param name="spawnRotation"></param>
    public void SetVFX(VFXID vFXID, Vector3 spawnPosition, Vector3 spawnRotation,Transform parent)
    {
        if (vFXID == VFXID.None)
            return;

        StartCoroutine(CreateVFX_Prefab(vFXID, spawnPosition, spawnRotation, parent));
    }

    /// <summary>
    /// Select Random VFX by VFX Category ID
    /// </summary>
    /// <param name="category"></param>
    /// <returns></returns>
    public VFXID GetCategoryRandomVFXID(VFXCategory category)
    {
        List<VFXScriptable> vFXListsTemp = new List<VFXScriptable>();

        for (int i = 0; i < VFXes.Count; i++)
        {
            if (VFXes[i].VFXCategory == category)
                vFXListsTemp.Add(VFXes[i]);
        }

        if (vFXListsTemp.Count == 0)
            return VFXID.None;
        else
            return vFXListsTemp[UnityEngine.Random.Range(0, vFXListsTemp.Count)].VFXId;
    }


    private IEnumerator CreateVFX_Prefab(VFXID vFXID, Vector3 spawnPosition, Vector3 spawnRotation, Transform parent)
    {
        //Run GC
        //Resources.UnloadUnusedAssets();

        VFXScriptable vFXListsTemp = new VFXScriptable();
        for (int i = 0; i < VFXes.Count; i++)
        {
            if (VFXes[i].VFXId == vFXID)
            {
                vFXListsTemp = VFXes[i];
                break;
            }
        }

        //If VFX not found in the list
        if (vFXListsTemp == null)
            yield break;

        GameObject tempObject = null;
        Transform IDParent = null;
        if (vFXListsTemp.ObjectPoolActive)
        {
            Transform categoryParent = IsHave(this.transform, vFXListsTemp.VFXCategory.ToString());
            IDParent = IsHave(categoryParent, vFXListsTemp.VFXId.ToString());

            tempObject = IDParent.GetComponent<ObjectPool>().GetObject();
            tempObject.transform.position = spawnPosition;
            tempObject.transform.eulerAngles = spawnRotation;
        }
        else
        {
            ResourceRequest resource = null;

            resource = Resources.LoadAsync(ResourceFolderName_VFXPrefabs + vFXListsTemp.ResourcesName, typeof(GameObject));
            while (!resource.isDone)
            {
                yield return resource;
            }

            if (resource.asset == null)
            {
                Debug.LogError("VFX Prefab is Null");
                yield break;
            }

            tempObject = Instantiate(resource.asset, spawnPosition + vFXListsTemp.OffSetPos, Quaternion.Euler(spawnRotation + vFXListsTemp.OffSetRot)) as GameObject;
        }

        if (vFXListsTemp.AutoDestroyActive)
        {
            cacheVFX.Remove(tempObject);
            Destroy(tempObject, vFXListsTemp.AutoDestroyTime);
        }

        if (vFXListsTemp.ObjectPoolActive)
        {
            StartCoroutine(Delay(vFXListsTemp.AutoDestroyTime, IDParent, tempObject));
        }
        else
        {
            cacheVFX.Add(tempObject);
            tempObject.transform.SetParent(parent);
        }
    }

    IEnumerator Delay(float time, Transform target, GameObject temp)
    {
        yield return new WaitForSeconds(time);
        target.GetComponent<ObjectPool>().ReturnObject(temp);
    }

    /// <summary>
    /// Delete All Created VFX at End of Level
    /// </summary>
    public IEnumerator SetClearVFX()
    {
        for (int i = 0; i < cacheVFX.Count; i++)
        {
            if (cacheVFX[i] != null)
            {
                cacheVFX[i].SetActive(false);
                Destroy(cacheVFX[i]);
            }
        }

        //Waiting after Forced Destroy
        yield return new WaitForSeconds(0.05f);
        cacheVFX = new List<GameObject>();

        //Run GC
        Resources.UnloadUnusedAssets();
        yield return null;
    }
}

using Photon.Pun;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

using Hashtable = ExitGames.Client.Photon.Hashtable;

public class SceneGenerator : MonoBehaviourPunCallbacks
{
    #region Private Fields

    [SerializeField] private Transform ParentClusterWood;

    [SerializeField] private List<ResourcesData> resourcesData = new List<ResourcesData>();

    private Dictionary<string, string> propertiesValue = new Dictionary<string, string>();

    #endregion

    #region public Fields

    [Serializable]
    public class RemoteData
    {
        public int blockId;
        public int currentValue;
    }

    [Serializable]
    public class RemoteResourceData
    {
        public List<RemoteData> remoteDatas = new List<RemoteData>();
    }

    #endregion

    #region Private Methods

    private IEnumerator GenerateScene()
    {
        //Type Wood Create
        for (int i = 0; i < resourcesData.Count; i++)
        {
            if (i >= ParentClusterWood.childCount) break;

            Transform parent = ParentClusterWood.GetChild(i);
            Resource wood = GPUInstancerController.Instance.GetObject(resourcesData[i].id).GetComponent<Resource>();
            wood.Initialize(this, i);

            wood.transform.SetParent(parent);
            wood.transform.localPosition = Vector3.zero;

            resourcesData[i].obj = wood.gameObject;
            resourcesData[i].resource = wood;

            yield return new WaitForEndOfFrame();
        }

        CheckPropertiesValues();

        if (!PhotonNetwork.IsMasterClient)
        {
            //Get Room Properties
            OnRoomPropertiesUpdate(PhotonNetwork.CurrentRoom.CustomProperties);
        }
    }

    private void CheckPropertiesValues()
    {
        List<VariableID> resourcesIdList = new List<VariableID>();
        foreach (VariableID id in Enum.GetValues(typeof(VariableID)))
        {
            resourcesIdList.Add(id);
        }

        VariableID idStart = VariableID.Wood1;
        for (int i = (int)idStart; i < resourcesIdList.Count; i++)
        {
            string key = i.ToString();

            List<ResourcesData> resourcesIdDatas = resourcesData.FindAll(x => x.resource.resourceType == (VariableID)i).OrderBy(x => x.resource.ResourceID).ToList();

            RemoteResourceData remoteResourceDatas = new RemoteResourceData();
            remoteResourceDatas.remoteDatas = new List<RemoteData>();

            for (int j = 0; j < resourcesIdDatas.Count; j++)
            {
                RemoteData rd = new RemoteData();
                rd.blockId = resourcesIdDatas[j].resource.ResourceID;
                rd.currentValue = resourcesIdDatas[j].resource.CurrentValue;
                remoteResourceDatas.remoteDatas.Add(rd);
            }

            if (resourcesIdDatas.Count > 0)
            {
                string value = JsonUtility.ToJson(remoteResourceDatas);
                propertiesValue.Add(key, value);
            }
        }
    }

    private string UpdatePropertiesData(string jsonData, RemoteData rd)
    {
        RemoteResourceData remoteResourceDatas = JsonUtility.FromJson<RemoteResourceData>(jsonData);

        for (int i = 0; i < remoteResourceDatas.remoteDatas.Count; i++)
        {
            if (remoteResourceDatas.remoteDatas[i].blockId == rd.blockId)
            {
                remoteResourceDatas.remoteDatas[i] = rd;
                break;
            }
        }

        return JsonUtility.ToJson(remoteResourceDatas);
    }

    private void DestroyScene()
    {
        foreach (var item in resourcesData)
        {
            Destroy(item.obj);

            item.resource = null;
        }

        propertiesValue = new Dictionary<string, string>();
    }

    #endregion

    #region public Methods

    public void CreateScene()
    {
        StopAllCoroutines();
        DestroyScene();
        StartCoroutine(GenerateScene());
    }

    public void ChangeValueResource(Resource resource, int value)
    {
        UpdateRoomProperties((int)resource.resourceType, resource.ResourceID, value);
    }

    public void UpdateRoomProperties(int resourceType, int resourceID, int value)
    {
        RemoteData remoteData = new RemoteData();
        remoteData.blockId = resourceID;
        remoteData.currentValue = value;

        string valueData = UpdatePropertiesData(propertiesValue[resourceType.ToString()], remoteData);
        propertiesValue[resourceType.ToString()] = valueData;

        Hashtable properties = new Hashtable { { resourceType.ToString(), valueData } };
        PhotonNetwork.CurrentRoom.SetCustomProperties(properties);
    }

    #endregion

    #region PUN CALLBACKS

    public override void OnRoomPropertiesUpdate(Hashtable propertiesThatChanged)
    {
        List<string> keys = new List<string>();
        foreach (KeyValuePair<string, string> pair in propertiesValue)
        {
            keys.Add(pair.Key);
        }

        for (int i = 0; i < keys.Count; i++)
        {
            if (propertiesThatChanged.ContainsKey(keys[i]))
            {
                if (propertiesThatChanged[keys[i]] == null)
                {
                    continue;
                }

                propertiesValue[keys[i]] = (string)propertiesThatChanged[keys[i]];

                //Update Visual
                VariableID id = (VariableID)int.Parse(keys[i]);
                string jsonData = propertiesValue[keys[i]];
                RemoteResourceData remoteResourceDatas = JsonUtility.FromJson<RemoteResourceData>(jsonData);
                for (int j = 0; j < remoteResourceDatas.remoteDatas.Count; j++)
                {
                    int blockId = remoteResourceDatas.remoteDatas[j].blockId;
                    ResourcesData rd = resourcesData.Find(x => x.resource.ResourceID == blockId);
                    rd.resource.ChangeValueResource(remoteResourceDatas.remoteDatas[j].currentValue, false);
                }
            }
        }
    }

    #endregion

}

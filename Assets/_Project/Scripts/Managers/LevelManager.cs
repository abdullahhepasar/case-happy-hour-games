using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using System.Collections;

public class LevelManager : MonoBehaviour
{
    public static LevelManager Instance;

    #region Delegates

    public delegate void DCreateLevel();
    public DCreateLevel DCreateLevelAct;

    #endregion

    public LevelSettingsScriptable LevelSettings;

    [HideInInspector] public List<LevelScriptable> Levels = new List<LevelScriptable>();

    private string ResourceFolderName_LevelScriptable = "Level/Scriptable/";
    private string ResourceFolderName_LevelPrefabs = "Level/Prefabs/";

    private string PrefName_CurrentLevel = "CurrentLevel";

    private List<GameObject> cacheLevels = new List<GameObject>();

    private Vector3 spawnPos;

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

    public void SetFirstSettings()
    {
        LevelSettings.CurrentLevelIndex = 0;
    }

    public IEnumerator LoadDatas()
    {
        //Load Data
        yield return StartCoroutine(GetAndLoadLevelDatas());

        GetActualLevelIndex();
        //Load Current Level Data
        GetLevelData(LevelSettings.CurrentLevelIndex);
        //Create Level
        yield return StartCoroutine(Level());
    }

    private IEnumerator GetAndLoadLevelDatas()
    {
        Levels = new List<LevelScriptable>();

        LevelScriptable[] tempLevelScriptable = Resources.LoadAll<LevelScriptable>(ResourceFolderName_LevelScriptable) as LevelScriptable[];
        tempLevelScriptable = tempLevelScriptable.OrderBy(e => e.OrderId).ToArray();

        for (int i = 0; i < tempLevelScriptable.Length; i++)
        {
            Levels.Add(tempLevelScriptable[i]);
        }

        yield return null;
    }

    /// <summary>
    /// Load Current Level Data
    /// </summary>
    public bool GetLevelData(int levelIndex)
    {
        if (Levels.Count > 0)
        {
            CheckScriptableLevels(levelIndex);

            LevelSettings.CurrentLevelData = Levels[levelIndex];
            return true;
        }

        return false;
    }

    /// <summary>
    /// Load Next Level
    /// </summary>
    public void NextLevel(bool isRandom)
    {
        if (isRandom)
        {
            int oldLevel = LevelSettings.CurrentLevelIndex;

            do
            {
                LevelSettings.CurrentLevelIndex = Random.Range(0, Levels.Count);
            } while (oldLevel == LevelSettings.CurrentLevelIndex);
        }
        else
        {
            LevelSettings.CurrentLevelIndex++;

            if (LevelSettings.CurrentLevelIndex >= Levels.Count)
                LevelSettings.CurrentLevelIndex = 0;
        }

        GetLevelData(LevelSettings.CurrentLevelIndex);

        LevelSettings.ActualCurrentLevelIndex++;
        SaveLocal();
    }

    private void SaveLocal()
    {
        PlayerPrefs.SetInt(PrefName_CurrentLevel, LevelSettings.ActualCurrentLevelIndex);
    }

    private int GetActualLevelIndex()
    {
        if (PlayerPrefs.HasKey(PrefName_CurrentLevel))
            LevelSettings.ActualCurrentLevelIndex = PlayerPrefs.GetInt(PrefName_CurrentLevel);
        else
            LevelSettings.ActualCurrentLevelIndex = LevelSettings.CurrentLevelIndex;

        LevelSettings.CurrentLevelIndex = LevelSettings.ActualCurrentLevelIndex % Levels.Count;

        return LevelSettings.ActualCurrentLevelIndex;
    }

    /// <summary>
    /// Init Level
    /// </summary>
    /// <param name="forceReload">if Lose condition, replay level instantiate (Recommended: Check Reset status)</param>
    /// <returns></returns>
    public IEnumerator Level(bool forceReload = false)
    {
        switch (LevelSettings.levelVariation)
        {
            case LevelVariation.Default:
                DefaultLevel(GetLevelIndex());
                break;
            case LevelVariation.Shifter:
                yield return StartCoroutine(CreateShifter(forceReload));
                break;
            default:
                break;
        }
    }

    public void DefaultLevel(int levelIndex = 0)
    {
        LevelSettings.ActualCurrentLevelIndex = levelIndex;
        SaveLocal();

        if (DCreateLevelAct != null)
            DCreateLevelAct();
    }

    private IEnumerator CreateShifter(bool forceReload = false)
    {

        if (forceReload)
            yield return StartCoroutine(ForceReload(GetCurrentLevelData()));
        else
        {
            CheckScriptableLevels(LevelSettings.CurrentLevelIndex);

            ResourceRequest resource = null;

            for (int i = LevelSettings.CurrentLevelIndex - (LevelSettings.LevelCreateCounter / 2); i < LevelSettings.CurrentLevelIndex + (LevelSettings.LevelCreateCounter / 2) + 1; i++)
            {
                if (i < 0)
                    continue;

                bool isHave = false;

                for (int j = 0; j < cacheLevels.Count; j++)
                {
                    if (i >= Levels.Count)
                        CheckScriptableLevels(i);

                    if (cacheLevels[j].GetComponent<Level>().levelScriptable.LevelID == Levels[i].LevelID)
                    {
                        isHave = true;
                        break;
                    }
                }

                if (isHave)
                    continue;

                resource = Resources.LoadAsync(ResourceFolderName_LevelPrefabs + Levels[i].LevelPrefabName, typeof(GameObject));
                while (!resource.isDone)
                {
                    yield return resource;
                }

                if (resource.asset == null)
                {
                    Debug.LogError("Level Prefab is Null->" + i);
                    continue;
                }

                spawnPos += Levels[i].SpawnPositionOffset;
                GameObject tempLevel = Instantiate(resource.asset, spawnPos, Quaternion.Euler(Levels[i].SpawnRotation)) as GameObject;
                tempLevel.GetComponent<Level>().levelScriptable = Levels[i];
                tempLevel.name = Levels[i].LevelID.ToString();
                Levels[i].LevelPrefab = tempLevel.GetComponent<Level>();

                AddCacheLevel(tempLevel);
            }

            CacheControl();
        }

        if (DCreateLevelAct != null)
            DCreateLevelAct();

        yield return null;
    }

    private IEnumerator ForceReload(LevelScriptable currentLevel)
    {
        Vector3 posTemp = currentLevel.LevelPrefab.transform.position;

        DestroyImmediate(currentLevel.LevelPrefab.gameObject);

        for (int i = 0; i < cacheLevels.Count; i++)
        {
            if (cacheLevels[i] == null)
            {
                //Reload
                ResourceRequest resource = null;

                resource = Resources.LoadAsync(ResourceFolderName_LevelPrefabs + currentLevel.LevelPrefabName, typeof(GameObject));
                while (!resource.isDone)
                {
                    yield return resource;
                }

                if (resource.asset == null)
                {
                    Debug.LogError("Level Prefab is Null->" + i);
                    continue;
                }

                GameObject tempLevel = Instantiate(resource.asset, posTemp, Quaternion.Euler(currentLevel.SpawnRotation)) as GameObject;
                tempLevel.GetComponent<Level>().levelScriptable = currentLevel;
                tempLevel.name = currentLevel.LevelID.ToString();
                currentLevel.LevelPrefab = tempLevel.GetComponent<Level>();

                cacheLevels[i] = tempLevel;
            }
        }
    }

    private void CacheControl()
    {
        while (cacheLevels.Count > LevelSettings.LevelCreateCounter)
        {
            DestroyImmediate(cacheLevels[0]);
            cacheLevels.RemoveAt(0);
        }

        Resources.UnloadUnusedAssets();
    }

    private void AddCacheLevel(GameObject temp) => cacheLevels.Add(temp);

    private void CheckScriptableLevels(int checkIndex)
    {
        while (checkIndex >= Levels.Count)
        {
            List<LevelScriptable> tempList = new List<LevelScriptable>();

            if (checkIndex + (LevelSettings.LevelCreateCounter / 2) > Levels.Count)
            {
                for (int i = 0; i < Levels.Count; i++)
                    tempList.Add(Levels[i]);

                for (int i = 0; i < tempList.Count; i++)
                    Levels.Add(tempList[i]);
            }
        }
    }

    public int GetLevelIndexForUI() => LevelSettings.ActualCurrentLevelIndex + 1;

    public int GetLevelIndex() => LevelSettings.ActualCurrentLevelIndex;

    public LevelScriptable GetCurrentLevelData() => LevelSettings.CurrentLevelData;

}

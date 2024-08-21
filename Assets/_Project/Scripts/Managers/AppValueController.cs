using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class AppValueController : MonoBehaviour
{
    public static AppValueController Instance;

    #region Private Fields

    private string ResourceFolderName_VariablesScriptable = "Variables/";

    private PlayerResourceData _playerResourceData = new PlayerResourceData();

    #endregion

    #region Public Fields

    public delegate void DL_EconomyUpdate();
    public DL_EconomyUpdate DL_EconomyUpdated;

    public delegate void DL_VariableUpdate(VariableID id);
    public DL_VariableUpdate DL_VariableUpdated;

    [HideInInspector] public List<VariableScriptable> Variables = new List<VariableScriptable>();

    public PlayerResourceData PlayerResourceData 
    {
        get { return _playerResourceData; } 
        set { _playerResourceData = value; }
    }
    #endregion

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
        if (!PlayerPrefs.HasKey(ConfigDefaults.playerNamePrefKey))
        {
            string defaultName = "User";
            PlayerPrefs.SetString(ConfigDefaults.playerNamePrefKey, defaultName);
        }

        //Game Economy
        yield return StartCoroutine(GetVariables());

        GetPlayerResourceData(true);
    }

    public void GetPlayerResourceData(bool newActive = false)
    {
        if (newActive)
            PlayerResourceData = new PlayerResourceData();

        PlayerResourceData.resWood = GetVariable(VariableID.ResWood);
    }

    #region VARIABLE

    /// <summary>
    /// Fetch VC values every time the app is opened
    /// </summary>
    private IEnumerator GetVariables()
    {
        Variables = new List<VariableScriptable>();

        VariableScriptable[] tempVariables = Resources.LoadAll<VariableScriptable>(ResourceFolderName_VariablesScriptable) as VariableScriptable[];
        tempVariables = tempVariables.OrderBy(e => e.variableID).ToArray();

        for (int i = 0; i < tempVariables.Length; i++)
        {
            tempVariables[i].GetValue();
            Variables.Add(tempVariables[i]);
        }

        yield return null;
    }

    /// <summary>
    /// Fetch specific VC amount
    /// </summary>
    /// <param name="virtualCurrencyID"></param>
    /// <returns></returns>
    public int GetVariable(VariableID variableID)
    {
        for (int i = 0; i < Variables.Count; i++)
        {
            if (Variables[i].variableID == variableID)
                return Variables[i].Value;
        }

        return 0;
    }

    /// <summary>
    /// Update Game Economy
    /// </summary>
    /// <param name="virtualCurrencyID"></param>
    /// <param name="fee"></param>
    /// <returns></returns>
    public bool SetVariable(VariableID variableID, int fee)
    {
        for (int i = 0; i < Variables.Count; i++)
        {
            if (Variables[i].variableID == variableID)
            {
                // Not Enough Money
                if (Variables[i].Value + fee < 0)
                    return false;

                Variables[i].Value += fee;

                Variables[i].SetValue();

                if (DL_EconomyUpdated != null)
                    DL_EconomyUpdated();

                return true;
            }
        }

        return false;
    }

    public void SetForceVariable(VariableID variableID, int value)
    {
        for (int i = 0; i < Variables.Count; i++)
        {
            if (Variables[i].variableID == variableID)
            {
                Variables[i].Value = value;
                Variables[i].SetValue();

                if (DL_EconomyUpdated != null)
                    DL_EconomyUpdated();
            }
        }
    }

    /// <summary>
    /// Is there enough value
    /// </summary>
    /// <param name="virtualCurrencyID"></param>
    /// <param name="fee"></param>
    /// <returns></returns>
    public bool CheckVariableValueEnough(VariableID variableID, int fee)
    {
        for (int i = 0; i < Variables.Count; i++)
        {
            if (Variables[i].variableID == variableID)
                if (Variables[i].Value + fee < 0)
                    return false;
                else
                    return true;
        }

        return false;
    }

    public VariableID ConvertPrefabIdToResourceDataId(VariableID id)
    {
        VariableID value = VariableID.Empty;
        switch (id)
        {
            case VariableID.Wood1:
                value = VariableID.ResWood;
                break;
            default:
                break;
        }

        return value;
    }

    #endregion

    public string GetFormatNumber(int num)
    {
        if (num >= 100000000)
        {
            return (num / 1000000D).ToString("0.#M");
        }
        if (num >= 1000000)
        {
            return (num / 1000000D).ToString("0.##M");
        }
        if (num >= 100000)
        {
            return (num / 1000D).ToString("0.#k");
        }
        if (num >= 10000)
        {
            return (num / 1000D).ToString("0.##k");
        }

        return num.ToString("#,0");
    }

    #region Playfab Field

    public void SaveForce()
    {
        if (PlayerMultiplayer.LocalPlayerInstance == null) return;

        PlayfabSetPlayerResourceData(this.PlayerResourceData);
    }

    public void PlayfabGetPlayerResourceData(string jsonData)
    {
        if (string.IsNullOrEmpty(jsonData))
        {
            PlayfabSetPlayerResourceData(this.PlayerResourceData);
            return;
        }

        PlayerResourceData playfabPayerResourceData = new PlayerResourceData();
        playfabPayerResourceData = JsonUtility.FromJson<PlayerResourceData>(jsonData);
        PlayerResourceData = playfabPayerResourceData;

        SetForceVariable(VariableID.ResWood, PlayerResourceData.resWood);
    }

    public void PlayfabSetPlayerResourceData(PlayerResourceData playerResourceData)
    {
        string toStr = JsonUtility.ToJson(playerResourceData);
        PlayfabManager.Instance.SetUserData(ConfigDefaults.PlayerResourceDataText, toStr);
    }

    #endregion
}

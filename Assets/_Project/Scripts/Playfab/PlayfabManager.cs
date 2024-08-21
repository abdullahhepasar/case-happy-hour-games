using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.Networking;
using PlayFab.ClientModels;

[RequireComponent(typeof(PlayfabClientAPIController))]
[RequireComponent(typeof(PlayfabMultiplayerAPIController))]
public class PlayfabManager : MonoBehaviour
{
    #region Private Field

    private bool _networkActive = false;

    #endregion

    #region public Fields

    public static PlayfabManager Instance;

    [HideInInspector] public PlayfabClientAPIController PCC;
    [HideInInspector] public PlayfabMultiplayerAPIController PMC;

    public delegate void DL_PCC_LoginComplete(bool active);
    public static DL_PCC_LoginComplete DL_PCC_LoginCompleted;

    public delegate void DL_PMC_TicketCreate(string ticketId);
    public static DL_PMC_TicketCreate DL_PMC_TicketCreated;

    public delegate void DL_PMC_MatchStatusUpdate(string statusMatch, string matchId);
    public static DL_PMC_MatchStatusUpdate DL_PMC_MatchStatusUpdated;

    public delegate void DL_PMC_MatchStartUpdate(List<string> memberEntityIdList);
    public static DL_PMC_MatchStartUpdate DL_PMC_MatchStartUpdated;

    public delegate void DL_PMC_MatchCancelUpdate();
    public static DL_PMC_MatchCancelUpdate DL_PMC_MatchCancelUpdated;

    public bool NetworkActive
    {
        get { return _networkActive; }
        set { _networkActive = value; }
    }

    #endregion

    #region MonoBehaviour CallBacks

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

        SettingConfig();
    }

    #endregion

    #region Private Methods

    private void SettingConfig()
    {
        PCC = this.GetComponent<PlayfabClientAPIController>();
        PMC = this.GetComponent<PlayfabMultiplayerAPIController>();
    }

    private string GetPlayerID()
    {
        string id = SystemInfo.deviceUniqueIdentifier;

#if UNITY_EDITOR
        id += "Unity";
#endif

        return id;
    }

    #endregion

    #region Public Methots

    public IEnumerator LoadDatas()
    {
        yield return StartCoroutine(CheckInternetConnection());

        Login();
    }

    public void Login()
    {
        if (NetworkActive)
        {
            string playerID = GetPlayerID();
            LoginWithCustomID(playerID);
        }
    }

    public void LoginWithCustomID(string playerID)
    {
        PCC.SetLoginPlayfab(playerID);
    }

    public void StartPhotonManager(string PhotonCustomAuthenticationToken, string PlayfabId, string Nickname)
    {
        LauncherController.Instance.ConnectToRegionMaster(PhotonCustomAuthenticationToken, PlayfabId, Nickname);
    }

    public void StartMatch(string creatorEntityId, string roomName, List<string> memberEntityIdList)
    {
        StartCoroutine(LauncherController.Instance.Matchmaking(creatorEntityId, roomName, memberEntityIdList));
    }

    #region PLAYER(USER) DATA

    public void SetUserData(string keyID, string data)
    {
        Dictionary<string, string> Data = new Dictionary<string, string>()
            {
                { keyID,  data }
            };

        PCC.UpdateUserData(Data, null, UserDataPermission.Public);
    }

    public void GetUserData(GetUserDataResult result)
    {
        if (result.Data.ContainsKey(ConfigDefaults.PlayerResourceDataText))
            AppValueController.Instance.PlayfabGetPlayerResourceData(result.Data[ConfigDefaults.PlayerResourceDataText].Value);
        else
            AppValueController.Instance.SaveForce();
    }

    #endregion

    #endregion

    #region TOOLS

    public void DebugLog(string text, Color color)
    {
        Debug.Log($"<color=#{ColorUtility.ToHtmlStringRGB(color)}>{text}</color>");
    }

    private IEnumerator CheckInternetConnection()
    {
        NetworkActive = true;
        yield break;

        UnityWebRequest request = new UnityWebRequest("http://google.com");
        request.timeout = 3;

        yield return request.SendWebRequest();
        if (request.error != null)
            NetworkActive = false;
        else
            NetworkActive = true;
    }

    #endregion
}

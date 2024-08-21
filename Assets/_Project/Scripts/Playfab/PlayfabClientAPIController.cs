using PlayFab;
using Photon.Pun;
using PlayFab.ClientModels;
using PlayFab.CloudScriptModels;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayfabClientAPIController : MonoBehaviour
{
    #region private Fields

    private PlayfabManager playfabManager;

    #endregion

    #region public Fields

    public bool LOGINACTIVE = false;

    public LoginResult loginResult = new LoginResult();
    public GetAccountInfoResult getAccountInfoResult = new GetAccountInfoResult();
    public GetUserDataResult UserDataResult = new GetUserDataResult();
    public List<StatisticValue> statisticValues = new List<StatisticValue>();
    public Dictionary<string, string> TitleData = new Dictionary<string, string>();

    public static string SessionTicket;
    public static string EntityId;
    public static string EntityType;

    #endregion

    #region MonoBehaviour CallBacks

    private void Awake()
    {
        playfabManager = GetComponent<PlayfabManager>();
    }

    #endregion

    #region LOGIN

    private PlayerProfileViewConstraints GetPlayerProfileViewConstraints()
    {
        PlayerProfileViewConstraints playerProfileViewConstraints = new PlayerProfileViewConstraints
        {
            ShowAvatarUrl = true,
            ShowBannedUntil = false,
            ShowCampaignAttributions = false,
            ShowContactEmailAddresses = true,
            ShowCreated = true,
            ShowDisplayName = true,
            ShowExperimentVariants = false,
            ShowLastLogin = true,
            ShowLinkedAccounts = false,
            ShowLocations = false,
            ShowMemberships = false,
            ShowOrigination = false,
            ShowPushNotificationRegistrations = false,
            ShowStatistics = true,
            ShowTags = true,
            ShowTotalValueToDateInUsd = false,
            ShowValuesToDate = false
        };

        return playerProfileViewConstraints;
    }

    private GetPlayerCombinedInfoRequestParams GetPlayerCombinedInfoRequestParams()
    {
        GetPlayerCombinedInfoRequestParams getPlayerCombinedInfoRequestParams = new GetPlayerCombinedInfoRequestParams
        {
            GetCharacterInventories = true,
            GetCharacterList = true,
            GetPlayerProfile = true,
            GetPlayerStatistics = true,
            GetTitleData = true,
            GetUserAccountInfo = true,
            GetUserData = true,
            GetUserInventory = true,
            GetUserReadOnlyData = true,
            GetUserVirtualCurrency = true,
            PlayerStatisticNames = PlayerStatisticNames(),
            ProfileConstraints = GetPlayerProfileViewConstraints(),
            TitleDataKeys = TitleDataKeys(),
            UserDataKeys = UserDataKeys(),
            UserReadOnlyDataKeys = UserReadOnlyDataKeys()
        };

        return getPlayerCombinedInfoRequestParams;
    }

    private List<string> PlayerStatisticNames() => new List<string>();

    private List<string> TitleDataKeys() => new List<string>();

    private List<string> UserDataKeys() => new List<string>();

    private List<string> UserReadOnlyDataKeys() => new List<string>();

    public void SetLoginPlayfab(string PlayerID)
    {
        LoginWithCustomID(PlayerID);
    }

    private void LoginWithCustomID(string PlayerID)
    {
        PlayFabClientAPI.LoginWithCustomID(new LoginWithCustomIDRequest()
        {
            TitleId = PlayFabSettings.TitleId,
            CreateAccount = true,
            CustomId = PlayerID,
            InfoRequestParameters = GetPlayerCombinedInfoRequestParams()
        }, OnAuthenticatePlayFabSuccess, OnAuthenticatePlayFabError);
    }

    private void OnAuthenticatePlayFabSuccess(LoginResult e)
    {
        loginResult = e;

        playfabManager.DebugLog("OnAuthenticatePlayFabSuccess->" + loginResult.PlayFabId, Color.green);

        SessionTicket = loginResult.SessionTicket;
        EntityId = PlayFabSettings.staticPlayer.EntityId;
        EntityType = PlayFabSettings.staticPlayer.EntityType;

        //Player Statistic verilerini ekle
        statisticValues = loginResult.InfoResultPayload.PlayerStatistics;

        //Title Datayý Al
        TitleData = loginResult.InfoResultPayload.TitleData;

        StartCoroutine(StartAfterLogin());
    }

    private void OnAuthenticatePlayFabError(PlayFabError error)
    {
        playfabManager.DebugLog("OnAuthenticatePlayFabError->" + error.GenerateErrorReport(), Color.red);

        LOGINACTIVE = false;

        if (PlayfabManager.DL_PCC_LoginCompleted != null)
            PlayfabManager.DL_PCC_LoginCompleted(false);
    }

    #endregion

    #region Private Methods

    private IEnumerator StartAfterLogin()
    {
        //Player In Title Account Info
        GetAccountInfo(loginResult.PlayFabId);

        //Player Data (Title)
        GetUserData(loginResult.PlayFabId, null);

        var fParams = new Dictionary<string, object>();
        fParams.Add(ConfigDefaults.TablePartitionKey, loginResult.PlayFabId);
        CallExecuteFunction(ConfigDefaults.FUNCTION_NAME_GAME_LAUNCH_COUNTER, fParams);

        yield return new WaitForEndOfFrame();

        LOGINACTIVE = true;

        if (PlayfabManager.DL_PCC_LoginCompleted != null)
            PlayfabManager.DL_PCC_LoginCompleted(true);
    }

    #endregion

    #region PLAYER

    private void GetAccountInfo(string PlayfabId)
    {
        //https://docs.microsoft.com/en-us/rest/api/playfab/client/account-management/get-account-info?view=playfab-rest

        PlayFabClientAPI.GetAccountInfo(new GetAccountInfoRequest()
        {
            PlayFabId = PlayfabId
        },
        result =>
        {
            playfabManager.DebugLog("OnGetAccountInfoRequest-> DisplayName->" + result.AccountInfo.TitleInfo.DisplayName, Color.green);

            getAccountInfoResult = result;

            if (string.IsNullOrEmpty(result.AccountInfo.TitleInfo.DisplayName))
                UpdateUserTitleDisplayName(PlayerPrefs.GetString(ConfigDefaults.playerNamePrefKey));
            else
                GetPhotonAuthenticationToken(PhotonNetwork.PhotonServerSettings.AppSettings.AppIdRealtime, loginResult.PlayFabId, result.AccountInfo.TitleInfo.DisplayName);
        },
        error =>
        {
            playfabManager.DebugLog("OnGetAccountInfoPlayFabError->" + error.GenerateErrorReport(), Color.red);
        });
    }

    public void UpdateUserTitleDisplayName(string DisplayName)
    {
        //https://docs.microsoft.com/en-us/rest/api/playfab/client/account-management/update-user-title-display-name?view=playfab-rest

        PlayFabClientAPI.UpdateUserTitleDisplayName(new UpdateUserTitleDisplayNameRequest
        {
            DisplayName = DisplayName
        },
        result =>
        {
            Debug.Log("OnUpdateUserTitleDisplayName->" + result.DisplayName + " Updated");

            GetPhotonAuthenticationToken(PhotonNetwork.PhotonServerSettings.AppSettings.AppIdRealtime, loginResult.PlayFabId, result.DisplayName);
        },
        error =>
        {
            Debug.Log("OnUpdateUserTitleDisplayNamePlayFabError->" + error.GenerateErrorReport());
        });
    }

    public void UpdateUserData(Dictionary<string, string> Data, List<string> KeysToRemove, UserDataPermission userDataPermission)
    {
        //https://docs.microsoft.com/en-us/rest/api/playfab/client/player-data-management/update-user-data?view=playfab-rest

        PlayFabClientAPI.UpdateUserData(new UpdateUserDataRequest()
        {
            Data = Data,
            KeysToRemove = KeysToRemove,
            Permission = userDataPermission
        },
        result =>
        {
            playfabManager.DebugLog("OnUpdateUserDataRequest->" + result.DataVersion, Color.green);
        },
        error =>
        {
            playfabManager.DebugLog("OnUpdateUserDataRequestPlayFabError->" + error.GenerateErrorReport(), Color.red);
        });
    }

    public void GetUserData(string PlayFabId, List<string> Keys)
    {
        //https://docs.microsoft.com/en-us/rest/api/playfab/client/player-data-management/get-user-data?view=playfab-rest

        PlayFabClientAPI.GetUserData(new GetUserDataRequest()
        {
            PlayFabId = PlayFabId,
            Keys = Keys
        },
        result =>
        {
            playfabManager.DebugLog("OnGetUserDataRequest->" + result.Data, Color.green);

            UserDataResult = result;

            playfabManager.GetUserData(UserDataResult);
        },
        error =>
        {
            playfabManager.DebugLog("OnGetUserDataPlayFabError->" + error.GenerateErrorReport(), Color.red);
        });
    }

    #endregion

    #region PHOTON AUTHENTICATION

    public void GetPhotonAuthenticationToken(string AppIdRealtime, string PlayfabId, string Nickname)
    {
        //https://docs.microsoft.com/en-us/rest/api/playfab/client/authentication/get-photon-authentication-token?view=playfab-rest
        //https://doc.photonengine.com/en-us/realtime/current/reference/playfab

        PlayFabClientAPI.GetPhotonAuthenticationToken(new GetPhotonAuthenticationTokenRequest()
        {
            PhotonApplicationId = AppIdRealtime
        },
        result =>
        {
            playfabManager.DebugLog("OnGetPhotonAuthenticationToken", Color.green);

            playfabManager.StartPhotonManager(result.PhotonCustomAuthenticationToken, PlayfabId, Nickname);
        },
        error =>
        {
            playfabManager.DebugLog("OnGetPhotonAuthenticationTokenPlayFabError", Color.red);
        });
    }

    #endregion

    #region CLOUD

    public void CallExecuteFunction(string functionName, Dictionary<string, object> fParams)
    {
        // Create the reset request
        var request = new ExecuteFunctionRequest()
        {
            Entity = new PlayFab.CloudScriptModels.EntityKey()
            {
                Id = PlayFabSettings.staticPlayer.EntityId, //Get this from when you logged in,
                Type = PlayFabSettings.staticPlayer.EntityType, //Get this from when you logged in
            },
            FunctionName = functionName,
            FunctionParameter = fParams,
            GeneratePlayStreamEvent = true
        };

        playfabManager.DebugLog("request::" + request.ToJson(), Color.white);

        PlayFabCloudScriptAPI.ExecuteFunction(request, (ExecuteFunctionResult result) =>
        {
            if (result.FunctionResultTooLarge ?? false)
            {
                playfabManager.DebugLog("This can happen if you exceed the limit that can be returned from an Azure Function, See PlayFab Limits Page for details.", Color.white);
                return;
            }

            playfabManager.DebugLog($"The {result.FunctionName} function took {result.ExecutionTimeMilliseconds} to complete", Color.white);
            playfabManager.DebugLog($"Result: {result.FunctionResult.ToString()}", Color.white);

            int count;
            if (int.TryParse(result.FunctionResult.ToString(), out count))
            {
                AppValueController.Instance.SetForceVariable(VariableID.GameLaunchCounter, count);

                if (AppValueController.Instance.DL_VariableUpdated != null)
                    AppValueController.Instance.DL_VariableUpdated(VariableID.GameLaunchCounter);
            }

        }, (PlayFabError error) =>
        {
            playfabManager.DebugLog($"Opps Something went wrong: {error.GenerateErrorReport()}", Color.white);
        });
    }

    #endregion
}

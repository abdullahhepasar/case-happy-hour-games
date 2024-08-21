using PlayFab;
using Photon.Pun;
using Photon.Realtime;
using System.Collections;
using System.Collections.Generic;

public class LauncherController : MonoBehaviourPunCallbacks
{
    #region Private Fields

    private bool isConnecting;
    private bool isLobbyConnected;

    private string gameVersion = "1";

    private bool joinWaitingInRoom = false;
    private string joinWaitingInRoomName;
    private string[] joinWaitingExpectedUsers;

    private List<RoomInfo> _cahceRoomList = new List<RoomInfo>();

    #endregion

    #region public Fields

    public static LauncherController Instance;

    #endregion

    #region MonoBehaviour CallBacks

    private void Awake()
    {
        //Check if instance already exists
        if (Instance == null)
        {
            //if not, set instance to this
            Instance = this;
        }

        StartSettings();
    }

    #endregion

    #region Public Methods

    public void ConnectToRegionMaster(string photonCustomAuthenticationToken, string PlayfabId, string nickName)
    {
        isConnecting = true;

        if (!PhotonNetwork.IsConnectedAndReady)
        {
            PhotonNetwork.NickName = nickName;

            AuthenticationValues authValues = new AuthenticationValues();
            authValues.AuthType = CustomAuthenticationType.Custom;
            authValues.AddAuthParameter("username", PlayfabManager.Instance.PCC.loginResult.PlayFabId);
            authValues.AddAuthParameter("token", photonCustomAuthenticationToken);
            authValues.UserId = PlayfabId;
            PhotonNetwork.AuthValues = authValues;

            PhotonNetwork.ConnectUsingSettings();
            PhotonNetwork.GameVersion = this.gameVersion;
        }
    }

    public IEnumerator Matchmaking(string creatorEntityId, string roomName, List<string> memberEntityIdList)
    {
        while (!PhotonNetwork.IsConnected || !isLobbyConnected)
        {
            yield return null;
        }

        MatchmakingPlayfabCreateAndJoinLobby(creatorEntityId, roomName, memberEntityIdList);
    }

    #endregion

    #region Private Methods

    private void StartSettings()
    {
        PhotonNetwork.AutomaticallySyncScene = true;

        isConnecting = false;
        isLobbyConnected = false;

        joinWaitingInRoom = false;
        joinWaitingInRoomName = "";
    }

    private void JoinGame(string roomName, string[] expectedUsers)
    {
        ResetData();
        PhotonNetwork.JoinRoom(roomName);
    }

    private void FindTargetRoom()
    {
        for (int i = 0; i < _cahceRoomList.Count; i++)
        {
            if (_cahceRoomList[i].Name == this.joinWaitingInRoomName)
            {
                JoinGame(this.joinWaitingInRoomName, joinWaitingExpectedUsers);
                break;
            }
        }
    }

    private void ResetData()
    {
        joinWaitingInRoom = false;
        this.joinWaitingInRoomName = null;
        this.joinWaitingInRoomName = null;
        joinWaitingExpectedUsers = new string[0];
    }

    #endregion

    #region Matchmaking System with Playfab

    private void MatchmakingPlayfabCreateAndJoinLobby(string creatorEntityId, string roomName, List<string> memberEntityIdList)
    {
        string entityId = PlayFabSettings.staticPlayer.EntityId; // PlayFab user's entity Id
        string entityType = PlayFabSettings.staticPlayer.EntityType; // PlayFab user's entity type

        joinWaitingExpectedUsers = new string[memberEntityIdList.Count];
        for (int i = 0; i < joinWaitingExpectedUsers.Length; i++)
        {
            joinWaitingExpectedUsers[i] = memberEntityIdList[i];
        }

        //Only Room Create on Master
        if (entityId != creatorEntityId)
        {
            //Other Players Join Waiting
            joinWaitingInRoom = true;
            joinWaitingInRoomName = roomName;

            //Check Other Rooms
            FindTargetRoom();
            return;
        }

        RoomOptions roomOptions = new RoomOptions
        {
            PublishUserId = true,
            MaxPlayers = memberEntityIdList.Count,
            IsVisible = true,
            PlayerTtl = 0,
            EmptyRoomTtl = 1000
        };

        PhotonNetwork.CreateRoom(roomName, roomOptions, TypedLobby.Default);
    }

    #endregion

    #region MonoBehaviourPunCallbacks CallBacks

    public override void OnConnectedToMaster()
    {
        if (!isConnecting) return;

        PhotonNetwork.JoinLobby(TypedLobby.Default);
    }

    public override void OnJoinedLobby()
    {
        isLobbyConnected = true;
    }

    public override void OnLeftLobby()
    {
        isLobbyConnected = false;
    }

    public override void OnDisconnected(DisconnectCause cause)
    {
        isConnecting = false;

        UIManager.Instance.UICreatePrefabs((int)UIPrefabNames.MainMenu);
    }

    public override void OnJoinedRoom()
    {
        GameManager.Instance.GameSettings();
    }

    public override void OnLeftRoom()
    {
        ResetData();
    }

    public override void OnRoomListUpdate(List<RoomInfo> roomList)
    {
        _cahceRoomList = roomList;

        if (!joinWaitingInRoom) return;

        FindTargetRoom();
    }

    #endregion
}

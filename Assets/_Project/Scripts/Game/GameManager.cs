using Photon.Pun;
using Photon.Realtime;
using System.Collections;
using UnityEngine;

public class GameManager : MonoBehaviourPunCallbacks
{
    #region Private Fields

    [SerializeField] private GameObject PlayerMultiplayerPrefab;

    [SerializeField] private Transform[] PlayerSpawnPositions;

    [SerializeField] private SceneGenerator sceneGenerator;

    private const string _folderNamePhoton = "Photon/";

    #endregion

    #region Public Fields

    public static GameManager Instance;

    public UnityCharacter[] unityCharacters;

    public delegate void DL_GM_GamePause();
    public DL_GM_GamePause DL_GM_GamePauseChanged;

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
    }

    #endregion

    #region Photon Callbacks

    public override void OnJoinedRoom()
    {
        if (PlayerMultiplayer.LocalPlayerInstance != null) return;

        CreateObject(this.PlayerMultiplayerPrefab);
    }

    public override void OnPlayerLeftRoom(Player other)
    {
        DebugLog("OnPlayerLeftRoom() " + other.NickName, Color.green);
    }

    #endregion

    #region Public Methods

    public void GameSettings()
    {
        if (!PhotonNetwork.IsConnected)
        {
            UIManager.Instance.UICreatePrefabs((int)UIPrefabNames.MainMenu);

            return;
        }

        if (PlayerMultiplayerPrefab == null)
        {
            DebugLog("<b>Missing</b></Color> PlayerMultiplayer Reference.", Color.red);
        }
        else
        {
            if (PhotonNetwork.InRoom && PlayerMultiplayer.LocalPlayerInstance == null)
            {
                CreateObject(this.PlayerMultiplayerPrefab);
            }
            else
            {
                DebugLog("Ignoring scene load for " + SceneManagerHelper.ActiveSceneName);
            }
        }

        //Scene Create
        sceneGenerator.CreateScene();

        UIManager.Instance.UICreatePrefabs((int)UIPrefabNames.GamePlay);
    }

    public Transform GetPlayerTransform(int index) => PlayerSpawnPositions[index];

    public UnityCharacter GetUnityCharacter(int index) => unityCharacters[index];

    public void LeaveRoom()
    {
        StartCoroutine(DisconectLoad());
    }

    public void QuitApplication()
    {
        Application.Quit();
    }

    #endregion

    #region Private Methods

    private IEnumerator DisconectLoad()
    {
        PhotonNetwork.LeaveRoom();

        UILoading.Instance.SetTargetValue(0f);

        while (PhotonNetwork.InRoom)
        {
            yield return null;
        }

        UILoading.Instance.SetTargetValue(1f);
        StartCoroutine(UILoading.Instance.LoadingComplete());

        UIManager.Instance.UICreatePrefabs((int)UIPrefabNames.MainMenu);
    }

    public static GameObject CreateObject(GameObject obj, Vector3 pos = default, Vector3 rot = default, string path = _folderNamePhoton)
    {
        return PhotonNetwork.Instantiate(path + obj.name, pos, Quaternion.Euler(rot), 0);
    }

    private void DebugLog(string text, Color color = default)
    {
        if (color == default) color = Color.white;

        Debug.Log($"<color=#{ColorUtility.ToHtmlStringRGB(color)}>{text}</color>");
    }

    #endregion
}

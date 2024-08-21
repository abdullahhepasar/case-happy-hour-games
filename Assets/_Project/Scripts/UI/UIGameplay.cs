using Photon.Pun;
using UnityEngine;
using UnityEngine.UI;

public class UIGameplay : MonoBehaviour, IUIDependencies
{
    public RectTransform coinTarget;
    public Text MoneyText;
    public Text MatchIDText;

    [SerializeField] private SelectCharacters selectCharacters;

    public void OnEnable()
    {
        
    }

    public void OnDisable()
    {
        
    }

    void Start()
    {
        OnStart();
    }

    void OnStart()
    {
        UIUpdate();

        selectCharacters.Initialize(UIManager.Instance.CanvasStatic, PlayerMultiplayer.LocalPlayerInstance.GetComponent<PlayerMultiplayer>());
    }

    public void UIUpdate()
    {
        MoneyText.text = AppValueController.Instance.GetFormatNumber(AppValueController.Instance.GetVariable(VariableID.MN));
        SetLanguage();

        MatchIDText.text = PhotonNetwork.InRoom ? "Match ID:" + PhotonNetwork.CurrentRoom.Name : "";
    }

    public void SetLanguage()
    {

    }

    private void SetGameobjectState(GameObject gameObject, bool state)
    {
        gameObject.SetActive(state);
    }

    #region SET BUTTONS

    public void SetSettingsButton()
    {
        VibrationsController.Instance.SetVibration_Soft();

        UIManager.Instance.UIPopupCreatePrefabs((int)UIPrefabNames.PopupSettings);
    }

    public void SetCancelMatchButton()
    {
        PlayfabManager.Instance.PMC.CancelMatchmakingTicket();
        GameManager.Instance.LeaveRoom();
    }

    #endregion

    public void UIChange(int index)
    {
        UIManager.Instance.UICreatePrefabs(index);
    }
}
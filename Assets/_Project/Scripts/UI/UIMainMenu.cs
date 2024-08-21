using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

using Color = UnityEngine.Color;

public class UIMainMenu : MonoBehaviour, IUIDependencies
{
    #region public Fields

    public Text LevelText;
    public Text MoneyText;
    public Text GameLaunchCounterText;
    public Text QueueStatusText;

    public UIAnimationCoin uIAnimationCoin;

    public GameObject ConnectButton;
    public GameObject FindMatchButton;
    public GameObject CancelMatchButton;

    #endregion

    #region MonoBehaviour CallBacks

    public void OnEnable()
    {
        AppValueController.Instance.DL_EconomyUpdated += DL_EconomyUpdate;
        AppValueController.Instance.DL_VariableUpdated += DL_VariableUpdated;

        PlayfabManager.DL_PCC_LoginCompleted += DL_LoginCompleted;
        PlayfabManager.DL_PMC_TicketCreated += DL_PMC_TicketCreated;
        PlayfabManager.DL_PMC_MatchStatusUpdated += DL_PMC_MatchStatusUpdated;
        PlayfabManager.DL_PMC_MatchStartUpdated += DL_PMC_MatchStartUpdated;
        PlayfabManager.DL_PMC_MatchCancelUpdated += DL_PMC_MatchCancelUpdated;

        ConnectButton.SetActive(!PlayfabManager.Instance.PCC.LOGINACTIVE);
        FindMatchButton.SetActive(PlayfabManager.Instance.PCC.LOGINACTIVE);
        CancelMatchButton.SetActive(false);

        UIUpdate();
    }

    public void OnDisable()
    {
        AppValueController.Instance.DL_EconomyUpdated -= DL_EconomyUpdate;
        AppValueController.Instance.DL_VariableUpdated -= DL_VariableUpdated;

        PlayfabManager.DL_PCC_LoginCompleted -= DL_LoginCompleted;
        PlayfabManager.DL_PMC_TicketCreated -= DL_PMC_TicketCreated;
        PlayfabManager.DL_PMC_MatchStatusUpdated -= DL_PMC_MatchStatusUpdated;
        PlayfabManager.DL_PMC_MatchStartUpdated -= DL_PMC_MatchStartUpdated;
        PlayfabManager.DL_PMC_MatchCancelUpdated -= DL_PMC_MatchCancelUpdated;
    }

    #endregion

    public void SetCoinAnimation()
    {
        uIAnimationCoin.SetCrowdCoinsAnimation();
    }

    public void UIUpdate()
    {
        LevelText.text = LanguageManager.Instance.CLS.LevelShortText + " " + LevelManager.Instance.GetLevelIndexForUI().ToString();
        SetLanguage();

        MoneyText.text = AppValueController.Instance.GetFormatNumber(AppValueController.Instance.GetVariable(VariableID.MN));

        GameLaunchCounterText.text = LanguageManager.Instance.CLS.GameLaunchCounterText + " " + AppValueController.Instance.GetVariable(VariableID.GameLaunchCounter).ToString();
    }

    public void SetLanguage()
    {

    }

    public void UIChange(int index)
    {
        UIManager.Instance.UICreatePrefabs(index);
    }

    #region DELEGATES

    private void DL_EconomyUpdate()
    {

    }

    private void DL_VariableUpdated(VariableID id)
    {
        switch (id)
        {
            case VariableID.GameLaunchCounter:
                GameLaunchCounterText.text = LanguageManager.Instance.CLS.GameLaunchCounterText + " " + AppValueController.Instance.GetVariable(id).ToString();
                break;
            default:
                break;
        }
    }

    private void DL_LoginCompleted(bool active)
    {
        ConnectButton.SetActive(!active);
        FindMatchButton.SetActive(active);
        CancelMatchButton.SetActive(false);
    }

    private void DL_PMC_TicketCreated(string ticketId)
    {
        string text = "Ticket Created:" + ticketId + " \n";
        QueueStatusText.text += $"<color=#{ColorUtility.ToHtmlStringRGB(Color.green)}>{text}</color>";

        FindMatchButton.SetActive(false);
        CancelMatchButton.SetActive(true);
    }

    private void DL_PMC_MatchStatusUpdated(string statusMatch, string matchId)
    {
        string text = "Match Status:" + statusMatch;
        if (!string.IsNullOrEmpty(matchId))
            text += " Match Id:" + matchId;

        text += "\n";

        QueueStatusText.text += $"<color=#{ColorUtility.ToHtmlStringRGB(Color.green)}>{text}</color>";

        CancelMatchButton.SetActive(true);
    }

    private void DL_PMC_MatchStartUpdated(List<string> memberEntityIdList)
    {
        string text = "Match Started:" + "\n" + "Members: \n";

        for (int i = 0; i < memberEntityIdList.Count; i++)
        {
            text += memberEntityIdList[i] + "\n";
        }

        QueueStatusText.text += $"<color=#{ColorUtility.ToHtmlStringRGB(Color.green)}>{text}</color>";

        //CancelMatchButton.SetActive(false);
    }

    private void DL_PMC_MatchCancelUpdated()
    {
        QueueStatusText.text = "CANCEL MATCH";

        FindMatchButton.SetActive(true);
        CancelMatchButton.SetActive(false);
    }

    #endregion

    #region SET BUTTONS

    public void SetSettingsButton()
    {
        VibrationsController.Instance.SetVibration_Soft();

        UIManager.Instance.UIPopupCreatePrefabs((int)UIPrefabNames.PopupSettings);
    }

    public void SetConnectButton()
    {
        PlayfabManager.Instance.Login();
    }

    public void SetFindMatchButton()
    {
        QueueStatusText.text = "";
        FindMatchButton.SetActive(false);

        string text = "Submitting Ticket \n";
        QueueStatusText.text += $"<color=#{ColorUtility.ToHtmlStringRGB(Color.yellow)}>{text}</color>";

        PlayfabManager.Instance.PMC.CreateMatchmakingTicket(PlayfabMultiplayerAPIController.DefaultQueueMatchmaking);
    }

    public void SetCancelMatchButton()
    {
        PlayfabManager.Instance.PMC.CancelMatchmakingTicket();
    }

    #endregion
}

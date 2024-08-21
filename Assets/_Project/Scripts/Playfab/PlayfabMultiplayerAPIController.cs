using Photon.Pun;
using PlayFab;
using PlayFab.MultiplayerModels;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayfabMultiplayerAPIController : MonoBehaviour
{
    #region private Fields

    private PlayfabManager playfabManager;

    private string _currentTicketId;

    private string _currentQueueName;

    private IEnumerator GetMatchmakingTicketCoroutine;

    private GetMatchResult _currentMatchResult;

    private string playPrefscurrentTicketId = "currentTicketId";

    private string playPrefscurrentQueueName = "currentQueueName";

    private bool checkOldSessionTicket;

    #endregion

    #region public Fields

    public static string DefaultQueueMatchmaking = "DefaultQueueMatchmaking";

    public string CurrentTicketId { get { return _currentTicketId; } set { _currentTicketId = value; } }

    public string CurrentQueueName { get { return _currentQueueName; } set { _currentQueueName = value; } }

    public GetMatchResult CurrentMatchResult { get { return _currentMatchResult; } set { _currentMatchResult = value; } }

    #endregion

    #region MonoBehaviour CallBacks

    private void Awake()
    {
        playfabManager = GetComponent<PlayfabManager>();
    }

    #endregion

    #region MATCHMAKING

    private void MatchmakingReset()
    {
        CurrentTicketId = PlayerPrefs.HasKey(playPrefscurrentTicketId) ? PlayerPrefs.GetString(playPrefscurrentTicketId) : null;
        CurrentQueueName = PlayerPrefs.HasKey(playPrefscurrentQueueName) ? PlayerPrefs.GetString(playPrefscurrentQueueName) : null;
        CurrentMatchResult = null;
    }

    public void CreateMatchmakingTicket(string queueName)
    {
        //https://learn.microsoft.com/en-us/rest/api/playfab/multiplayer/matchmaking/create-matchmaking-ticket?view=playfab-rest

        MatchmakingReset();

        if (!string.IsNullOrEmpty(CurrentTicketId) && !string.IsNullOrEmpty(CurrentQueueName))
        {
            checkOldSessionTicket = true;

            //Check Old Ticket
            if (GetMatchmakingTicketCoroutine != null)
                StopCoroutine(GetMatchmakingTicketCoroutine);

            GetMatchmakingTicketCoroutine = GetMatchmakingTicket(CurrentQueueName, CurrentTicketId);
            StartCoroutine(GetMatchmakingTicketCoroutine);

            return;
        }

        CurrentQueueName = queueName;
        PlayerPrefs.SetString(playPrefscurrentQueueName, CurrentQueueName);

        PlayFabMultiplayerAPI.CreateMatchmakingTicket(new CreateMatchmakingTicketRequest
        {
            Creator = new MatchmakingPlayer
            {
                Entity = new EntityKey
                {
                    Id = PlayfabClientAPIController.EntityId,
                    Type = PlayfabClientAPIController.EntityType
                },
                Attributes = new MatchmakingPlayerAttributes
                {
                    DataObject = new 
                    {
                        Region = PhotonNetwork.CloudRegion
                    }
                }
            },

            GiveUpAfterSeconds = 120,

            QueueName = queueName
        }, OnMatchmakingTicketCreated, OnMatchmakingError);
    }

    private void OnMatchmakingTicketCreated(CreateMatchmakingTicketResult result)
    {
        playfabManager.DebugLog("OnMatchmakingTicketCreatedSuccess->" + result.TicketId, Color.green);

        CurrentTicketId = result.TicketId;
        PlayerPrefs.SetString(playPrefscurrentTicketId, CurrentTicketId);

        if (GetMatchmakingTicketCoroutine != null)
            StopCoroutine(GetMatchmakingTicketCoroutine);

        GetMatchmakingTicketCoroutine = GetMatchmakingTicket(CurrentQueueName, CurrentTicketId);
        StartCoroutine(GetMatchmakingTicketCoroutine);

        if (PlayfabManager.DL_PMC_TicketCreated != null)
            PlayfabManager.DL_PMC_TicketCreated(CurrentTicketId);
    }

    private void OnMatchmakingError(PlayFabError error)
    {
        playfabManager.DebugLog($"OnMatchmakingError: {error.GenerateErrorReport()}", Color.red);
    }

    public IEnumerator GetMatchmakingTicket(string queueName, string ticketId)
    {
        //https://learn.microsoft.com/en-us/rest/api/playfab/multiplayer/matchmaking/get-matchmaking-ticket?view=playfab-rest

        while (true)
        {
            PlayFabMultiplayerAPI.GetMatchmakingTicket(new GetMatchmakingTicketRequest
            {
                TicketId = ticketId,
                QueueName = queueName
            }, OnGetMatchmakingTicket, OnGetMatchmakingTicketError);

            yield return new WaitForSeconds(6f);
        }
    }

    private void OnGetMatchmakingTicket(GetMatchmakingTicketResult result)
    {
        if (PlayfabManager.DL_PMC_MatchStatusUpdated != null)
            PlayfabManager.DL_PMC_MatchStatusUpdated(result.Status, result.MatchId);

        switch (result.Status)
        {
            case "WaitingForMatch":
                break;
            case "WaitingForPlayers":
                break;
            case "WaitingForServer":
                break;
            case "Matched":
                if (GetMatchmakingTicketCoroutine != null)
                    StopCoroutine(GetMatchmakingTicketCoroutine);

                GetMatch(CurrentQueueName, result.MatchId);
                break;
            case "Canceled":
                CancelComplete();
                break;
        }
    }

    private void OnGetMatchmakingTicketError(PlayFabError error)
    {
        playfabManager.DebugLog($"OnGetMatchmakingTicketError: {error.GenerateErrorReport()}", Color.red);

        CancelComplete(true);
    }

    public void GetMatch(string queueName, string matchId)
    {
        //https://learn.microsoft.com/en-us/rest/api/playfab/multiplayer/matchmaking/get-match?view=playfab-rest

        PlayFabMultiplayerAPI.GetMatch(new GetMatchRequest
        {
            MatchId = matchId,
            QueueName = queueName
        }, OnGetMatch, OnMatchmakingError);
    }

    private void OnGetMatch(GetMatchResult result)
    {
        playfabManager.DebugLog("OnGetMatchSuccess->" + result.MatchId, Color.green);

        _currentMatchResult = result;

        List<string> memberEntityIdList = new List<string>();
        for (int i = 0; i < _currentMatchResult.Members.Count; i++)
        {
            memberEntityIdList.Add(_currentMatchResult.Members[i].Entity.Id);
        }

        if (PlayfabManager.DL_PMC_MatchStartUpdated != null)
            PlayfabManager.DL_PMC_MatchStartUpdated(memberEntityIdList);

        string creatorEntityId = _currentMatchResult.Members[0].Entity.Id;
        string roomName = result.MatchId;
        playfabManager.StartMatch(creatorEntityId, roomName, memberEntityIdList);
    }

    public void CancelMatchmakingTicket()
    {
        PlayFabMultiplayerAPI.CancelMatchmakingTicket(new CancelMatchmakingTicketRequest
        {
            TicketId = CurrentTicketId,
            QueueName = CurrentQueueName,
        }, OnCancelMatchmakingTicketResult, OnCancelMatchmakingTicketError);
    }

    private void OnCancelMatchmakingTicketResult(CancelMatchmakingTicketResult result)
    {
        playfabManager.DebugLog("OnCancelMatchmakingTicketResultSuccess", Color.green);

        CancelComplete(true);
    }

    private void OnCancelMatchmakingTicketError(PlayFabError error)
    {
        playfabManager.DebugLog($"OnCancelMatchmakingTicketError: {error.GenerateErrorReport()}", Color.red);

        CancelComplete(true);
    }

    private void CancelComplete(bool force = false)
    {
        PlayerPrefs.DeleteKey(playPrefscurrentQueueName);
        PlayerPrefs.DeleteKey(playPrefscurrentTicketId);

        CurrentTicketId = null;
        string oldQueueName = CurrentQueueName;
        CurrentQueueName = null;

        if (GetMatchmakingTicketCoroutine != null)
            StopCoroutine(GetMatchmakingTicketCoroutine);

        if (!force)
        {
            if (checkOldSessionTicket && !string.IsNullOrEmpty(oldQueueName))
            {
                checkOldSessionTicket = false;
                CreateMatchmakingTicket(oldQueueName);
            }
        }

        if (PlayfabManager.DL_PMC_MatchCancelUpdated != null)
            PlayfabManager.DL_PMC_MatchCancelUpdated();
    }

    #endregion
}

using UnityEngine;
using Photon.Pun;
using TMPro;

[RequireComponent(typeof(TMP_InputField))]
public class PlayerNameInputField : MonoBehaviour
{
    #region Private Fields

    private TMP_InputField _inputField;

    #endregion

    #region MonoBehaviour CallBacks

    void Start()
    {
        string defaultName = string.Empty;
        _inputField = this.GetComponent<TMP_InputField>();

        if (_inputField != null)
        {
            if (PlayerPrefs.HasKey(ConfigDefaults.playerNamePrefKey))
            {
                defaultName = PlayerPrefs.GetString(ConfigDefaults.playerNamePrefKey);
                _inputField.text = defaultName;
            }
        }

        PhotonNetwork.NickName = defaultName;
    }

    #endregion

    #region Public Methods

    /// <summary>
    /// Sets the name of the player, and save it in the PlayerPrefs for future sessions.
    /// </summary>
    /// <param name="value">The name of the Player</param>
    public void SetPlayerName()
    {
        // #Important
        if (string.IsNullOrEmpty(_inputField.text))
        {
            Debug.LogError("Player Name is null or empty");
            return;
        }

        PlayfabManager.Instance.PCC.UpdateUserTitleDisplayName(_inputField.text);

        PlayerPrefs.SetString(ConfigDefaults.playerNamePrefKey, _inputField.text);
    }

    #endregion
}


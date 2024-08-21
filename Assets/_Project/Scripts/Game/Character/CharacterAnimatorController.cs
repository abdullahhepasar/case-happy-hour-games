using Photon.Pun;
using UnityEngine;

public class CharacterAnimatorController : MonoBehaviourPun
{
    #region Private Fields

    private CharacterController _characterController;

    [SerializeField] private Animator _animator;

    private string _animIdle = "Idle";
    public string AnimIdle { get => _animIdle; }

    private string _animGathering = "Gathering";
    public string AnimGathering { get => _animGathering; }

    #endregion

    #region MonoBehaviour CallBacks

    // Start is called before the first frame update
    void Awake()
    {
        _characterController = GetComponent<CharacterController>();
        SetBool(AnimIdle, true);
    }

    // Update is called once per frame
    void Update()
    {
        if (photonView.IsMine == false && PhotonNetwork.IsConnected == true)
        {
            return;
        }

        if (!_animator)
        {
            return;
        }

    }

    #endregion

    #region Public Methods

    public void SetBool(string name, bool active)
    {
        _animator.SetBool(name, active);
    }

    public void AnimEventGathering()
    {
        _characterController.Gathering();
    }

    #endregion
}

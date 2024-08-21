using EPOOutline;
using Photon.Pun;
using UnityEngine;
using UnityEngine.AI;

[RequireComponent(typeof(CharacterAnimatorController))]
public class CharacterController : MonoBehaviourPunCallbacks, IPunObservable
{
    #region Private Fields

    private PlayerMultiplayer _playerMultiplayer;

    private CharacterAnimatorController _characterAnimatorController;

    private Outlinable _outlinable;

    private Vector3 _clickedPosition;

    private NavMeshAgent _navMeshAgent;
    private float _defaultStoppingDistance;

    private bool _selected = false;

    private Resource _currentResource;

    #endregion

    #region public Fields

    public bool Selected 
    { 
        get { return _selected; }
        set 
        {
            _selected = value;

            Outline(_selected);
        }
    }

    public Resource CurrentResource
    {
        get { return _currentResource; }
        set { _currentResource = value; }
    }

    #endregion

    #region MonoBehaviour CallBacks

    private void Awake()
    {
        _characterAnimatorController = GetComponent<CharacterAnimatorController>();

        _outlinable = GetComponent<Outlinable>();
        Outline(Selected);
    }

    private void Update()
    {
        if (photonView.IsMine)
        {
            UpdateStatus();
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        Resource resource = other.GetComponent<Resource>();
        if (resource != null)
        {
            if (!resource.active) return;

            _characterAnimatorController.SetBool(_characterAnimatorController.AnimIdle, false);
            _characterAnimatorController.SetBool(_characterAnimatorController.AnimGathering, true);

            CurrentResource = resource;
        }
    }

    private void OnTriggerExit(Collider other)
    {
        Resource resource = other.GetComponent<Resource>();
        if (resource != null)
        {
            _characterAnimatorController.SetBool(_characterAnimatorController.AnimGathering, false);

            CurrentResource = null;
        }
    }

    #endregion

    #region Private Methods

    private void Outline(bool active)
    {
        _outlinable.enabled = active;
    }

    private void UpdateStatus()
    {
        _navMeshAgent.destination = _clickedPosition;

        if (_navMeshAgent.stoppingDistance != 1f)
        {
            _navMeshAgent.isStopped = false;
            _navMeshAgent.stoppingDistance = 1f;
        }

        if (Vector3.Distance(transform.position, _clickedPosition) < _navMeshAgent.stoppingDistance + 0.1f)
        {
            _navMeshAgent.stoppingDistance = _defaultStoppingDistance;
            _clickedPosition = this.transform.position;

            _characterAnimatorController.SetBool(_characterAnimatorController.AnimIdle, true);
        }
    }

    Vector3 CheckNavmeshPoint(Vector3 point)
    {
        Vector3 bestClosePoint = this.transform.position;

        NavMeshHit hit;
        if (NavMesh.SamplePosition(point, out hit, _defaultStoppingDistance, NavMesh.AllAreas))
            bestClosePoint = hit.position;

        return bestClosePoint;
    }

    #endregion

    #region Public Methods

    public void Initialize(PlayerMultiplayer pm)
    {
        if (!photonView.IsMine) return;

        this._playerMultiplayer = pm;

        _navMeshAgent = GetComponent<NavMeshAgent>();
        _defaultStoppingDistance = _navMeshAgent.stoppingDistance;

        _clickedPosition = this.transform.position;
    }

    public void MoveToClick(Vector3 position)
    {
        _clickedPosition = CheckNavmeshPoint(position);

        _characterAnimatorController.SetBool(_characterAnimatorController.AnimIdle, false);
    }

    public void Gathering()
    {
        if (!photonView.IsMine) return;

        if (CurrentResource == null) return;

        if(!CurrentResource.active)
        {
            _characterAnimatorController.SetBool(_characterAnimatorController.AnimGathering, false);

            CurrentResource = null;

            return;
        }

        int gatherValue = 1;

        int resourceCurrentValue = CurrentResource.CurrentValue;

        if (resourceCurrentValue - gatherValue >= 0)
        {
            resourceCurrentValue--;

            //Update Resource Data
            _playerMultiplayer.AddResourceData(CurrentResource.resourceType, gatherValue);

            CurrentResource.ChangeValueResource(resourceCurrentValue);
        }
    }

    #endregion

    #region IPunObservable Implementation

    public void OnPhotonSerializeView(PhotonStream stream, PhotonMessageInfo info)
    {
        if (stream.IsWriting)
        {

        }
        else
        {

        }
    }

    #endregion
}

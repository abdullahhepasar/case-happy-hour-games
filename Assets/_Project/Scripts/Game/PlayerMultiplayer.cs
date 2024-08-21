using UnityEngine;
using Photon.Pun;
using System.Collections.Generic;

public class PlayerMultiplayer : MonoBehaviourPunCallbacks, IPunObservable
{
    #region Private Fields

    [SerializeField] private int unitCount = 3;

    [SerializeField] private List<CharacterController> units = new List<CharacterController>();

    private string _tagGround = "Ground";
    private string _LayerMaskInputPosition = "Default";
    private float _hitDistance = 1000f;

    private Camera _camera;
    private RaycastHit _hit;
    private Ray _ray;

    #endregion

    #region Public Fields

    public static GameObject LocalPlayerInstance;

    public List<CharacterController> GetUnits() => this.units;

    #endregion

    #region MonoBehaviour CallBacks

    private void Awake()
    {
        if (photonView.IsMine)
        {
            LocalPlayerInstance = gameObject;
        }

        DontDestroyOnLoad(gameObject);
    }

    private void Start()
    {
        if (!photonView.IsMine) return;

        UnityCharacter unityCharacter = GameManager.Instance.GetUnityCharacter(PhotonNetwork.CurrentRoom.PlayerCount - 1);
        CreateUnits(unityCharacter);

        _camera = Camera.main;
    }

    private void Update()
    {
        if (!photonView.IsMine) return;

        //Select and Click to Move Target
        ProcessInputs();
    }

    #endregion

    #region Private Methods

    private void ProcessInputs()
    {
        //Select in SelectCharacters
        //Then selected charachters move

        if (Input.GetMouseButtonDown(1))
        {
            Vector3 clickPosition = MousePositionInWorld(_tagGround);

            if (clickPosition != Vector3.zero)
            {
                List<CharacterController> selectedUnits = GetSelectedCharacters(true);
                MoveCharacters(selectedUnits, clickPosition);

                SelectCharacters(false);
            }
        }
    }

    private Vector3 MousePositionInWorld(string targetTag)
    {
        _ray = _camera.ScreenPointToRay(Input.mousePosition);

        int layer_mask = LayerMask.GetMask(_LayerMaskInputPosition);
        float distance = _hitDistance;

        Physics.Raycast(_ray, out _hit, distance, layer_mask);

        if (_hit.collider == null ||
            !_hit.collider.gameObject.CompareTag(targetTag))
            return Vector3.zero;

        return _hit.point;
    }

    private void SelectCharacters(bool active)
    {
        for (int i = 0; i < GetUnits().Count; i++)
        {
            GetUnits()[i].Selected = active;
        }
    }

    private List<CharacterController> GetSelectedCharacters(bool selected) => GetUnits().FindAll(x => x.Selected == selected);

    private void MoveCharacters(List<CharacterController> moveCharacters, Vector3 targetPosition)
    {
        for (int i = 0; i < moveCharacters.Count; i++)
        {
            moveCharacters[i].MoveToClick(targetPosition);
        }
    }

    private void CreateUnits(UnityCharacter unityCharacter)
    {
        for (int i = 0; i < unitCount; i++)
        {
            Vector3 pos = GameManager.Instance.GetPlayerTransform(PhotonNetwork.CurrentRoom.PlayerCount - 1).position;
            float posOffset = (float)i * 1f;
            pos = new Vector3(pos.x + posOffset, pos.y, pos.z);

            Vector3 rot = Vector3.zero;
            CharacterController unit = GameManager.CreateObject(unityCharacter.prefab, pos, rot).GetComponent<CharacterController>();
            unit.Initialize(this);
            units.Add(unit);
        }
    }

    #endregion

    #region public Methods

    public void AddResourceData(VariableID id, int value)
    {
        VariableID resourceId = AppValueController.Instance.ConvertPrefabIdToResourceDataId(id);

        AppValueController.Instance.SetVariable(resourceId, value);
        AppValueController.Instance.GetPlayerResourceData();
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

using UnityEngine;
using UnityEngine.UI;

public class SelectCharacters : MonoBehaviour
{
    #region Private Fields

    [SerializeField] private Image targetImage;
    private Canvas targetCanvas;

    private Vector3 startPos;
    private RectTransform boxTransform;
    private bool isSelecting;

    private PlayerMultiplayer playerMultiplayer;

    [SerializeField] private Image SelectBoxActivateBG;
    private bool selectActivation = false;

    #endregion

    #region Private Methods

    private void SetBoxState(bool active)
    {
        targetImage.gameObject.SetActive(active);
    }

    private void SelectBaxActivation()
    {
        selectActivation = !selectActivation;

        SelectBoxActivateBG.color = selectActivation ? Color.green : Color.red;
    }

    #endregion

    #region public Methods

    public void Initialize(Canvas canvas, PlayerMultiplayer pm)
    {
        targetCanvas = canvas;

        playerMultiplayer = pm;

        boxTransform = targetImage.GetComponent<RectTransform>();
        boxTransform.pivot = Vector2.one * 0.5f;
        boxTransform.anchorMin = Vector2.one * 0.5f;
        boxTransform.anchorMax = Vector2.one * 0.5f;

        SetBoxState(isSelecting);

        selectActivation = false;
        SelectBaxActivation();
    }

    #endregion

    #region MonoBehaviour CallBacks

    void Update()
    {
        if (!selectActivation)
        {
            isSelecting = false;
            SetBoxState(isSelecting);
            return;
        }

        if (Input.GetMouseButtonDown(0))
        {
            startPos = Input.mousePosition;

            isSelecting = true;
            SetBoxState(isSelecting);
        }
        else if (Input.GetMouseButtonUp(0))
        {
            isSelecting = false;
            SetBoxState(isSelecting);
        }

        if (isSelecting)
        {
            Vector3 center = Vector3.Lerp(startPos, Input.mousePosition, 0.5f);

            Bounds bounds = new Bounds();
            bounds.center = center;

            float xRectSize = Mathf.Abs(startPos.x - Input.mousePosition.x);
            float yRectSize = Mathf.Abs(startPos.y - Input.mousePosition.y);

            bounds.size = new Vector3(xRectSize, yRectSize, 0);

            boxTransform.position = center;
            boxTransform.sizeDelta = targetCanvas.transform.InverseTransformVector(bounds.size);

            foreach (CharacterController unit in playerMultiplayer.GetUnits())
            {
                Vector3 screenPos = Camera.main.WorldToScreenPoint(unit.gameObject.transform.position);
                screenPos.z = 0;

                if (bounds.Contains(screenPos))
                    unit.Selected = true;
            }
        }
    }

    #endregion

    #region BUTTONS CALL

    public void SetSelectBoxActivateButton()
    {
        SelectBaxActivation();
    }

    #endregion
}

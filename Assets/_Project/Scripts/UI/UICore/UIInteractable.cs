using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using UnityEngine.EventSystems;

public class UIInteractable : MonoBehaviour, IPointerDownHandler, IPointerUpHandler
{
    private Image image;
    private Text text;
    private Button button;
    private Slider slider;

    private bool pressing;
    private bool Interactable = true;
    
    public void SetInteractable(bool interact)
    {
        Interactable = interact;
    }

    public GameObject disabledImage;
    private GameObject lockedImage;
    
    [SerializeField] private GameObject price;
    [SerializeField] private Text priceText;
    [SerializeField] private GameObject freeText;

    internal float input;

    [SerializeField] private UIInteractID uIInteractID;
    [SerializeField] private int powerIndex;
    
    public UIInteractID GetUIInteractId() => uIInteractID;

    // Start is called before the first frame update
    void Awake()
    {
        SetSetings();
    }

    private void SetSetings()
    {
        image = GetComponent<Image>();
        text = GetComponent<Text>();
        button = GetComponent<Button>();
        slider = GetComponent<Slider>();
    }

    private void OnEnable()
    {
        UIManager.Instance.uIInteractableController.DICinterabtableActiveAct += EventInteractable;
    }
    
    public void ShowPrice()
    {
        price.SetActive(true);
        freeText.SetActive(false);
    }
    
    public void HidePrice()
    {
        price.SetActive(false);
        freeText.SetActive(true);
    }

    private void Update()
    {
        if (slider)
        {
            if (pressing)
                input = slider.value;
            else
                input = 0f;
        }
    }

    public void OnPointerDown(PointerEventData eventData)
    {
        OnPress(true);
    }

    public void OnPointerUp(PointerEventData eventData)
    {
        OnPress(false);
    }

    void OnPress(bool isPressed)
    {
        if (!Interactable)
            return;

        if (isPressed)
            pressing = true;
        else
            pressing = false;

        if (pressing)
            UIManager.Instance.uIInteractableController.ExecuteInteract(this.uIInteractID);
    }
    
    public void ChangeUIActivation(bool enable)
    {
        Debug.Log("UI enable : " + enable);
        if (enable)
        {
            disabledImage.SetActive(false);
            Interactable = true;
        }
        else
        {
            disabledImage.SetActive(true);
            Interactable = false;
        }
    }

    private void OnDisable()
    {
        UIManager.Instance.uIInteractableController.DICinterabtableActiveAct -= EventInteractable;

        pressing = false;
    }

    #region EVENTS

    public void EventInteractable(bool active)
    {
        Interactable = active;
    }

    #endregion
}

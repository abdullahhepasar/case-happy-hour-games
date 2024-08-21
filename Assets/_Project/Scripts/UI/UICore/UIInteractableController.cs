using UnityEngine;

public class UIInteractableController : MonoBehaviour
{
    public delegate void DICinterabtableActive(bool active);
    public DICinterabtableActive DICinterabtableActiveAct;
    
    public delegate void DICAddInteractable(bool active);
    public DICAddInteractable DICAddInteractableAct;

    public bool interabtableActive = true;
   
    public void GetInput(UIInteractable uIInteractable)
    {
        if (uIInteractable == null)
            return;
    }

    public void ExecuteInteract(UIInteractID uIInteractID)
    {
        DebugLog("UIInteractableController->ExecuteInteract->" + uIInteractID, Color.green);

        VibrationsController.Instance.SetVibration_Soft();

        switch (uIInteractID)
        {
            case UIInteractID.None:
                DebugLog("UIInteractableController->SetInteract->UIInteractID Not Definition", Color.red);
                break;
            case UIInteractID.ID1:
                DebugLog("UIInteractableController->SetInteract->ID 1", Color.green);

                break;
            case UIInteractID.ID2:
                DebugLog("UIInteractableController->SetInteract->ID 2", Color.green);


                break;
            case UIInteractID.ID3:
                DebugLog("UIInteractableController->SetInteract->ID 3", Color.green);

                break;
            case UIInteractID.ID4:
                DebugLog("UIInteractableController->SetInteract->ID 4", Color.green);

                break;
            case UIInteractID.ID5:
                DebugLog("UIInteractableController->SetInteract->ID 5", Color.green);

                break;

            case UIInteractID.ID6:
                DebugLog("UIInteractableController->SetInteract->ID 6", Color.green);

                break;
            default:
                break;
        }
    }

    #region DELEGATES

    public void EventUIActivity(bool active)
    {
        this.interabtableActive = active;

        if (DICinterabtableActiveAct != null)
            DICinterabtableActiveAct(interabtableActive);
    }

    #endregion

    #region TOOLS

    public void DebugLog(string text, Color color)
    {
        Debug.Log($"<color=#{ColorUtility.ToHtmlStringRGB(color)}>{text}</color>");
    }

    #endregion
}

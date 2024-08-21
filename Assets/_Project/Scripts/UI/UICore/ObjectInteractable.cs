using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Collider))]
public class ObjectInteractable : MonoBehaviour
{
    private bool pressing;
    private bool Interactable = true;

    [SerializeField] private UIInteractID uIInteractID;

    public bool PressTypeMouseDown = true;

    private void OnEnable()
    {
        if (UIManager.Instance)
            UIManager.Instance.uIInteractableController.DICinterabtableActiveAct += EventInteractable;
    }

    private void OnMouseEnter()
    {
        pressing = true;
    }

    private void OnMouseExit()
    {
        pressing = false;
    }

    void OnMouseDown()
    {
        if (PressTypeMouseDown)
            OnPress(pressing);
    }

    private void OnMouseUp()
    {
        if (!PressTypeMouseDown)
            OnPress(pressing);
    }

    void OnPress(bool isPressed)
    {
        if (!Interactable)
            return;

        if (pressing)
            UIManager.Instance.uIInteractableController.ExecuteInteract(this.uIInteractID);
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

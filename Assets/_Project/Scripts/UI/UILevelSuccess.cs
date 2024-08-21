using UnityEngine;

public class UILevelSuccess : MonoBehaviour, IUIDependencies
{
    public void OnEnable()
    {
        UIUpdate();
    }

    private void Start()
    {
        UIManager.Instance.UIPopupDestroyPrefabTransform();
    }

    public void UIUpdate()
    {
        SetLanguage();
    }

    public void SetLanguage()
    {
        
    }

    public void UIChange(int index)
    {
        UIManager.Instance.UICreatePrefabs(index);
    }

    #region SET BUTTONS

    public void SetNextLevel()
    {        
        //GameManager.Instance.NextLevelForUI(false, true);
    }

    public void OnNextLevelButtonPressed()
    {
        UIChange((int)UIPrefabNames.GamePlay);
        SetNextLevel();
    }

    public void SetMenuButton()
    {
        UIChange((int)UIPrefabNames.MainMenu);
    }

    #endregion


    public void OnDisable()
    {
       
    }
}

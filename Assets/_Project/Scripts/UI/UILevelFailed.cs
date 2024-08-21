using DG.Tweening;
using UnityEngine;

public class UILevelFailed : MonoBehaviour, IUIDependencies
{
    public Transform SmileBG;

    public GameObject RetrytButton;

    private bool retryPressed = false;

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

    public void SetTryAgainLevel()
    {
        //GameManager.Instance.NextLevelForUI(true, true);
    }

    public void OnRetryButtonPressed()
    {
        if (!retryPressed)
        {
            retryPressed = true;
            UIChange((int)UIPrefabNames.GamePlay);
            SetTryAgainLevel();
        }
    }

    public void SetMenuButton()
    {
        SetTryAgainLevel();
        UIChange((int)UIPrefabNames.MainMenu);
    }

    #endregion

    #region Animation

    public void StartAnimLoops()
    {
        ImageRotateLeft(SmileBG);
    }

    public void ImageRotateLeft(Transform target, bool infinity = true)
    {
        Vector3 targetRot = new Vector3(0f, 0f, 10f);
        float duration = 0.5f;
        target.DORotate(targetRot, duration).SetEase(Ease.Linear).OnComplete(() =>
        {
            if(infinity)
                ImageRotateRight(target, infinity);
        });
    }

    public void ImageRotateRight(Transform target, bool infinity = true)
    {
        Vector3 targetRot = new Vector3(0f, 0f, -10f);
        float duration = 0.5f;
        target.DORotate(targetRot, duration).SetEase(Ease.Linear).OnComplete(() =>
        {
            if (infinity)
                ImageRotateLeft(target, infinity);
        });
    }

    #endregion

    public void OnDisable()
    {
        
    }
}

using UnityEngine;
using UnityEngine.UI;

public class UIPopupSettings : MonoBehaviour, IUIDependencies
{
    public Image SoundIcon;
    public Image VibrationIcon;

    public Sprite SoundOn;
    public Sprite SoundOff;

    public Sprite VibrationOn;
    public Sprite VibrationOff;

    public void OnEnable()
    {
        UIUpdate();
    }

    public void UIUpdate()
    {
        SetLanguage();

        CheckSprites();
    }

    public void SetLanguage()
    {
        
    }

    public void CheckSprites()
    {
        SoundIcon.sprite = (SoundManager.Instance.GetSoundForCheck()) ? SoundOn : SoundOff;

        VibrationIcon.sprite = (VibrationsController.Instance.GetVibrateActive()) ? VibrationOn : VibrationOff;
    }

    public void UIChange(int index)
    {
        UIManager.Instance.UICreatePrefabs(index);
    }

    #region SET BUTTONS

    public void SetSoundButton()
    {
        if (SoundManager.Instance.GetSoundForCheck())
        {
            //Deactive
            SoundManager.Instance.SetSound(-1);
        }
        else
        {
            //Active
            SoundManager.Instance.SetSound(1);
        }

        CheckSprites();
    }

    public void SetVibrateButton()
    {
        if (VibrationsController.Instance.GetVibrateActive())
        {
            VibrationsController.Instance.SetVibrateSettings(VibrationsController.VibrateStates.Deactive);
        }
        else
        {
            VibrationsController.Instance.SetVibrateSettings(VibrationsController.VibrateStates.Active);
        }

        CheckSprites();
    }

    public void SetBackButton()
    {
        StartCoroutine(UIManager.Instance.UIPopupDestroyPrefabTransformAnimation());
    }

    #endregion

    public void OnDisable()
    {
        
    }
}

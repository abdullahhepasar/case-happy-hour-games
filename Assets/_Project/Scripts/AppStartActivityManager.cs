using UnityEngine;
using System.Collections.Generic;
using System.Collections;

public class AppStartActivityManager : MonoBehaviour
{
    #region public Fields

    public static AppStartActivityManager Instance;

    public delegate void DInitializedApp();
    public DInitializedApp DInitializedAppAct;

    public List<GameObject> MANAGERS = new List<GameObject>();

    #endregion

    #region MonoBehaviour CallBacks

    private void Awake()
    {
        //Check if instance already exists
        if (Instance == null)
        {
            //if not, set instance to this
            Instance = this;
        }
    }

    #endregion

    #region Private Methods

    private void SetAppSettings()
    {
        Application.targetFrameRate = 60;
        Input.multiTouchEnabled = false;
    }

    private void Start()
    {
        for (int i = 0; i < MANAGERS.Count; i++)
        {
            Instantiate(MANAGERS[i]);
        }

        Debug.Log("AppStartActivityManager Complete");

        FirstRunApp();
    }

    private void FirstRunApp()
    {
        SetAppSettings();

        if (!PlayerPrefs.HasKey(ConfigDefaults.FirstRunApp))
        {
            PlayerPrefs.SetString(ConfigDefaults.FirstRunApp, "active");

            //Settings
            QualityManager.Instance.SetFirstSettings();

            //Sound/Music
            SoundManager.Instance.SetFirstSettings();

            //Vibrate
            VibrationsController.Instance.SetVibrateSettings(VibrationsController.VibrateStates.Active);

            //Level
            LevelManager.Instance.SetFirstSettings();
        }

        //Init App
        StartCoroutine(LoadApp());
    }

    private IEnumerator LoadApp()
    {
        UILoading.Instance.SetTargetValue(0f);

        yield return StartCoroutine(LoadManagersProcess());

        UILoading.Instance.SetTargetValue(1f);

        UIStart();

        if (DInitializedAppAct != null)
            DInitializedAppAct();

        yield return null;
    }

    private IEnumerator LoadManagersProcess()
    {
        yield return StartCoroutine(LanguageManager.Instance.LoadDatas());

        yield return StartCoroutine(UILoading.Instance.LoadDatas());

        yield return StartCoroutine(AppValueController.Instance.LoadDatas());

        yield return StartCoroutine(LevelManager.Instance.LoadDatas());

        yield return StartCoroutine(VFXManager.Instance.LoadDatas());

        yield return StartCoroutine(SoundManager.Instance.LoadDatas());

        yield return StartCoroutine(VibrationsController.Instance.LoadDatas());

        yield return StartCoroutine(GPUInstancerController.Instance.LoadDatas());

        yield return StartCoroutine(PlayfabManager.Instance.LoadDatas());

        yield return StartCoroutine(UIManager.Instance.LoadDatas());
    }

    private void UIStart()
    {
        UIManager.Instance.UICreatePrefabs((int)UIPrefabNames.MainMenu);

        StartCoroutine(UILoading.Instance.LoadingComplete());
    }

    #endregion
}

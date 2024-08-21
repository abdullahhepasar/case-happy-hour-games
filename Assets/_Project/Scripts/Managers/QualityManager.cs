using System.Collections;
using UnityEngine;

public class QualityManager : MonoBehaviour
{
    public static QualityManager Instance;

    public delegate void DelegateQualityDetect();
    public DelegateQualityDetect delegateQualityDetect;

    #region Veriables_AutoAdaptQuality
    private bool AutoAdaptQualityActive = false;
    /// <summary>
    /// The number of data points to calculate the average FPS over.
    /// </summary>
    int numberOfDataPoints;
    /// <summary>
    /// The current average fps.
    /// </summary>
    public float currentAverageFps;
    /// <summary>
    /// The time interval in which the class checks for the framerate and adapts quality accordingly.
    /// </summary>
    public float TimeIntervalToAdaptQualitySettings = 10f;
    /// <summary>
    /// The lower FPS threshold. Decrease quality when FPS falls below this.
    /// </summary>
    public float LowerFPSThreshold = 30f;
    /// <summary>
    /// The upper FPS threshold. Increase quality when FPS is above this.
    /// </summary>
    public float UpperFPSThreshold = 50f;
    /// <summary>
    /// The stability of the current quality setting. Below 0 if changes have been
    /// made, otherwise positive.
    /// </summary>
    int stability;
    /// <summary>
    /// Tracks whether quality was improved or worsened.
    /// </summary>
    bool lastMovementWasDown;
    /// <summary>
    /// Counter that keeps track when the script can't decide between lowering or increasing quality.
    /// </summary>
    int flickering;
    #endregion

    [Header("Min Width: Below 1920 and RAM: Below 4500 phones")]
    public bool veryLowDeviceActive = false;
    public int veryLowDeviceWidthMinLimit = 1920;
    public int veryLowDeviceRAMMinLimit = 4500;

    [Header("SETTINGS")]
    public string QualityIndexText = "QualityIndex";
    public string ResolutionIndexText = "ResolutionIndex";
    public string FPSIndexText = "FPSIndex";
    public string RenderDistanceText = "RenderDistance";

    int QualityIndex = 0;
    int ResolutionIndex = 0;
    int FPSIndex = 0;
    int RenderDistance = 0;

    [HideInInspector] public int ScreenWidth, ScreenHeight;

    void Awake()
    {
        //Check if instance already exists
        if (Instance == null)
        {
            //if not, set instance to this
            Instance = this;
        }
        //If instance already exists and it's not this:
        else if (Instance != this)
        {
            Destroy(gameObject);
        }

        //Sets this to not be destroyed when reloading scene
        DontDestroyOnLoad(gameObject);
    }

    private void Start()
    {
        Application.lowMemory += OnLowMemory;

        SetUpdateSettings();

        //Initialize if active AutoAdaptQuality
        if (AutoAdaptQualityActive)
            StartAdaptQuality();
    }

    private void OnLowMemory()
    {
        veryLowDeviceActive = true;
        // release all cached textures
        Resources.UnloadUnusedAssets();
    }

    void Update()
    {
        UpdateCumulativeAverageFPS(1 / Time.deltaTime);
    }

    /// <summary>
    /// Updates the cumulative average FPS.
    /// </summary>
    /// <param name="newFPS">New FPS.</param>
    float UpdateCumulativeAverageFPS(float newFPS)
    {
        ++numberOfDataPoints;
        currentAverageFps += (newFPS - currentAverageFps) / numberOfDataPoints;

        return currentAverageFps;
    }

    #region Auto Adapt Quality

    void StartAdaptQuality()
    {
        StartCoroutine(AdaptQuality());
    }

    /// <summary>
    /// Sets the quality accordingly to the current thresholds.
    /// </summary>
    IEnumerator AdaptQuality()
    {
        while (true)
        {
            yield return new WaitForSeconds(TimeIntervalToAdaptQualitySettings);

            if (Debug.isDebugBuild)
            {
                //Debug.Log("Current Average Framerate is: " + currentAverageFps);
            }

            // Decrease level if framerate too low
            if (currentAverageFps < LowerFPSThreshold)
            {
                QualitySettings.DecreaseLevel();
                --stability;
                if (!lastMovementWasDown)
                {
                    ++flickering;
                }
                lastMovementWasDown = true;
                if (Debug.isDebugBuild)
                {
                    Debug.Log("Reducing Quality Level, now " + QualitySettings.names[QualitySettings.GetQualityLevel()]);
                }

                // In case we are "flickering" (switching between two quality settings),
                // stop it, using the lower quality level.
                if (flickering > 1)
                {
                    if (Debug.isDebugBuild)
                    {
                        Debug.Log(string.Format(
                          "Flickering detected, staying at {0} to stabilise.",
                          QualitySettings.names[QualitySettings.GetQualityLevel()]));
                    }

                    QualitySettings.SetQualityLevel(0);
                    //Destroy(this);
                }

            }
            else
              // Increase level if framerate is too high
              if (currentAverageFps > UpperFPSThreshold)
            {
                //Maximum Medium a çeksin otomatik
                if (QualitySettings.GetQualityLevel() < 1)
                    QualitySettings.IncreaseLevel();
                --stability;
                if (lastMovementWasDown)
                {
                    ++flickering;
                }
                lastMovementWasDown = false;
                if (Debug.isDebugBuild)
                {
                    //Debug.Log("Increasing Quality Level, now " + QualitySettings.names[QualitySettings.GetQualityLevel()]);
                }
            }
            else
            {
                ++stability;
            }

            // If we had a framerate in the range between 25 and 60 frames three times
            // in a row, we consider this pretty stable and remove this script.
            if (stability > 3)
            {
                if (Debug.isDebugBuild)
                {
                    Debug.Log("Framerate is stable now, removing automatic adaptation.");
                }
                //Destroy(this);
            }

            QualityChange();

            // Reset moving average
            numberOfDataPoints = 0;
            currentAverageFps = 0;
        }
    }

    #endregion


    public void QualityChange()
    {
        QualitySettings.SetQualityLevel(GetQuality());

        float ScaleScreen = (float)ScreenHeight / (float)ScreenWidth;

        float middleScale = 1920f * ScaleScreen;
        float lowScale = 1280f * ScaleScreen;
        float verylowScale = 960f * ScaleScreen;

#if UNITY_ANDROID
        /*switch (GetResolution())
        {
            case 0://LOW
                Screen.SetResolution(1280, (int)lowScale, true);
                break;
            case 1://MEDIUM
                   //Cok Dusuk Modeller
                if (Screen.width <= 1920 || SystemInfo.systemMemorySize <= 3000)
                    Screen.SetResolution(1280, (int)lowScale, true);
                else
                    Screen.SetResolution(1920, (int)middleScale, true);
                break;
            case 2://HIGH
                   //Screen.SetResolution(AppValueController.Instance.ScreenWidth, AppValueController.Instance.ScreenHeight, true);

                if (Screen.width <= 1920 || SystemInfo.systemMemorySize <= 3000)
                    Screen.SetResolution(1280, (int)lowScale, true);
                else
                    Screen.SetResolution(1920, (int)middleScale, true);
                break;
            default:
                //Screen.SetResolution(AppValueController.Instance.ScreenWidth, AppValueController.Instance.ScreenHeight, true);

                if (Screen.width <= 1920 || SystemInfo.systemMemorySize <= 3000)
                    Screen.SetResolution(1280, (int)lowScale, true);
                else
                    Screen.SetResolution(1920, (int)middleScale, true);
                break;
        }

        //Low Mobiles
        if (Screen.width <= veryLowDeviceWidthMinLimit && SystemInfo.systemMemorySize <= veryLowDeviceRAMMinLimit)
        {
            veryLowDeviceActive = true;
            //960:540 -> 1 GB ye yaklaşık telefonlar her türlü buradan çıkmasınlar
            if (Screen.width < 1000)
                Screen.SetResolution(960, (int)verylowScale, true);
            else if (Screen.width <= 1920)
            {
                Screen.SetResolution(1280, (int)lowScale, true);
            }
        }*/
#endif

        if (Application.platform == RuntimePlatform.WindowsEditor)
            Application.targetFrameRate = 300;
        else
        {
            switch (GetFPSIndex())
            {
                case 0:
                    Application.targetFrameRate = 30;
                    break;
                case 1:
                    Application.targetFrameRate = 60;
                    break;
                case 2:
                    Application.targetFrameRate = 120;
                    break;
                default:
                    break;
            }
        }

        if (delegateQualityDetect != null)
            delegateQualityDetect();
    }

#region SETTINGS

    public void SetFirstSettings()
    {
        QualityIndex = 2;
        ResolutionIndex = 2;
        FPSIndex = 2;
        RenderDistance = 150;

        /*if (Screen.width < 1920 || SystemInfo.systemMemorySize < 3000)
        {
            //Low Devices
            QualityIndex = 1;   //Very Low
            ResolutionIndex = 1;
            FPSIndex = 1;
            RenderDistance = 100;
        }*/

        SetQuality(0);
        SetResolution(0);
        SetFPS(0);
        SetRenderDistance(0);

        PlayerPrefs.SetInt(ConfigDefaults.ScreenWidthText, Screen.width);
        PlayerPrefs.SetInt(ConfigDefaults.ScreenHeightText, Screen.height);
    }

    public void GetSettings()
    {
        ScreenWidth = PlayerPrefs.GetInt(ConfigDefaults.ScreenWidthText);
        ScreenHeight = PlayerPrefs.GetInt(ConfigDefaults.ScreenHeightText);
    }

    public void SetUpdateSettings()
    {
        //START -> App starts on every boot

        QualityIndex = GetQuality();
        ResolutionIndex = GetResolution();
        FPSIndex = GetFPSIndex();
        RenderDistance = GetRenderDistance();

        QualityChange();

        GetSettings();
    }

    /// <summary>
    /// Increase - Decrease Quality Index
    /// </summary>
    /// <param name="index"></param>
    public void SetQuality(int index)
    {
        QualityIndex += index;

        if (QualityIndex > 2)
        {
            QualityIndex = 2;
        }
        else if (QualityIndex < 0)
        {
            QualityIndex = 0;
        }

        PlayerPrefs.SetInt(QualityIndexText, QualityIndex);

        QualityChange();
    }

    public int GetQuality()
    {
        return PlayerPrefs.GetInt(QualityIndexText);
    }

    public void SetResolution(int index)
    {
        ResolutionIndex += index;

        if (ResolutionIndex > 2)
        {
            ResolutionIndex = 2;
        }
        else if (ResolutionIndex < 0)
        {
            ResolutionIndex = 0;
        }

        PlayerPrefs.SetInt(ResolutionIndexText, ResolutionIndex);

        QualityChange();
    }

    public int GetResolution()
    {
        return PlayerPrefs.GetInt(ResolutionIndexText);
    }

    public void SetFPS(int index)
    {
        FPSIndex += index;

        if (FPSIndex > 2)
        {
            FPSIndex = 2;
        }
        else if (FPSIndex < 0)
        {
            FPSIndex = 0;
        }

        PlayerPrefs.SetInt(FPSIndexText, FPSIndex);

        QualityChange();
    }

    private int GetFPSIndex()
    {
        return PlayerPrefs.GetInt(ResolutionIndexText);
    }

    /// <summary>
    /// Call from UI
    /// </summary>
    /// <returns></returns>
    public int GetFPS()
    {
        int FPS = 60;

        switch (PlayerPrefs.GetInt(FPSIndexText))
        {
            case 0:
                FPS = 30;
                break;
            case 1:
                FPS = 60;
                break;
            case 2:
                FPS = 120;
                break;
            default:
                break;
        }

        return FPS;
    }

    public void SetRenderDistance(int index)
    {
        RenderDistance += index;

        if (RenderDistance > 300)
        {
            RenderDistance = 300;
        }
        else if (RenderDistance < 50)
        {
            RenderDistance = 50;
        }

        PlayerPrefs.SetInt(RenderDistanceText, RenderDistance);

        QualityChange();
    }

    public int GetRenderDistance()
    {
        return PlayerPrefs.GetInt(RenderDistanceText);
    }

    public void SetLoadBias(float bias)
    {
        QualitySettings.lodBias = bias;
    }

    public float GetLoadBias()
    {
        return QualitySettings.lodBias;
    }

#endregion
}

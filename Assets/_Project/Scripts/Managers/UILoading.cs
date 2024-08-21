using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class UILoading : MonoBehaviour
{
    public static UILoading Instance;

    [Header("Text Language")]
    [SerializeField] private Text LoadingText;

    [Header("Slider Loading Component")]
    [SerializeField] private Slider LoadingSlider;

    [Header("Image Loading Component")]
    [SerializeField] private Image LoadingBar;

    [Header("Info Sliders Animation Speeds")]
    [SerializeField] private float sliderSmoothSpeed = 1.5f;

    [Header("Slider Component")]
    [SerializeField] private GameObject UILoadingContainer;

    private float _targetSliderValue;
    public float TargetSliderValue
    {
        get { return _targetSliderValue; }
        set { _targetSliderValue = value; }
    }

    [SerializeField] private float waitLoadingScreenForAnim = 1f;

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

    public IEnumerator LoadDatas()
    {
        LoadingActive();
        UIUpdate();

        yield return null;
    }

    public void UIUpdate()
    {
        SetLanguage();

        //LoadingSlider.gameObject.SetActive(false);
    }

    private void SetLanguage()
    {
        LoadingText.text = LanguageManager.Instance.CLS.LoadingText;
    }

    public void SetTargetValue(float value)
    {
        TargetSliderValue = value;
    }

    // Update is called once per frame
    void Update()
    {
        LoadingSlider.value = Mathf.Lerp(LoadingSlider.value, TargetSliderValue, Time.deltaTime * sliderSmoothSpeed);
        LoadingBar.fillAmount = Mathf.Lerp(LoadingBar.fillAmount, TargetSliderValue, Time.deltaTime * sliderSmoothSpeed);
    }

    public void LoadingActive()
    {
        UILoadingContainer.SetActive(true);
    }

    public IEnumerator LoadingComplete()
    {
        yield return new WaitForSeconds(waitLoadingScreenForAnim);

        ResetLoading();
    }

    public void ResetLoading()
    {
        UILoadingContainer.SetActive(false);
        LoadingSlider.value = 0f;
        LoadingBar.fillAmount = 0f;
        TargetSliderValue = 0f;
    }
}

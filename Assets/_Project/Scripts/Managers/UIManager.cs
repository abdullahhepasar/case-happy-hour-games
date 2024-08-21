using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using UIAnimatorCore;
using System.Collections.Generic;
using System.Linq;

public class UIManager : MonoBehaviour
{
    public static UIManager Instance;

    [HideInInspector] public UIInteractableController uIInteractableController;

    public delegate void DL_UIManager_UICreateComplete();
    public DL_UIManager_UICreateComplete DL_UIManager_UICreateCompleted;

    public List<UIScriptable> UIs = new List<UIScriptable>();

    [Header("Static Canvas")]
    [SerializeField] private Transform StaticCanvas_UIPrefabCreateTransform;
    [SerializeField] private Transform StaticCanvas_UIPopupPrefabCreateTransform;
    [SerializeField] private Transform StaticCanvas_UITempTransform;

    [Header("Dynamic Canvas")]
    [SerializeField] private Transform DynamicCanvas_UIPrefabCreateTransform;
    [SerializeField] private Transform DynamicCanvas_UIPopupPrefabCreateTransform;
    [SerializeField] private Transform DynamicCanvas_UITempTransform;

    [Header("Canvas ların Scale Oranları")]
    [SerializeField] private GameObject canvasScaler_static;
    public Canvas CanvasStatic { get { return canvasScaler_static.GetComponent<Canvas>(); } }

    [SerializeField] private GameObject canvasScaler_dynamic;

    bool UIAnimationActive = false;
    bool UIPopupAnimationActive = false;

    [Header("Resources Folder Name")]
    private string ResourceFolderName_UI = "UI/UIPrefabs/";
    private string ResourceFolderName_ScriptableUI = "UI/Scriptable/";

    private GameObject CurrentUI;
    private GameObject CurrentPopupUI;

    private void Awake()
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
        uIInteractableController = this.gameObject.AddComponent<UIInteractableController>();

        CheckSafeArea();
        CheckTabletMode();
        //SetCameraResponsiveVertivalFOV(Camera.main, 36f);
        SetCameraResponsiveOrthographicSize(Camera.main);

        yield return LoadUIDatas();

        SetRenderCameraActive(false);

        yield return null;
    }

    private IEnumerator LoadUIDatas()
    {
        UIs = new List<UIScriptable>();

        UIScriptable[] temps = Resources.LoadAll<UIScriptable>(ResourceFolderName_ScriptableUI) as UIScriptable[];
        temps = temps.OrderBy(e => e.UIPrefabID).ToArray();

        for (int i = 0; i < temps.Length; i++)
        {
            UIs.Add(temps[i]);
        }

        yield return null;
    }

    void CheckSafeArea()
    {

    }

    public void CheckTabletMode()
    {
        float SceleScreenWitdhHeight = (float)QualityManager.Instance.ScreenWidth / (float)QualityManager.Instance.ScreenHeight;

        if (SceleScreenWitdhHeight < 1.68f)
        {
            canvasScaler_static.GetComponent<CanvasScaler>().matchWidthOrHeight = 0f;
            canvasScaler_dynamic.GetComponent<CanvasScaler>().matchWidthOrHeight = 0f;
        }
        else
        {
            canvasScaler_static.GetComponent<CanvasScaler>().matchWidthOrHeight = 1f;
            canvasScaler_dynamic.GetComponent<CanvasScaler>().matchWidthOrHeight = 1f;
        }
    }

    /// <summary>
    /// Camera Orthographic setting width and height are 
    /// adjusted equally for different devices
    /// </summary>
    private void SetCameraResponsiveOrthographicSize(Camera camera)
    {
        return;

        float horizontalResolution = 1920;
        float currentAspect = (float)Screen.width / (float)Screen.height;
        camera.orthographicSize = horizontalResolution / currentAspect / 290;
        camera.fieldOfView = horizontalResolution / currentAspect / 340;
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="camera">Target Camera</param>
    /// <param name="hFOVInDeg">Usually 36f => FOV: 60</param>
    /// <param name="aspectRatio">camera.aspect</param>
    private void SetCameraResponsiveVertivalFOV(Camera camera, float hFOVInDeg = 36f)
    {
        //return;

        float hFOVInRads = hFOVInDeg * Mathf.Deg2Rad;
        float currentAspect = (float)Screen.width / (float)Screen.height;
        float vFOVInRads = 2 * Mathf.Atan(Mathf.Tan(hFOVInRads / 2) / currentAspect);
        float vFOV = vFOVInRads * Mathf.Rad2Deg;
        camera.fieldOfView = vFOV;
    }

    public void SetCanvasScaler(float scale)
    {
        float SceleScreenWitdhHeight = (float)QualityManager.Instance.ScreenWidth / (float)QualityManager.Instance.ScreenHeight;

        if (SceleScreenWitdhHeight >= 2f)
        {
            canvasScaler_static.GetComponent<CanvasScaler>().matchWidthOrHeight = scale;
            canvasScaler_dynamic.GetComponent<CanvasScaler>().matchWidthOrHeight = scale;
        }
    }

    public void UICreatePrefabs(int _indexPrefab)
    {
        if (!UIAnimationActive)
            StartCoroutine(UICreatePrefabsAnimation(_indexPrefab));

        UIAnimationActive = true;
    }

    IEnumerator UICreatePrefabsAnimation(int _index)
    {
        Resources.UnloadUnusedAssets();

        //Sound
        // SoundManager.Instance.CreateSound(SoundID.Click, false);

        GameObject tempUI_static = null;
        GameObject tempUI_dynamic = null;

        UIPrefabNames uIPrefabNames = (UIPrefabNames)_index;

        for (int i = 0; i < UIs.Count; i++)
        {
            if (uIPrefabNames == UIs[i].UIPrefabID)
            {
                //Canvas Layer Set
                if (UIs[i].Dynamic_LayerSortOrder)
                    canvasScaler_dynamic.GetComponent<Canvas>().sortingOrder = canvasScaler_static.GetComponent<Canvas>().sortingOrder + 1;
                else
                    canvasScaler_dynamic.GetComponent<Canvas>().sortingOrder = canvasScaler_static.GetComponent<Canvas>().sortingOrder;

                //Resourece Load
                ResourceRequest resource = null;
                resource = Resources.LoadAsync(ResourceFolderName_UI + UIs[i].Static_UIResourcesName, typeof(GameObject));
                while (!resource.isDone)
                {
                    yield return resource;
                }

                tempUI_static = Instantiate(resource.asset, StaticCanvas_UITempTransform) as GameObject;

                tempUI_static.SetActive(false);

                //If it has a Dynamic Object, start it on a different Canvas
                if (UIs[i].DynamicCanvasActive)
                {
                    ResourceRequest resource_dynamic = null;
                    resource_dynamic = Resources.LoadAsync(ResourceFolderName_UI + UIs[i].Dynamic_UIResourcesName, typeof(GameObject));
                    while (!resource_dynamic.isDone)
                    {
                        yield return resource_dynamic;
                    }
                    tempUI_dynamic = Instantiate(resource_dynamic.asset, DynamicCanvas_UITempTransform) as GameObject;
                    tempUI_dynamic.SetActive(false);
                }

                SetCurrentUI(tempUI_static);

                break;
            }
        }

        yield return StartCoroutine(UIDestroyPrefabTransformAnimation());

        //BASLAT - Static ve Dynamic
        foreach (Transform item in StaticCanvas_UIPrefabCreateTransform)
        {
            Destroy(item.gameObject);
        }

        foreach (Transform item in DynamicCanvas_UIPrefabCreateTransform)
        {
            Destroy(item.gameObject);
        }

        if (tempUI_static != null)
        {
            //tempUI_static.transform.parent = StaticCanvas_UIPrefabCreateTransform;
            tempUI_static.transform.SetParent(StaticCanvas_UIPrefabCreateTransform);
            tempUI_static.SetActive(true);
        }

        if (tempUI_dynamic != null)
        {
            //tempUI_dynamic.transform.parent = DynamicCanvas_UIPrefabCreateTransform;
            tempUI_dynamic.transform.SetParent(DynamicCanvas_UIPrefabCreateTransform);
            tempUI_dynamic.SetActive(true);
        }

        UIAnimationActive = false;

        if (DL_UIManager_UICreateCompleted != null)
            DL_UIManager_UICreateCompleted();

        yield return null;
    }

    private IEnumerator UIDestroyPrefabTransformAnimation()
    {
        float timer = 0f;
        //Sound
        // SoundManager.Instance.CreateSound(SoundID.Click, false);

        foreach (Transform item in StaticCanvas_UIPrefabCreateTransform)
        {
            if (item.GetComponent<UIAnimator>())
            {
                item.GetComponent<UIAnimator>().PlayAnimation(AnimSetupType.Outro);

                if (timer < item.GetComponent<UIAnimator>().GetAnimationDuration(AnimSetupType.Outro))
                    timer = item.GetComponent<UIAnimator>().GetAnimationDuration(AnimSetupType.Outro);
            }
        }

        yield return new WaitForSeconds(timer);

        foreach (Transform item in StaticCanvas_UIPrefabCreateTransform)
        {
            Destroy(item.gameObject);
        }

        yield return null;
    }

    public void UIPopupCreatePrefabs(int _indexPrefab)
    {
        if (!UIPopupAnimationActive)
            StartCoroutine(UIPopupCreatePrefabsAnimation(_indexPrefab));

        UIPopupAnimationActive = true;
    }

    IEnumerator UIPopupCreatePrefabsAnimation(int _index)
    {
        Resources.UnloadUnusedAssets();

        //Sound
        // SoundManager.Instance.CreateSound(SoundID.Click, false);

        GameObject tempUI_static = null;
        GameObject tempUI_dynamic = null;

        UIPrefabNames uIPrefabNames = (UIPrefabNames)_index;

        for (int i = 0; i < UIs.Count; i++)
        {
            if (uIPrefabNames == UIs[i].UIPrefabID)
            {
                //Canvas Layer Set -> NOW REMOVED -> CAUSE DYNAMIC CRUSHING OBJECT WHEN SETTING IS ON ON THE GAME SCREEN
                /*if (UIPrefab[i].Dynamic_LayerSortOrder)
                    canvasScaler_dynamic.GetComponent<Canvas>().sortingOrder = canvasScaler_static.GetComponent<Canvas>().sortingOrder + 1;
                else
                    canvasScaler_dynamic.GetComponent<Canvas>().sortingOrder = canvasScaler_static.GetComponent<Canvas>().sortingOrder;*/

                //Resourece Load
                ResourceRequest resource = null;
                resource = Resources.LoadAsync(ResourceFolderName_UI + UIs[i].Static_UIResourcesName, typeof(GameObject));
                while (!resource.isDone)
                {
                    yield return resource;
                }
                tempUI_static = Instantiate(resource.asset, StaticCanvas_UITempTransform) as GameObject;

                tempUI_static.SetActive(false);

                //Dynamic Objesi var ise farklı Canvas da baslat
                if (UIs[i].DynamicCanvasActive)
                {
                    ResourceRequest resource_dynamic = null;
                    resource_dynamic = Resources.LoadAsync(ResourceFolderName_UI + UIs[i].Dynamic_UIResourcesName, typeof(GameObject));
                    while (!resource_dynamic.isDone)
                    {
                        yield return resource_dynamic;
                    }
                    tempUI_dynamic = Instantiate(resource_dynamic.asset, DynamicCanvas_UITempTransform) as GameObject;
                    tempUI_dynamic.SetActive(false);
                }

                break;
            }
        }

        //yield return StartCoroutine(UIPopupDestroyPrefabTransformAnimation());

        //START - Static ve Dynamic
        foreach (Transform item in StaticCanvas_UIPopupPrefabCreateTransform)
        {
            Destroy(item.gameObject);
        }

        foreach (Transform item in DynamicCanvas_UIPopupPrefabCreateTransform)
        {
            Destroy(item.gameObject);
        }

        if (tempUI_static != null)
        {
            //tempUI_static.transform.parent = StaticCanvas_UIPopupPrefabCreateTransform;
            tempUI_static.transform.SetParent(StaticCanvas_UIPopupPrefabCreateTransform);
            tempUI_static.SetActive(true);

            SetCurrentPopupUI(tempUI_static);
        }

        if (tempUI_dynamic != null)
        {
            //tempUI_dynamic.transform.parent = DynamicCanvas_UIPopupPrefabCreateTransform;
            tempUI_dynamic.transform.SetParent(DynamicCanvas_UIPopupPrefabCreateTransform);
            tempUI_dynamic.SetActive(true);
        }

        UIPopupAnimationActive = false;

        if (DL_UIManager_UICreateCompleted != null)
            DL_UIManager_UICreateCompleted();

        yield return null;
    }

    public IEnumerator UIPopupDestroyPrefabTransformAnimation()
    {
        float timer = 0f;
        //Sound
        // SoundManager.Instance.CreateSound(SoundID.Click, false);

        foreach (Transform item in StaticCanvas_UIPopupPrefabCreateTransform)
        {
            if (item.GetComponent<UIAnimator>())
            {
                item.GetComponent<UIAnimator>().PlayAnimation(AnimSetupType.Outro);

                if (timer < item.GetComponent<UIAnimator>().GetAnimationDuration(AnimSetupType.Outro))
                    timer = item.GetComponent<UIAnimator>().GetAnimationDuration(AnimSetupType.Outro);
            }
        }

        yield return new WaitForSeconds(timer);

        foreach (Transform item in StaticCanvas_UIPopupPrefabCreateTransform)
        {
            Destroy(item.gameObject);
        }

        yield return null;
    }

    public void UIPopupDestroyPrefabTransform()
    {
        //Sound
        // SoundManager.Instance.CreateSound(SoundID.Click, false);

        foreach (Transform item in StaticCanvas_UIPopupPrefabCreateTransform)
        {
            Destroy(item.gameObject);
        }
    }

    private void SetCurrentUI(GameObject currentUI)
    {
        if (currentUI.GetComponent<IUIDependencies>() != null)
            CurrentUI = currentUI;
    }

    private void SetCurrentPopupUI(GameObject currentUI)
    {
        if (currentUI.GetComponent<IUIDependencies>() != null)
            CurrentPopupUI = currentUI;
    }

    /// <summary>
    /// Get Active UI Gameobject
    /// </summary>
    /// <returns></returns>
    public GameObject GetCurrentUI() => CurrentUI;

    public GameObject GetCurrentPopupUI() => CurrentPopupUI;

    #region FOR World Gameobjects

    [SerializeField] private Camera RenderCameraForGameobject;
    [SerializeField] private Transform RenderCameraTargetPosition;
    [SerializeField] private RenderTexture RenderTextureForGameobject;

    public void SetRenderGameobjectSettings(GameObject target, Vector3 targetPos, Vector3 targetRot, string targetLayer = "RenderObject")
    {
        //RenderCameraTargetPosition.position = GameManager.Instance.PlateRenderCameraPivot.position;

        RenderCameraForGameobject.gameObject.transform.position = RenderCameraTargetPosition.position;
        //RenderCameraForGameobject.gameObject.transform.eulerAngles = GameManager.Instance.PlateRenderCameraPivot.eulerAngles;

        //Only Target Layer
        LayerMask targetLayerIndex = 1 << LayerMask.NameToLayer(targetLayer);
        RenderCameraForGameobject.cullingMask = targetLayerIndex;

        RenderCameraForGameobject.targetTexture = RenderTextureForGameobject;

        Transform[] childs = target.GetComponentsInChildren<Transform>();
        foreach (Transform child in childs)
        {
            child.gameObject.layer = LayerMask.NameToLayer(targetLayer);
        }

        //target.transform.SetParent(RenderCameraTargetPosition);
        //target.transform.localPosition = targetPos;
        //target.transform.eulerAngles = targetRot;

        SetRenderCameraActive(true);
    }

    public void SetRenderCameraActive(bool active) => RenderCameraForGameobject.gameObject.SetActive(active);

    #endregion
}

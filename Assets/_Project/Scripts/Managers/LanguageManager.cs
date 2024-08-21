using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class LanguageManager : MonoBehaviour
{
    public static LanguageManager Instance;

    private string LanguageSettingName = "Language";

    [SerializeField] private LanguageID CurrentLanguage = LanguageID.en;

    public List<LanguageScriptable> Languages = new List<LanguageScriptable>();

    [HideInInspector] public LanguageScriptable CLS;

    private string ResourceFolderName_ScriptableLanguage = "Language/";

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
        yield return StartCoroutine(LoadLanguageDatas());

        GetLanguage();

        //OVERRIDE EN
        SetLanguage(LanguageID.en);

        yield return null;
    }

    private IEnumerator LoadLanguageDatas()
    {
        Languages = new List<LanguageScriptable>();

        LanguageScriptable[] temps = Resources.LoadAll<LanguageScriptable>(ResourceFolderName_ScriptableLanguage) as LanguageScriptable[];
        temps = temps.OrderBy(e => e.languageID).ToArray();

        for (int i = 0; i < temps.Length; i++)
        {
            Languages.Add(temps[i]);
        }

        yield return null;
    }

    private void GetLanguage()
    {
        try
        {
            if (PlayerPrefs.HasKey(LanguageSettingName))
            {
                CurrentLanguage = (LanguageID)Enum.Parse(typeof(LanguageID), PlayerPrefs.GetString(LanguageSettingName));
                return;
            }
        }
        catch (Exception err)
        {
            Debug.LogError("Language Manager -> GetLanguage Error -> " + err.Message);
        }


        if (Application.systemLanguage == SystemLanguage.Turkish)
        {
            PlayerPrefs.SetString(LanguageSettingName, LanguageID.tr.ToString());
            CurrentLanguage = LanguageID.tr;
        }
        else if (Application.systemLanguage == SystemLanguage.Portuguese)
        {
            PlayerPrefs.SetString(LanguageSettingName, LanguageID.pt.ToString());
            CurrentLanguage = LanguageID.pt;
        }
        else if (Application.systemLanguage == SystemLanguage.Spanish)
        {
            PlayerPrefs.SetString(LanguageSettingName, LanguageID.es.ToString());
            CurrentLanguage = LanguageID.es;
        }
        else if (Application.systemLanguage == SystemLanguage.German)
        {
            PlayerPrefs.SetString(LanguageSettingName, LanguageID.de.ToString());
            CurrentLanguage = LanguageID.de;
        }
        else if (Application.systemLanguage == SystemLanguage.Russian)
        {
            PlayerPrefs.SetString(LanguageSettingName, LanguageID.ru.ToString());
            CurrentLanguage = LanguageID.ru;
        }
        else if (Application.systemLanguage == SystemLanguage.French)
        {
            PlayerPrefs.SetString(LanguageSettingName, LanguageID.fr.ToString());
            CurrentLanguage = LanguageID.fr;
        }
        else
        {
            PlayerPrefs.SetString(LanguageSettingName, LanguageID.en.ToString());
            CurrentLanguage = LanguageID.en;
        }

        SetLanguageScriptable(CurrentLanguage);
    }

    private void Save(LanguageID LanguageID)
    {
        //Save
        PlayerPrefs.SetString(LanguageSettingName, LanguageID.ToString());
    }

    private void SetLanguageScriptable(LanguageID LanguageID)
    {
        for (int i = 0; i < Languages.Count; i++)
        {
            if (Languages[i].languageID == CurrentLanguage)
            {
                CLS = Languages[i];
                break;
            }
        }
    }

    #region SETTINGS

    /// <summary>
    /// Language option can be changed from Settings
    /// </summary>
    /// <param name="LanguageID"></param>
    public void SetLanguage(LanguageID LanguageID)
    {
        this.CurrentLanguage = LanguageID;

        //Save
        Save(this.CurrentLanguage);

        SetLanguageScriptable(CurrentLanguage);
    }

    #endregion


}

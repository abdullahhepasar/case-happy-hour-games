using MoreMountains.NiceVibrations;
using UnityEngine;
using System;
using System.Collections.Generic;
using System.Collections;
using System.Linq;

public class VibrationsController : MonoBehaviour
{
    public static VibrationsController Instance;

    public enum VibrateStates
    {
        Active,
        Deactive
    }

    private string VibrateState = "VibrateActive";

    public VibrateStates CurrentVibrateStates = VibrateStates.Active;

    public List<VibrateScriptable> Vibrates = new List<VibrateScriptable>();

    private string ResourceFolderName_ScriptableVibrate = "Vibrate/Scriptable/";

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
        GetVibrateSettings();
        SetVibrateSettings(CurrentVibrateStates);

        yield return LoadVibrateDatas();

        yield return null;
    }

    private IEnumerator LoadVibrateDatas()
    {
        Vibrates = new List<VibrateScriptable>();

        VibrateScriptable[] temps = Resources.LoadAll<VibrateScriptable>(ResourceFolderName_ScriptableVibrate) as VibrateScriptable[];
        temps = temps.OrderBy(e => e.VibrateName).ToArray();

        for (int i = 0; i < temps.Length; i++)
        {
            Vibrates.Add(temps[i]);
        }

        yield return null;
    }

    public void SetVibrateSettings(VibrateStates vibrateStates)
    {
        PlayerPrefs.SetString(VibrateState, vibrateStates.ToString());
        CurrentVibrateStates = vibrateStates;

        SetVibrateActive(CurrentVibrateStates);
    }

    public VibrateStates GetVibrateSettings()
    {
        string vibrateID = PlayerPrefs.GetString(VibrateState);
        VibrateStates vibrateStatesTemp = (VibrateStates)Enum.Parse(typeof(VibrateStates), vibrateID);

        CurrentVibrateStates = vibrateStatesTemp;

        //return ((VibrateStates.Active == vibrateStatesTemp) ? VibrateStates.Active : VibrateStates.Deactive);
        return vibrateStatesTemp;
    }

    public bool GetVibrateActive()
    {
        if (CurrentVibrateStates == VibrateStates.Active)
            return true;

        return false;
    }

    private void SetVibrateActive(VibrateStates vibrateStates)
    {
        MMVibrationManager.SetHapticsActive((vibrateStates == VibrateStates.Active) ? true : false);
    }

    #region SET VIBRATE

    public void SetVibration_Selection()
    {
        if (!GetVibrateActive())
            return;

        MMVibrationManager.Haptic(HapticTypes.Selection, false, true, this);
    }

    public void SetVibration_Success()
    {
        if (!GetVibrateActive())
            return;

        MMVibrationManager.Haptic(HapticTypes.Success, false, true, this);
    }

    public void SetVibration_Warning()
    {
        if (!GetVibrateActive())
            return;

        MMVibrationManager.Haptic(HapticTypes.Warning, false, true, this);
    }

    public void SetVibration_Failure()
    {
        if (!GetVibrateActive())
            return;

        MMVibrationManager.Haptic(HapticTypes.Failure, false, true, this);
    }

    public void SetVibration_Rigid()
    {
        if (!GetVibrateActive())
            return;

        MMVibrationManager.Haptic(HapticTypes.RigidImpact, false, true, this);
    }

    public void SetVibration_Soft()
    {
        MMVibrationManager.Haptic(HapticTypes.SoftImpact, false, true, this);
    }

    public void SetVibration_Light()
    {
        if (!GetVibrateActive())
            return;

        MMVibrationManager.Haptic(HapticTypes.LightImpact, false, true, this);
    }

    public void SetVibration_Medium()
    {
        if (!GetVibrateActive())
            return;

        MMVibrationManager.Haptic(HapticTypes.MediumImpact, false, true, this);
    }

    public void SetVibration_Heavy()
    {
        if (!GetVibrateActive())
            return;

        MMVibrationManager.Haptic(HapticTypes.HeavyImpact, false, true, this);
    }


    /// <summary>
    /// Rumble: Vibrate from slow to accelerating
    /// //NOTE: Not working on some android phones
    /// </summary>
    /// <param name="transientIntensity"></param>
    /// <param name="transientSharpness"></param>
    /// <param name="rumble"></param>
    public void SetVibration_Transient(float transientIntensity, float transientSharpness, bool rumble)
    {
        if (!GetVibrateActive())
            return;

        MMVibrationManager.TransientHaptic(transientIntensity, transientSharpness, rumble, this);
    }

    /// <summary>
    /// Rumble: Vibrate from slow to accelerating
    /// fullIntensityForIOS : It only works on IOS
    /// </summary>
    /// <param name="continuousIntensity"></param>
    /// <param name="continuousSharpness"></param>
    /// <param name="continuousDuration"></param>
    /// <param name="hapticTypes"></param>
    /// <param name="rumble"></param>
    /// <param name="fullIntensityForIOS"></param>
    public void SetVibration_ContinuousHaptics(float continuousIntensity, float continuousSharpness, float continuousDuration, HapticTypes hapticTypes, bool rumble, bool fullIntensityForIOS)
    {
        if (!GetVibrateActive())
            return;

        MMVibrationManager.ContinuousHaptic(continuousIntensity, continuousSharpness, continuousDuration, hapticTypes, this, rumble, -1, fullIntensityForIOS);
    }

    public void SetVibration_AdvancedHapticPattern(VibrateIOSAdvancedID vibrateIOSAdvancedID)
    {
        if (!GetVibrateActive())
            return;

        VibrateScriptable vibrateScriptable = GetVibrateScriptable(vibrateIOSAdvancedID);
        if (vibrateScriptable == null)
            return;

#if Unity_IOS
        MMVibrationManager.AdvancedHapticPattern(vibrateScriptable.AHAPFile.text, null, null, -1, null, null, null, -1, HapticTypes.LightImpact);
#endif
    }

    #endregion

    private VibrateScriptable GetVibrateScriptable(VibrateIOSAdvancedID vibrateIOSAdvancedID)
    {
        for (int i = 0; i < Vibrates.Count; i++)
        {
            if (Vibrates[i].VIosID == vibrateIOSAdvancedID)
                return Vibrates[i];
        }

        return null;
    }
}

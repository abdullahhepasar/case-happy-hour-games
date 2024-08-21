using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

using Random = UnityEngine.Random;

public class SoundManager : MonoBehaviour
{
    public static SoundManager Instance;

    #region veriable_MUSIC

    [SerializeField] private AudioSource MusicSource;

    private string SettingMusicVolumeText = "SettingMusicVolume";

    private int SettingMusicVolumeIndex = 0;

    public List<MusicScriptable> Musics = new List<MusicScriptable>();

    public int MusicIndex = 0;

    IEnumerator SetNextPlayMusic;

    #endregion

    #region veriable_SOUND

    public List<SoundScriptable> Sounds = new List<SoundScriptable>();

    [SerializeField] private GameObject SoundPrefab;

    private string SettingSoundText = "SettingSound";

    private int SettingSoundIndex = 0;  //0: Off | 1: On

    [SerializeField] private ObjectPool soundPool;

    #endregion

    [Space(10f)]
    private string ResourceFolderName_Music = "Sound/Music/";
    private string ResourceFolderName_Sound = "Sound/UI/";
    private string ResourceFolderName_ScriptableSound = "Sound/Scriptable/Sounds/";
    private string ResourceFolderName_ScriptableMusic = "Sound/Scriptable/Musics/";

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
        ResetManager();

        CheckSoundSystem();

        SetObjectPoling();

        yield return LoadSoundDatas();

        yield return LoadMusicDatas();

        yield return null;
    }

    private IEnumerator LoadSoundDatas()
    {
        Sounds = new List<SoundScriptable>();

        SoundScriptable[] temps = Resources.LoadAll<SoundScriptable>(ResourceFolderName_ScriptableSound) as SoundScriptable[];
        temps = temps.OrderBy(e => e.audioClipName).ToArray();

        for (int i = 0; i < temps.Length; i++)
        {
            Sounds.Add(temps[i]);
        }

        yield return null;
    }

    private IEnumerator LoadMusicDatas()
    {
        Musics = new List<MusicScriptable>();

        MusicScriptable[] temps = Resources.LoadAll<MusicScriptable>(ResourceFolderName_ScriptableMusic) as MusicScriptable[];
        temps = temps.OrderBy(e => e.audioClipName).ToArray();

        for (int i = 0; i < temps.Length; i++)
        {
            Musics.Add(temps[i]);
        }

        yield return null;
    }

    private void SetObjectPoling()
    {
        soundPool.SetStart(SoundPrefab, 20);
    }

    public void CheckSoundSystem()
    {
        SettingMusicVolumeIndex = GetMusicVolume();
        SettingSoundIndex = GetSound();
    }

    /// <summary>
    /// Play Music
    /// </summary>
    /// <param name="musicList"></param>
    public void PlayMusic(MusicScriptable musicList)
    {
        MusicIndex = Random.Range(0, Musics.Count);
        SetupNextMusic(StartMusic());
    }

    /// <summary>
    /// Next Play Music
    /// </summary>
    public void NextPlayMusic()
    {
        //Delete Ram Cache
        Resources.UnloadUnusedAssets();
        
        MusicIndex++;
        if (Musics.Count <= MusicIndex)
            MusicIndex = 0;

        SetupNextMusic(StartMusic());
    }

    private float StartMusic()
    {
        AudioClip clip = Resources.Load(ResourceFolderName_Music + Musics[MusicIndex].audioClipName, typeof(AudioClip)) as AudioClip;

        if (clip == null)
            return 0f;

        MusicSource.clip = clip;
        MusicSource.volume = Musics[MusicIndex].musicVolume * ((float)SettingMusicVolumeIndex / 100f);
        MusicSource.Play();

        return clip.length;
    }

    private void SetupNextMusic(float clipLength)
    {
        if (SetNextPlayMusic != null)
        {
            StopCoroutine(SetNextPlayMusic);
            SetNextPlayMusic = null;
        }

        SetNextPlayMusic = SetNextMusic(clipLength);
        StartCoroutine(SetNextPlayMusic);
    }

    IEnumerator SetNextMusic(float time)
    {
        yield return new WaitForSeconds(time);
        NextPlayMusic();
    }

    /// <summary>
    /// Create Sound Prefab
    /// (Option: Pitch)
    /// </summary>
    /// <param name="soundID"></param>
    /// <param name="pitchActive"></param>
    public void CreateSound(SoundID soundID, bool pitchActive)
    {
        if (!GetSoundForCheck())
            return;

        for (int i = 0; i < Sounds.Count; i++)
        {
            if (Sounds[i].soundID == soundID)
            {
                AudioClip clip = Resources.Load(ResourceFolderName_Sound + Sounds[i].audioClipName, typeof(AudioClip)) as AudioClip;

                GameObject temp = null;
                if (pitchActive)
                    temp = ActiveForPitch(soundID);

                if (!pitchActive || temp == null)
                    temp = soundPool.GetObject();

                temp.GetComponent<SoundPrefab>().SetSettings(clip, Sounds[i], pitchActive);

                //Destroy(temp, clip.length);
                if (!pitchActive)
                    StartCoroutine(Delay(temp, clip.length));
            }
        }
    }

    IEnumerator Delay(GameObject temp, float time) 
    {
        yield return new WaitForSeconds(time);
        soundPool.ReturnObject(temp);
    }

    public void ReturnPool(GameObject temp)
    {
        soundPool.ReturnObject(temp);
    }

    private GameObject ActiveForPitch(SoundID soundID)
    {
        foreach (Transform item in soundPool.transform)
        {
            if (item.gameObject.activeSelf)
                if (item.GetComponent<SoundPrefab>().soundScriptable.soundID == soundID)
                    return item.gameObject;
        }

        return null;
    }

    private void ResetManager()
    {
        foreach (ObjectPool pool in GetComponentsInChildren<ObjectPool>())
        {
            pool.Reset();
        }
    }

    #region SETTINGS

    public void SetFirstSettings()
    {
        //Between 0 to 100. 0 is deactive
        SettingMusicVolumeIndex = 0;
        SetMusicVolume(100);

        SettingSoundIndex = 1;
        SetSound(0);
    }

    public void SetMusicVolume(int volumeAmount)
    {
        SettingMusicVolumeIndex += volumeAmount;

        if (SettingMusicVolumeIndex > 100)
        {
            SettingMusicVolumeIndex = 100;
        }
        else if (SettingMusicVolumeIndex < 0)
        {
            SettingMusicVolumeIndex = 0;
        }

        PlayerPrefs.SetInt(SettingMusicVolumeText, SettingMusicVolumeIndex);

        MusicSource.volume = (float)SettingMusicVolumeIndex / 100f;

        CheckSoundSystem();
    }

    public int GetMusicVolume()
    {
        return PlayerPrefs.GetInt(SettingMusicVolumeText);
    }

    public void SetMusicBackgroundVolumeSetting(float volume)
    {
        MusicSource.volume = volume * ((float)SettingMusicVolumeIndex / 100f);
    }

    public void SetSound(int index)
    {
        SettingSoundIndex += index;

        if (SettingSoundIndex > 1)
        {
            SettingSoundIndex = 1;
        }
        else if (SettingSoundIndex < 0)
        {
            SettingSoundIndex = 0;
        }

        PlayerPrefs.SetInt(SettingSoundText, SettingSoundIndex);

        CheckSoundSystem();
    }

    public int GetSound()
    {
        return PlayerPrefs.GetInt(SettingSoundText);
    }

    /// <summary>
    /// Sound active if True
    /// </summary>
    /// <returns></returns>
    public bool GetSoundForCheck()
    {
        SettingSoundIndex = PlayerPrefs.GetInt(SettingSoundText);
        if (SettingSoundIndex == 1)
            return true;
        else
            return false;
    }

    #endregion
}

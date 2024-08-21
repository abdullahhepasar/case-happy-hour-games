using System.Collections;
using UnityEngine;

[RequireComponent(typeof(AudioSource))]
public class SoundPrefab : MonoBehaviour
{
    public AudioSource audioSource;

    [HideInInspector] public SoundScriptable soundScriptable;

    private float pitchChange = 0;

    IEnumerator IEDelay;

    public void SetSettings(AudioClip audioClip, SoundScriptable SS ,bool pitch)
    {
        this.soundScriptable = SS;

        audioSource.clip = audioClip;
        audioSource.volume = soundScriptable.volume;
        audioSource.loop = soundScriptable.loopActive;

        if (audioSource.pitch >= soundScriptable.pitchMax)
            audioSource.pitch = soundScriptable.pitchMax;
        else
            audioSource.pitch = soundScriptable.pitchDefault + pitchChange;

        audioSource.Play();

        if (pitch)
        {
            pitchChange += soundScriptable.pitchChange;
            SetDelay(soundScriptable.pitchDelayDeactivateTime);
        }
    }

    private void SetDelay(float delay)
    {
        if (IEDelay != null)
        {
            StopCoroutine(IEDelay);
            IEDelay = null;
        }

        if (IEDelay == null)
        {
            IEDelay = Delay(delay);
            StartCoroutine(IEDelay);
        }
    }

    IEnumerator Delay(float delay)
    {
        yield return new WaitForSeconds(delay);
        SoundManager.Instance.ReturnPool(this.gameObject);
    }

    private void OnDisable()
    {
        pitchChange = 0;
        soundScriptable = null;

        audioSource.clip = null;
        audioSource.pitch = 1f;
        audioSource.volume = 1;
        audioSource.loop = false;
    }
}

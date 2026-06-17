using System.Collections;
using UnityEngine;
using UnityEngine.Audio;

public class AudioManager : MonoBehaviour
{
    public static AudioManager Instance;

    [Header("Mezclador de audio")]
    [SerializeField] private AudioMixer audioMixer;

    [Header("Música")]
    [SerializeField] private AudioSource musicSource;
    [SerializeField] private AudioClip[] normalMusicClips;
    [SerializeField] private AudioClip stormMusicClip;

    [Header("Ambiente")]
    [SerializeField] private AudioSource ambientSource;
    [SerializeField] private AudioClip natureAmbientClip;
    [SerializeField] private AudioClip stormAmbientClip;

    [Header("Efectos de interfaz y eventos")]
    [SerializeField] private AudioSource uiSource;
    [SerializeField] private AudioClip visitorJoinClip;
    [SerializeField] private AudioClip colonistDeathClip;
    [SerializeField] private AudioClip clickClip;
    [SerializeField] private AudioClip messageClip;
    [SerializeField] private AudioClip notAllowedClip;
    [SerializeField] private AudioClip winClip;
    [SerializeField] private AudioClip loseClip;

    private Coroutine musicRoutine;
    private int currentMusicIndex = 0;
    private bool stormActive = false;

    private void Awake()
    {
        if (Instance == null) Instance = this;
        else
        {
            Destroy(gameObject);
            return;
        }

        DontDestroyOnLoad(gameObject);
    }

    private void Start()
    {
        StartNormalMusic();
        PlayNatureAmbient();
    }

    public void StartNormalMusic()
    {
        stormActive = false;

        if (musicRoutine != null)
        {
            StopCoroutine(musicRoutine);
        }

        musicRoutine = StartCoroutine(NormalMusicRoutine());
    }

    private IEnumerator NormalMusicRoutine()
    {
        if (musicSource == null || normalMusicClips == null || normalMusicClips.Length == 0)
        {
            yield break;
        }

        while (!stormActive)
        {
            AudioClip clip = normalMusicClips[currentMusicIndex];

            musicSource.clip = clip;
            musicSource.loop = false;
            musicSource.Play();

            currentMusicIndex++;

            if (currentMusicIndex >= normalMusicClips.Length)
            {
                currentMusicIndex = 0;
            }

            yield return new WaitForSeconds(clip.length);
        }
    }

    public void StartStormAudio()
    {
        stormActive = true;

        if (musicRoutine != null)
        {
            StopCoroutine(musicRoutine);
            musicRoutine = null;
        }

        if (musicSource != null && stormMusicClip != null)
        {
            musicSource.Stop();
            musicSource.clip = stormMusicClip;
            musicSource.loop = true;
            musicSource.Play();
        }

        if (ambientSource != null && stormAmbientClip != null)
        {
            ambientSource.Stop();
            ambientSource.clip = stormAmbientClip;
            ambientSource.loop = true;
            ambientSource.Play();
        }
    }

    public void StopStormAudio()
    {
        stormActive = false;

        PlayNatureAmbient();
        StartNormalMusic();
    }

    private void PlayNatureAmbient()
    {
        if (ambientSource == null || natureAmbientClip == null) return;

        ambientSource.Stop();
        ambientSource.clip = natureAmbientClip;
        ambientSource.loop = true;
        ambientSource.Play();
    }

    public void PlayVisitorJoin()
    {
        PlayUISound(visitorJoinClip);
    }

    public void PlayColonistDeath()
    {
        PlayUISound(colonistDeathClip);
    }

    public void PlayClick()
    {
        PlayUISound(clickClip);
    }

    public void PlayMessage()
    {
        PlayUISound(messageClip);
    }

    public void PlayNotAllowed()
    {
        PlayUISound(notAllowedClip);
    }

    public void PlayWin()
    {
        PlayUISound(winClip);
    }

    public void PlayLose()
    {
        PlayUISound(loseClip);
    }

    private void PlayUISound(AudioClip clip)
    {
        if (uiSource == null || clip == null) return;

        uiSource.PlayOneShot(clip);
    }

    public void SetMasterVolume(float value)
    {
        SetMixerVolume("MasterVolume", value);
    }

    public void SetMusicVolume(float value)
    {
        SetMixerVolume("MusicVolume", value);
    }

    public void SetSFXVolume(float value)
    {
        SetMixerVolume("SFXVolume", value);
    }

    public void SetAmbientVolume(float value)
    {
        SetMixerVolume("AmbientVolume", value);
    }

    private void SetMixerVolume(string parameterName, float value)
    {
        if (audioMixer == null) return;

        value = Mathf.Clamp(value, 0.0001f, 1f);
        float volumeDb = Mathf.Log10(value) * 20f;

        audioMixer.SetFloat(parameterName, volumeDb);
    }
}
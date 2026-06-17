using UnityEngine;

public enum ColonistLoopSound
{
    None,
    Walk,
    Sleep,
    Chop,
    Mine,
    Build
}

public class ColonistAudio : MonoBehaviour
{
    [Header("Fuentes de audio")]
    [SerializeField] private AudioSource loopSource;
    [SerializeField] private AudioSource oneShotSource;

    [Header("Clips de acciones")]
    [SerializeField] private AudioClip walkClip;
    [SerializeField] private AudioClip eatClip;
    [SerializeField] private AudioClip sleepClip;
    [SerializeField] private AudioClip chopClip;
    [SerializeField] private AudioClip mineClip;
    [SerializeField] private AudioClip buildClip;

    [Header("Volumen por distancia")]
    [SerializeField] private float maxHearingDistance = 12f;
    [SerializeField] private float minVolume = 0f;
    [SerializeField] private float maxVolume = 1f;

    private Camera mainCamera;
    private ColonistLoopSound currentLoopSound = ColonistLoopSound.None;

    private void Awake()
    {
        mainCamera = Camera.main;

        if (loopSource != null)
        {
            loopSource.loop = true;
            loopSource.playOnAwake = false;
        }

        if (oneShotSource != null)
        {
            oneShotSource.loop = false;
            oneShotSource.playOnAwake = false;
        }
    }

    private void Update()
    {
        UpdateVolumeByCameraDistance();
    }

    private void UpdateVolumeByCameraDistance()
    {
        if (mainCamera == null)
        {
            mainCamera = Camera.main;
        }

        if (mainCamera == null) return;

        float distance = Vector3.Distance(transform.position, mainCamera.transform.position);
        float t = Mathf.Clamp01(distance / maxHearingDistance);
        float volume = Mathf.Lerp(maxVolume, minVolume, t);

        if (loopSource != null)
        {
            loopSource.volume = volume;
        }

        if (oneShotSource != null)
        {
            oneShotSource.volume = volume;
        }
    }

    public void SetWalking(bool walking)
    {
        if (walking)
        {
            PlayLoop(ColonistLoopSound.Walk);
        }
        else if (currentLoopSound == ColonistLoopSound.Walk)
        {
            StopLoop();
        }
    }

    public void PlayEat()
    {
        PlayOneShot(eatClip);
    }

    public void PlaySleepLoop()
    {
        PlayLoop(ColonistLoopSound.Sleep);
    }

    public void PlayChopLoop()
    {
        PlayLoop(ColonistLoopSound.Chop);
    }

    public void PlayMineLoop()
    {
        PlayLoop(ColonistLoopSound.Mine);
    }

    public void PlayBuildLoop()
    {
        PlayLoop(ColonistLoopSound.Build);
    }

    public void StopLoop()
    {
        currentLoopSound = ColonistLoopSound.None;

        if (loopSource != null)
        {
            loopSource.Stop();
            loopSource.clip = null;
        }
    }

    private void PlayLoop(ColonistLoopSound sound)
    {
        if (loopSource == null) return;
        if (currentLoopSound == sound && loopSource.isPlaying) return;

        AudioClip clip = GetClipForLoop(sound);

        if (clip == null) return;

        currentLoopSound = sound;
        loopSource.clip = clip;
        loopSource.loop = true;
        loopSource.Play();
    }

    private AudioClip GetClipForLoop(ColonistLoopSound sound)
    {
        switch (sound)
        {
            case ColonistLoopSound.Walk:
                return walkClip;
            case ColonistLoopSound.Sleep:
                return sleepClip;
            case ColonistLoopSound.Chop:
                return chopClip;
            case ColonistLoopSound.Mine:
                return mineClip;
            case ColonistLoopSound.Build:
                return buildClip;
            default:
                return null;
        }
    }

    private void PlayOneShot(AudioClip clip)
    {
        if (oneShotSource == null || clip == null) return;

        oneShotSource.PlayOneShot(clip);
    }
}
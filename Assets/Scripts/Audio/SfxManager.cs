using UnityEngine;

[RequireComponent(typeof(AudioSource))]
public class SfxManager : MonoBehaviour
{
    public static SfxManager Instance { get; private set; }

    [Header("Clips")]
    [SerializeField] private AudioClip winClip;
    [SerializeField] private AudioClip clickClip;
    [SerializeField] private AudioClip hitClip;
    [SerializeField] private AudioClip moveClip;

    [Header("Output")]
    [SerializeField] private AudioSource sfxSource;
    [SerializeField] private bool dontDestroyOnLoad = true;
    [SerializeField] [Range(0f, 1f)] private float volume = 1f;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;

        if (sfxSource == null)
            sfxSource = GetComponent<AudioSource>();

        sfxSource.playOnAwake = false;
        sfxSource.loop = false;

        if (dontDestroyOnLoad)
            DontDestroyOnLoad(gameObject);
    }

    public void PlayWin()
    {
        PlayOneShot(winClip);
    }

    public void PlayClick()
    {
        PlayOneShot(clickClip);
    }

    public void PlayHit()
    {
        PlayOneShot(hitClip);
    }

    public void PlayMove()
    {
        PlayOneShot(moveClip);
    }

    private void PlayOneShot(AudioClip clip)
    {
        if (clip == null || sfxSource == null)
            return;

        sfxSource.PlayOneShot(clip, volume);
    }
}

using System.Collections;
using UnityEngine;

public class MusicPlayer : MonoBehaviour
{
    private static MusicPlayer Singleton;
    private static AudioSource _musicPlayer;
    private static AudioSource _oneShotPlayer;

    [SerializeField]
    private AudioClip backgroundMusic;
    private void Awake()
    {
        if (Singleton)
        {
            Destroy(gameObject);
            return;
        }

        Singleton = this;
        _musicPlayer = GetComponent<AudioSource>();
        _oneShotPlayer = gameObject.AddComponent<AudioSource>();
        _oneShotPlayer.priority = 10;
    }
    private void OnEnable()
    {
        GameController.OnGameStarted += () => PlayMusic(backgroundMusic, 3f, 0.6f);
    }
    private void OnDisable()
    {
        GameController.OnGameStarted -= () => PlayMusic(backgroundMusic, 3f, 0.6f);
    }

    public static void PlayMusic(AudioClip clip, float fadeInSpeed = 1f, float maxVolume = 1f)
    {
        _musicPlayer.clip = clip;
        _musicPlayer.Play();
        Singleton.StartCoroutine(FadeInVolume());

        IEnumerator FadeInVolume()
        {
            float progress = 0f;

            while (progress < fadeInSpeed)
            {
                progress += Time.deltaTime;
                float volume = Mathf.Lerp(0, maxVolume, progress);
                _musicPlayer.volume = volume;

                yield return null;
            }
        }
    }
    public static void PlayClipOnce(AudioClip clip, float volume = 1f)
    {
        _oneShotPlayer.PlayOneShot(clip, volume);
    }

    private void OnDestroy()
    {
        Singleton = null;
    }
}

using UnityEngine;

namespace LostCave.Audio
{
    public class AudioManager : MonoBehaviour
    {
        public static AudioManager Instance { get; private set; }

        [Header("Audio Sources")]
        [SerializeField] private AudioSource musicSource;
        [SerializeField] private AudioSource ambienceSource;
        [SerializeField] private AudioSource sfxSource;

        [Header("Default Volumes")]
        [Range(0f, 1f)] public float musicVolume = 0.75f;
        [Range(0f, 1f)] public float ambienceVolume = 0.85f;
        [Range(0f, 1f)] public float sfxVolume = 1.0f;

        [Header("Default Audio Clips")]
        public AudioClip defaultCaveAmbience;
        public AudioClip defaultBackgroundMusic;
        public AudioClip buttonClickSound;
        public AudioClip checkpointSound;
        public AudioClip levelCompleteSound;
        public AudioClip playerHitSound;

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }

            Instance = this;
            DontDestroyOnLoad(gameObject);

            InitializeAudioSources();
            LoadVolumePreferences();
        }

        private void Start()
        {
            if (defaultCaveAmbience != null && ambienceSource != null && !ambienceSource.isPlaying)
            {
                PlayAmbience(defaultCaveAmbience);
            }

            if (defaultBackgroundMusic != null && musicSource != null && !musicSource.isPlaying)
            {
                PlayMusic(defaultBackgroundMusic);
            }
        }

        private void InitializeAudioSources()
        {
            if (musicSource == null)
            {
                GameObject musicObj = new GameObject("MusicSource");
                musicObj.transform.SetParent(transform);
                musicSource = musicObj.AddComponent<AudioSource>();
                musicSource.loop = true;
                musicSource.playOnAwake = false;
            }

            if (ambienceSource == null)
            {
                GameObject ambObj = new GameObject("AmbienceSource");
                ambObj.transform.SetParent(transform);
                ambienceSource = ambObj.AddComponent<AudioSource>();
                ambienceSource.loop = true;
                ambienceSource.playOnAwake = false;
            }

            if (sfxSource == null)
            {
                GameObject sfxObj = new GameObject("SFXSource");
                sfxObj.transform.SetParent(transform);
                sfxSource = sfxObj.AddComponent<AudioSource>();
                sfxSource.loop = false;
                sfxSource.playOnAwake = false;
            }
        }

        public void PlayMusic(AudioClip clip, float fadeDuration = 1.0f)
        {
            if (clip == null || musicSource == null) return;
            musicSource.clip = clip;
            musicSource.volume = musicVolume;
            musicSource.Play();
        }

        public void PlayAmbience(AudioClip clip)
        {
            if (clip == null || ambienceSource == null) return;
            ambienceSource.clip = clip;
            ambienceSource.volume = ambienceVolume;
            ambienceSource.Play();
        }

        public void PlaySFX(AudioClip clip, float volumeScale = 1.0f)
        {
            if (clip == null || sfxSource == null) return;
            sfxSource.PlayOneShot(clip, sfxVolume * volumeScale);
        }

        public void PlaySFXAtPosition(AudioClip clip, Vector3 position, float volume = 1.0f, float maxDistance = 25f)
        {
            if (clip == null) return;
            GameObject tempGO = new GameObject("TempAudio_" + clip.name);
            tempGO.transform.position = position;
            AudioSource aSource = tempGO.AddComponent<AudioSource>();
            aSource.clip = clip;
            aSource.volume = sfxVolume * volume;
            aSource.spatialBlend = 1.0f; // 3D sound
            aSource.minDistance = 2f;
            aSource.maxDistance = maxDistance;
            aSource.rolloffMode = AudioRolloffMode.Linear;
            aSource.Play();
            Destroy(tempGO, clip.length + 0.1f);
        }

        public void PlayButtonClick()
        {
            if (buttonClickSound != null)
            {
                PlaySFX(buttonClickSound, 0.9f);
            }
        }

        public void SetMusicVolume(float val)
        {
            musicVolume = Mathf.Clamp01(val);
            if (musicSource != null) musicSource.volume = musicVolume;
            PlayerPrefs.SetFloat("MusicVolume", musicVolume);
        }

        public void SetAmbienceVolume(float val)
        {
            ambienceVolume = Mathf.Clamp01(val);
            if (ambienceSource != null) ambienceSource.volume = ambienceVolume;
            PlayerPrefs.SetFloat("AmbienceVolume", ambienceVolume);
        }

        public void SetSFXVolume(float val)
        {
            sfxVolume = Mathf.Clamp01(val);
            if (sfxSource != null) sfxSource.volume = sfxVolume;
            PlayerPrefs.SetFloat("SFXVolume", sfxVolume);
        }

        private void LoadVolumePreferences()
        {
            musicVolume = PlayerPrefs.GetFloat("MusicVolume", musicVolume);
            ambienceVolume = PlayerPrefs.GetFloat("AmbienceVolume", ambienceVolume);
            sfxVolume = PlayerPrefs.GetFloat("SFXVolume", sfxVolume);

            if (musicSource != null) musicSource.volume = musicVolume;
            if (ambienceSource != null) ambienceSource.volume = ambienceVolume;
            if (sfxSource != null) sfxSource.volume = sfxVolume;
        }
    }
}

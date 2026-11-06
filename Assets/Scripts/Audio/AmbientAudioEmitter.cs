using UnityEngine;

namespace LostCave.Audio
{
    [RequireComponent(typeof(AudioSource))]
    public class AmbientAudioEmitter : MonoBehaviour
    {
        [SerializeField] private AudioClip soundClip;
        [Range(0f, 1f)] [SerializeField] private float volume = 0.6f;
        [SerializeField] private float minDistance = 2f;
        [SerializeField] private float maxDistance = 18f;
        [SerializeField] private bool randomizePitch = true;
        [SerializeField] private bool loop = true;

        private AudioSource audioSource;

        private void Awake()
        {
            audioSource = GetComponent<AudioSource>();
            audioSource.clip = soundClip;
            audioSource.volume = volume;
            audioSource.spatialBlend = 1.0f; // full 3D sound
            audioSource.minDistance = minDistance;
            audioSource.maxDistance = maxDistance;
            audioSource.rolloffMode = AudioRolloffMode.Linear;
            audioSource.loop = loop;
            audioSource.playOnAwake = true;

            if (randomizePitch)
            {
                audioSource.pitch = Random.Range(0.92f, 1.08f);
            }
        }

        private void Start()
        {
            if (audioSource.clip != null && !audioSource.isPlaying)
            {
                audioSource.Play();
            }
        }
    }
}

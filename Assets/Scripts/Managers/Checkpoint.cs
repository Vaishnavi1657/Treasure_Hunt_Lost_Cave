using UnityEngine;
using LostCave.Audio;

namespace LostCave.Managers
{
    public class Checkpoint : MonoBehaviour
    {
        [Header("Checkpoint Settings")]
        public Transform respawnPoint;
        public Light activeIndicatorLight;
        public AudioClip checkpointClip;
        public bool isActivated = false;

        private void Start()
        {
            if (respawnPoint == null) respawnPoint = transform;
            if (activeIndicatorLight != null)
            {
                activeIndicatorLight.enabled = isActivated;
            }
        }

        private void OnTriggerEnter(Collider other)
        {
            if (isActivated) return;

            if (other.CompareTag("Player"))
            {
                ActivateCheckpoint();
            }
        }

        public void ActivateCheckpoint()
        {
            isActivated = true;
            if (activeIndicatorLight != null)
            {
                activeIndicatorLight.enabled = true;
            }

            if (checkpointClip != null && AudioManager.Instance != null)
            {
                AudioManager.Instance.PlaySFX(checkpointClip, 0.9f);
            }

            if (GameManager.Instance != null)
            {
                GameManager.Instance.SetCheckpoint(respawnPoint.position, respawnPoint.rotation);
                GameManager.Instance.ShowNotification("Checkpoint Activated!");
            }
        }
    }
}

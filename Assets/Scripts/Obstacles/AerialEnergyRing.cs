using UnityEngine;
using LostCave.Audio;
using LostCave.Player;
using LostCave.Managers;

namespace LostCave.Obstacles
{
    public class AerialEnergyRing : MonoBehaviour
    {
        [Header("Spin Settings")]
        public float spinSpeed = 45.0f;
        public float bobSpeed = 2.0f;
        public float bobHeight = 0.35f;

        [Header("Ring Visuals")]
        public Transform ringMesh;
        public Light ringLight;
        public Color activeColor = new Color(0.1f, 0.95f, 1.0f);
        public Color collectedColor = new Color(0.2f, 1.0f, 0.5f);

        [Header("Audio")]
        public AudioClip ringPassClip;

        private bool isCollected = false;
        private Vector3 startPos;

        private void Start()
        {
            startPos = transform.position;
            if (ringMesh == null) ringMesh = transform;
            if (ringLight == null) ringLight = GetComponentInChildren<Light>();
        }

        private void Update()
        {
            if (ringMesh != null)
            {
                ringMesh.Rotate(Vector3.forward, spinSpeed * Time.deltaTime, Space.Self);
            }

            transform.position = startPos + Vector3.up * Mathf.Sin(Time.time * bobSpeed) * bobHeight;
        }

        private void OnTriggerEnter(Collider other)
        {
            if (isCollected) return;

            if (other.CompareTag("Player") || other.GetComponent<PlayerFlightController>() != null || other.GetComponentInParent<PlayerFlightController>() != null)
            {
                CollectRing();
            }
        }

        private void CollectRing()
        {
            isCollected = true;

            if (ringPassClip != null && AudioManager.Instance != null)
            {
                AudioManager.Instance.PlaySFX(ringPassClip, 1.0f);
            }
            else if (AudioManager.Instance != null && AudioManager.Instance.checkpointSound != null)
            {
                AudioManager.Instance.PlaySFX(AudioManager.Instance.checkpointSound, 1.0f);
            }

            if (PlayerFlightController.Instance != null)
            {
                PlayerFlightController.Instance.TriggerRingBoost(1.35f, 2.5f);
            }

            if (GameManager.Instance != null)
            {
                GameManager.Instance.ShowHUDNotification("ENERGY RING! NITRO BOOST ACTIVATED!");
            }

            if (ringLight != null)
            {
                ringLight.color = collectedColor;
                ringLight.intensity = 6.0f;
            }

            Destroy(gameObject, 0.6f);
        }
    }
}

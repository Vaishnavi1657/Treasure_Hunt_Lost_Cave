using System.Collections;
using UnityEngine;
using LostCave.Audio;
using LostCave.Camera;

namespace LostCave.Obstacles
{
    public class CollapsingTile : MonoBehaviour
    {
        [Header("Timing")]
        public float collapseDelay = 0.75f;
        public float respawnDelay = 6.0f;
        public float shakeIntensity = 0.06f;
        public float fallSpeed = 16.0f;

        [Header("Audio")]
        public AudioClip crackClip;
        public AudioClip collapseClip;

        [Header("Visual Feedback")]
        public Color warningColor = new Color(0.85f, 0.35f, 0.2f);

        private Vector3 initialPosition;
        private Quaternion initialRotation;
        private Material tileMaterial;
        private Color initialColor;
        private Collider tileCollider;
        private bool isTriggered = false;

        private void Awake()
        {
            initialPosition = transform.position;
            initialRotation = transform.rotation;
            tileCollider = GetComponent<Collider>();

            Renderer rend = GetComponent<Renderer>();
            if (rend != null)
            {
                tileMaterial = rend.material;
                initialColor = tileMaterial.color;
            }
        }

        private void OnCollisionEnter(Collision collision)
        {
            if (isTriggered) return;

            if (collision.gameObject.CompareTag("Player"))
            {
                // Only trigger if player lands on top
                foreach (ContactPoint contact in collision.contacts)
                {
                    if (contact.normal.y < -0.5f)
                    {
                        StartCoroutine(CollapseRoutine());
                        break;
                    }
                }
            }
        }

        private void OnTriggerEnter(Collider other)
        {
            if (isTriggered) return;
            if (other.CompareTag("Player"))
            {
                StartCoroutine(CollapseRoutine());
            }
        }

        private IEnumerator CollapseRoutine()
        {
            isTriggered = true;

            // Play cracking sound
            if (crackClip != null && AudioManager.Instance != null)
            {
                AudioManager.Instance.PlaySFXAtPosition(crackClip, transform.position, 1.0f);
            }

            if (ThirdPersonCamera.Instance != null)
            {
                ThirdPersonCamera.Instance.TriggerShake(0.12f, collapseDelay);
            }

            // Warning shake and color shift
            float elapsed = 0f;
            while (elapsed < collapseDelay)
            {
                elapsed += Time.deltaTime;
                float progress = elapsed / collapseDelay;

                Vector3 jitter = Random.insideUnitSphere * shakeIntensity * progress;
                transform.position = initialPosition + jitter;

                if (tileMaterial != null)
                {
                    tileMaterial.color = Color.Lerp(initialColor, warningColor, progress);
                }

                yield return null;
            }

            // Play collapse rumble sound
            if (collapseClip != null && AudioManager.Instance != null)
            {
                AudioManager.Instance.PlaySFXAtPosition(collapseClip, transform.position, 1.0f);
            }

            // Fall downwards
            tileCollider.enabled = false;
            float fallTime = 0f;
            while (fallTime < 1.8f)
            {
                fallTime += Time.deltaTime;
                transform.position += Vector3.down * fallSpeed * Time.deltaTime;
                transform.Rotate(Random.Range(-30f, 30f) * Time.deltaTime, 0f, Random.Range(-30f, 30f) * Time.deltaTime);
                yield return null;
            }

            // Hide
            SetVisible(false);

            // Wait for respawn
            yield return new WaitForSeconds(respawnDelay);

            // Reset tile
            transform.position = initialPosition;
            transform.rotation = initialRotation;
            if (tileMaterial != null) tileMaterial.color = initialColor;
            tileCollider.enabled = true;
            SetVisible(true);
            isTriggered = false;
        }

        private void SetVisible(bool visible)
        {
            Renderer rend = GetComponent<Renderer>();
            if (rend != null) rend.enabled = visible;
        }
    }
}

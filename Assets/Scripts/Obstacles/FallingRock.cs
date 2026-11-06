using System.Collections;
using UnityEngine;
using LostCave.Audio;
using LostCave.Player;
using LostCave.Camera;

namespace LostCave.Obstacles
{
    public class FallingRock : MonoBehaviour
    {
        [Header("Ceiling Rock Object")]
        public Transform rockMesh;
        public GameObject groundWarningIndicator;
        public ParticleSystem impactDustEffect;

        [Header("Falling Parameters")]
        public float warningDelay = 1.1f;
        public float fallGravity = 35f;
        public float resetDelay = 5.0f;
        public int damage = 1;
        public float knockbackStrength = 8f;

        [Header("Audio")]
        public AudioClip fallingSound;
        public AudioClip impactSound;

        private Vector3 initialRockLocalPos;
        private Quaternion initialRockLocalRot;
        private bool hasTriggered = false;
        private float groundY;

        private void Awake()
        {
            if (rockMesh != null)
            {
                initialRockLocalPos = rockMesh.localPosition;
                initialRockLocalRot = rockMesh.localRotation;
            }

            if (groundWarningIndicator != null)
            {
                groundWarningIndicator.SetActive(false);
                groundY = groundWarningIndicator.transform.position.y;
            }
            else
            {
                groundY = transform.position.y - 6f;
            }
        }

        private void OnTriggerEnter(Collider other)
        {
            if (hasTriggered) return;

            if (other.CompareTag("Player"))
            {
                StartCoroutine(TriggerFallingRockRoutine());
            }
        }

        private IEnumerator TriggerFallingRockRoutine()
        {
            hasTriggered = true;

            // Activate ground warning indicator
            if (groundWarningIndicator != null)
            {
                groundWarningIndicator.SetActive(true);
            }

            // Play falling whistling whoosh
            if (fallingSound != null && AudioManager.Instance != null)
            {
                AudioManager.Instance.PlaySFXAtPosition(fallingSound, transform.position, 0.9f);
            }

            // Small ceiling wobble
            float elapsed = 0f;
            while (elapsed < warningDelay)
            {
                elapsed += Time.deltaTime;
                if (rockMesh != null)
                {
                    rockMesh.localPosition = initialRockLocalPos + Random.insideUnitSphere * 0.05f;
                }
                yield return null;
            }

            // Hide warning
            if (groundWarningIndicator != null)
            {
                groundWarningIndicator.SetActive(false);
            }

            // Drop rock rapidly
            float velocityY = 0f;
            Vector3 rockWorldPos = rockMesh != null ? rockMesh.position : transform.position;

            while (rockWorldPos.y > groundY + 0.3f)
            {
                velocityY += fallGravity * Time.deltaTime;
                rockWorldPos.y -= velocityY * Time.deltaTime;
                if (rockMesh != null) rockMesh.position = rockWorldPos;

                // Check distance to player for impact
                if (PlayerController.Instance != null)
                {
                    float dist = Vector3.Distance(rockWorldPos, PlayerController.Instance.transform.position + Vector3.up);
                    if (dist < 1.4f)
                    {
                        Vector3 knockDir = (PlayerController.Instance.transform.position - rockWorldPos).normalized + Vector3.up * 0.5f;
                        PlayerController.Instance.TakeDamage(damage, knockDir * knockbackStrength);
                    }
                }

                yield return null;
            }

            // Impact with ground
            if (rockMesh != null)
            {
                rockWorldPos.y = groundY + 0.3f;
                rockMesh.position = rockWorldPos;
            }

            // Play heavy ground impact
            if (impactSound != null && AudioManager.Instance != null)
            {
                AudioManager.Instance.PlaySFXAtPosition(impactSound, rockWorldPos, 1.0f);
            }

            // Dust and screen shake
            if (impactDustEffect != null)
            {
                impactDustEffect.transform.position = rockWorldPos;
                impactDustEffect.Play();
            }

            if (ThirdPersonCamera.Instance != null)
            {
                ThirdPersonCamera.Instance.TriggerShake(0.45f, 0.35f);
            }

            // Reset after cooldown
            yield return new WaitForSeconds(resetDelay);

            if (rockMesh != null)
            {
                rockMesh.localPosition = initialRockLocalPos;
                rockMesh.localRotation = initialRockLocalRot;
            }

            hasTriggered = false;
        }
    }
}

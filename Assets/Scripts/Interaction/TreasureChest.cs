using System.Collections;
using UnityEngine;
using LostCave.Audio;
using LostCave.Managers;

namespace LostCave.Interaction
{
    public class TreasureChest : MonoBehaviour
    {
        [Header("Chest Model")]
        public Transform chestLid;
        public float openAngle = -110f;
        public float openSpeed = 2.5f;

        [Header("Effects")]
        public ParticleSystem goldSparkles;
        public Light chestInternalLight;
        public float maxLightIntensity = 6f;

        [Header("Audio")]
        public AudioClip chestOpenClip;
        public AudioClip victoryFanfareClip;

        [Header("Interaction Radius")]
        public float interactionDistance = 3.0f;

        private bool isOpen = false;
        private bool isPlayerNearby = false;

        private void Start()
        {
            if (chestInternalLight != null)
            {
                chestInternalLight.intensity = 0f;
            }
        }

        private void Update()
        {
            if (isOpen) return;

            if (Player.PlayerController.Instance != null)
            {
                float dist = Vector3.Distance(transform.position, Player.PlayerController.Instance.transform.position);
                bool wasNearby = isPlayerNearby;
                isPlayerNearby = dist <= interactionDistance;

                if (isPlayerNearby && !wasNearby)
                {
                    if (GameManager.Instance != null)
                    {
                        GameManager.Instance.ShowInteractionPrompt("Press [E] to Open the Treasure Chest");
                    }
                }
                else if (!isPlayerNearby && wasNearby)
                {
                    if (GameManager.Instance != null)
                    {
                        GameManager.Instance.HideInteractionPrompt();
                    }
                }

                if (isPlayerNearby && Input.GetKeyDown(KeyCode.E))
                {
                    StartCoroutine(OpenChestRoutine());
                }
            }
        }

        private IEnumerator OpenChestRoutine()
        {
            isOpen = true;

            if (GameManager.Instance != null)
            {
                GameManager.Instance.HideInteractionPrompt();
            }

            // Play chest creak audio
            if (chestOpenClip != null && AudioManager.Instance != null)
            {
                AudioManager.Instance.PlaySFX(chestOpenClip, 1.0f);
            }

            // Animate lid opening smoothly
            if (chestLid != null)
            {
                Quaternion startRot = chestLid.localRotation;
                Quaternion targetRot = startRot * Quaternion.Euler(openAngle, 0f, 0f);

                float t = 0f;
                while (t < 1f)
                {
                    t += Time.deltaTime * openSpeed;
                    chestLid.localRotation = Quaternion.Slerp(startRot, targetRot, t);

                    if (chestInternalLight != null)
                    {
                        chestInternalLight.intensity = Mathf.Lerp(0f, maxLightIntensity, t);
                    }

                    yield return null;
                }
            }

            // Burst particles
            if (goldSparkles != null)
            {
                goldSparkles.Play();
            }

            // Play victory fanfare
            if (victoryFanfareClip != null && AudioManager.Instance != null)
            {
                AudioManager.Instance.PlaySFX(victoryFanfareClip, 1.0f);
            }

            yield return new WaitForSeconds(1.5f);

            // Notify GameManager
            if (GameManager.Instance != null)
            {
                GameManager.Instance.CompleteGame();
            }
        }
    }
}

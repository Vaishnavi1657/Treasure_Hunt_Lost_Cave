using System.Collections;
using UnityEngine;
using LostCave.Audio;
using LostCave.Player;

namespace LostCave.Obstacles
{
    public class PressurePlate : MonoBehaviour
    {
        [Header("Trap Components")]
        public Transform plateTransform;
        public Transform trapSpikes;
        public float depressionDepth = 0.08f;
        public float spikeEruptHeight = 1.4f;

        [Header("Timing")]
        public float spikeSpeed = 14.0f;
        public float trapHoldTime = 1.8f;
        public float resetSpeed = 4.0f;

        [Header("Damage")]
        public int damage = 1;
        public float knockback = 7f;

        [Header("Audio")]
        public AudioClip clickClip;
        public AudioClip spikesClip;

        private Vector3 initialPlatePos;
        private Vector3 initialSpikesPos;
        private bool isTriggered = false;

        private void Awake()
        {
            if (plateTransform != null) initialPlatePos = plateTransform.localPosition;
            if (trapSpikes != null) initialSpikesPos = trapSpikes.localPosition;
        }

        private void OnTriggerEnter(Collider other)
        {
            if (isTriggered) return;

            if (other.CompareTag("Player"))
            {
                StartCoroutine(TriggerTrapRoutine());
            }
        }

        private IEnumerator TriggerTrapRoutine()
        {
            isTriggered = true;

            // Depress plate
            if (plateTransform != null)
            {
                plateTransform.localPosition = initialPlatePos + Vector3.down * depressionDepth;
            }

            // Mechanical click
            if (clickClip != null && AudioManager.Instance != null)
            {
                AudioManager.Instance.PlaySFXAtPosition(clickClip, transform.position, 0.9f);
            }

            yield return new WaitForSeconds(0.15f);

            // Spikes shoot up
            if (spikesClip != null && AudioManager.Instance != null)
            {
                AudioManager.Instance.PlaySFXAtPosition(spikesClip, transform.position, 1.0f);
            }

            if (trapSpikes != null)
            {
                Vector3 targetSpikePos = initialSpikesPos + Vector3.up * spikeEruptHeight;
                while (Vector3.Distance(trapSpikes.localPosition, targetSpikePos) > 0.05f)
                {
                    trapSpikes.localPosition = Vector3.MoveTowards(trapSpikes.localPosition, targetSpikePos, Time.deltaTime * spikeSpeed);
                    yield return null;
                }

                // Check player collision with spikes
                if (PlayerController.Instance != null)
                {
                    float dist = Vector3.Distance(transform.position, PlayerController.Instance.transform.position);
                    if (dist < 2.2f)
                    {
                        Vector3 knockDir = (PlayerController.Instance.transform.position - transform.position).normalized + Vector3.up * 0.7f;
                        PlayerController.Instance.TakeDamage(damage, knockDir * knockback);
                    }
                }
            }

            yield return new WaitForSeconds(trapHoldTime);

            // Retract spikes
            if (trapSpikes != null)
            {
                while (Vector3.Distance(trapSpikes.localPosition, initialSpikesPos) > 0.05f)
                {
                    trapSpikes.localPosition = Vector3.MoveTowards(trapSpikes.localPosition, initialSpikesPos, Time.deltaTime * resetSpeed);
                    yield return null;
                }
                trapSpikes.localPosition = initialSpikesPos;
            }

            // Reset plate position
            if (plateTransform != null)
            {
                plateTransform.localPosition = initialPlatePos;
            }

            isTriggered = false;
        }
    }
}

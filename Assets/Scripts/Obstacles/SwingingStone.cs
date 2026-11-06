using UnityEngine;
using LostCave.Audio;
using LostCave.Player;
using LostCave.Camera;

namespace LostCave.Obstacles
{
    public class SwingingStone : MonoBehaviour
    {
        [Header("Pendulum Motion")]
        public Transform pivot;
        public Transform stoneBoulder;
        public float swingFrequency = 1.8f;
        public float maxAngle = 55.0f;
        public Vector3 swingAxis = Vector3.forward;

        [Header("Damage & Impact")]
        public int damage = 1;
        public float knockbackStrength = 12f;

        [Header("Audio")]
        public AudioClip whooshClip;
        public AudioClip hitClip;
        public float whooshInterval = 1.74f;

        private float timer = 0f;
        private float whooshTimer = 0f;

        private void Update()
        {
            timer += Time.deltaTime;
            float angle = Mathf.Sin(timer * swingFrequency) * maxAngle;

            if (pivot != null)
            {
                pivot.localRotation = Quaternion.AngleAxis(angle, swingAxis);
            }

            // Periodic whoosh sound near the apex / center
            whooshTimer += Time.deltaTime;
            if (whooshTimer >= whooshInterval)
            {
                whooshTimer = 0f;
                if (whooshClip != null && AudioManager.Instance != null && stoneBoulder != null)
                {
                    AudioManager.Instance.PlaySFXAtPosition(whooshClip, stoneBoulder.position, 0.75f, 20f);
                }
            }
        }

        private void OnTriggerEnter(Collider other)
        {
            if (other.CompareTag("Player"))
            {
                if (PlayerController.Instance != null)
                {
                    Vector3 stonePos = stoneBoulder != null ? stoneBoulder.position : transform.position;
                    Vector3 hitDir = (PlayerController.Instance.transform.position - stonePos).normalized + Vector3.up * 0.4f;
                    PlayerController.Instance.TakeDamage(damage, hitDir * knockbackStrength);

                    if (hitClip != null && AudioManager.Instance != null)
                    {
                        AudioManager.Instance.PlaySFXAtPosition(hitClip, stonePos, 1.0f);
                    }

                    if (ThirdPersonCamera.Instance != null)
                    {
                        ThirdPersonCamera.Instance.TriggerShake(0.3f, 0.25f);
                    }
                }
            }
        }
    }
}

using System.Collections;
using UnityEngine;
using LostCave.Audio;
using LostCave.Player;
using LostCave.Managers;

namespace LostCave.Obstacles
{
    public class CrystalLaserBarrier : MonoBehaviour
    {
        [Header("Beam Motion")]
        public bool isSweeping = true;
        public float sweepSpeed = 2.0f;
        public float sweepDistance = 3.5f;
        public Vector3 sweepAxis = Vector3.up;

        [Header("Laser Beam Visuals")]
        public Transform beamTransform;
        public Light laserLight;

        [Header("Damage Settings")]
        public int damage = 999;
        public float hitCooldown = 0.5f;

        private float lastHitTime = -999f;
        private Vector3 initialLocalPos;
        private bool hasTriggeredFatalCrash = false;

        private void Start()
        {
            if (beamTransform == null) beamTransform = transform;
            initialLocalPos = beamTransform.localPosition;
            if (laserLight == null) laserLight = GetComponentInChildren<Light>();
        }

        private void Update()
        {
            if (hasTriggeredFatalCrash) return;

            if (isSweeping && beamTransform != null)
            {
                float offset = Mathf.Sin(Time.time * sweepSpeed) * sweepDistance;
                beamTransform.localPosition = initialLocalPos + sweepAxis * offset;
            }

            CheckProximityHit();
        }

        private void CheckProximityHit()
        {
            if (hasTriggeredFatalCrash) return;
            if (GameManager.Instance != null && GameManager.Instance.IsGameOver) return;

            GameObject player = GameObject.FindGameObjectWithTag("Player");
            if (player == null) return;

            Transform targetBeam = beamTransform != null ? beamTransform : transform;
            Vector3 localPos = targetBeam.InverseTransformPoint(player.transform.position);

            // Laser beam cylinder is 8.8m wide (X: -4.5 to +4.5), 0.8m high (Y: -0.7 to +0.7), 1.2m thick (Z: -1.0 to +1.0)
            if (Mathf.Abs(localPos.x) < 4.8f && Mathf.Abs(localPos.y) < 1.0f && Mathf.Abs(localPos.z) < 1.2f)
            {
                TriggerFatalHit(player);
            }
        }

        private void OnTriggerEnter(Collider other)
        {
            CheckHit(other);
        }

        private void OnTriggerStay(Collider other)
        {
            CheckHit(other);
        }

        private void CheckHit(Collider other)
        {
            if (hasTriggeredFatalCrash) return;
            if (GameManager.Instance != null && GameManager.Instance.IsGameOver) return;

            PlayerController pc = other.GetComponent<PlayerController>();
            if (pc == null) pc = other.GetComponentInParent<PlayerController>();

            if (pc != null || other.CompareTag("Player") || other.GetComponent<PlayerFlightController>() != null || other.GetComponentInParent<PlayerFlightController>() != null)
            {
                TriggerFatalHit(other.gameObject);
            }
        }

        public void TriggerFatalHit(GameObject playerObj)
        {
            if (hasTriggeredFatalCrash) return;
            hasTriggeredFatalCrash = true;

            PlayerFlightController pfc = playerObj.GetComponent<PlayerFlightController>();
            if (pfc == null) pfc = playerObj.GetComponentInParent<PlayerFlightController>();
            if (pfc != null) pfc.isFlightActive = false;

            PlayerController pc = playerObj.GetComponent<PlayerController>();
            if (pc == null) pc = playerObj.GetComponentInParent<PlayerController>();
            if (pc != null)
            {
                pc.TakeDamage(damage, Vector3.zero);
            }

            if (GameManager.Instance != null)
            {
                GameManager.Instance.UpdateHUDHealth(0, 3);
                GameManager.Instance.ShowHUDNotification("VAPORIZED BY LASER GRID! RESTARTING...");
                GameManager.Instance.TriggerGameOver();
                GameManager.Instance.StartCoroutine(LaserRestartRoutine());
            }
        }

        private static IEnumerator LaserRestartRoutine()
        {
            yield return new WaitForSecondsRealtime(1.2f);
            if (GameManager.Instance != null)
            {
                GameManager.Instance.RestartCurrentLevel();
            }
        }
    }
}

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using LostCave.Audio;
using LostCave.Managers;
using LostCave.Camera;
using LostCave.Interaction;

namespace LostCave.Player
{
    public class RollerCoasterCart : MonoBehaviour
    {
        public static RollerCoasterCart Instance { get; private set; }

        [Header("Track Path")]
        public List<Vector3> trackWaypoints = new List<Vector3>();
        public int currentWaypointIndex = 0;
        public float arrivalThreshold = 2.0f;

        [Header("Speeds")]
        public float baseSpeed = 16.0f;
        public float downhillBoost = 8.0f;
        public float playerBoost = 6.0f;
        public float turnBankingSpeed = 12.0f;

        [Header("Cart State")]
        public bool isCoasterActive = true;
        public bool isJumping = false;
        public bool isDucking = false;
        public float currentLean = 0f; // -1 = Left, +1 = Right
        public float leanOffsetDistance = 1.3f;
        public float leanAngle = 28.0f;

        [Header("Jump Settings")]
        public float jumpForce = 11.0f;
        public float jumpGravity = -28.0f;
        private float jumpYVelocity = 0f;
        public float currentJumpYOffset = 0f;

        [Header("Occupant / Passenger")]
        public Transform passengerSeat;
        public GameObject playerObject;
        public Transform cartBodyVisual;

        [Header("Audio")]
        public AudioClip railRumbleClip;
        public AudioClip jumpThrusterClip;
        public AudioClip screechBrakeClip;
        public AudioClip hitClip;

        private AudioSource railAudioSource;
        private float currentSpeed;
        private Vector3 visualOffset = Vector3.zero;

        private void Awake()
        {
            Instance = this;
            currentSpeed = baseSpeed;
        }

        private void Start()
        {
            if (playerObject == null)
            {
                playerObject = GameObject.FindGameObjectWithTag("Player");
            }

            if (playerObject != null)
            {
                // Mount player in cart
                PlayerController pc = playerObject.GetComponent<PlayerController>();
                if (pc != null) pc.enabled = false;

                CharacterController cc = playerObject.GetComponent<CharacterController>();
                if (cc != null) cc.enabled = false;

                playerObject.transform.SetParent(passengerSeat != null ? passengerSeat : transform);
                // Seat player in the cart
                playerObject.transform.localPosition = Vector3.zero;
                playerObject.transform.localRotation = Quaternion.identity;

                ExplorerVisualRig rig = playerObject.GetComponentInChildren<ExplorerVisualRig>();
                if (rig != null)
                {
                    // Pose arms forward holding the safety rail
                    if (rig.leftArm != null) rig.leftArm.localRotation = Quaternion.Euler(45f, 15f, 0f);
                    if (rig.rightArm != null) rig.rightArm.localRotation = Quaternion.Euler(45f, -15f, 0f);
                    // Pose legs seated forward inside the cart
                    if (rig.leftLeg != null) rig.leftLeg.localRotation = Quaternion.Euler(-70f, 5f, 0f);
                    if (rig.rightLeg != null) rig.rightLeg.localRotation = Quaternion.Euler(-70f, -5f, 0f);
                }
            }

            // Setup Rail Sound
            railAudioSource = gameObject.AddComponent<AudioSource>();
            railAudioSource.loop = true;
            railAudioSource.volume = 0.4f;
            railAudioSource.spatialBlend = 0.2f;
            if (railRumbleClip != null)
            {
                railAudioSource.clip = railRumbleClip;
                railAudioSource.Play();
            }
            else if (AudioManager.Instance != null && AudioManager.Instance.defaultCaveAmbience != null)
            {
                railAudioSource.clip = AudioManager.Instance.defaultCaveAmbience;
                railAudioSource.pitch = 1.3f;
                railAudioSource.Play();
            }

            if (ThirdPersonCamera.Instance != null)
            {
                ThirdPersonCamera.Instance.target = transform;
                ThirdPersonCamera.Instance.defaultDistance = 5.0f;
                ThirdPersonCamera.Instance.targetOffset = new Vector3(0f, 1.2f, 0f);
            }
        }

        private void Update()
        {
            if (GameManager.Instance != null && GameManager.Instance.IsGameOver)
            {
                if (Input.GetKeyDown(KeyCode.R) || Input.GetKeyDown(KeyCode.Space) || Input.GetKeyDown(KeyCode.Return))
                {
                    GameManager.Instance.RestartCurrentLevel();
                }
                return;
            }

            if (!isCoasterActive) return;
            if (GameManager.Instance != null && GameManager.Instance.IsGamePaused) return;

            HandleCoasterInput();
            HandleCoasterMovement();
            HandleJumpPhysics();
            UpdateVisuals();
        }

        private void HandleCoasterInput()
        {
            // Lean Left / Right
            float targetLean = Input.GetAxisRaw("Horizontal"); // A/D or Left/Right
            currentLean = Mathf.Lerp(currentLean, targetLean, Time.deltaTime * turnBankingSpeed);

            // Duck & Brake under low arches
            isDucking = Input.GetKey(KeyCode.S) || Input.GetKey(KeyCode.DownArrow);

            // Jump over broken rail gaps
            if (Input.GetKeyDown(KeyCode.Space) && !isJumping)
            {
                isJumping = true;
                jumpYVelocity = jumpForce;
                if (jumpThrusterClip != null && AudioManager.Instance != null)
                {
                    AudioManager.Instance.PlaySFX(jumpThrusterClip, 1.0f);
                }
            }

            // Boost or Brake
            bool isBoosting = Input.GetKey(KeyCode.LeftShift) || Input.GetKey(KeyCode.RightShift) || Input.GetKey(KeyCode.W);
            float targetSpeed = isDucking ? 4.0f : (baseSpeed + (isBoosting ? playerBoost : 0f));

            // Downhill gravity acceleration
            if (trackWaypoints.Count > currentWaypointIndex)
            {
                Vector3 toTarget = trackWaypoints[currentWaypointIndex] - transform.position;
                if (toTarget.y < -0.5f)
                {
                    targetSpeed += downhillBoost;
                }
            }

            currentSpeed = Mathf.Lerp(currentSpeed, targetSpeed, Time.deltaTime * 3.0f);

            if (railAudioSource != null)
            {
                railAudioSource.pitch = Mathf.Lerp(0.8f, 1.5f, currentSpeed / (baseSpeed + playerBoost + downhillBoost));
            }
        }

        private void HandleCoasterMovement()
        {
            if (trackWaypoints.Count == 0 || currentWaypointIndex >= trackWaypoints.Count)
            {
                // Reached destination!
                OnReachedEnd();
                return;
            }

            Vector3 targetPos = trackWaypoints[currentWaypointIndex];
            Vector3 toTarget = targetPos - transform.position;
            toTarget.y = 0f; // Calculate horizontal distance
            float dist = Vector3.Distance(transform.position, targetPos);

            if (dist < arrivalThreshold)
            {
                currentWaypointIndex++;
                if (currentWaypointIndex >= trackWaypoints.Count)
                {
                    OnReachedEnd();
                    return;
                }
                targetPos = trackWaypoints[currentWaypointIndex];
            }

            // Move along rails
            Vector3 moveDir = (targetPos - transform.position).normalized;
            transform.position += moveDir * currentSpeed * Time.deltaTime;

            // Rotate towards target node
            if (moveDir.sqrMagnitude > 0.001f)
            {
                Quaternion targetRot = Quaternion.LookRotation(moveDir, Vector3.up);
                // Apply banking into rotation
                float bank = -currentLean * leanAngle;
                targetRot *= Quaternion.Euler(0f, 0f, bank);
                transform.rotation = Quaternion.Slerp(transform.rotation, targetRot, Time.deltaTime * 10f);
            }
        }

        private void HandleJumpPhysics()
        {
            if (isJumping)
            {
                jumpYVelocity += jumpGravity * Time.deltaTime;
                currentJumpYOffset += jumpYVelocity * Time.deltaTime;

                if (currentJumpYOffset <= 0f)
                {
                    currentJumpYOffset = 0f;
                    isJumping = false;
                    jumpYVelocity = 0f;
                }
            }
        }

        private void UpdateVisuals()
        {
            // Apply lean horizontal offset and duck vertical offset to cart visual
            Vector3 leanOffset = transform.right * (currentLean * leanOffsetDistance);
            Vector3 jumpOffset = Vector3.up * currentJumpYOffset;
            Vector3 duckOffset = isDucking ? Vector3.down * 0.45f : Vector3.zero;

            if (cartBodyVisual != null)
            {
                cartBodyVisual.localPosition = leanOffset + jumpOffset + duckOffset;
                cartBodyVisual.localRotation = Quaternion.Euler(0f, 0f, -currentLean * leanAngle);
            }

            // Duck passenger: seat lowers by 0.65m so head ducks safely inside the cart walls
            if (passengerSeat != null)
            {
                passengerSeat.localPosition = new Vector3(0f, isDucking ? -0.40f : 0.25f, -0.05f);
            }
        }

        public void TriggerDamage(int damage)
        {
            TriggerCrash(true);
        }

        public void TriggerCrash(bool fatal = true)
        {
            if (!isCoasterActive) return;
            isCoasterActive = false;
            currentSpeed = 0f;

            if (railAudioSource != null) railAudioSource.Stop();
            if (hitClip != null && AudioManager.Instance != null)
            {
                AudioManager.Instance.PlaySFX(hitClip, 1.0f);
            }

            if (ThirdPersonCamera.Instance != null)
            {
                ThirdPersonCamera.Instance.TriggerShake(0.8f, 0.6f);
            }

            if (playerObject != null)
            {
                PlayerController pc = playerObject.GetComponent<PlayerController>();
                if (pc != null)
                {
                    pc.TakeDamage(999, Vector3.zero); // Instantly fatal: health drops to 0 and calls Die()
                }
            }

            if (GameManager.Instance != null)
            {
                GameManager.Instance.TriggerGameOver();
            }

            StartCoroutine(CrashRestartRoutine());
        }

        private IEnumerator CrashRestartRoutine()
        {
            // Allow 1.2 seconds for the dramatic crash sound and game over to show, then restart
            yield return new WaitForSecondsRealtime(1.2f);
            if (GameManager.Instance != null)
            {
                GameManager.Instance.RestartCurrentLevel();
            }
        }

        private void OnReachedEnd()
        {
            isCoasterActive = false;
            currentSpeed = 0f;

            if (railAudioSource != null)
            {
                railAudioSource.Stop();
            }

            if (screechBrakeClip != null && AudioManager.Instance != null)
            {
                AudioManager.Instance.PlaySFX(screechBrakeClip, 1.0f);
            }

            // Allow player out and switch camera back to player
            if (playerObject != null)
            {
                playerObject.transform.SetParent(null);
                CharacterController cc = playerObject.GetComponent<CharacterController>();
                if (cc != null) cc.enabled = true;

                PlayerController pc = playerObject.GetComponent<PlayerController>();
                if (pc != null) pc.enabled = true;

                if (ThirdPersonCamera.Instance != null)
                {
                    ThirdPersonCamera.Instance.target = playerObject.transform;
                    ThirdPersonCamera.Instance.defaultDistance = 4.5f;
                }
            }

            if (GameManager.Instance != null)
            {
                GameManager.Instance.ShowHUDNotification("YOU HAVE ARRIVED AT THE LOST TREASURE!");
                GameManager.Instance.UpdateHUDObjective("Approach the Golden Treasure Chest and press [E] to claim your prize!");
            }
        }
    }
}

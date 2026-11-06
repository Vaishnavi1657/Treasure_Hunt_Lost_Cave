using System.Collections;
using UnityEngine;
using LostCave.Audio;
using LostCave.Managers;
using LostCave.Camera;

namespace LostCave.Player
{
    [RequireComponent(typeof(PlayerController))]
    public class PlayerFlightController : MonoBehaviour
    {
        public static PlayerFlightController Instance { get; private set; }

        [Header("Flight Speeds")]
        public float normalFlightSpeed = 6.5f;
        public float boostSpeed = 10.0f;
        public float brakeSpeed = 3.5f;
        public float idleSpeed = 4.0f;
        public float yawSpeed = 65.0f;
        public float rollSpeed = 60.0f;
        public float liftSpeed = 5.0f;
        public float descendSpeed = 4.0f;

        // Backward compatibility
        public float cruiseSpeed
        {
            get => normalFlightSpeed;
            set => normalFlightSpeed = value;
        }

        [Header("Altitude & Aerodynamic Float")]
        public float defaultFlightAltitude = 4.5f;
        public float minFlightAltitude = 2.0f;
        public float maxFlightAltitude = 8.5f;
        public float altitudeStabilityForce = 4.5f;
        public float currentTargetAltitude = 4.5f;

        [Header("Flight Limits")]
        public float maxPitchAngle = 25.0f;
        public float maxRollAngle = 30.0f;

        [Header("Wing Visuals")]
        public Transform leftWing;
        public Transform rightWing;
        public Light flightLight;

        [Header("Audio")]
        public AudioClip flightLoopClip;
        public AudioClip boostClip;
        public AudioClip ringCollectClip;
        public AudioClip crashClip;

        // State
        public bool isFlightActive = true;
        public bool isBoosting = false;
        private float currentSpeed;
        private float currentPitch = 0f;
        private float currentRoll = 0f;
        private CharacterController charController;
        private PlayerController playerController;
        private AudioSource flightAudioSource;

        private void Awake()
        {
            Instance = this;
            charController = GetComponent<CharacterController>();
            playerController = GetComponent<PlayerController>();
            currentSpeed = idleSpeed;
        }

        private void Start()
        {
            // Set initial cruising altitude
            currentTargetAltitude = Mathf.Clamp(transform.position.y > 1.5f ? transform.position.y : defaultFlightAltitude, minFlightAltitude, maxFlightAltitude);

            // Temporarily disable standard ground gravity logic while in flight mode
            if (playerController != null)
            {
                playerController.enabled = false;
            }

            // Setup Flight Audio
            flightAudioSource = gameObject.AddComponent<AudioSource>();
            flightAudioSource.loop = true;
            flightAudioSource.volume = 0.35f;
            flightAudioSource.spatialBlend = 0f;
            if (AudioManager.Instance != null && AudioManager.Instance.defaultCaveAmbience != null)
            {
                flightAudioSource.clip = AudioManager.Instance.defaultCaveAmbience;
                flightAudioSource.Play();
            }

            CreateFlightWings();
        }

        private void CreateFlightWings()
        {
            if (leftWing != null && rightWing != null) return;

            Transform spine = transform.Find("VisualRig/Hips/Torso");
            if (spine == null) spine = transform;

            // Material for wings
            Material wingMat = Resources.Load<Material>("Mat_CrystalTurquoise");
            if (wingMat == null)
            {
                Renderer[] rends = GetComponentsInChildren<Renderer>();
                if (rends.Length > 0) wingMat = rends[0].sharedMaterial;
            }

            // Left Wing
            GameObject lWingObj = GameObject.CreatePrimitive(PrimitiveType.Cube);
            lWingObj.name = "CrystalFlightWing_L";
            lWingObj.transform.SetParent(spine);
            lWingObj.transform.localPosition = new Vector3(-0.45f, 0.2f, -0.2f);
            lWingObj.transform.localRotation = Quaternion.Euler(15f, -25f, -20f);
            lWingObj.transform.localScale = new Vector3(1.4f, 0.06f, 0.45f);
            if (lWingObj.GetComponent<Collider>()) DestroyImmediate(lWingObj.GetComponent<Collider>());
            if (wingMat != null) lWingObj.GetComponent<Renderer>().sharedMaterial = wingMat;
            leftWing = lWingObj.transform;

            // Right Wing
            GameObject rWingObj = GameObject.CreatePrimitive(PrimitiveType.Cube);
            rWingObj.name = "CrystalFlightWing_R";
            rWingObj.transform.SetParent(spine);
            rWingObj.transform.localPosition = new Vector3(0.45f, 0.2f, -0.2f);
            rWingObj.transform.localRotation = Quaternion.Euler(15f, 25f, 20f);
            rWingObj.transform.localScale = new Vector3(1.4f, 0.06f, 0.45f);
            if (rWingObj.GetComponent<Collider>()) DestroyImmediate(rWingObj.GetComponent<Collider>());
            if (wingMat != null) rWingObj.GetComponent<Renderer>().sharedMaterial = wingMat;
            rightWing = rWingObj.transform;

            // Flight Glow Light
            GameObject lightObj = new GameObject("FlightGlowLight");
            lightObj.transform.SetParent(spine);
            lightObj.transform.localPosition = new Vector3(0f, 0.5f, 0f);
            flightLight = lightObj.AddComponent<Light>();
            flightLight.type = LightType.Point;
            flightLight.color = new Color(0.1f, 0.95f, 1.0f);
            flightLight.range = 8f;
            flightLight.intensity = 3.2f;
            flightLight.shadows = LightShadows.None;
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

            if (!isFlightActive) return;
            if (GameManager.Instance != null && GameManager.Instance.IsGamePaused) return;

            if (transform.position.y < -10.0f)
            {
                TriggerFlightCrash("FELL INTO THE ABYSS! RESTARTING...");
                return;
            }

            HandleFlightInput();
            AnimateWings();
        }

        private void HandleFlightInput()
        {
            // Forward Speed calculation: Comfortable cruise, brake on S, boost on Shift
            bool isPressingForward = Input.GetKey(KeyCode.W) || Input.GetKey(KeyCode.UpArrow);
            bool isBraking = Input.GetKey(KeyCode.S) || Input.GetKey(KeyCode.DownArrow);
            isBoosting = Input.GetKey(KeyCode.LeftShift) || Input.GetKey(KeyCode.RightShift);

            float targetSpeed = normalFlightSpeed;
            if (isBraking)
            {
                targetSpeed = brakeSpeed;
            }
            else if (isBoosting)
            {
                targetSpeed = boostSpeed;
            }
            else if (!isPressingForward)
            {
                targetSpeed = idleSpeed;
            }

            currentSpeed = Mathf.Lerp(currentSpeed, targetSpeed, Time.deltaTime * 3.5f);

            // Responsive Yaw Steering & Roll Banking
            float yawInput = Input.GetAxis("Horizontal"); // A/D or Left/Right
            transform.Rotate(Vector3.up, yawInput * yawSpeed * Time.deltaTime, Space.World);

            // Vertical Climb / Descend input: Space = Ascend, C / Left Ctrl = Descend
            float climbInput = 0f;
            if (Input.GetKey(KeyCode.Space)) climbInput = 1f;
            if (Input.GetKey(KeyCode.C) || Input.GetKey(KeyCode.LeftControl)) climbInput = -1f;

            if (Mathf.Abs(climbInput) > 0.05f)
            {
                currentTargetAltitude = Mathf.Clamp(currentTargetAltitude + climbInput * liftSpeed * Time.deltaTime, minFlightAltitude, maxFlightAltitude);
            }
            else
            {
                // Aerodynamic ground cushion: if flying near the floor, smoothly float back up to cruising altitude
                if (currentTargetAltitude < 3.8f)
                {
                    currentTargetAltitude = Mathf.MoveTowards(currentTargetAltitude, defaultFlightAltitude, Time.deltaTime * 2.5f);
                }
            }

            // Aerodynamic lift counteracting gravity and holding altitude stably in mid-air
            float altitudeDelta = currentTargetAltitude - transform.position.y;
            float verticalVelocity = altitudeDelta * altitudeStabilityForce;

            // Gentle organic hover bobbing
            float hoverBob = Mathf.Sin(Time.time * 2.5f) * 0.12f;

            // Strictly horizontal forward direction so forward flight NEVER pulls down into the floor
            Vector3 forwardHeading = Vector3.ProjectOnPlane(transform.forward, Vector3.up).normalized;
            if (forwardHeading.sqrMagnitude < 0.001f) forwardHeading = transform.forward;

            Vector3 forwardMove = forwardHeading * currentSpeed;
            Vector3 verticalMove = Vector3.up * (verticalVelocity + hoverBob);
            Vector3 finalMove = (forwardMove + verticalMove) * Time.deltaTime;

            if (charController != null && charController.enabled)
            {
                charController.Move(finalMove);
            }
            else
            {
                transform.position += finalMove;
            }

            // Visual pitch & roll: 
            // Nose gently tilts up when ascending, tilts down when diving, or stays level when cruising
            float targetPitch = -climbInput * 16.0f;
            currentPitch = Mathf.Lerp(currentPitch, targetPitch, Time.deltaTime * 4.0f);

            float targetRoll = -yawInput * maxRollAngle;
            currentRoll = Mathf.Lerp(currentRoll, targetRoll, Time.deltaTime * rollSpeed * 0.1f);

            Quaternion flightRotation = Quaternion.Euler(currentPitch, transform.eulerAngles.y, currentRoll);
            transform.rotation = Quaternion.Slerp(transform.rotation, flightRotation, Time.deltaTime * 8f);

            // Audio pitch modulation with speed
            if (flightAudioSource != null)
            {
                flightAudioSource.pitch = Mathf.Lerp(0.85f, 1.25f, currentSpeed / boostSpeed);
            }
        }

        private void AnimateWings()
        {
            if (leftWing != null && rightWing != null)
            {
                float flap = Mathf.Sin(Time.time * (isBoosting ? 14f : 8f)) * 10f;
                leftWing.localRotation = Quaternion.Euler(15f + flap, -25f, -20f - flap);
                rightWing.localRotation = Quaternion.Euler(15f + flap, 25f, 20f + flap);
            }
        }

        public void TriggerRingBoost(float boostMultiplier = 1.25f, float duration = 1.8f)
        {
            StartCoroutine(RingBoostRoutine(boostMultiplier, duration));
        }

        private IEnumerator RingBoostRoutine(float multiplier, float duration)
        {
            float origNormal = normalFlightSpeed;
            float origBoost = boostSpeed;

            normalFlightSpeed = Mathf.Min(origNormal * multiplier, 9.5f);
            boostSpeed = Mathf.Min(origBoost * multiplier, 12.0f);

            if (flightLight != null) flightLight.intensity = 5.0f;

            yield return new WaitForSeconds(duration);

            normalFlightSpeed = origNormal;
            boostSpeed = origBoost;
            if (flightLight != null) flightLight.intensity = 3.2f;
        }

        private void OnControllerColliderHit(ControllerColliderHit hit)
        {
            if (!isFlightActive) return;
            if (GameManager.Instance != null && GameManager.Instance.IsGameOver) return;

            // Ignore rings and exits
            if (hit.gameObject.GetComponent<LostCave.Obstacles.AerialEnergyRing>() != null || 
                hit.gameObject.GetComponentInParent<LostCave.Obstacles.AerialEnergyRing>() != null ||
                hit.gameObject.GetComponent<LostCave.Managers.LevelExit>() != null ||
                hit.gameObject.GetComponentInParent<LostCave.Managers.LevelExit>() != null)
            {
                return;
            }

            // Any obstacle, wall, ceiling, pylon, or rock impact in flight is fatal!
            TriggerFlightCrash("CRASHED INTO TUNNEL OBSTACLE! RESTARTING...");
        }

        private void OnTriggerEnter(Collider other)
        {
            if (!isFlightActive) return;
            if (GameManager.Instance != null && GameManager.Instance.IsGameOver) return;

            if (other.GetComponent<LostCave.Obstacles.CrystalLaserBarrier>() != null || 
                other.GetComponentInParent<LostCave.Obstacles.CrystalLaserBarrier>() != null ||
                other.name.Contains("Laser") || other.name.Contains("Barrier"))
            {
                TriggerFlightCrash("VAPORIZED BY LASER GRID! RESTARTING...");
            }
        }

        public void TriggerFlightCrash(string failureMessage)
        {
            if (!isFlightActive) return;
            isFlightActive = false;
            currentSpeed = 0f;

            if (flightAudioSource != null) flightAudioSource.Stop();
            if (crashClip != null && AudioManager.Instance != null)
            {
                AudioManager.Instance.PlaySFX(crashClip, 1.0f);
            }

            if (ThirdPersonCamera.Instance != null)
            {
                ThirdPersonCamera.Instance.TriggerShake(0.8f, 0.5f);
            }

            if (playerController != null)
            {
                playerController.TakeDamage(999, Vector3.zero);
            }

            if (GameManager.Instance != null)
            {
                GameManager.Instance.UpdateHUDHealth(0, 3);
                GameManager.Instance.ShowHUDNotification(failureMessage);
                GameManager.Instance.TriggerGameOver();
                GameManager.Instance.StartCoroutine(FlightRestartRoutine());
            }
        }

        private static IEnumerator FlightRestartRoutine()
        {
            yield return new WaitForSecondsRealtime(1.2f);
            if (GameManager.Instance != null)
            {
                GameManager.Instance.RestartCurrentLevel();
            }
        }

        private void OnDestroy()
        {
            if (playerController != null)
            {
                playerController.enabled = true;
            }
        }
    }
}

using System.Collections;
using UnityEngine;
using LostCave.Audio;
using LostCave.Managers;
using LostCave.Camera;

namespace LostCave.Player
{
    [RequireComponent(typeof(CharacterController))]
    public class PlayerController : MonoBehaviour
    {
        public static PlayerController Instance { get; private set; }

        [Header("Movement Speeds")]
        public float walkSpeed = 4.5f;
        public float runSpeed = 7.5f;
        public float rotationSpeed = 12f;

        [Header("Jumping & Gravity")]
        public float jumpHeight = 1.75f;
        public float gravity = -24f;
        public float groundCheckDistance = 0.25f;
        public LayerMask groundLayers = ~0;

        [Header("Player Stats")]
        public int maxHealth = 3;
        public int currentHealth;
        public bool isInvulnerable = false;
        public float invulnerabilityDuration = 1.5f;

        [Header("Audio Clips")]
        public AudioClip footstepWalkClip;
        public AudioClip footstepRunClip;
        public AudioClip jumpClip;
        public AudioClip landClip;
        public AudioClip hurtClip;

        [Header("References")]
        public ExplorerVisualRig visualRig;
        public Transform cameraTransform;

        // Internal State
        private CharacterController controller;
        private Vector3 verticalVelocity = Vector3.zero;
        private Vector3 knockbackVelocity = Vector3.zero;
        private bool isGrounded;
        private bool wasGrounded;
        private float stepTimer = 0f;
        private Renderer[] childRenderers;

        private void Awake()
        {
            Instance = this;
            controller = GetComponent<CharacterController>();
            currentHealth = maxHealth;
            childRenderers = GetComponentsInChildren<Renderer>();
        }

        private void Start()
        {
            if (cameraTransform == null && UnityEngine.Camera.main != null)
            {
                cameraTransform = UnityEngine.Camera.main.transform;
            }

            if (visualRig == null)
            {
                visualRig = GetComponentInChildren<ExplorerVisualRig>();
            }
        }

        private void Update()
        {
            if (GameManager.Instance != null && (GameManager.Instance.IsGamePaused || GameManager.Instance.IsGameOver)) return;

            // Fatal fall check: falling into the abyss or outside the cave
            if (transform.position.y < -4.0f && currentHealth > 0)
            {
                TakeDamage(maxHealth, Vector3.zero);
                return;
            }

            CheckGroundStatus();
            HandleMovement();
            HandleJumpAndGravity();
            HandleKnockback();
            UpdateVisualsAndAudio();
        }

        private void CheckGroundStatus()
        {
            wasGrounded = isGrounded;
            // Ground check combining controller isGrounded and raycast for precision
            isGrounded = controller.isGrounded;
            if (!isGrounded)
            {
                RaycastHit hit;
                Vector3 rayOrigin = transform.position + Vector3.up * 0.1f;
                if (Physics.Raycast(rayOrigin, Vector3.down, out hit, groundCheckDistance + 0.1f, groundLayers, QueryTriggerInteraction.Ignore))
                {
                    isGrounded = true;
                }
            }

            if (isGrounded && verticalVelocity.y < 0)
            {
                verticalVelocity.y = -2f; // maintain solid ground contact
            }

            // Landing sound
            if (!wasGrounded && isGrounded && verticalVelocity.y < -4f)
            {
                PlayAudio(landClip, 0.8f);
            }
        }

        private void HandleMovement()
        {
            float horizontal = Input.GetAxisRaw("Horizontal");
            float vertical = Input.GetAxisRaw("Vertical");
            Vector3 inputDir = new Vector3(horizontal, 0f, vertical).normalized;

            bool isRunning = Input.GetKey(KeyCode.LeftShift) || Input.GetKey(KeyCode.RightShift);
            float targetSpeed = isRunning ? runSpeed : walkSpeed;

            Vector3 moveDirection = Vector3.zero;

            if (inputDir.magnitude > 0.05f)
            {
                Vector3 camForward = Vector3.forward;
                Vector3 camRight = Vector3.right;

                if (cameraTransform != null)
                {
                    camForward = cameraTransform.forward;
                    camRight = cameraTransform.right;
                    camForward.y = 0f;
                    camRight.y = 0f;
                    camForward.Normalize();
                    camRight.Normalize();
                }

                moveDirection = (camForward * inputDir.z + camRight * inputDir.x).normalized;

                // Rotate player towards movement direction
                Quaternion targetRotation = Quaternion.LookRotation(moveDirection, Vector3.up);
                transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, Time.deltaTime * rotationSpeed);
            }

            // Move character controller
            Vector3 finalMove = moveDirection * targetSpeed;
            controller.Move(finalMove * Time.deltaTime);

            // Footstep sounds
            if (isGrounded && inputDir.magnitude > 0.05f)
            {
                stepTimer -= Time.deltaTime;
                float interval = isRunning ? 0.32f : 0.48f;
                if (stepTimer <= 0f)
                {
                    stepTimer = interval;
                    PlayAudio(isRunning ? footstepRunClip : footstepWalkClip, isRunning ? 0.85f : 0.6f);
                }
            }
            else
            {
                stepTimer = 0.1f;
            }
        }

        private void HandleJumpAndGravity()
        {
            if (isGrounded && Input.GetButtonDown("Jump"))
            {
                verticalVelocity.y = Mathf.Sqrt(jumpHeight * -2f * gravity);
                PlayAudio(jumpClip, 0.9f);
            }

            // Apply gravity
            verticalVelocity.y += gravity * Time.deltaTime;
            controller.Move(verticalVelocity * Time.deltaTime);
        }

        private void HandleKnockback()
        {
            if (knockbackVelocity.magnitude > 0.1f)
            {
                controller.Move(knockbackVelocity * Time.deltaTime);
                knockbackVelocity = Vector3.Lerp(knockbackVelocity, Vector3.zero, Time.deltaTime * 6f);
            }
        }

        private void UpdateVisualsAndAudio()
        {
            if (visualRig != null)
            {
                Vector3 horizontalVel = controller.velocity;
                horizontalVel.y = 0f;
                float speedPercent = horizontalVel.magnitude / runSpeed;
                visualRig.Animate(speedPercent, isGrounded, !isGrounded && verticalVelocity.y > 0, Input.GetAxis("Horizontal"));
            }
        }

        public void TakeDamage(int damage, Vector3 knockback)
        {
            if (isInvulnerable) return;

            currentHealth -= damage;
            PlayAudio(hurtClip, 1.0f);

            if (ThirdPersonCamera.Instance != null)
            {
                ThirdPersonCamera.Instance.TriggerShake(0.35f, 0.3f);
            }

            knockbackVelocity = knockback;

            if (currentHealth <= 0)
            {
                currentHealth = 0;
                if (GameManager.Instance != null)
                {
                    GameManager.Instance.UpdateHUDHealth(0, maxHealth);
                }
                Die();
            }
            else
            {
                if (GameManager.Instance != null)
                {
                    GameManager.Instance.UpdateHUDHealth(currentHealth, maxHealth);
                }
                StartCoroutine(InvulnerabilityRoutine());
            }
        }

        private IEnumerator InvulnerabilityRoutine()
        {
            isInvulnerable = true;
            float elapsed = 0f;
            bool visible = true;

            while (elapsed < invulnerabilityDuration)
            {
                elapsed += 0.12f;
                visible = !visible;
                SetMeshVisibility(visible);
                yield return new WaitForSeconds(0.12f);
            }

            SetMeshVisibility(true);
            isInvulnerable = false;
        }

        private void SetMeshVisibility(bool visible)
        {
            if (childRenderers == null) return;
            foreach (var r in childRenderers)
            {
                if (r != null) r.enabled = visible;
            }
        }

        public void Die()
        {
            currentHealth = 0;
            verticalVelocity = Vector3.zero;
            knockbackVelocity = Vector3.zero;

            if (GameManager.Instance != null)
            {
                GameManager.Instance.UpdateHUDHealth(0, maxHealth);
                GameManager.Instance.TriggerGameOver();
            }

            StartCoroutine(AutoRestartRoutine());
        }

        private System.Collections.IEnumerator AutoRestartRoutine()
        {
            yield return new WaitForSecondsRealtime(1.3f);
            if (GameManager.Instance != null && GameManager.Instance.IsGameOver)
            {
                GameManager.Instance.RestartCurrentLevel();
            }
        }

        public void RespawnAt(Vector3 respawnPosition, Quaternion respawnRotation)
        {
            controller.enabled = false;
            transform.position = respawnPosition;
            transform.rotation = respawnRotation;
            controller.enabled = true;

            verticalVelocity = Vector3.zero;
            knockbackVelocity = Vector3.zero;
            currentHealth = maxHealth;
            isInvulnerable = false;
            SetMeshVisibility(true);

            if (GameManager.Instance != null)
            {
                GameManager.Instance.UpdateHUDHealth(currentHealth, maxHealth);
            }
        }

        private void PlayAudio(AudioClip clip, float volume = 1f)
        {
            if (clip != null && AudioManager.Instance != null)
            {
                AudioManager.Instance.PlaySFX(clip, volume);
            }
        }
    }
}

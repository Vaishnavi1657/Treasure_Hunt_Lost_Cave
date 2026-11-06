using UnityEngine;

namespace LostCave.Camera
{
    public class ThirdPersonCamera : MonoBehaviour
    {
        public static ThirdPersonCamera Instance { get; private set; }

        [Header("Target Follow")]
        public Transform target;
        public Vector3 targetOffset = new Vector3(0f, 1.5f, 0f);

        [Header("Camera Distance")]
        public float defaultDistance = 4.5f;
        public float minDistance = 1.0f;
        public float maxDistance = 6.0f;
        public float zoomSpeed = 2.0f;
        public float smoothFollowSpeed = 12.0f;

        [Header("Orbit Controls")]
        public float mouseSensitivityX = 3.0f;
        public float mouseSensitivityY = 2.5f;
        public float minPitch = -25f;
        public float maxPitch = 65f;
        public bool invertY = false;

        [Header("Obstacle Collision")]
        public LayerMask collisionLayers = ~0; // all layers except ignore
        public float sphereRadius = 0.25f;
        public float collisionBuffer = 0.15f;

        private float currentYaw = 0f;
        private float currentPitch = 15f;
        private float currentDistance;
        private float targetDistance;

        [Header("Screen Shake")]
        private float shakeTimeRemaining = 0f;
        private float shakeIntensity = 0f;
        private Vector3 shakeOffset = Vector3.zero;

        private void Awake()
        {
            Instance = this;
            currentDistance = defaultDistance;
            targetDistance = defaultDistance;
        }

        private void Start()
        {
            if (target == null)
            {
                GameObject player = GameObject.FindGameObjectWithTag("Player");
                if (player != null) target = player.transform;
            }

            if (target != null)
            {
                currentYaw = target.eulerAngles.y;
                currentPitch = 14f;
            }

            SetCursorState(true);
        }

        private void Update()
        {
            bool isCartTarget = target != null && (target.GetComponent<LostCave.Player.RollerCoasterCart>() != null || target.GetComponentInParent<LostCave.Player.RollerCoasterCart>() != null);

            if (isCartTarget)
            {
                // In roller coaster mode, strictly lock the camera directly behind the cart's forward heading!
                currentYaw = Mathf.LerpAngle(currentYaw, target.eulerAngles.y, Time.deltaTime * 12.0f);
                currentPitch = Mathf.Lerp(currentPitch, 14.0f, Time.deltaTime * 6.0f);
                targetDistance = 4.8f;
                targetOffset = new Vector3(0f, 1.4f, 0f);
            }
            else if (Cursor.lockState == CursorLockMode.Locked)
            {
                float mouseX = Input.GetAxis("Mouse X") * mouseSensitivityX;
                float mouseY = Input.GetAxis("Mouse Y") * mouseSensitivityY * (invertY ? 1f : -1f);

                currentYaw += mouseX;
                currentPitch = Mathf.Clamp(currentPitch - mouseY, minPitch, maxPitch);

                // Smoothly auto-align camera yaw behind the player when running around corners (Temple Run feel)
                if (Mathf.Abs(mouseX) < 0.05f && target != null)
                {
                    float moveH = Input.GetAxisRaw("Horizontal");
                    float moveV = Input.GetAxisRaw("Vertical");
                    if (Mathf.Abs(moveH) > 0.1f || Mathf.Abs(moveV) > 0.1f)
                    {
                        currentYaw = Mathf.LerpAngle(currentYaw, target.eulerAngles.y, Time.deltaTime * 3.5f);
                    }
                }

                float scroll = Input.GetAxis("Mouse ScrollWheel");
                if (Mathf.Abs(scroll) > 0.01f)
                {
                    targetDistance = Mathf.Clamp(targetDistance - scroll * zoomSpeed, minDistance, maxDistance);
                }
            }

            // Update screen shake
            if (shakeTimeRemaining > 0)
            {
                shakeTimeRemaining -= Time.deltaTime;
                shakeOffset = Random.insideUnitSphere * shakeIntensity;
            }
            else
            {
                shakeOffset = Vector3.zero;
            }
        }

        private void LateUpdate()
        {
            if (target == null) return;

            Vector3 focusPoint = target.position + targetOffset;
            Quaternion rotation = Quaternion.Euler(currentPitch, currentYaw, 0f);

            // Compute ideal camera position
            Vector3 desiredDirection = rotation * -Vector3.forward;
            float desiredDist = targetDistance;

            // Wall collision detection via SphereCast
            RaycastHit hit;
            if (Physics.SphereCast(focusPoint, sphereRadius, desiredDirection, out hit, targetDistance, collisionLayers, QueryTriggerInteraction.Ignore))
            {
                if (hit.transform != target && !hit.transform.IsChildOf(target))
                {
                    desiredDist = Mathf.Clamp(hit.distance - collisionBuffer, minDistance, targetDistance);
                }
            }

            currentDistance = Mathf.Lerp(currentDistance, desiredDist, Time.deltaTime * smoothFollowSpeed);
            Vector3 finalPosition = focusPoint + desiredDirection * currentDistance + shakeOffset;

            transform.position = finalPosition;
            transform.LookAt(focusPoint + Vector3.up * 0.2f);
        }

        public void TriggerShake(float intensity, float duration)
        {
            shakeIntensity = intensity;
            shakeTimeRemaining = duration;
        }

        public void SetCursorState(bool locked)
        {
            Cursor.lockState = locked ? CursorLockMode.Locked : CursorLockMode.None;
            Cursor.visible = !locked;
        }
    }
}

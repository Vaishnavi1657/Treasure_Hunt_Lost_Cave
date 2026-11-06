using UnityEngine;
using LostCave.Player;
using LostCave.Managers;

namespace LostCave.Obstacles
{
    public enum CoasterHazardType
    {
        BrokenTrackGap,     // Requires jumping (Space)
        LowHangingBeam,     // Requires ducking (S / Down)
        LeftSideHazard,     // Requires leaning Right (D)
        RightSideHazard     // Requires leaning Left (A)
    }

    public class CoasterTrackObstacle : MonoBehaviour
    {
        public CoasterHazardType hazardType;
        public int damage = 999;
        public bool isResolved = false;

        private bool hasAvoided = false;
        private bool promptShown = false;

        private void Update()
        {
            if (isResolved) return;

            RollerCoasterCart cart = RollerCoasterCart.Instance;
            if (cart == null || !cart.isCoasterActive) return;
            if (GameManager.Instance != null && GameManager.Instance.IsGameOver) return;

            float dist = Vector3.Distance(transform.position, cart.transform.position);

            // 1. APPROACH WINDOW (within 6.5 meters of obstacle)
            if (dist < 6.5f)
            {
                // Show player a warning prompt once
                if (!promptShown && !hasAvoided)
                {
                    promptShown = true;
                    ShowApproachPrompt();
                }

                // Continuously monitor for player's avoidance action
                if (!hasAvoided && CheckAvoidance(cart))
                {
                    hasAvoided = true;
                }

                // Check cart's position relative to obstacle forward direction
                Vector3 localPos = transform.InverseTransformPoint(cart.transform.position);

                // If cart has reached or passed the obstacle plane (localPos.z >= 0.2f)
                if (localPos.z >= 0.2f)
                {
                    if (CheckAvoidance(cart))
                    {
                        hasAvoided = true;
                    }

                    isResolved = true;

                    if (hasAvoided)
                    {
                        ShowSuccessNotification();
                    }
                    else
                    {
                        TriggerFailure(cart);
                    }
                }
            }
        }

        private void OnTriggerEnter(Collider other)
        {
            if (isResolved) return;

            RollerCoasterCart cart = other.GetComponent<RollerCoasterCart>();
            if (cart == null) cart = other.GetComponentInParent<RollerCoasterCart>();

            if (cart != null)
            {
                if (CheckAvoidance(cart))
                {
                    hasAvoided = true;
                }
            }
        }

        private bool CheckAvoidance(RollerCoasterCart cart)
        {
            switch (hazardType)
            {
                case CoasterHazardType.BrokenTrackGap:
                    return cart.isJumping || cart.currentJumpYOffset > 0.15f || Input.GetKey(KeyCode.Space);

                case CoasterHazardType.LowHangingBeam:
                    return cart.isDucking || Input.GetKey(KeyCode.S) || Input.GetKey(KeyCode.DownArrow);

                case CoasterHazardType.LeftSideHazard:
                    return cart.currentLean > 0.18f || Input.GetKey(KeyCode.D) || Input.GetKey(KeyCode.RightArrow);

                case CoasterHazardType.RightSideHazard:
                    return cart.currentLean < -0.18f || Input.GetKey(KeyCode.A) || Input.GetKey(KeyCode.LeftArrow);
            }
            return false;
        }

        private void ShowApproachPrompt()
        {
            if (GameManager.Instance == null) return;

            switch (hazardType)
            {
                case CoasterHazardType.BrokenTrackGap:
                    GameManager.Instance.ShowHUDNotification("TRACK GAP AHEAD! PRESS [SPACE] TO JUMP!", 1.2f);
                    break;
                case CoasterHazardType.LowHangingBeam:
                    GameManager.Instance.ShowHUDNotification("LOW ARCH AHEAD! HOLD [S] TO DUCK!", 1.2f);
                    break;
                case CoasterHazardType.LeftSideHazard:
                    GameManager.Instance.ShowHUDNotification("SPIKED MACE LEFT! HOLD [D] TO LEAN RIGHT!", 1.2f);
                    break;
                case CoasterHazardType.RightSideHazard:
                    GameManager.Instance.ShowHUDNotification("SPIKED MACE RIGHT! HOLD [A] TO LEAN LEFT!", 1.2f);
                    break;
            }
        }

        private void ShowSuccessNotification()
        {
            if (GameManager.Instance == null) return;

            switch (hazardType)
            {
                case CoasterHazardType.BrokenTrackGap:
                    GameManager.Instance.ShowHUDNotification("AWESOME TRACK JUMP!");
                    break;
                case CoasterHazardType.LowHangingBeam:
                    GameManager.Instance.ShowHUDNotification("DUCK SUCCESSFUL!");
                    break;
                case CoasterHazardType.LeftSideHazard:
                    GameManager.Instance.ShowHUDNotification("RIGHT LEAN DODGE SUCCESS!");
                    break;
                case CoasterHazardType.RightSideHazard:
                    GameManager.Instance.ShowHUDNotification("LEFT LEAN DODGE SUCCESS!");
                    break;
            }
        }

        private void TriggerFailure(RollerCoasterCart cart)
        {
            if (GameManager.Instance != null)
            {
                switch (hazardType)
                {
                    case CoasterHazardType.BrokenTrackGap:
                        GameManager.Instance.ShowHUDNotification("MISSED TRACK GAP! PRESS [SPACE] TO JUMP!");
                        break;
                    case CoasterHazardType.LowHangingBeam:
                        GameManager.Instance.ShowHUDNotification("HEAD SMASH! FAILED TO DUCK UNDER LOW ARCH!");
                        break;
                    case CoasterHazardType.LeftSideHazard:
                        GameManager.Instance.ShowHUDNotification("SMASHED BY LEFT SPIKED MACE!");
                        break;
                    case CoasterHazardType.RightSideHazard:
                        GameManager.Instance.ShowHUDNotification("SMASHED BY RIGHT SPIKED MACE!");
                        break;
                }
            }

            cart.TriggerCrash(true);
        }
    }
}

using UnityEngine;

namespace LostCave.Player
{
    public class ExplorerVisualRig : MonoBehaviour
    {
        [Header("Procedural Rig Components")]
        public Transform hips;
        public Transform spine;
        public Transform head;
        public Transform leftArm;
        public Transform rightArm;
        public Transform leftLeg;
        public Transform rightLeg;
        public Transform backpack;

        [Header("Animation Parameters")]
        public float walkCycleSpeed = 8f;
        public float runCycleSpeed = 14f;
        public float armSwingAngle = 35f;
        public float legSwingAngle = 40f;
        public float spineTilt = 12f;

        private float animTime = 0f;
        private Vector3 defaultHipsLocalPos;
        private Quaternion defaultLeftArmRot;
        private Quaternion defaultRightArmRot;
        private Quaternion defaultLeftLegRot;
        private Quaternion defaultRightLegRot;

        private void Start()
        {
            if (hips != null) defaultHipsLocalPos = hips.localPosition;
            if (leftArm != null) defaultLeftArmRot = leftArm.localRotation;
            if (rightArm != null) defaultRightArmRot = rightArm.localRotation;
            if (leftLeg != null) defaultLeftLegRot = leftLeg.localRotation;
            if (rightLeg != null) defaultRightLegRot = rightLeg.localRotation;
        }

        public void Animate(float speedPercent, bool isGrounded, bool isJumping, float turnInput)
        {
            if (hips == null) return;

            if (isGrounded)
            {
                if (speedPercent > 0.05f)
                {
                    float cycleRate = Mathf.Lerp(walkCycleSpeed, runCycleSpeed, speedPercent);
                    animTime += Time.deltaTime * cycleRate;

                    float sin = Mathf.Sin(animTime);
                    float cos = Mathf.Cos(animTime);

                    // Bobbing
                    float bob = Mathf.Abs(sin) * 0.08f * speedPercent;
                    hips.localPosition = defaultHipsLocalPos + new Vector3(0f, bob, 0f);

                    // Leg swing
                    float legAngle = sin * legSwingAngle * speedPercent;
                    if (leftLeg != null) leftLeg.localRotation = defaultLeftLegRot * Quaternion.Euler(legAngle, 0f, 0f);
                    if (rightLeg != null) rightLeg.localRotation = defaultRightLegRot * Quaternion.Euler(-legAngle, 0f, 0f);

                    // Arm swing (opposite to leg)
                    float armAngle = -sin * armSwingAngle * speedPercent;
                    if (leftArm != null) leftArm.localRotation = defaultLeftArmRot * Quaternion.Euler(armAngle, 0f, 0f);
                    if (rightArm != null) rightArm.localRotation = defaultRightArmRot * Quaternion.Euler(-armAngle, 0f, 0f);

                    // Spine forward tilt when running
                    if (spine != null)
                    {
                        float tilt = spineTilt * speedPercent;
                        spine.localRotation = Quaternion.Euler(tilt, turnInput * 5f, -sin * 2f);
                    }
                }
                else
                {
                    // Idle breathing
                    animTime += Time.deltaTime * 2f;
                    float breath = Mathf.Sin(animTime) * 0.02f;
                    hips.localPosition = Vector3.Lerp(hips.localPosition, defaultHipsLocalPos + new Vector3(0f, breath, 0f), Time.deltaTime * 5f);

                    if (leftLeg != null) leftLeg.localRotation = Quaternion.Slerp(leftLeg.localRotation, defaultLeftLegRot, Time.deltaTime * 8f);
                    if (rightLeg != null) rightLeg.localRotation = Quaternion.Slerp(rightLeg.localRotation, defaultRightLegRot, Time.deltaTime * 8f);
                    if (leftArm != null) leftArm.localRotation = Quaternion.Slerp(leftArm.localRotation, defaultLeftArmRot, Time.deltaTime * 8f);
                    if (rightArm != null) rightArm.localRotation = Quaternion.Slerp(rightArm.localRotation, defaultRightArmRot, Time.deltaTime * 8f);
                    if (spine != null) spine.localRotation = Quaternion.Slerp(spine.localRotation, Quaternion.identity, Time.deltaTime * 8f);
                }
            }
            else
            {
                // In air / jumping pose
                if (leftLeg != null) leftLeg.localRotation = Quaternion.Slerp(leftLeg.localRotation, defaultLeftLegRot * Quaternion.Euler(-25f, 0f, 5f), Time.deltaTime * 8f);
                if (rightLeg != null) rightLeg.localRotation = Quaternion.Slerp(rightLeg.localRotation, defaultRightLegRot * Quaternion.Euler(20f, 0f, -5f), Time.deltaTime * 8f);
                if (leftArm != null) leftArm.localRotation = Quaternion.Slerp(leftArm.localRotation, defaultLeftArmRot * Quaternion.Euler(-50f, 0f, -15f), Time.deltaTime * 8f);
                if (rightArm != null) rightArm.localRotation = Quaternion.Slerp(rightArm.localRotation, defaultRightArmRot * Quaternion.Euler(-50f, 0f, 15f), Time.deltaTime * 8f);
            }
        }
    }
}

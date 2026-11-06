using UnityEngine;

namespace LostCave.Environment
{
    public class TorchFlicker : MonoBehaviour
    {
        [Header("Light")]
        public Light torchLight;
        public float baseIntensity = 3.5f;
        public float flickerRange = 0.8f;
        public float flickerSpeed = 8.0f;

        [Header("Flame Visual")]
        public Transform flameTransform;
        public float scaleJitter = 0.1f;

        private float randomOffset;
        private Vector3 initialFlameScale;

        private void Start()
        {
            if (torchLight == null) torchLight = GetComponentInChildren<Light>();
            if (flameTransform != null) initialFlameScale = flameTransform.localScale;
            randomOffset = Random.Range(0f, 100f);
        }

        private void Update()
        {
            float noise = Mathf.PerlinNoise((Time.time + randomOffset) * flickerSpeed, 0f);
            if (torchLight != null)
            {
                torchLight.intensity = baseIntensity + (noise - 0.5f) * flickerRange;
            }

            if (flameTransform != null)
            {
                float s = 1.0f + (noise - 0.5f) * scaleJitter;
                flameTransform.localScale = new Vector3(initialFlameScale.x * s, initialFlameScale.y * (1f + (noise - 0.5f) * 0.25f), initialFlameScale.z * s);
            }
        }
    }
}

using UnityEngine;

namespace LostCave.Environment
{
    public class CrystalGlow : MonoBehaviour
    {
        [Header("Pulsing Light")]
        public Light crystalLight;
        public float baseIntensity = 2.0f;
        public float pulseAmount = 0.8f;
        public float pulseSpeed = 2.5f;

        [Header("Floating / Hover")]
        public bool enableFloating = false;
        public float floatHeight = 0.15f;
        public float floatSpeed = 1.5f;
        public float rotateSpeed = 20.0f;

        private Material crystalMaterial;
        private Color baseEmissionColor;
        private Vector3 startPos;
        private float seed;

        private void Start()
        {
            if (crystalLight == null) crystalLight = GetComponentInChildren<Light>();
            startPos = transform.position;
            seed = Random.Range(0f, 10f);

            Renderer rend = GetComponent<Renderer>();
            if (rend != null)
            {
                crystalMaterial = rend.material;
                if (crystalMaterial.HasProperty("_EmissionColor"))
                {
                    baseEmissionColor = crystalMaterial.GetColor("_EmissionColor");
                }
            }
        }

        private void Update()
        {
            float pulse = Mathf.Sin((Time.time + seed) * pulseSpeed);
            float factor = 1.0f + pulse * 0.35f;

            if (crystalLight != null)
            {
                crystalLight.intensity = baseIntensity + pulse * pulseAmount;
            }

            if (crystalMaterial != null && baseEmissionColor != Color.black)
            {
                crystalMaterial.SetColor("_EmissionColor", baseEmissionColor * factor);
            }

            if (enableFloating)
            {
                transform.position = startPos + new Vector3(0f, Mathf.Sin((Time.time + seed) * floatSpeed) * floatHeight, 0f);
                transform.Rotate(Vector3.up * (rotateSpeed * Time.deltaTime));
            }
        }
    }
}

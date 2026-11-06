using UnityEngine;

namespace LostCave.Obstacles
{
    public class CollapsingBridge : MonoBehaviour
    {
        [Header("Bridge Settings")]
        public int tileCount = 8;
        public float tileLength = 1.8f;
        public float tileWidth = 2.4f;
        public GameObject tilePrefab;

        [Header("Bridge Pillars / Supports")]
        public Transform startAnchor;
        public Transform endAnchor;

        private void OnDrawGizmos()
        {
            Gizmos.color = Color.yellow;
            Vector3 start = transform.position;
            Vector3 end = transform.position + transform.forward * (tileCount * tileLength);
            Gizmos.DrawLine(start, end);
        }
    }
}

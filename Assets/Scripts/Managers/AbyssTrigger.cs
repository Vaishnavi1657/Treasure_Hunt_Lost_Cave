using UnityEngine;
using LostCave.Audio;
using LostCave.Player;

namespace LostCave.Managers
{
    public class AbyssTrigger : MonoBehaviour
    {
        [Header("Fall Penalty")]
        public int damageOnFall = 1;
        public AudioClip fallScreamOrSplash;

        private void OnTriggerEnter(Collider other)
        {
            if (other.CompareTag("Player"))
            {
                if (fallScreamOrSplash != null && AudioManager.Instance != null)
                {
                    AudioManager.Instance.PlaySFX(fallScreamOrSplash, 1.0f);
                }

                if (PlayerController.Instance != null)
                {
                    PlayerController.Instance.TakeDamage(PlayerController.Instance.maxHealth, Vector3.zero);
                }
                else if (GameManager.Instance != null)
                {
                    GameManager.Instance.TriggerGameOver();
                }
            }
        }
    }
}

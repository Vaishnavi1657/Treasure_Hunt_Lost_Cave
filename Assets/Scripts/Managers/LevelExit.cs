using System.Collections;
using UnityEngine;
using LostCave.Audio;

namespace LostCave.Managers
{
    public class LevelExit : MonoBehaviour
    {
        [Header("Next Level")]
        public string nextSceneName = "CaveLevel2";
        public float delayBeforeLoad = 1.5f;
        public AudioClip exitClip;

        private bool hasTriggered = false;

        private void OnTriggerEnter(Collider other)
        {
            if (hasTriggered) return;

            if (other.CompareTag("Player") || 
                (other.transform.root != null && other.transform.root.CompareTag("Player")) ||
                other.GetComponentInParent<LostCave.Player.PlayerController>() != null ||
                other.GetComponentInParent<LostCave.Player.PlayerFlightController>() != null)
            {
                StartCoroutine(CompleteLevelRoutine());
            }
        }

        private IEnumerator CompleteLevelRoutine()
        {
            hasTriggered = true;

            if (exitClip != null && AudioManager.Instance != null)
            {
                AudioManager.Instance.PlaySFX(exitClip, 1.0f);
            }

            if (GameManager.Instance != null)
            {
                GameManager.Instance.ShowNotification("Level Complete! Moving Ahead...");
            }

            yield return new WaitForSeconds(delayBeforeLoad);

            if (GameManager.Instance != null)
            {
                GameManager.Instance.LoadScene(nextSceneName);
            }
            else
            {
                UnityEngine.SceneManagement.SceneManager.LoadScene(nextSceneName);
            }
        }
    }
}

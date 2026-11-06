using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using LostCave.Audio;

namespace LostCave.UI
{
    public class GameCompleteUI : MonoBehaviour
    {
        public Button playAgainButton;
        public Button levelSelectButton;
        public Button mainMenuButton;

        private void Start()
        {
            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;

            if (playAgainButton != null) playAgainButton.onClick.AddListener(OnPlayAgain);
            if (levelSelectButton != null) levelSelectButton.onClick.AddListener(OnLevelSelect);
            if (mainMenuButton != null) mainMenuButton.onClick.AddListener(OnMainMenu);
        }

        private void OnPlayAgain()
        {
            PlayClick();
            SceneManager.LoadScene("CaveLevel1");
        }

        private void OnLevelSelect()
        {
            PlayClick();
            SceneManager.LoadScene("LevelSelection");
        }

        private void OnMainMenu()
        {
            PlayClick();
            SceneManager.LoadScene("MainMenu");
        }

        private void PlayClick()
        {
            if (AudioManager.Instance != null)
            {
                AudioManager.Instance.PlayButtonClick();
            }
        }
    }
}

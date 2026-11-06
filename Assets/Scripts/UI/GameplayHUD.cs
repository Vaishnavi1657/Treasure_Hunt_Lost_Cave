using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using LostCave.Audio;
using LostCave.Managers;

namespace LostCave.UI
{
    public class GameplayHUD : MonoBehaviour
    {
        [Header("Top HUD")]
        public Text levelTitleText;
        public Text objectiveText;
        public Text healthText;
        public Slider healthSlider;

        [Header("Center Alerts & Prompts")]
        public GameObject promptPanel;
        public Text promptText;
        public GameObject notificationPanel;
        public Text notificationText;

        [Header("Pause Menu")]
        public GameObject pauseMenuPanel;
        public Slider musicVolumeSlider;
        public Slider sfxVolumeSlider;
        public Button resumeButton;
        public Button restartButton;
        public Button levelSelectButton;
        public Button mainMenuButton;

        [Header("Game Over Menu")]
        public GameObject gameOverPanel;
        public Button retryCheckpointButton;
        public Button gameOverRestartButton;
        public Button gameOverMainMenuButton;

        private Coroutine notificationRoutine;

        private void Start()
        {
            if (promptPanel != null) promptPanel.SetActive(false);
            if (notificationPanel != null) notificationPanel.SetActive(false);
            if (pauseMenuPanel != null) pauseMenuPanel.SetActive(false);
            if (gameOverPanel != null) gameOverPanel.SetActive(false);

            // Hook up Pause Menu buttons
            if (resumeButton != null) resumeButton.onClick.AddListener(OnResumeClicked);
            if (restartButton != null) restartButton.onClick.AddListener(OnRestartClicked);
            if (levelSelectButton != null) levelSelectButton.onClick.AddListener(OnLevelSelectClicked);
            if (mainMenuButton != null) mainMenuButton.onClick.AddListener(OnMainMenuClicked);

            // Hook up Game Over buttons
            if (retryCheckpointButton != null) retryCheckpointButton.onClick.AddListener(OnRetryCheckpointClicked);
            if (gameOverRestartButton != null) gameOverRestartButton.onClick.AddListener(OnRestartClicked);
            if (gameOverMainMenuButton != null) gameOverMainMenuButton.onClick.AddListener(OnMainMenuClicked);

            // Hook up volume sliders
            if (musicVolumeSlider != null && AudioManager.Instance != null)
            {
                musicVolumeSlider.value = AudioManager.Instance.musicVolume;
                musicVolumeSlider.onValueChanged.AddListener(val => AudioManager.Instance.SetMusicVolume(val));
            }

            if (sfxVolumeSlider != null && AudioManager.Instance != null)
            {
                sfxVolumeSlider.value = AudioManager.Instance.sfxVolume;
                sfxVolumeSlider.onValueChanged.AddListener(val => AudioManager.Instance.SetSFXVolume(val));
            }
        }

        public void UpdateLevelInfo(string title, string objective)
        {
            if (levelTitleText != null) levelTitleText.text = title;
            if (objectiveText != null) objectiveText.text = objective;
        }

        public void UpdateHealth(int current, int max)
        {
            if (healthText != null) healthText.text = "HEALTH: " + current + " / " + max;
            if (healthSlider != null)
            {
                healthSlider.maxValue = max;
                healthSlider.value = current;
            }
        }

        public void ShowPrompt(string message)
        {
            if (promptPanel != null)
            {
                promptPanel.SetActive(true);
                if (promptText != null) promptText.text = message;
            }
        }

        public void HidePrompt()
        {
            if (promptPanel != null)
            {
                promptPanel.SetActive(false);
            }
        }

        public void ShowNotification(string message, float duration = 2.5f)
        {
            if (notificationPanel == null) return;
            if (notificationRoutine != null) StopCoroutine(notificationRoutine);
            notificationRoutine = StartCoroutine(NotificationRoutine(message, duration));
        }

        private IEnumerator NotificationRoutine(string message, float duration)
        {
            notificationPanel.SetActive(true);
            if (notificationText != null) notificationText.text = message;
            yield return new WaitForSecondsRealtime(duration);
            notificationPanel.SetActive(false);
        }

        public void SetPauseMenuVisible(bool visible)
        {
            if (pauseMenuPanel != null)
            {
                pauseMenuPanel.SetActive(visible);
                if (visible) pauseMenuPanel.transform.SetAsLastSibling();
            }
        }

        public void ShowGameOverScreen()
        {
            if (gameOverPanel != null)
            {
                gameOverPanel.SetActive(true);
                gameOverPanel.transform.SetAsLastSibling();
            }
            if (pauseMenuPanel != null)
            {
                pauseMenuPanel.SetActive(false);
            }
        }

        public void HideGameOverScreen()
        {
            if (gameOverPanel != null)
            {
                gameOverPanel.SetActive(false);
            }
        }

        private void Update()
        {
            // Game Over Menu interaction (works even when Time.timeScale = 0)
            if (gameOverPanel != null && gameOverPanel.activeSelf)
            {
                // Keyboard shortcuts for Game Over
                if (Input.GetKeyDown(KeyCode.R))
                {
                    OnRestartClicked();
                    return;
                }
                if (Input.GetKeyDown(KeyCode.C) || Input.GetKeyDown(KeyCode.Space) || Input.GetKeyDown(KeyCode.Return))
                {
                    OnRetryCheckpointClicked();
                    return;
                }
                if (Input.GetKeyDown(KeyCode.Escape) || Input.GetKeyDown(KeyCode.M))
                {
                    OnMainMenuClicked();
                    return;
                }

                // Direct mouse hit-test fallback
                if (Input.GetMouseButtonDown(0))
                {
                    Vector2 mousePos = Input.mousePosition;
                    if (retryCheckpointButton != null && RectTransformUtility.RectangleContainsScreenPoint(retryCheckpointButton.GetComponent<RectTransform>(), mousePos, null))
                    {
                        OnRetryCheckpointClicked();
                        return;
                    }
                    if (gameOverRestartButton != null && RectTransformUtility.RectangleContainsScreenPoint(gameOverRestartButton.GetComponent<RectTransform>(), mousePos, null))
                    {
                        OnRestartClicked();
                        return;
                    }
                    if (gameOverMainMenuButton != null && RectTransformUtility.RectangleContainsScreenPoint(gameOverMainMenuButton.GetComponent<RectTransform>(), mousePos, null))
                    {
                        OnMainMenuClicked();
                        return;
                    }
                }
            }
            // Pause Menu interaction
            else if (pauseMenuPanel != null && pauseMenuPanel.activeSelf)
            {
                if (Input.GetMouseButtonDown(0))
                {
                    Vector2 mousePos = Input.mousePosition;
                    if (resumeButton != null && RectTransformUtility.RectangleContainsScreenPoint(resumeButton.GetComponent<RectTransform>(), mousePos, null))
                    {
                        OnResumeClicked();
                        return;
                    }
                    if (restartButton != null && RectTransformUtility.RectangleContainsScreenPoint(restartButton.GetComponent<RectTransform>(), mousePos, null))
                    {
                        OnRestartClicked();
                        return;
                    }
                    if (levelSelectButton != null && RectTransformUtility.RectangleContainsScreenPoint(levelSelectButton.GetComponent<RectTransform>(), mousePos, null))
                    {
                        OnLevelSelectClicked();
                        return;
                    }
                    if (mainMenuButton != null && RectTransformUtility.RectangleContainsScreenPoint(mainMenuButton.GetComponent<RectTransform>(), mousePos, null))
                    {
                        OnMainMenuClicked();
                        return;
                    }
                }
            }
        }

        private void OnResumeClicked()
        {
            PlayClickAudio();
            if (GameManager.Instance != null) GameManager.Instance.TogglePause();
        }

        private void OnRestartClicked()
        {
            PlayClickAudio();
            if (GameManager.Instance != null) GameManager.Instance.RestartCurrentLevel();
        }

        private void OnLevelSelectClicked()
        {
            PlayClickAudio();
            if (GameManager.Instance != null) GameManager.Instance.LoadScene("LevelSelection");
        }

        private void OnMainMenuClicked()
        {
            PlayClickAudio();
            if (GameManager.Instance != null) GameManager.Instance.LoadMainMenu();
        }

        private void OnRetryCheckpointClicked()
        {
            PlayClickAudio();
            if (gameOverPanel != null) gameOverPanel.SetActive(false);
            Time.timeScale = 1.0f;
            if (GameManager.Instance != null)
            {
                GameManager.Instance.RespawnPlayer();
            }
            Cursor.lockState = CursorLockMode.Locked;
            Cursor.visible = false;
        }

        private void PlayClickAudio()
        {
            if (AudioManager.Instance != null)
            {
                AudioManager.Instance.PlayButtonClick();
            }
        }
    }
}

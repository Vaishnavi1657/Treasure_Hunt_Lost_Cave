using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;
using LostCave.Player;
using LostCave.UI;
using LostCave.Audio;

namespace LostCave.Managers
{
    public class GameManager : MonoBehaviour
    {
        public static GameManager Instance { get; private set; }

        [Header("Level Info")]
        public string currentLevelName = "Level 1: Cave Entrance";
        public string currentObjective = "Explore the cave and find a way deeper.";

        [Header("Player Tracking")]
        public Vector3 lastCheckpointPosition;
        public Quaternion lastCheckpointRotation;
        public int playerLives = 3;

        [Header("State")]
        public bool IsGamePaused { get; private set; }
        public bool IsGameOver { get; private set; }

        private GameplayHUD hud;

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }

            Instance = this;

            // Set initial spawn point as checkpoint
            GameObject player = GameObject.FindGameObjectWithTag("Player");
            if (player != null)
            {
                lastCheckpointPosition = player.transform.position;
                lastCheckpointRotation = player.transform.rotation;
            }
        }

        private void Start()
        {
            Time.timeScale = 1.0f;
            IsGamePaused = false;
            IsGameOver = false;

            hud = FindAnyObjectByType<GameplayHUD>();
            if (hud != null)
            {
                hud.UpdateLevelInfo(currentLevelName, currentObjective);
                if (PlayerController.Instance != null)
                {
                    hud.UpdateHealth(PlayerController.Instance.currentHealth, PlayerController.Instance.maxHealth);
                }
            }
        }

        private void Update()
        {
            if (Input.GetKeyDown(KeyCode.Escape) && !IsGameOver)
            {
                TogglePause();
            }
        }

        public void SetCheckpoint(Vector3 position, Quaternion rotation)
        {
            lastCheckpointPosition = position;
            lastCheckpointRotation = rotation;
        }

        public void ShowNotification(string message, float duration = 2.5f)
        {
            if (hud != null)
            {
                hud.ShowNotification(message, duration);
            }
        }

        public void ShowHUDNotification(string message, float duration = 2.5f)
        {
            ShowNotification(message, duration);
        }

        public void UpdateHUDObjective(string newObjective)
        {
            currentObjective = newObjective;
            if (hud != null)
            {
                hud.UpdateLevelInfo(currentLevelName, currentObjective);
            }
        }

        public void ShowInteractionPrompt(string text)
        {
            if (hud != null)
            {
                hud.ShowPrompt(text);
            }
        }

        public void HideInteractionPrompt()
        {
            if (hud != null)
            {
                hud.HidePrompt();
            }
        }

        public void UpdateHUDHealth(int current, int max)
        {
            if (hud != null)
            {
                hud.UpdateHealth(current, max);
            }
        }

        public void OnPlayerDied()
        {
            TriggerGameOver();
        }

        public void RespawnPlayer()
        {
            IsGameOver = false;
            IsGamePaused = false;
            Time.timeScale = 1.0f;
            if (hud != null)
            {
                hud.HideGameOverScreen();
            }
            if (PlayerController.Instance != null)
            {
                PlayerController.Instance.RespawnAt(lastCheckpointPosition, lastCheckpointRotation);
            }
            Cursor.lockState = CursorLockMode.Locked;
            Cursor.visible = false;
        }

        public void TriggerGameOver()
        {
            IsGameOver = true;
            Time.timeScale = 0f;
            if (hud != null)
            {
                hud.ShowGameOverScreen();
            }
            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;
        }

        public void CompleteGame()
        {
            LoadScene("GameComplete");
        }

        public void TogglePause()
        {
            IsGamePaused = !IsGamePaused;
            Time.timeScale = IsGamePaused ? 0f : 1f;

            if (hud != null)
            {
                hud.SetPauseMenuVisible(IsGamePaused);
            }

            Cursor.lockState = IsGamePaused ? CursorLockMode.None : CursorLockMode.Locked;
            Cursor.visible = IsGamePaused;
        }

        public void ResumeGame()
        {
            if (IsGamePaused)
            {
                TogglePause();
            }
        }

        public void RestartCurrentLevel()
        {
            Time.timeScale = 1.0f;
            SceneManager.LoadScene(SceneManager.GetActiveScene().name);
        }

        public void LoadScene(string sceneName)
        {
            Time.timeScale = 1.0f;
            SceneManager.LoadScene(sceneName);
        }

        public void LoadMainMenu()
        {
            Time.timeScale = 1.0f;
            SceneManager.LoadScene("MainMenu");
        }
    }
}

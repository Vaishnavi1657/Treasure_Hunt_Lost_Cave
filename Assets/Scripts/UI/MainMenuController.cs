using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using LostCave.Audio;

namespace LostCave.UI
{
    public class MainMenuController : MonoBehaviour
    {
        [Header("Menu Panels")]
        public GameObject mainPanel;
        public GameObject soundSettingsPanel;

        [Header("Main Buttons")]
        public Button startGameButton;
        public Button levelSelectButton;
        public Button controlsButton;
        public Button soundSettingsButton;
        public Button quitGameButton;

        [Header("Sound Settings Controls")]
        public Slider musicSlider;
        public Slider sfxSlider;
        public Button closeSoundSettingsButton;

        private void Awake()
        {
            EnsureInputModule.FixEventSystem();
        }

        private void Start()
        {
            Time.timeScale = 1.0f;
            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;

            EnsureInputModule.FixEventSystem();

            if (soundSettingsPanel != null) soundSettingsPanel.SetActive(false);

            if (startGameButton != null) startGameButton.onClick.AddListener(OnStartGame);
            if (levelSelectButton != null) levelSelectButton.onClick.AddListener(OnLevelSelect);
            if (controlsButton != null) controlsButton.onClick.AddListener(OnControls);
            if (soundSettingsButton != null) soundSettingsButton.onClick.AddListener(OnOpenSoundSettings);
            if (quitGameButton != null) quitGameButton.onClick.AddListener(OnQuitGame);

            if (closeSoundSettingsButton != null) closeSoundSettingsButton.onClick.AddListener(OnCloseSoundSettings);

            if (musicSlider != null && AudioManager.Instance != null)
            {
                musicSlider.value = AudioManager.Instance.musicVolume;
                musicSlider.onValueChanged.AddListener(val => AudioManager.Instance.SetMusicVolume(val));
            }

            if (sfxSlider != null && AudioManager.Instance != null)
            {
                sfxSlider.value = AudioManager.Instance.sfxVolume;
                sfxSlider.onValueChanged.AddListener(val => AudioManager.Instance.SetSFXVolume(val));
            }
        }

        private void Update()
        {
            // Direct mouse click fallback (guarantees clicks work 100% in all input modes)
            if (Input.GetMouseButtonDown(0))
            {
                Vector2 mousePos = Input.mousePosition;
                if (soundSettingsPanel != null && soundSettingsPanel.activeSelf)
                {
                    if (closeSoundSettingsButton != null && IsMouseOver(closeSoundSettingsButton.GetComponent<RectTransform>(), mousePos))
                    {
                        OnCloseSoundSettings();
                        return;
                    }
                }
                else
                {
                    if (startGameButton != null && IsMouseOver(startGameButton.GetComponent<RectTransform>(), mousePos))
                    {
                        OnStartGame();
                        return;
                    }
                    if (levelSelectButton != null && IsMouseOver(levelSelectButton.GetComponent<RectTransform>(), mousePos))
                    {
                        OnLevelSelect();
                        return;
                    }
                    if (controlsButton != null && IsMouseOver(controlsButton.GetComponent<RectTransform>(), mousePos))
                    {
                        OnControls();
                        return;
                    }
                    if (soundSettingsButton != null && IsMouseOver(soundSettingsButton.GetComponent<RectTransform>(), mousePos))
                    {
                        OnOpenSoundSettings();
                        return;
                    }
                    if (quitGameButton != null && IsMouseOver(quitGameButton.GetComponent<RectTransform>(), mousePos))
                    {
                        OnQuitGame();
                        return;
                    }
                }
            }

            // Keyboard navigation shortcuts
            if (Input.GetKeyDown(KeyCode.Return) || Input.GetKeyDown(KeyCode.KeypadEnter) || Input.GetKeyDown(KeyCode.Space))
            {
                OnStartGame();
            }
            else if (Input.GetKeyDown(KeyCode.L))
            {
                OnLevelSelect();
            }
            else if (Input.GetKeyDown(KeyCode.C))
            {
                OnControls();
            }
            else if (Input.GetKeyDown(KeyCode.S))
            {
                if (soundSettingsPanel != null && soundSettingsPanel.activeSelf)
                    OnCloseSoundSettings();
                else
                    OnOpenSoundSettings();
            }
            else if (Input.GetKeyDown(KeyCode.Escape))
            {
                if (soundSettingsPanel != null && soundSettingsPanel.activeSelf)
                    OnCloseSoundSettings();
                else
                    OnQuitGame();
            }
        }

        private bool IsMouseOver(RectTransform rt, Vector2 mousePos)
        {
            if (rt == null) return false;
            return RectTransformUtility.RectangleContainsScreenPoint(rt, mousePos, null);
        }

        public void OnStartGame()
        {
            PlayClick();
            SceneManager.LoadScene("CaveLevel1");
        }

        public void OnLevelSelect()
        {
            PlayClick();
            SceneManager.LoadScene("LevelSelection");
        }

        public void OnControls()
        {
            PlayClick();
            SceneManager.LoadScene("Instructions");
        }

        public void OnOpenSoundSettings()
        {
            PlayClick();
            if (soundSettingsPanel != null) soundSettingsPanel.SetActive(true);
        }

        public void OnCloseSoundSettings()
        {
            PlayClick();
            if (soundSettingsPanel != null) soundSettingsPanel.SetActive(false);
        }

        public void OnQuitGame()
        {
            PlayClick();
#if UNITY_EDITOR
            UnityEditor.EditorApplication.isPlaying = false;
#else
            Application.Quit();
#endif
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

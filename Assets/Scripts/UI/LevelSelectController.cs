using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using LostCave.Audio;

namespace LostCave.UI
{
    public class LevelSelectController : MonoBehaviour
    {
        public Button level1Button;
        public Button level2Button;
        public Button level3Button;
        public Button backButton;

        private void Awake()
        {
            EnsureInputModule.FixEventSystem();
        }

        private void Start()
        {
            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;

            EnsureInputModule.FixEventSystem();

            if (level1Button != null) level1Button.onClick.AddListener(() => LoadLevel("CaveLevel1"));
            if (level2Button != null) level2Button.onClick.AddListener(() => LoadLevel("CaveLevel2"));
            if (level3Button != null) level3Button.onClick.AddListener(() => LoadLevel("CaveLevel3"));
            if (backButton != null) backButton.onClick.AddListener(OnBack);
        }

        private void Update()
        {
            if (Input.GetMouseButtonDown(0))
            {
                Vector2 mousePos = Input.mousePosition;
                if (level1Button != null && RectTransformUtility.RectangleContainsScreenPoint(level1Button.GetComponent<RectTransform>(), mousePos, null))
                {
                    LoadLevel("CaveLevel1");
                    return;
                }
                if (level2Button != null && RectTransformUtility.RectangleContainsScreenPoint(level2Button.GetComponent<RectTransform>(), mousePos, null))
                {
                    LoadLevel("CaveLevel2");
                    return;
                }
                if (level3Button != null && RectTransformUtility.RectangleContainsScreenPoint(level3Button.GetComponent<RectTransform>(), mousePos, null))
                {
                    LoadLevel("CaveLevel3");
                    return;
                }
                if (backButton != null && RectTransformUtility.RectangleContainsScreenPoint(backButton.GetComponent<RectTransform>(), mousePos, null))
                {
                    OnBack();
                    return;
                }
            }

            if (Input.GetKeyDown(KeyCode.Alpha1) || Input.GetKeyDown(KeyCode.Keypad1))
            {
                LoadLevel("CaveLevel1");
            }
            else if (Input.GetKeyDown(KeyCode.Alpha2) || Input.GetKeyDown(KeyCode.Keypad2))
            {
                LoadLevel("CaveLevel2");
            }
            else if (Input.GetKeyDown(KeyCode.Alpha3) || Input.GetKeyDown(KeyCode.Keypad3))
            {
                LoadLevel("CaveLevel3");
            }
            else if (Input.GetKeyDown(KeyCode.Escape))
            {
                OnBack();
            }
        }

        public void LoadLevel(string sceneName)
        {
            PlayClick();
            SceneManager.LoadScene(sceneName);
        }

        public void OnBack()
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

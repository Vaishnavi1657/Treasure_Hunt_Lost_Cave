using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using LostCave.Audio;

namespace LostCave.UI
{
    public class InstructionsController : MonoBehaviour
    {
        public Button continueButton;
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

            if (continueButton != null) continueButton.onClick.AddListener(OnContinue);
            if (backButton != null) backButton.onClick.AddListener(OnBack);
        }

        private void Update()
        {
            if (Input.GetMouseButtonDown(0))
            {
                Vector2 mousePos = Input.mousePosition;
                if (continueButton != null && RectTransformUtility.RectangleContainsScreenPoint(continueButton.GetComponent<RectTransform>(), mousePos, null))
                {
                    OnContinue();
                    return;
                }
                if (backButton != null && RectTransformUtility.RectangleContainsScreenPoint(backButton.GetComponent<RectTransform>(), mousePos, null))
                {
                    OnBack();
                    return;
                }
            }

            if (Input.GetKeyDown(KeyCode.Return) || Input.GetKeyDown(KeyCode.KeypadEnter) || Input.GetKeyDown(KeyCode.Space))
            {
                OnContinue();
            }
            else if (Input.GetKeyDown(KeyCode.Escape))
            {
                OnBack();
            }
        }

        public void OnContinue()
        {
            PlayClick();
            SceneManager.LoadScene("CaveLevel1");
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

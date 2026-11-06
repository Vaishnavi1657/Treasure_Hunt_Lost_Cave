using System.IO;
using UnityEngine;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine.SceneManagement;

namespace LostCave.Editor
{
    [InitializeOnLoad]
    public static class AutoOpenMainMenu
    {
        static AutoOpenMainMenu()
        {
            EditorApplication.delayCall += CheckAndOpenDefaultScene;
        }

        private static void CheckAndOpenDefaultScene()
        {
            // Never execute when playing or entering Play Mode
            if (EditorApplication.isPlayingOrWillChangePlaymode) return;

            string mainMenuPath = "Assets/Scenes/MainMenu.unity";
            if (File.Exists(mainMenuPath))
            {
                Scene activeScene = SceneManager.GetActiveScene();
                if (string.IsNullOrEmpty(activeScene.path) || activeScene.name == "Untitled" || activeScene.name == "")
                {
                    EditorSceneManager.OpenScene(mainMenuPath);
                    Debug.Log("[Lost Cave] Automatically opened MainMenu.unity. Press Play (Ctrl+P) to start!");
                }
            }
        }
    }
}

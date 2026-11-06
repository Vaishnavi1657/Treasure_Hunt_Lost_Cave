using System.IO;
using UnityEngine;
using UnityEditor;

namespace LostCave.Editor
{
    [InitializeOnLoad]
    public static class AutoBuildTrigger
    {
        private const string FLAG_PATH = "Temp/lost_cave_build_v17_perfect_lethal_fair_obstacles.flag";

        static AutoBuildTrigger()
        {
            EditorApplication.delayCall += RunBuildOnce;
            EditorApplication.playModeStateChanged += OnPlayModeChanged;
        }

        private static void OnPlayModeChanged(PlayModeStateChange change)
        {
            if (change == PlayModeStateChange.EnteredEditMode)
            {
                RunBuildOnce();
            }
        }

        private static void RunBuildOnce()
        {
            if (File.Exists(FLAG_PATH)) return;

            // Never interrupt Play Mode
            if (EditorApplication.isPlayingOrWillChangePlaymode) return;

            try
            {
                Debug.Log(">>> [AutoBuildTrigger] Building fast coaster, fatal obstacles, and proper head clearance...");
                string activeScenePath = UnityEngine.SceneManagement.SceneManager.GetActiveScene().path;
                if (string.IsNullOrEmpty(activeScenePath)) activeScenePath = "Assets/Scenes/CaveLevel3.unity";

                CaveGameBuilder.BuildEntireGame();
                UnityEditor.SceneManagement.EditorSceneManager.OpenScene(activeScenePath);
                File.WriteAllText(FLAG_PATH, "built");
                Debug.Log(">>> [AutoBuildTrigger] Build completed successfully!");
            }
            catch (System.Exception ex)
            {
                Debug.LogError(">>> [AutoBuildTrigger] Error running build: " + ex.Message);
            }
        }
    }
}

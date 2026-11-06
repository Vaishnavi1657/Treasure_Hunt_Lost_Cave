using UnityEngine;
using UnityEngine.EventSystems;
#if ENABLE_INPUT_SYSTEM
using UnityEngine.InputSystem.UI;
#endif

namespace LostCave.UI
{
    [ExecuteAlways]
    public class EnsureInputModule : MonoBehaviour
    {
        private void Awake()
        {
            FixEventSystem();
        }

        private void OnEnable()
        {
            FixEventSystem();
        }

        public static void FixEventSystem()
        {
            EventSystem es = Object.FindAnyObjectByType<EventSystem>();
            if (es == null)
            {
                GameObject esObj = new GameObject("EventSystem");
                es = esObj.AddComponent<EventSystem>();
            }

#if ENABLE_INPUT_SYSTEM
            StandaloneInputModule sim = es.GetComponent<StandaloneInputModule>();
            if (sim != null)
            {
                if (Application.isPlaying)
                    Destroy(sim);
                else
                    DestroyImmediate(sim);
            }

            InputSystemUIInputModule ism = es.GetComponent<InputSystemUIInputModule>();
            if (ism == null)
            {
                ism = es.gameObject.AddComponent<InputSystemUIInputModule>();
            }
            if (ism != null && ism.actionsAsset == null)
            {
                ism.AssignDefaultActions();
            }
#else
            StandaloneInputModule sim = es.GetComponent<StandaloneInputModule>();
            if (sim == null)
            {
                es.gameObject.AddComponent<StandaloneInputModule>();
            }
#endif
        }
    }
}

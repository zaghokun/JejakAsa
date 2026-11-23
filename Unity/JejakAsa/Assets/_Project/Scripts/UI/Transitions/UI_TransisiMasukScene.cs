using UnityEngine;

namespace JejakAsa
{
    [DefaultExecutionOrder(-2000)]
    public class UI_TransisiMasukScene : MonoBehaviour
    {
        private const float Duration = 1.2f;
        private float startTime;

        public bool IsFinished { get; private set; }

        private void Awake()
        {
            if (StoryFlow_PengaturAlur.Instance.CurrentPhase != DayPhase.Prolog)
            {
                IsFinished = true;
                enabled = false;
                return;
            }

            startTime = Time.unscaledTime;
        }

        private void Update()
        {
            if (Time.unscaledTime - startTime >= Duration)
            {
                IsFinished = true;
                enabled = false;
            }
        }

        private void OnGUI()
        {
            GUI.depth = -2000;
            Color previous = GUI.color;
            GUI.Button(new Rect(0, 0, Screen.width, Screen.height), GUIContent.none, GUIStyle.none);
            GUI.color = new Color(1f, 1f, 1f, 1f - Mathf.Clamp01((Time.unscaledTime - startTime) / Duration));
            GUI.DrawTexture(new Rect(0, 0, Screen.width, Screen.height), Texture2D.blackTexture);
            GUI.color = previous;
        }
    }
}

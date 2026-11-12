using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;

namespace JejakAsa
{
    [DefaultExecutionOrder(-1000)]
    [DisallowMultipleComponent]
    public sealed class SessionMenu : MonoBehaviour
    {
        public bool isMainMenu;
        public bool IsPaused { get; private set; }

        private static SessionMenu active;
        private static int blockedFrame = -1;
        internal static bool InputBlocked => (active != null && active.IsPaused) || blockedFrame == Time.frameCount;

        private PlayerInteraction interaction;
        private PlayerController movement;
        private EventSystem pausedEventSystem;
        private bool movementWasEnabled;
        private bool confirming;
        private int selected;
        private float previousTimeScale;

        private void Awake()
        {
            active = this;
            interaction = GetComponent<PlayerInteraction>();
            movement = GetComponent<PlayerController>();
        }

        private void OnDisable()
        {
            if (IsPaused)
                Resume();
            if (active == this)
                active = null;
        }

        private void Update()
        {
            Keyboard keyboard = Keyboard.current;
            if (keyboard == null)
                return;

            if (!isMainMenu && keyboard.escapeKey.wasPressedThisFrame)
            {
                if (IsPaused)
                {
                    if (confirming) CancelReturnToMenu();
                    else Resume();
                }
                else if ((interaction == null || !interaction.IsDialogueOpen) && !EncounterController.AnyOpen)
                {
                    Pause();
                }
                return;
            }

            if (!isMainMenu && !IsPaused)
                return;

            if (keyboard.upArrowKey.wasPressedThisFrame || keyboard.downArrowKey.wasPressedThisFrame || keyboard.tabKey.wasPressedThisFrame)
                selected = 1 - selected;

            if (keyboard.digit1Key.wasPressedThisFrame)
                Activate(0);
            else if (keyboard.digit2Key.wasPressedThisFrame)
                Activate(1);
            else if (keyboard.enterKey.wasPressedThisFrame || keyboard.spaceKey.wasPressedThisFrame)
                Activate(selected);
            else if (isMainMenu && keyboard.escapeKey.wasPressedThisFrame)
                Application.Quit();
        }

        public void Pause()
        {
            if (isMainMenu || IsPaused)
                return;
            active = this;
            IsPaused = true;
            blockedFrame = Time.frameCount;
            previousTimeScale = Time.timeScale;
            Time.timeScale = 0f;
            selected = 0;
            confirming = false;
            if (movement != null)
            {
                movementWasEnabled = movement.enabled;
                movement.enabled = false;
            }
            pausedEventSystem = EventSystem.current;
            if (pausedEventSystem != null)
                pausedEventSystem.enabled = false;
        }

        public void Resume()
        {
            if (!IsPaused)
                return;
            blockedFrame = Time.frameCount;
            IsPaused = false;
            confirming = false;
            Time.timeScale = previousTimeScale;
            if (movement != null)
                movement.enabled = movementWasEnabled;
            if (pausedEventSystem != null)
                pausedEventSystem.enabled = true;
            pausedEventSystem = null;
        }

        public void RequestReturnToMenu()
        {
            if (!IsPaused)
                return;
            confirming = true;
            selected = 0;
        }

        public void CancelReturnToMenu()
        {
            if (!IsPaused)
                return;
            confirming = false;
            selected = 0;
        }

        public void ConfirmReturnToMenu()
        {
            if (!IsPaused || !confirming)
                return;
            Resume();
            Time.timeScale = 1f;
            SceneManager.LoadScene("Assets/_Project/Scenes/MainMenu.unity", LoadSceneMode.Single);
        }

        public void StartNewGame()
        {
            if (!isMainMenu)
                return;
            Time.timeScale = 1f;
            SceneManager.LoadScene("Assets/_Project/Scenes/Kampung.unity", LoadSceneMode.Single);
        }

        private void Activate(int choice)
        {
            if (isMainMenu)
            {
                if (choice == 0) StartNewGame();
                else Application.Quit();
            }
            else if (confirming)
            {
                if (choice == 0) CancelReturnToMenu();
                else ConfirmReturnToMenu();
            }
            else
            {
                if (choice == 0) Resume();
                else RequestReturnToMenu();
            }
        }

        private void OnGUI()
        {
            if (!isMainMenu && !IsPaused)
                return;

            CozyGuiTheme.EnsureInitialized();
            GUI.depth = -1000;
            float scale = CozyGuiTheme.Scale;
            Matrix4x4 previousMatrix = GUI.matrix;
            GUI.matrix = previousMatrix * Matrix4x4.Scale(new Vector3(scale, scale, 1f));
            float screenWidth = Screen.width / scale;
            float screenHeight = Screen.height / scale;
            GUI.DrawTexture(new Rect(0, 0, screenWidth, screenHeight), CozyGuiTheme.DimTex);

            float width = Mathf.Min(400f, screenWidth - 32f);
            float height = 250f;
            float x = (screenWidth - width) * 0.5f;
            float y = (screenHeight - height) * 0.5f;
            GUI.Box(new Rect(x, y, width, height), GUIContent.none, CozyGuiTheme.CardStyle);
            string title = isMainMenu ? "JEJAK ASA" : confirming ? "KEMBALI KE MENU?" : "JEDA";
            string description = isMainMenu ? "Mulai perjalanan baru" : confirming ? "Progres sesi ini akan hilang." : "Permainan dijeda.";
            GUI.Label(new Rect(x + 20f, y + 18f, width - 40f, 30f), title, CozyGuiTheme.RibbonStyle);
            GUI.Label(new Rect(x + 20f, y + 54f, width - 40f, 28f), description, CozyGuiTheme.OutcomeDescStyle);

            string first = isMainMenu ? "Mulai Permainan" : confirming ? "Batal" : "Lanjutkan";
            string second = isMainMenu ? "Keluar" : confirming ? "Ya, ke Menu" : "Menu Utama";
            if (GUI.Button(new Rect(x + 30f, y + 90f, width - 60f, 40f), (selected == 0 ? "> " : "") + first, CozyGuiTheme.TealBtnStyle))
                Activate(0);
            if (GUI.Button(new Rect(x + 30f, y + 138f, width - 60f, 40f), (selected == 1 ? "> " : "") + second, CozyGuiTheme.CoralBtnStyle))
                Activate(1);
            GUI.Label(new Rect(x + 12f, y + 196f, width - 24f, 40f), "[1/2] Pilih  |  Panah/Tab + Enter  |  Esc", CozyGuiTheme.HintStyle);
            GUI.matrix = previousMatrix;
        }
    }
}

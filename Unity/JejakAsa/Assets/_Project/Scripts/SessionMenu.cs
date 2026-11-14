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
        private enum MenuPage { Main, Load, Settings, ConfirmNew }
        private MenuPage page;
        private string notice = "";
        private const string VolumeKey = "JejakAsa_MasterVolume";
        private const string FullscreenKey = "JejakAsa_Fullscreen";

        public string MenuNotice => notice;


        private void Awake()
        {
            active = this;
            interaction = GetComponent<PlayerInteraction>();
            movement = GetComponent<PlayerController>();
            AudioListener.volume = Mathf.Clamp01(PlayerPrefs.GetFloat(VolumeKey, 1f));
            if (PlayerPrefs.HasKey(FullscreenKey))
                Screen.fullScreen = PlayerPrefs.GetInt(FullscreenKey) != 0;
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
                else if ((interaction == null || !interaction.IsDialogueOpen) && !EncounterController.AnyOpen &&
                    !StoryNpcController.AnyDialogueOpen && !StoryNpcController.EscapeConsumedThisFrame)
                {
                    Pause();
                }
                return;
            }

            if (!isMainMenu && !IsPaused)
                return;

            int count = isMainMenu ? (page == MenuPage.Main ? 5 : page == MenuPage.Load ? 4 : page == MenuPage.Settings ? 4 : 2) : 2;
            if (keyboard.upArrowKey.wasPressedThisFrame)
                selected = (selected + count - 1) % count;
            else if (keyboard.downArrowKey.wasPressedThisFrame || keyboard.tabKey.wasPressedThisFrame)
                selected = (selected + 1) % count;

            if (keyboard.digit1Key.wasPressedThisFrame) Activate(0);
            else if (keyboard.digit2Key.wasPressedThisFrame) Activate(1);
            else if (keyboard.digit3Key.wasPressedThisFrame && count > 2) Activate(2);
            else if (keyboard.digit4Key.wasPressedThisFrame && count > 3) Activate(3);
            else if (keyboard.digit5Key.wasPressedThisFrame && count > 4) Activate(4);
            else if (keyboard.enterKey.wasPressedThisFrame || keyboard.spaceKey.wasPressedThisFrame) Activate(selected);
            else if (isMainMenu && keyboard.escapeKey.wasPressedThisFrame && page != MenuPage.Main) ShowPage(MenuPage.Main);
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
            if (GameFlowManager.Instance != null)
                GameFlowManager.Instance.StartNewCampaign();

            SceneManager.LoadScene("Assets/_Project/Scenes/Kampung.unity", LoadSceneMode.Single);
        }

        public void StartCampaignFromHome()
        {
            Time.timeScale = 1f;
            if (GameFlowManager.Instance != null)
                GameFlowManager.Instance.StartNewCampaign();
            GameFlowManager.Instance.LastSpawnPoint = null;

            SceneManager.LoadScene("Assets/_Project/Scenes/Rumah.unity", LoadSceneMode.Single);
        }
        private void ShowPage(MenuPage next)
        {
            page = next;
            selected = 0;
            notice = "";
        }

        private void ContinueGame()
        {
            int latest = 0;
            string latestTime = "";
            for (int slot = 1; slot <= GameFlowManager.MaxSaveSlots; slot++)
            {
                if (!GameFlowManager.HasSave(slot)) continue;
                string time = PlayerPrefs.GetString("JejakAsa_SaveSlot_" + slot + "_Time", "");
                if (latest == 0 || string.CompareOrdinal(time, latestTime) > 0)
                {
                    latest = slot;
                    latestTime = time;
                }
            }
            if (latest == 0) notice = "Belum ada penyimpanan.";
            else LoadSlot(latest);
        }

        private void LoadSlot(int slot)
        {
            if (!GameFlowManager.HasSave(slot))
            {
                notice = "Slot " + slot + " kosong.";
                return;
            }
            if (!GameFlowManager.TryReadSave(slot, out GameProgressData data) ||
                (data.currentPhase != DayPhase.RumahPagi && data.currentPhase != DayPhase.RumahMalam &&
                 data.currentPhase != DayPhase.Prolog && data.currentPhase != DayPhase.Tamat))
            {
                notice = "Data slot " + slot + " tidak valid atau bukan save di rumah.";
                return;
            }
            if (!GameFlowManager.Instance.LoadGame(slot))
            {
                notice = "Gagal memuat slot " + slot + ".";
                return;
            }
            GameFlowManager.Instance.LastSpawnPoint = null;
            Time.timeScale = 1f;
            SceneManager.LoadScene("Assets/_Project/Scenes/Rumah.unity", LoadSceneMode.Single);
        }


        private void Activate(int choice)
        {
            if (isMainMenu)
            {
                if (page == MenuPage.Main)
                {
                    if (choice == 0) ShowPage(MenuPage.ConfirmNew);
                    else if (choice == 1) ContinueGame();
                    else if (choice == 2) ShowPage(MenuPage.Load);
                    else if (choice == 3) ShowPage(MenuPage.Settings);
                    else Application.Quit();
                }
                else if (page == MenuPage.ConfirmNew)
                {
                    if (choice == 0) ShowPage(MenuPage.Main);
                    else StartCampaignFromHome();
                }
                else if (page == MenuPage.Load)
                {
                    if (choice < 3) LoadSlot(choice + 1);
                    else ShowPage(MenuPage.Main);
                }
                else
                {
                    if (choice < 2)
                    {
                        AudioListener.volume = Mathf.Clamp01(AudioListener.volume + (choice == 0 ? -0.1f : 0.1f));
                        PlayerPrefs.SetFloat(VolumeKey, AudioListener.volume);
                    }
                    else if (choice == 2)
                    {
                        Screen.fullScreen = !Screen.fullScreen;
                        PlayerPrefs.SetInt(FullscreenKey, Screen.fullScreen ? 1 : 0);
                    }
                    else { ShowPage(MenuPage.Main); return; }
                    PlayerPrefs.Save();
                }
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

            float width = Mathf.Min(440f, screenWidth - 32f);
            int count = isMainMenu ? (page == MenuPage.Main ? 5 : page == MenuPage.Load ? 4 : page == MenuPage.Settings ? 4 : 2) : 2;
            float height = 150f + count * 48f;
            float x = (screenWidth - width) * 0.5f;
            float y = (screenHeight - height) * 0.5f;
            GUI.Box(new Rect(x, y, width, height), GUIContent.none, CozyGuiTheme.CardStyle);
            string title = isMainMenu ? (page == MenuPage.Load ? "MUAT PERMAINAN" : page == MenuPage.Settings ? "PENGATURAN" : page == MenuPage.ConfirmNew ? "PERMAINAN BARU?" : "JEJAK ASA") : confirming ? "KEMBALI KE MENU?" : "JEDA";
            string description = isMainMenu ? (page == MenuPage.ConfirmNew ? "Progres sesi ini akan hilang. Save tetap aman." : notice) : confirming ? "Progres sesi ini akan hilang." : "Permainan dijeda.";
            GUI.Label(new Rect(x + 20f, y + 18f, width - 40f, 30f), title, CozyGuiTheme.RibbonStyle);
            GUI.Label(new Rect(x + 20f, y + 54f, width - 40f, 28f), description, CozyGuiTheme.OutcomeDescStyle);

            for (int i = 0; i < count; i++)
            {
                string label;
                if (!isMainMenu) label = confirming ? (i == 0 ? "Batal" : "Ya, ke Menu") : (i == 0 ? "Lanjutkan" : "Menu Utama");
                else if (page == MenuPage.Main) label = new[] { "Permainan Baru", "Lanjutkan", "Muat", "Pengaturan", "Keluar" }[i];
                else if (page == MenuPage.ConfirmNew) label = i == 0 ? "Batal" : "Ya, mulai baru";
                else if (page == MenuPage.Load) label = i == 3 ? "Kembali" : "Slot " + (i + 1) + (GameFlowManager.HasSave(i + 1) ? " — " + PlayerPrefs.GetString("JejakAsa_SaveSlot_" + (i + 1) + "_Time", "-") : " — Kosong");
                else label = i == 0 ? "Volume - (" + Mathf.RoundToInt(AudioListener.volume * 100) + "%)" : i == 1 ? "Volume +" : i == 2 ? "Layar penuh: " + (Screen.fullScreen ? "Ya" : "Tidak") : "Kembali";
                if (GUI.Button(new Rect(x + 30f, y + 90f + i * 48f, width - 60f, 40f), (selected == i ? "> " : "") + label, i == 0 ? CozyGuiTheme.TealBtnStyle : CozyGuiTheme.CoralBtnStyle))
                    Activate(i);
            }
            GUI.Label(new Rect(x + 12f, y + height - 50f, width - 24f, 40f), "[1-5] Pilih  |  Panah/Tab + Enter  |  Esc", CozyGuiTheme.HintStyle);
            GUI.matrix = previousMatrix;
        }
    }
}

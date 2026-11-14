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
        private enum MenuPage { Main, Load, Settings, ConfirmNew, Items, Equipment, Exit, ConfirmQuit, ConfirmLoad }
        private MenuPage page;
        private int pendingLoadSlot;
        private BattleController pendingBattleAction;
        private bool pendingHealing;
        private Vector2 menuScroll;
        private GUIStyle rpgText;
        private GUIStyle rpgSmall;
        private GUIStyle rpgButton;
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
            pendingBattleAction = null;
            if (IsPaused)
                Resume();
            if (active == this)
                active = null;
        }

        private void Update()
        {
            if (pendingBattleAction != null && !IsPaused && Time.frameCount != blockedFrame)
            {
                BattleController battle = pendingBattleAction;
                pendingBattleAction = null;
                if (!(pendingHealing ? battle.PlayerUseHealing(true) : battle.EquipBasicWeapon()))
                    Debug.LogWarning("Aksi dari menu tidak dapat dijalankan karena kondisi battle berubah.");
            }
            Keyboard keyboard = Keyboard.current;
            if (keyboard == null)
                return;

            if (!isMainMenu && keyboard.escapeKey.wasPressedThisFrame)
            {
                if (IsPaused)
                {
                    if (confirming) CancelReturnToMenu();
                    else if (page == MenuPage.ConfirmLoad) ShowPage(MenuPage.Load);
                    else if (page == MenuPage.ConfirmQuit) ShowPage(MenuPage.Exit);
                    else if (page != MenuPage.Main) ShowPage(MenuPage.Main);
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

            int count = OptionCount();
            if (keyboard.upArrowKey.wasPressedThisFrame)
                selected = (selected + count - 1) % count;
            else if (keyboard.downArrowKey.wasPressedThisFrame || keyboard.tabKey.wasPressedThisFrame)
                selected = (selected + 1) % count;

            if (keyboard.digit1Key.wasPressedThisFrame) Activate(0);
            else if (keyboard.digit2Key.wasPressedThisFrame) Activate(1);
            else if (keyboard.digit3Key.wasPressedThisFrame && count > 2) Activate(2);
            else if (keyboard.digit4Key.wasPressedThisFrame && count > 3) Activate(3);
            else if (keyboard.digit5Key.wasPressedThisFrame && count > 4) Activate(4);
            else if (keyboard.digit6Key.wasPressedThisFrame && count > 5) Activate(5);
            else if (keyboard.digit7Key.wasPressedThisFrame && count > 6) Activate(6);
            else if (keyboard.digit8Key.wasPressedThisFrame && count > 7) Activate(7);
            else if (keyboard.digit9Key.wasPressedThisFrame && count > 8) Activate(8);
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
            page = MenuPage.Main;
            notice = "";
            pendingLoadSlot = 0;
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
            page = MenuPage.Main;
            pendingLoadSlot = 0;
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
            page = MenuPage.Exit;
        }

        public void CancelReturnToMenu()
        {
            if (!IsPaused)
                return;
            confirming = false;
            selected = 0;
            page = MenuPage.Exit;
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
            menuScroll = Vector2.zero;
            notice = "";
        }

        private int OptionCount()
        {
            if (isMainMenu)
                return page == MenuPage.Main ? 5 : page == MenuPage.Load || page == MenuPage.Settings ? 4 : 2;
            if (confirming || page == MenuPage.ConfirmQuit || page == MenuPage.ConfirmLoad)
                return 2;
            if (page == MenuPage.Main) return 6;
            if (page == MenuPage.Items) return ItemCount() + 1;
            if (page == MenuPage.Equipment) return 2;
            if (page == MenuPage.Settings || page == MenuPage.Load) return 4;
            return 3;
        }

        private int ItemCount()
        {
            var inventory = GameFlowManager.Instance.Progress.inventory;
            int count = 0;
            for (int i = 0; i < inventory.Count; i++)
                if (!string.IsNullOrEmpty(inventory[i]) && inventory[i] != BattleController.BasicWeaponId &&
                    inventory.IndexOf(inventory[i]) == i)
                    count++;
            return count;
        }

        private string ItemAt(int index)
        {
            var inventory = GameFlowManager.Instance.Progress.inventory;
            for (int i = 0; i < inventory.Count; i++)
            {
                string item = inventory[i];
                if (string.IsNullOrEmpty(item) || item == BattleController.BasicWeaponId || inventory.IndexOf(item) != i)
                    continue;
                if (index-- == 0) return item;
            }
            return null;
        }

        private int ItemQuantity(string item)
        {
            int count = 0;
            foreach (string entry in GameFlowManager.Instance.Progress.inventory)
                if (entry == item) count++;
            return count;
        }

        private static string ItemName(string item)
        {
            return item == BattleController.HealingItemId ? "Pemulih HP" : item;
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
            if (!isMainMenu && page != MenuPage.ConfirmLoad)
            {
                pendingLoadSlot = slot;
                ShowPage(MenuPage.ConfirmLoad);
                return;
            }
            if (!Application.CanStreamedLevelBeLoaded("Assets/_Project/Scenes/Rumah.unity"))
            {
                notice = "Scene rumah tidak tersedia; save tidak dimuat.";
                return;
            }
            if (!GameFlowManager.Instance.LoadGame(slot))
            {
                notice = "Gagal memuat slot " + slot + ".";
                return;
            }
            if (IsPaused) Resume();
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
                    ApplySetting(choice);
            }
            else if (confirming)
            {
                if (choice == 0) CancelReturnToMenu();
                else ConfirmReturnToMenu();
            }
            else if (page == MenuPage.Main)
            {
                if (choice == 0) Resume();
                else if (choice == 1) ShowPage(MenuPage.Items);
                else if (choice == 2) ShowPage(MenuPage.Equipment);
                else if (choice == 3) ShowPage(MenuPage.Settings);
                else if (choice == 4) ShowPage(MenuPage.Load);
                else ShowPage(MenuPage.Exit);
            }
            else if (page == MenuPage.Items)
            {
                string item = ItemAt(choice);
                if (item == null) ShowPage(MenuPage.Main);
                else if (item != BattleController.HealingItemId)
                    notice = "Catatan hadiah ini tidak dapat digunakan sebagai item battle.";
                else
                {
                    BattleController battle = BattleController.Active;
                    if (battle == null || battle.IsBattleOver)
                        notice = "Item pemulih hanya dapat digunakan saat battle.";
                    else if (!battle.IsPlayerTurn || battle.ActionLocked)
                        notice = "Tunggu giliranmu untuk menggunakan item.";
                    else if (battle.PlayerHP >= battle.playerMaxHP)
                        notice = "HP sudah penuh.";
                    else
                    {
                        pendingBattleAction = battle;
                        pendingHealing = true;
                        Resume();
                    }
                }
            }
            else if (page == MenuPage.Equipment)
            {
                if (choice == 1) { ShowPage(MenuPage.Main); return; }
                var progress = GameFlowManager.Instance.Progress;
                if (!progress.inventory.Contains(BattleController.BasicWeaponId))
                    notice = "Senjata dasar belum dimiliki.";
                else if (BattleController.Active != null && !BattleController.Active.IsBattleOver)
                {
                    BattleController battle = BattleController.Active;
                    if (!battle.IsPlayerTurn || battle.ActionLocked)
                        notice = "Tunggu giliranmu untuk mengganti senjata.";
                    else
                    {
                        pendingBattleAction = battle;
                        pendingHealing = false;
                        Resume();
                    }
                }
                else
                {
                    progress.equippedWeapon = progress.equippedWeapon == BattleController.BasicWeaponId ? "" : BattleController.BasicWeaponId;
                    notice = progress.equippedWeapon == "" ? "Senjata dilepas." : "Senjata dasar dipasang.";
                }
            }
            else if (page == MenuPage.Settings) ApplySetting(choice);
            else if (page == MenuPage.Load)
            {
                if (choice < 3) LoadSlot(choice + 1);
                else ShowPage(MenuPage.Main);
            }
            else if (page == MenuPage.ConfirmLoad)
            {
                if (choice == 0) ShowPage(MenuPage.Load);
                else LoadSlot(pendingLoadSlot);
            }
            else if (page == MenuPage.ConfirmQuit)
            {
                if (choice == 0) ShowPage(MenuPage.Exit);
                else Application.Quit();
            }
            else if (page == MenuPage.Exit)
            {
                if (choice == 0) ShowPage(MenuPage.Main);
                else if (choice == 1) RequestReturnToMenu();
                else ShowPage(MenuPage.ConfirmQuit);
            }
        }

        private void ApplySetting(int choice)
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

        private static void DrawRpgPanel(Rect rect)
        {
            Color previous = GUI.color;
            GUI.color = new Color(0.73f, 0.84f, 0.94f, 1f);
            GUI.DrawTexture(rect, Texture2D.whiteTexture);
            GUI.color = new Color(0.07f, 0.15f, 0.33f, 1f);
            GUI.DrawTexture(new Rect(rect.x + 3f, rect.y + 3f, rect.width - 6f, rect.height - 6f), Texture2D.whiteTexture);
            GUI.color = previous;
        }

        private string PauseOption(int index)
        {
            if (confirming) return index == 0 ? "Batal" : "Ya, ke menu utama";
            if (page == MenuPage.ConfirmQuit) return index == 0 ? "Batal" : "Ya, keluar game";
            if (page == MenuPage.ConfirmLoad) return index == 0 ? "Batal" : "Ya, muat slot " + pendingLoadSlot;
            if (page == MenuPage.Main)
            {
                switch (index)
                {
                    case 0: return "Lanjutkan";
                    case 1: return "Item";
                    case 2: return "Senjata";
                    case 3: return "Pengaturan";
                    case 4: return "Muat";
                    default: return "Keluar";
                }
            }
            if (page == MenuPage.Items)
            {
                string item = ItemAt(index);
                return item == null ? "Kembali" : ItemName(item) + "  x" + ItemQuantity(item);
            }
            if (page == MenuPage.Equipment)
                return index == 0 ? "Senjata dasar  +5" : "Kembali";
            if (page == MenuPage.Settings)
                return index == 0 ? "Volume -  (" + Mathf.RoundToInt(AudioListener.volume * 100) + "%)" :
                    index == 1 ? "Volume +" : index == 2 ? "Layar penuh: " + (Screen.fullScreen ? "Ya" : "Tidak") : "Kembali";
            if (page == MenuPage.Load)
                return index == 3 ? "Kembali" : "Slot " + (index + 1) + (GameFlowManager.HasSave(index + 1) ? "  —  Tersimpan" : "  —  Kosong");
            return index == 0 ? "Kembali" : index == 1 ? "Menu utama" : "Keluar game";
        }

        private void DrawPauseMenu()
        {
            if (rpgText == null)
            {
                rpgText = new GUIStyle(GUI.skin.label) { fontSize = 20, fontStyle = FontStyle.Bold };
                rpgText.normal.textColor = Color.white;
                rpgSmall = new GUIStyle(GUI.skin.label) { fontSize = 15, wordWrap = true };
                rpgSmall.normal.textColor = new Color(0.86f, 0.91f, 1f);
                rpgButton = new GUIStyle { fontSize = 17, alignment = TextAnchor.MiddleLeft, padding = new RectOffset(12, 4, 2, 2) };
                rpgButton.normal.textColor = rpgButton.hover.textColor = rpgButton.active.textColor = Color.white;
            }

            GUI.depth = -1000;
            Matrix4x4 previousMatrix = GUI.matrix;
            float scale = CozyGuiTheme.Scale;
            GUI.matrix = previousMatrix * Matrix4x4.Scale(new Vector3(scale, scale, 1f));
            float sw = Screen.width / scale;
            float sh = Screen.height / scale;
            GUI.DrawTexture(new Rect(0, 0, sw, sh), CozyGuiTheme.DimTex);
            float width = Mathf.Min(800f, sw - 32f);
            float height = Mathf.Min(390f, sh - 32f);
            float x = (sw - width) * 0.5f;
            float y = (sh - height) * 0.5f;
            float commandWidth = width * 0.37f;
            Rect status = new Rect(x, y, width - commandWidth - 10f, height);
            Rect commands = new Rect(status.xMax + 10f, y, commandWidth, height);
            DrawRpgPanel(status);
            DrawRpgPanel(commands);

            var progress = GameFlowManager.Instance.Progress;
            GUI.Label(new Rect(status.x + 20f, y + 18f, status.width - 40f, 30f), "VICKY", rpgText);
            GUI.Label(new Rect(status.x + 20f, y + 54f, status.width - 40f, 24f),
                $"Hari {progress.currentDay}     Level {progress.currentLevel}", rpgSmall);
            string detail;
            if (confirming) detail = "Kembali ke menu utama? Progres sesi yang belum disimpan akan hilang.";
            else if (page == MenuPage.ConfirmQuit) detail = "Keluar dari game? Progres sesi yang belum disimpan akan hilang.";
            else if (page == MenuPage.ConfirmLoad) detail = "Muat slot " + pendingLoadSlot + "? Progres sesi yang belum disimpan akan diganti.";
            else if (page == MenuPage.Items)
            {
                string item = ItemAt(selected);
                detail = item == null ? "Tidak ada item yang dipilih." : ItemName(item) + "\nJumlah: " + ItemQuantity(item) +
                    (item == BattleController.HealingItemId ? "\nPulihkan hingga 12 HP; pilih saat giliran battle." : "\nCatatan hadiah; tidak dapat digunakan di battle.");
            }
            else if (page == MenuPage.Equipment)
                detail = progress.inventory.Contains(BattleController.BasicWeaponId)
                    ? "Senjata dasar: +5 serangan\n" + (progress.equippedWeapon == BattleController.BasicWeaponId ? "Status: dipasang" : "Status: belum dipasang") + "\nPilih untuk mengganti."
                    : "Senjata dasar belum dimiliki.\nDiperoleh dari reward level 2.";
            else if (page == MenuPage.Settings) detail = "Atur volume dan mode layar. Pengaturan tersimpan di perangkat ini.";
            else if (page == MenuPage.Load) detail = "Pilih slot save dari rumah.\nMemuat akan meminta konfirmasi terlebih dahulu.";
            else if (page == MenuPage.Exit) detail = "Pilih kembali ke menu utama atau keluar game.";
            else detail = "Permainan dijeda.\nPilih perintah di sebelah kanan.";
            GUI.Label(new Rect(status.x + 20f, y + 94f, status.width - 40f, height - 165f), detail, rpgSmall);
            if (!string.IsNullOrEmpty(notice))
                GUI.Label(new Rect(status.x + 20f, y + height - 72f, status.width - 40f, 42f), notice, rpgSmall);
            GUI.Label(new Rect(status.x + 20f, y + height - 28f, status.width - 40f, 20f), "Panah / Tab + Enter   •   Esc kembali", rpgSmall);

            string heading = confirming || page == MenuPage.ConfirmQuit || page == MenuPage.ConfirmLoad ? "KONFIRMASI" :
                page == MenuPage.Items ? "ITEM" : page == MenuPage.Equipment ? "SENJATA" :
                page == MenuPage.Settings ? "PENGATURAN" : page == MenuPage.Load ? "MUAT" : page == MenuPage.Exit ? "KELUAR" : "PERINTAH";
            GUI.Label(new Rect(commands.x + 14f, y + 16f, commands.width - 28f, 28f), heading, rpgText);
            int count = OptionCount();
            float rowHeight = 38f;
            float viewHeight = height - 62f;
            if (selected * rowHeight < menuScroll.y) menuScroll.y = selected * rowHeight;
            if ((selected + 1) * rowHeight > menuScroll.y + viewHeight) menuScroll.y = (selected + 1) * rowHeight - viewHeight;
            Rect viewport = new Rect(commands.x + 8f, y + 48f, commands.width - 16f, viewHeight);
            menuScroll = GUI.BeginScrollView(viewport, menuScroll, new Rect(0f, 0f, viewport.width - 18f, count * rowHeight));
            for (int i = 0; i < count; i++)
            {
                Rect row = new Rect(0f, i * rowHeight, viewport.width - 20f, rowHeight - 2f);
                if (i == selected)
                {
                    Color old = GUI.color;
                    GUI.color = new Color(0.23f, 0.39f, 0.68f, 1f);
                    GUI.DrawTexture(row, Texture2D.whiteTexture);
                    GUI.color = old;
                }
                if (GUI.Button(row, (i == selected ? "> " : "  ") + PauseOption(i), rpgButton))
                {
                    selected = i;
                    Activate(i);
                }
            }
            GUI.EndScrollView();
            GUI.matrix = previousMatrix;
        }

        private void OnGUI()
        {
            if (!isMainMenu && !IsPaused)
                return;

            CozyGuiTheme.EnsureInitialized();
            if (!isMainMenu)
            {
                DrawPauseMenu();
                return;
            }
            GUI.depth = -1000;
            float scale = CozyGuiTheme.Scale;
            Matrix4x4 previousMatrix = GUI.matrix;
            GUI.matrix = previousMatrix * Matrix4x4.Scale(new Vector3(scale, scale, 1f));
            float screenWidth = Screen.width / scale;
            float screenHeight = Screen.height / scale;
            GUI.DrawTexture(new Rect(0, 0, screenWidth, screenHeight), CozyGuiTheme.DimTex);

            float width = Mathf.Min(440f, screenWidth - 32f);
            int count = OptionCount();
            float height = 150f + count * 48f;
            float x = (screenWidth - width) * 0.5f;
            float y = (screenHeight - height) * 0.5f;
            GUI.Box(new Rect(x, y, width, height), GUIContent.none, CozyGuiTheme.CardStyle);
            string title = page == MenuPage.Load ? "MUAT PERMAINAN" : page == MenuPage.Settings ? "PENGATURAN" : page == MenuPage.ConfirmNew ? "PERMAINAN BARU?" : "JEJAK ASA";
            string description = page == MenuPage.ConfirmNew ? "Progres sesi ini akan hilang. Save tetap aman." : notice;
            GUI.Label(new Rect(x + 20f, y + 18f, width - 40f, 30f), title, CozyGuiTheme.RibbonStyle);
            GUI.Label(new Rect(x + 20f, y + 54f, width - 40f, 28f), description, CozyGuiTheme.OutcomeDescStyle);

            for (int i = 0; i < count; i++)
            {
                string label;
                if (page == MenuPage.Main) label = new[] { "Permainan Baru", "Lanjutkan", "Muat", "Pengaturan", "Keluar" }[i];
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

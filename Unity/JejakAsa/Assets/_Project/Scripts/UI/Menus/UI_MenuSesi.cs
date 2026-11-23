using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;

namespace JejakAsa
{
    [DefaultExecutionOrder(-1000)]
    [DisallowMultipleComponent]
    public sealed class UI_MenuSesi : MonoBehaviour
    {
        public bool isMainMenu;
        public bool IsPaused { get; private set; }

        private static UI_MenuSesi active;
        private static int blockedFrame = -1;
        internal static bool InputBlocked => (active != null && active.IsPaused) || blockedFrame == Time.frameCount || P03_SponsorDanPosko.PresentationActive;

        private Mission_PengantaranBantuan interaction;
        private Player_Gerak movement;
        private EventSystem pausedEventSystem;
        private bool movementWasEnabled;
        private bool confirming;
        private int selected;
        private float previousTimeScale;
        private enum MenuPage { Main, Load, Settings, ConfirmNew, Exit, ConfirmQuit, ConfirmLoad }
        private MenuPage page;
        private int pendingLoadSlot;
        private Texture2D menuBackground;
        private Texture2D titleLogo;
        private float pageOpenedAt;
        private float highlightRow;
        private Vector2 lastPointerPosition;
        private bool hasPointerPosition;
        private string notice = "";
        private const string VolumeKey = "JejakAsa_MasterVolume";
        private const string FullscreenKey = "JejakAsa_Fullscreen";
        private static readonly string[] MainMenuOptions = { "Permainan Baru", "Lanjutkan", "Muat Data", "Pengaturan", "Keluar" };

        public string MenuNotice => notice;

        // Dipakai Canvas "UI_MenuUtama" (tampilan uGUI pixel). Kalau true, menu utama versi OnGUI ga digambar,
        // tapi logika (halaman, pilihan, keyboard) tetap jalan di sini.
        public bool pakaiMenuBaru;
        public string HalamanSekarang => page.ToString();
        public int PilihanSekarang => selected;
        public int JumlahPilihan => OptionCount();
        public string KeteranganHalaman => PageDescription();
        public string LabelPilihan(int i) => RowLabel(i);
        public void SorotPilihan(int i) { if (i >= 0 && i < OptionCount()) selected = i; }
        public void PilihDariTombol(int i) { if (i < 0 || i >= OptionCount()) return; selected = i; Activate(i); }


        private void Awake()
        {
            active = this;
            interaction = GetComponent<Mission_PengantaranBantuan>();
            movement = GetComponent<Player_Gerak>();
            menuBackground = Resources.Load<Texture2D>("JejakAsaUI/MenuBackground");
            titleLogo = Resources.Load<Texture2D>("JejakAsaUI/Generated/JejakAsaLogo");
            pageOpenedAt = Time.unscaledTime;
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
            if (P03_SponsorDanPosko.PresentationActive) return;
            UpdatePointerSelection();
            highlightRow = Mathf.MoveTowards(highlightRow, selected, Time.unscaledDeltaTime * 10f);
            Keyboard keyboard = Keyboard.current;
            if (keyboard == null)
                return;

            if (!isMainMenu && keyboard.escapeKey.wasPressedThisFrame)
            {
                if (!IsPaused && (Interaction_Rumah.AnyMenuOpen || Interaction_GerbangPosko.AnyMenuOpen)) return;
                if (IsPaused)
                {
                    if (confirming) CancelReturnToMenu();
                    else if (page == MenuPage.ConfirmLoad) ShowPage(MenuPage.Load);
                    else if (page == MenuPage.ConfirmQuit) ShowPage(MenuPage.Exit);
                    else if (page != MenuPage.Main) ShowPage(MenuPage.Main);
                    else Resume();
                }
                else if ((interaction == null || !interaction.IsDialogueOpen) &&
                    !StoryDialog_InteraksiNPC.AnyDialogueOpen && !StoryDialog_InteraksiNPC.EscapeConsumedThisFrame)
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
            else if (keyboard.enterKey.wasPressedThisFrame || keyboard.spaceKey.wasPressedThisFrame) Activate(selected);
            else if (isMainMenu && keyboard.escapeKey.wasPressedThisFrame && page != MenuPage.Main) ShowPage(MenuPage.Main);
        }

        private void UpdatePointerSelection()
        {
            if ((!isMainMenu && !IsPaused) || (isMainMenu && pakaiMenuBaru) || Mouse.current == null)
                return;
            Vector2 position = Mouse.current.position.ReadValue();
            bool moved = !hasPointerPosition || (position - lastPointerPosition).sqrMagnitude > .01f;
            lastPointerPosition = position;
            hasPointerPosition = true;
            // A resting mouse must not take selection back from keyboard navigation.
            if (!moved && !Mouse.current.leftButton.wasPressedThisFrame) return;
            float scale = UI_TemaVisual.Scale;
            Vector2 point = new Vector2(position.x, Screen.height - position.y) / scale;
            Rect area = RowsArea();
            float height = page == MenuPage.Load ? 76 : 62;
            for (int i = 0; i < OptionCount(); i++)
                if (new Rect(area.x, area.y + i * height, area.width, height - 8).Contains(point))
                {
                    selected = i;
                    return;
                }
        }

        private Rect RowsArea()
        {
            float scale = UI_TemaVisual.Scale;
            float sw = Screen.width / scale, sh = Screen.height / scale;
            if (isMainMenu && page == MenuPage.Main)
            {
                float entry = 12 * (1 - Mathf.SmoothStep(0, 1, Mathf.Clamp01((Time.unscaledTime - pageOpenedAt) / .2f)));
                return new Rect(64, (sh - 656) / 2 + entry + 334, 366, 285);
            }
            float cardHeight = Mathf.Min(656, 280 + OptionCount() * 64);
            return new Rect((sw - 720) / 2 + 60, (sh - cardHeight) / 2 + 214, 600, 320);
        }

        public void Pause()
        {
            if (isMainMenu || IsPaused || P03_SponsorDanPosko.PresentationActive)
                return;
            active = this;
            IsPaused = true;
            blockedFrame = Time.frameCount;
            previousTimeScale = Time.timeScale;
            Time.timeScale = 0f;
            selected = 0;
            confirming = false;
            page = MenuPage.Main;
            pageOpenedAt = Time.unscaledTime;
            highlightRow = 0f;
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
            pageOpenedAt = Time.unscaledTime;
            highlightRow = 0f;
        }

        public void CancelReturnToMenu()
        {
            if (!IsPaused)
                return;
            confirming = false;
            selected = 0;
            page = MenuPage.Exit;
            pageOpenedAt = Time.unscaledTime;
            highlightRow = 0f;
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
            if (StoryFlow_PengaturAlur.Instance != null)
                StoryFlow_PengaturAlur.Instance.StartNewCampaign();

            SceneManager.LoadScene("Assets/_Project/Scenes/Kampung.unity", LoadSceneMode.Single);
        }

        public void StartCampaignFromHome()
        {
            Time.timeScale = 1f;
            if (StoryFlow_PengaturAlur.Instance != null)
                StoryFlow_PengaturAlur.Instance.StartNewCampaign(true);
            StoryFlow_PengaturAlur.Instance.LastSpawnPoint = null;

            SceneManager.LoadScene("Assets/_Project/Scenes/Rumah.unity", LoadSceneMode.Single);
        }
        private void ShowPage(MenuPage next)
        {
            page = next;
            selected = 0;
            pageOpenedAt = Time.unscaledTime;
            highlightRow = 0f;
            notice = "";
        }

        private int OptionCount()
        {
            if (isMainMenu)
            {
                if (page == MenuPage.Main) return MainMenuOptions.Length;
                if (page == MenuPage.Load || page == MenuPage.Settings) return 4;
                return 2;
            }
            if (confirming || page == MenuPage.ConfirmQuit || page == MenuPage.ConfirmLoad)
                return 2;
            if (page == MenuPage.Main || page == MenuPage.Settings || page == MenuPage.Load)
                return 4;
            return 3;
        }


        private void ContinueGame()
        {
            int latest = 0;
            string latestTime = "";
            for (int slot = 1; slot <= StoryFlow_PengaturAlur.MaxSaveSlots; slot++)
            {
                if (!StoryFlow_PengaturAlur.HasSave(slot)) continue;
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
            if (!StoryFlow_PengaturAlur.HasSave(slot))
            {
                notice = "Slot " + slot + " kosong.";
                return;
            }
            if (!StoryFlow_PengaturAlur.TryReadSave(slot, out GameProgressData data) ||
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
            if (!StoryFlow_PengaturAlur.Instance.LoadGame(slot))
            {
                notice = "Gagal memuat slot " + slot + ".";
                return;
            }
            if (IsPaused) Resume();
            StoryFlow_PengaturAlur.Instance.LastSpawnPoint = null;
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
                    else ShowPage(MenuPage.ConfirmQuit);
                }
                else if (page == MenuPage.ConfirmNew)
                {
                    if (choice == 0) ShowPage(MenuPage.Main);
                    else StartCampaignFromHome();
                }
                else if (page == MenuPage.ConfirmQuit)
                {
                    if (choice == 0) ShowPage(MenuPage.Main);
                    else Application.Quit();
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
                else if (choice == 1) ShowPage(MenuPage.Settings);
                else if (choice == 2) ShowPage(MenuPage.Load);
                else ShowPage(MenuPage.Exit);
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

        private string RowLabel(int i)
        {
            if (confirming) return i == 0 ? "Kembali" : "Ya, ke menu utama";
            if (page == MenuPage.ConfirmQuit) return i == 0 ? "Kembali" : "Ya, keluar game";
            if (page == MenuPage.ConfirmLoad) return i == 0 ? "Kembali" : "Ya, muat perjalanan";
            if (page == MenuPage.ConfirmNew) return i == 0 ? "Kembali" : "Mulai perjalanan";
            if (page == MenuPage.Load)
            {
                if (i == 3) return "Kembali";
                var meta = StoryFlow_PengaturAlur.GetSlotMeta(i + 1);
                return "Slot 0" + (i + 1) + (meta.exists ? "     Hari " + meta.day + " / 5" : "     Belum tersimpan");
            }
            if (page == MenuPage.Settings)
                return i == 0 ? "Volume turun     " + Mathf.RoundToInt(AudioListener.volume * 100) + "%" :
                    i == 1 ? "Volume naik" : i == 2 ? "Layar penuh     " + (Screen.fullScreen ? "Aktif" : "Nonaktif") : "Kembali";
            if (isMainMenu) return MainMenuOptions[i];
            if (page == MenuPage.Exit) return i == 0 ? "Kembali" : i == 1 ? "Menu utama" : "Keluar game";
            return i == 0 ? "Lanjutkan perjalanan" : i == 1 ? "Pengaturan" : i == 2 ? "Muat perjalanan" : "Keluar";
        }

        private void DrawRows(Rect area)
        {
            int count = OptionCount();
            float rowHeight = page == MenuPage.Load ? 76 : 62;
            // A fixed set of at most five choices fits at the shared 1280 x 720 reference size.
            for (int i = 0; i < count; i++)
            {
                Rect row = new Rect(area.x, area.y + i * rowHeight, area.width, rowHeight - 8);
                bool chosen = i == selected;
                GUIStyle style = chosen ? UI_TemaVisual.TealBtnStyle : UI_TemaVisual.AmberBtnStyle;
                if (UI_TemaVisual.Button(row, GUIContent.none, style, false)) { selected = i; Activate(i); break; }
                GUIStyle label = chosen ? UI_TemaVisual.White : UI_TemaVisual.ButtonLabel;
                GUI.Label(new Rect(row.x + 16, row.y, 28, row.height), (i + 1).ToString("00"), chosen ? UI_TemaVisual.White : UI_TemaVisual.Small);
                GUI.Label(new Rect(row.x + 54, row.y, row.width - 80, row.height), RowLabel(i), label);
                GUI.Label(new Rect(row.xMax - 28, row.y, 20, row.height), chosen ? ">" : "", label);
                if (page == MenuPage.Load && i < 3)
                {
                    var meta = StoryFlow_PengaturAlur.GetSlotMeta(i + 1);
                    GUI.Label(new Rect(row.x + 16, row.yMax + 1, row.width - 20, 18), meta.exists ? meta.timestamp : "Simpan progres melalui tempat tidur di rumah.", UI_TemaVisual.Small);
                }
            }
        }

        private string PageTitle()
        {
            if (confirming) return "Kembali ke awal?";
            return page switch
            {
                MenuPage.Load => "Jejak tersimpan", MenuPage.Settings => "Atur kenyamanan",
                MenuPage.ConfirmNew => "Langkah pertama", MenuPage.ConfirmQuit => "Sampai jumpa?",
                MenuPage.ConfirmLoad => "Lanjutkan jejak ini?", MenuPage.Exit => "Akhiri sejenak",
                _ => "Ambil waktu sejenak."
            };
        }

        private string PageDescription()
        {
            if (confirming) return "Progres yang belum disimpan akan hilang saat kembali ke menu utama.";
            return page switch
            {
                MenuPage.Load => "Pilih satu dari tiga catatan perjalananmu.",
                MenuPage.Settings => "Sesuaikan suara dan tampilan untuk perjalananmu.",
                MenuPage.ConfirmNew => "Mulai dari rumah Vicky pada hari pertama. Slot simpan lama tetap tersedia.",
                MenuPage.ConfirmQuit => "Progres yang belum disimpan akan hilang saat keluar.",
                MenuPage.ConfirmLoad => "Muat slot " + pendingLoadSlot + ". Progres sesi yang belum disimpan akan diganti.",
                MenuPage.Exit => "Simpan di rumah sebelum mengakhiri perjalanan.",
                _ => "Kampung menunggu. Lanjutkan saat kamu siap."
            };
        }

        private void DrawJournal(Rect r)
        {
            UI_TemaVisual.Header(r, "CATATAN RELAWAN", "Setiap langkah berarti.", "Bantuan sampai, harapan tumbuh.");
            var flow = StoryFlow_PengaturAlur.Instance;
            GUI.Label(new Rect(r.x + 28, r.y + 194, r.width - 56, 20), "TUJUAN SAAT INI", UI_TemaVisual.Eyebrow);
            GUI.Label(new Rect(r.x + 28, r.y + 224, r.width - 56, 68), UI_TujuanDanProgres.GetObjective(flow.Progress), UI_TemaVisual.LabelStyle);
            UI_TemaVisual.Rule(r.x + 28, r.y + 312, r.width - 56);
            GUI.Label(new Rect(r.x + 28, r.y + 334, r.width - 56, 24), "Hari " + flow.CurrentDay + " / 5  ·  " + UI_TemaVisual.PhaseName(flow.CurrentPhase), UI_TemaVisual.Body);
            UI_TemaVisual.Journey(new Rect(r.x + 28, r.y + 396, r.width - 70, 100), flow.CurrentDay, flow.CurrentPhase == DayPhase.Tamat ? 5 : flow.CurrentDay - 1);
            GUI.Label(new Rect(r.x + 28, r.y + 536, r.width - 56, 48), "WASD  Bergerak     Shift  Berlari\nE  Berinteraksi        Esc  Jeda", UI_TemaVisual.Small);
        }

        private void OnGUI()
        {
            if (!isMainMenu && !IsPaused) return;
            if (isMainMenu && pakaiMenuBaru) return;
            UI_TemaVisual.EnsureInitialized();
            GUI.depth = -1000;
            Matrix4x4 old = GUI.matrix;
            float scale = UI_TemaVisual.Scale;
            GUI.matrix = old * Matrix4x4.Scale(new Vector3(scale, scale, 1));
            float sw = Screen.width / scale, sh = Screen.height / scale;
            if (isMainMenu && menuBackground != null) GUI.DrawTexture(new Rect(0, 0, sw, sh), menuBackground, ScaleMode.ScaleAndCrop);
            else GUI.DrawTexture(new Rect(0, 0, sw, sh), UI_TemaVisual.DimTex);
            float entry = 12 * (1 - Mathf.SmoothStep(0, 1, Mathf.Clamp01((Time.unscaledTime - pageOpenedAt) / .2f)));
            float y = (sh - 656) / 2 + entry;
            bool cover = isMainMenu && page == MenuPage.Main;
            if (cover)
            {
                Rect sheet = new Rect(32, y, 430, 656);
                UI_TemaVisual.Fill(new Rect(0,0,490,sh), new Color(.04f,.10f,.07f,.55f));

                if (titleLogo != null) GUI.DrawTexture(new Rect(54, y + 75, 386, 150), titleLogo, ScaleMode.ScaleToFit, true);
                else GUI.Label(new Rect(64, y + 100, 360, 100), "JEJAK ASA", UI_TemaVisual.Title);
                GUI.Label(new Rect(68, y + 244, 354, 48), "Bantuan sampai,\nharapan tumbuh.", UI_TemaVisual.White);
                UI_TemaVisual.Rule(68, y + 312, 354);
                DrawRows(RowsArea());

                Rect caption = new Rect(sw - 464, sh - 159, 420, 113);
                UI_TemaVisual.Card(caption);
                var captionArt = Resources.Load<Texture2D>("JejakAsaUI/Generated/VolunteerDialogPanel");
                if (captionArt != null) GUI.DrawTexture(caption, captionArt, ScaleMode.StretchToFill, true);
                GUI.Label(new Rect(caption.x + 24, caption.y + 16, 372, 22), "PULANG. PEDULI. BERGERAK BERSAMA.", UI_TemaVisual.Eyebrow);
                GUI.Label(new Rect(caption.x + 24, caption.y + 44, 372, 47), "Lima hari menyusuri kampung.\nSatu langkah kecil untuk sesama.", UI_TemaVisual.Body);
                if (!string.IsNullOrEmpty(notice))
                {
                    Rect toast = new Rect(490, 32, Mathf.Min(680, sw - 522), 62);
                    UI_TemaVisual.Card(toast); GUI.Label(new Rect(toast.x + 20, toast.y + 8, toast.width - 40, 46), notice, UI_TemaVisual.Body);
                }
            }
            else
            {
                if (isMainMenu) GUI.DrawTexture(new Rect(0, 0, sw, sh), UI_TemaVisual.DimTex);
                float cardHeight = Mathf.Min(656, 280 + OptionCount() * 64);
                float x = (sw - 720) / 2;
                float top = (sh - cardHeight) / 2;
                Rect sheet = new Rect(x,top,720,cardHeight);
                UI_TemaVisual.Header(sheet,isMainMenu ? "JEJAK ASA" : "PERJALANAN DIJEDA",PageTitle(),PageDescription());
                DrawRows(RowsArea());
                if (!string.IsNullOrEmpty(notice)) GUI.Label(new Rect(x+60,top+cardHeight-100,600,44),notice,UI_TemaVisual.Body);
                GUI.Label(new Rect(x+60,top+cardHeight-48,600,22),"Mouse / Atas-Bawah  Pilih     Klik / Enter  Lanjut     Esc  Kembali",UI_TemaVisual.HintStyle);
            }
            GUI.matrix = old;
        }
    }
}

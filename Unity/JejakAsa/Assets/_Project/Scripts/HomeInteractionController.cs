using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;

namespace JejakAsa
{
    /// <summary>
    /// Pengendali interaksi di dalam interior Rumah (R04 / AB-D02).
    /// Mengatur pintu keluar menuju posko dan tempat tidur untuk istirahat tidur & simpan game (Slot 1..3).
    /// </summary>
    [DisallowMultipleComponent]
    public class HomeInteractionController : MonoBehaviour
    {
        [Header("Referensi")]
        public Transform playerTransform;
        public Transform doorTransform;
        public Transform bedTransform;
        public float interactionRadius = 2.2f;

        [Header("Scene Target")]
        public string poskoScenePath = "Assets/_Project/Scenes/Posko.unity";

        private bool nearDoor;
        private bool nearBed;
        private bool isBedMenuOpen;
        private int pendingSaveSlot;
        private string feedbackNotice = string.Empty;
        private float feedbackTimer;

        private void Start()
        {
            if (playerTransform == null)
            {
                var playerObj = GameObject.FindWithTag("Player");
                if (playerObj != null)
                    playerTransform = playerObj.transform;
            }
        }

        private void Update()
        {
            if (feedbackTimer > 0f)
            {
                feedbackTimer -= Time.deltaTime;
                if (feedbackTimer <= 0f)
                    feedbackNotice = string.Empty;
            }

            if (playerTransform == null || SessionMenu.InputBlocked)
                return;

            nearDoor = doorTransform != null && Vector3.Distance(doorTransform.position, playerTransform.position) <= interactionRadius;
            nearBed = bedTransform != null && Vector3.Distance(bedTransform.position, playerTransform.position) <= interactionRadius;

            Keyboard kb = Keyboard.current;
            if (kb == null)
                return;

            // 1. Interaksi Pintu Keluar Rumah
            if (nearDoor && !isBedMenuOpen && kb.eKey.wasPressedThisFrame)
            {
                TryExitToPosko();
            }

            // 2. Interaksi Tempat Tidur
            if (nearBed && !isBedMenuOpen && kb.eKey.wasPressedThisFrame)
            {
                isBedMenuOpen = true;
            }
            else if (isBedMenuOpen)
            {
                if (pendingSaveSlot != 0)
                {
                    if (kb.escapeKey.wasPressedThisFrame || kb.digit1Key.wasPressedThisFrame)
                        pendingSaveSlot = 0;
                    else if (kb.digit2Key.wasPressedThisFrame)
                        ConfirmSave();
                }
                else if (kb.escapeKey.wasPressedThisFrame || kb.digit5Key.wasPressedThisFrame)
                {
                    isBedMenuOpen = false;
                }
                else if (kb.digit1Key.wasPressedThisFrame)
                    TryRestAndAdvanceDay();
                else if (kb.digit2Key.wasPressedThisFrame)
                    DoSave(1);
                else if (kb.digit3Key.wasPressedThisFrame)
                    DoSave(2);
                else if (kb.digit4Key.wasPressedThisFrame)
                    DoSave(3);
            }
        }

        public void TryExitToPosko()
        {
            if (GameFlowManager.Instance.CanGoToPoskoFromHome(out string reason))
            {
                GameFlowManager.Instance.GoToPoskoFromHome();
                GameFlowManager.Instance.LastSpawnPoint = "RumahVicky";
                SceneManager.LoadScene(poskoScenePath, LoadSceneMode.Single);
            }
            else
            {
                ShowNotice(reason);
            }
        }

        public void TryRestAndAdvanceDay()
        {
            if (GameFlowManager.Instance.CanRestAndAdvanceDay(out string reason))
            {
                GameFlowManager.Instance.RestAndAdvanceDay();
                isBedMenuOpen = false;
                ShowNotice(GameFlowManager.Instance.CurrentPhase == DayPhase.Tamat
                    ? "Kelima level selesai! Tekan Esc untuk kembali ke menu utama."
                    : $"Selamat pagi! Memasuki Hari ke-{GameFlowManager.Instance.CurrentDay}.");
            }
            else
            {
                ShowNotice(reason);
            }
        }

        public void DoSave(int slot)
        {
            if (slot < 1 || slot > GameFlowManager.MaxSaveSlots)
            {
                ShowNotice($"Gagal menyimpan ke Slot {slot}.");
                return;
            }
            if (GameFlowManager.HasSave(slot))
            {
                pendingSaveSlot = slot;
                return;
            }
            SaveSlot(slot);
        }

        public void ConfirmSave()
        {
            if (pendingSaveSlot == 0)
                return;
            int slot = pendingSaveSlot;
            pendingSaveSlot = 0;
            SaveSlot(slot);
        }

        private void SaveSlot(int slot)
        {
            if (GameFlowManager.Instance.SaveGame(slot))
            {
                ShowNotice($"Progres berhasil disimpan ke Slot {slot}!");
            }
            else
            {
                ShowNotice($"Gagal menyimpan ke Slot {slot}.");
            }
        }

        private void ShowNotice(string msg)
        {
            feedbackNotice = msg;
            feedbackTimer = 3.5f;
        }

        private void OnGUI()
        {
            if (SessionMenu.InputBlocked)
                return;

            CozyGuiTheme.EnsureInitialized();

            // Pemberitahuan feedback
            if (!string.IsNullOrEmpty(feedbackNotice))
            {
                float nW = 440f;
                float nH = 40f;
                float nX = (Screen.width - nW) * 0.5f;
                float nY = 30f;

                CozyGuiTheme.BannerStyle.normal.background = CozyGuiTheme.BannerInfoTex;
                CozyGuiTheme.BannerStyle.normal.textColor = new Color(0.12f, 0.28f, 0.44f);
                GUI.Label(new Rect(nX, nY, nW, nH), feedbackNotice, CozyGuiTheme.BannerStyle);
            }

            // Prompt Pintu
            if (nearDoor && !isBedMenuOpen)
            {
                float pW = 280f;
                float pH = 44f;
                float pX = (Screen.width - pW) * 0.5f;
                float pY = Screen.height - 110f;

                GUI.DrawTexture(new Rect(pX + 2, pY + 2, pW, pH), CozyGuiTheme.ShadowTex);
                GUI.Box(new Rect(pX, pY, pW, pH), GUIContent.none, CozyGuiTheme.CardStyle);
                GUI.Label(new Rect(pX, pY + 10, pW, 24), "Tekan [E] Keluar Menuju Posko", CozyGuiTheme.HintStyle);
            }
            // Prompt Tempat Tidur
            else if (nearBed && !isBedMenuOpen)
            {
                float pW = 320f;
                float pH = 44f;
                float pX = (Screen.width - pW) * 0.5f;
                float pY = Screen.height - 110f;

                GUI.DrawTexture(new Rect(pX + 2, pY + 2, pW, pH), CozyGuiTheme.ShadowTex);
                GUI.Box(new Rect(pX, pY, pW, pH), GUIContent.none, CozyGuiTheme.CardStyle);
                GUI.Label(new Rect(pX, pY + 10, pW, 24), "Tekan [E] Tempat Tidur (Istirahat & Save)", CozyGuiTheme.HintStyle);
            }

            // Menu Tempat Tidur
            if (isBedMenuOpen)
            {
                GUI.depth = -100;
                float scale = CozyGuiTheme.Scale;
                Matrix4x4 prevMatrix = GUI.matrix;
                GUI.matrix = prevMatrix * Matrix4x4.Scale(new Vector3(scale, scale, 1f));

                float sw = Screen.width / scale;
                float sh = Screen.height / scale;

                GUI.DrawTexture(new Rect(0, 0, sw, sh), CozyGuiTheme.DimTex);

                float cardW = 460f;
                float cardH = 390f;
                float cardX = (sw - cardW) * 0.5f;
                float cardY = (sh - cardH) * 0.5f;

                GUI.DrawTexture(new Rect(cardX + 4, cardY + 6, cardW, cardH), CozyGuiTheme.ShadowTex);
                GUI.Box(new Rect(cardX, cardY, cardW, cardH), GUIContent.none, CozyGuiTheme.CardStyle);

                GUI.Label(new Rect(cardX + 16, cardY + 16, 160, 24), "KAMAR VICKY", CozyGuiTheme.RibbonStyle);
                string desc = $"Hari {GameFlowManager.Instance.CurrentDay} • Fase: {GameFlowManager.Instance.CurrentPhase}";
                GUI.Label(new Rect(cardX + 16, cardY + 48, cardW - 32, 22), desc, CozyGuiTheme.OutcomeDescStyle);

                if (pendingSaveSlot != 0)
                {
                    GUI.Label(new Rect(cardX + 20, cardY + 90, cardW - 40, 48), $"Timpa save Slot {pendingSaveSlot}? Data lama akan hilang.", CozyGuiTheme.OutcomeDescStyle);
                    if (GUI.Button(new Rect(cardX + 20, cardY + 150, cardW - 40, 42), "1. Batal", CozyGuiTheme.TealBtnStyle))
                        pendingSaveSlot = 0;
                    if (GUI.Button(new Rect(cardX + 20, cardY + 200, cardW - 40, 42), "2. Ya, timpa save", CozyGuiTheme.CoralBtnStyle))
                        ConfirmSave();
                }
                else
                {
                    float rowY = cardY + 80f;
                    bool canRest = GameFlowManager.Instance.CanRestAndAdvanceDay(out string restReason);
                    if (GUI.Button(new Rect(cardX + 20, rowY, cardW - 40, 42), "1. Istirahat Tidur (Lanjut Hari Berikutnya)", canRest ? CozyGuiTheme.TealBtnStyle : CozyGuiTheme.LockedBtnStyle))
                        TryRestAndAdvanceDay();
                    if (!canRest && !string.IsNullOrEmpty(restReason))
                        GUI.Label(new Rect(cardX + 24, rowY + 44, cardW - 48, 20), restReason, CozyGuiTheme.SubLabelStyle);

                    rowY += 66f;
                    for (int slot = 1; slot <= GameFlowManager.MaxSaveSlots; slot++)
                    {
                        string time = GameFlowManager.HasSave(slot) ? PlayerPrefs.GetString("JejakAsa_SaveSlot_" + slot + "_Time", "-") : "Kosong";
                        if (GUI.Button(new Rect(cardX + 20, rowY, cardW - 40, 38), $"{slot + 1}. Simpan Slot {slot} ({time})", CozyGuiTheme.AmberBtnStyle))
                            DoSave(slot);
                        rowY += 44f;
                    }
                    if (GUI.Button(new Rect(cardX + 20, rowY, cardW - 40, 36), "5. Batal / Bangkit", CozyGuiTheme.CoralBtnStyle))
                        isBedMenuOpen = false;
                }

                GUI.Label(new Rect(cardX + 20, cardY + cardH - 24, cardW - 40, 18), "[1-5] Pilih Opsi  |  [Esc] Batal", CozyGuiTheme.HintStyle);

                GUI.matrix = prevMatrix;
            }
        }
    }
}


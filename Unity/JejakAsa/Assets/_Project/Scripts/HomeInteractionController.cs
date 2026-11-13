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
        public string kampungScenePath = "Assets/_Project/Scenes/Kampung.unity";
        public string poskoScenePath = "";

        private bool nearDoor;
        private bool nearBed;
        private bool isBedMenuOpen;
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
                if (kb.escapeKey.wasPressedThisFrame || kb.digit4Key.wasPressedThisFrame)
                {
                    isBedMenuOpen = false;
                }
                else if (kb.digit1Key.wasPressedThisFrame)
                {
                    TryRestAndAdvanceDay();
                }
                else if (kb.digit2Key.wasPressedThisFrame)
                {
                    DoSave(1);
                }
                else if (kb.digit3Key.wasPressedThisFrame)
                {
                    DoSave(2);
                }
            }
        }

        public void TryExitToPosko()
        {
            if (GameFlowManager.Instance.CanGoToPoskoFromHome(out string reason))
            {
                GameFlowManager.Instance.GoToPoskoFromHome();
                GameFlowManager.Instance.LastSpawnPoint = "RumahVicky";
                string targetScene = !string.IsNullOrEmpty(kampungScenePath) ? kampungScenePath : poskoScenePath;
                SceneManager.LoadScene(targetScene, LoadSceneMode.Single);
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
                ShowNotice($"Selamat pagi! Memasuki Hari ke-{GameFlowManager.Instance.CurrentDay}.");
            }
            else
            {
                ShowNotice(reason);
            }
        }

        public void DoSave(int slot)
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
                float cardH = 340f;
                float cardX = (sw - cardW) * 0.5f;
                float cardY = (sh - cardH) * 0.5f;

                GUI.DrawTexture(new Rect(cardX + 4, cardY + 6, cardW, cardH), CozyGuiTheme.ShadowTex);
                GUI.Box(new Rect(cardX, cardY, cardW, cardH), GUIContent.none, CozyGuiTheme.CardStyle);

                GUI.Label(new Rect(cardX + 16, cardY + 16, 160, 24), "KAMAR VICKY", CozyGuiTheme.RibbonStyle);
                string desc = $"Hari {GameFlowManager.Instance.CurrentDay} • Fase: {GameFlowManager.Instance.CurrentPhase}";
                GUI.Label(new Rect(cardX + 16, cardY + 48, cardW - 32, 22), desc, CozyGuiTheme.OutcomeDescStyle);

                float rowY = cardY + 80f;

                // 1. Tidur / Istirahat
                bool canRest = GameFlowManager.Instance.CanRestAndAdvanceDay(out string restReason);
                if (GUI.Button(new Rect(cardX + 20, rowY, cardW - 40, 42), "1. Istirahat Tidur (Lanjut Hari Berikutnya)", canRest ? CozyGuiTheme.TealBtnStyle : CozyGuiTheme.LockedBtnStyle))
                {
                    TryRestAndAdvanceDay();
                }
                if (!canRest && !string.IsNullOrEmpty(restReason))
                {
                    GUI.Label(new Rect(cardX + 24, rowY + 44, cardW - 48, 20), "⚠ " + restReason, CozyGuiTheme.SubLabelStyle);
                }

                rowY += 66f;

                // 2. Simpan Progres Slot 1 & Slot 2
                float btnHalf = (cardW - 50f) * 0.5f;
                string s1Time = GameFlowManager.HasSave(1) ? GameFlowManager.GetSlotMeta(1).timestamp : "Kosong";
                string s2Time = GameFlowManager.HasSave(2) ? GameFlowManager.GetSlotMeta(2).timestamp : "Kosong";

                if (GUI.Button(new Rect(cardX + 20, rowY, btnHalf, 42), $"2. Simpan Slot 1\n({s1Time})", CozyGuiTheme.AmberBtnStyle))
                {
                    DoSave(1);
                }
                if (GUI.Button(new Rect(cardX + 30 + btnHalf, rowY, btnHalf, 42), $"3. Simpan Slot 2\n({s2Time})", CozyGuiTheme.AmberBtnStyle))
                {
                    DoSave(2);
                }

                rowY += 56f;

                // Tutup
                if (GUI.Button(new Rect(cardX + 20, rowY, cardW - 40, 36), "4. Batal / Bangkit", CozyGuiTheme.CoralBtnStyle))
                {
                    isBedMenuOpen = false;
                }

                GUI.Label(new Rect(cardX + 20, cardY + cardH - 24, cardW - 40, 18), "[1/2/3/4] Pilih Opsi  |  [Esc] Tutup", CozyGuiTheme.HintStyle);

                GUI.matrix = prevMatrix;
            }
        }
    }
}


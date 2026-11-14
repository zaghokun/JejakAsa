using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;

namespace JejakAsa
{
    /// <summary>
    /// Pengendali gerbang keluar posko bersama (R03 / AB-D02).
    /// Memberi pilihan berangkat ke Level, pulang ke Rumah, atau Batal.
    /// </summary>
    [DisallowMultipleComponent]
    public class PoskoGateController : MonoBehaviour
    {
        [Header("Konfigurasi Interaksi")]
        public Transform playerTransform;
        public float interactionRadius = 2.5f;

        [Header("Scene Target")]
        public string levelScenePath = "Assets/_Project/Scenes/Kampung.unity";
        public string homeScenePath = "Assets/_Project/Scenes/Rumah.unity";

        private bool isPlayerInRange;
        private bool isGateMenuOpen;
        private string lockedDepartReason;
        private string lockedHomeReason;
        internal bool IsMenuOpen => isGateMenuOpen;

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
            if (playerTransform == null || SessionMenu.InputBlocked)
                return;

            float dist = Vector3.Distance(transform.position, playerTransform.position);
            isPlayerInRange = dist <= interactionRadius;

            Keyboard kb = Keyboard.current;
            if (kb == null)
                return;

            if (isPlayerInRange && !isGateMenuOpen && kb.eKey.wasPressedThisFrame && PoskoDistributionTrigger.IsNearestInteraction(this, playerTransform))
            {
                OpenGateMenu();
            }
            else if (isGateMenuOpen)
            {
                if (kb.escapeKey.wasPressedThisFrame || kb.digit3Key.wasPressedThisFrame)
                {
                    CloseGateMenu();
                }
                else if (kb.digit1Key.wasPressedThisFrame)
                {
                    TryDepartToLevel();
                }
                else if (kb.digit2Key.wasPressedThisFrame)
                {
                    TryGoHome();
                }
            }
        }

        public void OpenGateMenu()
        {
            isGateMenuOpen = true;
            GameFlowManager.Instance.CanDepartToLevel(out lockedDepartReason);
            GameFlowManager.Instance.CanGoHomeFromPosko(out lockedHomeReason);
        }

        public void CloseGateMenu()
        {
            isGateMenuOpen = false;
        }

        public void TryDepartToLevel()
        {
            if (GameFlowManager.Instance.CanDepartToLevel(out string reason))
            {
                int level = GameFlowManager.Instance.CurrentLevel;
                if (level < 1 || level > 5)
                {
                    lockedDepartReason = "Level aktif tidak valid.";
                    return;
                }
                string scenePath = level == 1 ? levelScenePath : $"Assets/_Project/Scenes/Kampung_L{level}.unity";
                if (SceneUtility.GetBuildIndexByScenePath(scenePath) < 0)
                {
                    lockedDepartReason = "Wilayah misi tidak tersedia di build.";
                    return;
                }
                GameFlowManager.Instance.LastSpawnPoint = "PosBantuan";
                GameFlowManager.Instance.DepartToLevel();
                isGateMenuOpen = false;
                SceneManager.LoadScene(scenePath, LoadSceneMode.Single);
            }
            else
            {
                lockedDepartReason = reason;
            }
        }

        public void TryGoHome()
        {
            if (GameFlowManager.Instance.CanGoHomeFromPosko(out string reason))
            {
                GameFlowManager.Instance.GoHomeFromPosko();
                isGateMenuOpen = false;
                SceneManager.LoadScene(homeScenePath, LoadSceneMode.Single);
            }
            else
            {
                lockedHomeReason = reason;
            }
        }

        private void OnGUI()
        {
            if (SessionMenu.InputBlocked)
                return;

            CozyGuiTheme.EnsureInitialized();

            // 1. Prompt saat pemain berada di dekat gerbang
            if (isPlayerInRange && !isGateMenuOpen)
            {
                float pW = 280f;
                float pH = 44f;
                float pX = (Screen.width - pW) * 0.5f;
                float pY = Screen.height - 110f;

                GUI.DrawTexture(new Rect(pX + 2, pY + 2, pW, pH), CozyGuiTheme.ShadowTex);
                GUI.Box(new Rect(pX, pY, pW, pH), GUIContent.none, CozyGuiTheme.CardStyle);
                GUI.Label(new Rect(pX, pY + 10, pW, 24), "Tekan [E] Buka Gerbang Posko", CozyGuiTheme.HintStyle);
                return;
            }

            // 2. Menu Gerbang Posko
            if (isGateMenuOpen)
            {
                GUI.depth = -100;
                float scale = CozyGuiTheme.Scale;
                Matrix4x4 prevMatrix = GUI.matrix;
                GUI.matrix = prevMatrix * Matrix4x4.Scale(new Vector3(scale, scale, 1f));

                float sw = Screen.width / scale;
                float sh = Screen.height / scale;

                GUI.DrawTexture(new Rect(0, 0, sw, sh), CozyGuiTheme.DimTex);

                float cardW = 440f;
                float cardH = 340f;
                float cardX = (sw - cardW) * 0.5f;
                float cardY = (sh - cardH) * 0.5f;

                GUI.DrawTexture(new Rect(cardX + 4, cardY + 6, cardW, cardH), CozyGuiTheme.ShadowTex);
                GUI.Box(new Rect(cardX, cardY, cardW, cardH), GUIContent.none, CozyGuiTheme.CardStyle);

                // Header
                GUI.Label(new Rect(cardX + 16, cardY + 16, 150, 24), "GERBANG POSKO", CozyGuiTheme.RibbonStyle);
                GUI.Label(new Rect(cardX + 16, cardY + 48, cardW - 32, 22), "Pilih tujuan perjalananmu keluar posko:", CozyGuiTheme.OutcomeDescStyle);

                float rowY = cardY + 84f;

                // Opsi 1: Berangkat ke Level
                bool canDepart = GameFlowManager.Instance.CanDepartToLevel(out string departReason);
                if (GUI.Button(new Rect(cardX + 20, rowY, cardW - 40, 42), "1. Berangkat ke Wilayah Misi (Level)", canDepart ? CozyGuiTheme.TealBtnStyle : CozyGuiTheme.LockedBtnStyle))
                {
                    TryDepartToLevel();
                }
                if (!canDepart || !string.IsNullOrEmpty(lockedDepartReason))
                {
                    GUI.Label(new Rect(cardX + 24, rowY + 44, cardW - 48, 20), "⚠ " + (canDepart ? lockedDepartReason : departReason), CozyGuiTheme.SubLabelStyle);
                }

                rowY += 68f;

                // Opsi 2: Pulang ke Rumah
                bool canGoHome = GameFlowManager.Instance.CanGoHomeFromPosko(out string homeReason);
                if (GUI.Button(new Rect(cardX + 20, rowY, cardW - 40, 42), "2. Pulang ke Rumah", canGoHome ? CozyGuiTheme.AmberBtnStyle : CozyGuiTheme.LockedBtnStyle))
                {
                    TryGoHome();
                }
                if (!canGoHome && !string.IsNullOrEmpty(homeReason))
                {
                    GUI.Label(new Rect(cardX + 24, rowY + 44, cardW - 48, 20), "⚠ " + homeReason, CozyGuiTheme.SubLabelStyle);
                }

                rowY += 68f;

                // Opsi 3: Batal
                if (GUI.Button(new Rect(cardX + 20, rowY, cardW - 40, 38), "3. Batal / Tutup Gerbang", CozyGuiTheme.CoralBtnStyle))
                {
                    CloseGateMenu();
                }

                GUI.Label(new Rect(cardX + 20, cardY + cardH - 24, cardW - 40, 18), "[1/2/3] Pilih Opsi  |  [Esc] Tutup", CozyGuiTheme.HintStyle);

                GUI.matrix = prevMatrix;
            }
        }
    }
}


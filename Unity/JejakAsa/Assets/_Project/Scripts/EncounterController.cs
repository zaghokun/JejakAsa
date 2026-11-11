using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

namespace JejakAsa
{
    /// <summary>
    /// Mengelola pertemuan intimidasi preman di gang utama (U09 &amp; U10):
    /// memicu pilihan dalam radius 1,8 m, menjaga opsi Menghindar tanpa langsung memicu ulang
    /// sebelum keluar radius 2,5 m, serta mengelola Bantuan Warga saat Kepercayaan &gt;= 1.
    /// </summary>
    [DisallowMultipleComponent]
    public sealed class EncounterController : MonoBehaviour
    {
        internal static readonly List<EncounterController> Active = new List<EncounterController>();

        public static bool AnyOpen
        {
            get
            {
                for (int i = 0; i < Active.Count; i++)
                {
                    EncounterController enc = Active[i];
                    if (enc != null && enc.isEncounterOpen)
                        return true;
                }
                return false;
            }
        }

        [Header("Encounter Identity & Radius (U09)")]
        [SerializeField] private string encounterName = "Preman Kampung";
        [SerializeField, TextArea(2, 4)] private string introDialogue = "Eits, mau ke mana bawa sembako banyak-banyak? Gang utama ini dijaga!";
        [SerializeField, Min(0.1f)] private float triggerRadius = 1.8f;
        [SerializeField, Min(0.2f)] private float rearmRadius = 2.5f;

        [Header("Scene References (U09 / U10)")]
        [SerializeField] private PlayerInteraction playerInteraction;
        [SerializeField] private GameObject obstacleObject;
        [SerializeField] private GameObject premanVisual;
        [SerializeField] private GameObject activeMarkerObject;
        [SerializeField] private Vector3 distractedLocalOffset = new Vector3(-1.45f, 0.8f, 1.1f);

        public event Action<EncounterController> BattleRequested;

        private bool isEncounterOpen;
        private bool isResolved;
        private bool awaitingExit;
        private bool battleRequestPending;
        private int openedFrame = -1;
        private string statusFeedback = "";

        private Vector3 premanInitialLocalPosition;
        private Quaternion premanInitialLocalRotation;
        private bool premanInitialCaptured;
        private Collider premanCollider;

        public string EncounterName => encounterName;
        public bool IsEncounterOpen => isEncounterOpen;
        public bool IsResolved => isResolved;
        public bool IsAwaitingExit => awaitingExit;
        public bool HasBattleRequestPending => battleRequestPending;
        public bool IsCitizenHelpAvailable => playerInteraction != null && playerInteraction.Trust >= 1 && !isResolved;
        public GameObject ObstacleObject => obstacleObject;
        public GameObject PremanVisual => premanVisual;
        public GameObject ActiveMarkerObject => activeMarkerObject;

        private void OnEnable()
        {
            if (!Active.Contains(this))
                Active.Add(this);
        }

        private void OnDisable()
        {
            if (isEncounterOpen)
            {
                isEncounterOpen = false;
                playerInteraction?.SetEncounterMovementLock(false);
            }
            Active.Remove(this);
        }

        private void Awake()
        {
            CaptureInitialPremanTransform();
        }

        private void Start()
        {
            ResolvePlayerInteraction();
        }

        /// <summary>
        /// Menghubungkan referensi scene/uji secara terprogram untuk PlayMode tests.
        /// </summary>
        public void ConfigureForTests(PlayerInteraction interaction, GameObject obstacle = null, GameObject preman = null, GameObject marker = null)
        {
            playerInteraction = interaction;
            obstacleObject = obstacle;
            premanVisual = preman;
            activeMarkerObject = marker;
            premanInitialCaptured = false;
            CaptureInitialPremanTransform();
        }

        private void CaptureInitialPremanTransform()
        {
            if (premanVisual == null || premanInitialCaptured)
                return;

            premanInitialLocalPosition = premanVisual.transform.localPosition;
            premanInitialLocalRotation = premanVisual.transform.localRotation;
            premanCollider = premanVisual.GetComponent<Collider>();
            premanInitialCaptured = true;
        }

        private void ResolvePlayerInteraction()
        {
            if (playerInteraction == null)
                playerInteraction = FindFirstObjectByType<PlayerInteraction>();
        }

        private float HorizontalDistanceToPlayer()
        {
            if (playerInteraction == null)
                return float.MaxValue;

            Vector3 offset = playerInteraction.transform.position - transform.position;
            offset.y = 0f;
            return offset.magnitude;
        }

        private void Update()
        {
            ResolvePlayerInteraction();
            if (playerInteraction == null)
                return;

            if (!isEncounterOpen)
            {
                if (isResolved)
                    return;

                if (playerInteraction.Outcome != MissionOutcome.InProgress
                    || playerInteraction.IsDialogueOpen
                    || playerInteraction.IsOutcomeOpen)
                {
                    return;
                }

                float dist = HorizontalDistanceToPlayer();
                if (awaitingExit)
                {
                    if (dist > rearmRadius)
                        awaitingExit = false;
                    return;
                }

                if (dist <= triggerRadius && !AnyOpen)
                    OpenEncounter();

                return;
            }

            if (playerInteraction.Outcome != MissionOutcome.InProgress || playerInteraction.IsOutcomeOpen)
            {
                isEncounterOpen = false;
                return;
            }

            if (Time.frameCount <= openedFrame)
                return;

            Keyboard kb = Keyboard.current;
            if (kb == null)
                return;

            if (kb.escapeKey.wasPressedThisFrame || kb.digit1Key.wasPressedThisFrame)
            {
                AvoidEncounter();
            }
            else if (kb.digit2Key.wasPressedThisFrame)
            {
                TryRequestBattle();
            }
            else if (kb.digit3Key.wasPressedThisFrame)
            {
                TryUseCitizenHelp();
            }
        }

        public void OpenEncounter()
        {
            if (isResolved || isEncounterOpen)
                return;

            isEncounterOpen = true;
            openedFrame = Time.frameCount;
            statusFeedback = "";
            playerInteraction?.SetEncounterMovementLock(true);
        }

        /// <summary>
        /// U09: Menghindar dari preman di posisi yang sama dan mengembalikan kendali ke pemain.
        /// Encounter tidak akan aktif ulang sampai pemain keluar radius 2,5 m lalu mendekat lagi.
        /// </summary>
        public void AvoidEncounter()
        {
            if (!isEncounterOpen || isResolved)
                return;

            isEncounterOpen = false;
            awaitingExit = true;
            statusFeedback = "";
            playerInteraction?.SetEncounterMovementLock(false);
        }

        /// <summary>
        /// Slot transisi Bertengkar untuk arena Battle (B / U15). Menolak pemanggilan ganda saat tidak valid.
        /// </summary>
        public bool TryRequestBattle()
        {
            if (!isEncounterOpen || isResolved || battleRequestPending)
                return false;

            if (BattleRequested != null)
            {
                battleRequestPending = true;
                statusFeedback = "";
                BattleRequested.Invoke(this);
                return true;
            }

            statusFeedback = "Arena Battle sedang disiapkan tim Battle. Pilih Menghindar atau Bantuan Warga.";
            return false;
        }

        /// <summary>
        /// U10: Memakai Bantuan Warga jika kepercayaan minimal 1. Menyingkirkan penghalang tanpa battle
        /// dan tanpa mengurangi poin kepercayaan.
        /// </summary>
        public bool TryUseCitizenHelp()
        {
            if (!isEncounterOpen || isResolved)
                return false;

            if (!IsCitizenHelpAvailable)
            {
                statusFeedback = "Bantu minimal satu warga terlebih dahulu.";
                return false;
            }

            isEncounterOpen = false;
            isResolved = true;
            awaitingExit = false;
            battleRequestPending = false;
            statusFeedback = "";

            ApplyClearedState(distractedPreman: true);
            playerInteraction.SetEncounterMovementLock(false);
            playerInteraction.OpenSystemDialogue(
                "Bantuan Warga",
                "Warga sekitar datang mengajak preman mengobrol dan mengalihkan perhatiannya!\n\nGang utama kini aman dilewati tanpa perlu bertarung.");

            return true;
        }

        /// <summary>
        /// Menyelesaikan penghadang setelah menang battle (U15) atau pembersihan jalur non-dialog.
        /// </summary>
        public void ResolveFromBattleVictory()
        {
            isEncounterOpen = false;
            isResolved = true;
            awaitingExit = false;
            battleRequestPending = false;
            statusFeedback = "";

            ApplyClearedState(distractedPreman: false);
            playerInteraction?.SetEncounterMovementLock(false);
        }

        /// <summary>
        /// Mengembalikan preman, penghalang, dan trigger ke keadaan awal sesi (U08 &amp; U17).
        /// </summary>
        public void ResetEncounter()
        {
            isEncounterOpen = false;
            isResolved = false;
            awaitingExit = false;
            battleRequestPending = false;
            statusFeedback = "";

            if (obstacleObject != null)
                obstacleObject.SetActive(true);

            if (activeMarkerObject != null)
                activeMarkerObject.SetActive(true);

            if (premanVisual != null)
            {
                premanVisual.SetActive(true);
                if (premanInitialCaptured)
                {
                    premanVisual.transform.localPosition = premanInitialLocalPosition;
                    premanVisual.transform.localRotation = premanInitialLocalRotation;
                }
                if (premanCollider != null)
                    premanCollider.enabled = true;
            }
        }

        private void ApplyClearedState(bool distractedPreman)
        {
            if (obstacleObject != null)
                obstacleObject.SetActive(false);

            if (activeMarkerObject != null)
                activeMarkerObject.SetActive(false);

            if (premanVisual != null)
            {
                if (premanCollider != null)
                    premanCollider.enabled = false;

                if (distractedPreman)
                {
                    premanVisual.transform.localPosition = distractedLocalOffset;
                    premanVisual.transform.localRotation = Quaternion.Euler(0f, -65f, 0f);
                }
                else
                {
                    premanVisual.SetActive(false);
                }
            }
        }

        private void OnGUI()
        {
            if (!isEncounterOpen || playerInteraction == null || playerInteraction.IsOutcomeOpen)
                return;

            CozyGuiTheme.EnsureInitialized();
            GUI.depth = -90;

            GUI.DrawTexture(new Rect(0, 0, Screen.width, Screen.height), CozyGuiTheme.DimTex);

            float popW = Mathf.Min(410f, Screen.width - 32f);
            float popH = 368f;
            float popX = (Screen.width - popW) * 0.5f;
            float popY = Mathf.Max(20f, (Screen.height - popH) * 0.5f);

            GUI.DrawTexture(new Rect(popX + 4, popY + 6, popW, popH), CozyGuiTheme.ShadowTex);
            GUI.Box(new Rect(popX, popY, popW, popH), GUIContent.none, CozyGuiTheme.CardStyle);

            CozyGuiTheme.OutcomeHeaderStyle.normal.background = CozyGuiTheme.OutcomeFailTex;
            GUI.Label(new Rect(popX + (popW - 220f) * 0.5f, popY - 13f, 220f, 26f), encounterName.ToUpperInvariant(), CozyGuiTheme.OutcomeHeaderStyle);

            GUI.Label(new Rect(popX + 20f, popY + 18f, popW - 40f, 44f), introDialogue, CozyGuiTheme.OutcomeDescStyle);

            int currentTrust = playerInteraction.Trust;
            GUI.Label(
                new Rect(popX + 20f, popY + 62f, popW - 40f, 22f),
                $"Kepercayaan Warga: <b><color=#B45309>{currentTrust} / 3</color></b>",
                CozyGuiTheme.OutcomeDescStyle);

            float btnX = popX + 24f;
            float btnW = popW - 48f;

            if (GUI.Button(new Rect(btnX, popY + 92f, btnW, 36f), "1. Menghindar (Lewat Jalur Memutar)", CozyGuiTheme.TealBtnStyle))
            {
                AvoidEncounter();
            }

            if (GUI.Button(new Rect(btnX, popY + 136f, btnW, 36f), "2. Bertengkar (Masuk Battle)", CozyGuiTheme.CoralBtnStyle))
            {
                TryRequestBattle();
            }

            bool canHelp = IsCitizenHelpAvailable;
            Rect helpBtnRect = new Rect(btnX, popY + 180f, btnW, 36f);
            Rect reasonRect = new Rect(btnX, popY + 224f, btnW, 38f);

            if (canHelp)
            {
                if (GUI.Button(helpBtnRect, "3. Bantuan Warga (Alihkan Perhatian)", CozyGuiTheme.AmberBtnStyle))
                {
                    TryUseCitizenHelp();
                }

                CozyGuiTheme.BannerStyle.normal.background = CozyGuiTheme.BannerSuccessTex;
                CozyGuiTheme.BannerStyle.normal.textColor = new Color(0.05f, 0.45f, 0.30f);
                GUI.Label(reasonRect, "Warga siap membantu membuka jalan tanpa bertarung.", CozyGuiTheme.BannerStyle);
            }
            else
            {
                bool previousEnabled = GUI.enabled;
                GUI.enabled = false;
                GUI.Button(helpBtnRect, "3. Bantuan Warga (Terkunci)", CozyGuiTheme.LockedBtnStyle);
                GUI.enabled = previousEnabled;

                GUI.Label(reasonRect, "Bantu minimal satu warga terlebih dahulu", CozyGuiTheme.LockReasonStyle);
            }

            if (!string.IsNullOrEmpty(statusFeedback))
            {
                CozyGuiTheme.BannerStyle.normal.background = CozyGuiTheme.BannerInfoTex;
                CozyGuiTheme.BannerStyle.normal.textColor = new Color(0.55f, 0.35f, 0.05f);
                GUI.Label(new Rect(btnX, popY + 272f, btnW, 44f), statusFeedback, CozyGuiTheme.BannerStyle);
            }

            GUI.Label(
                new Rect(popX + 16f, popY + popH - 26f, popW - 32f, 18f),
                "Pilih tombol atau tekan [1] Menghindar  |  [2] Battle  |  [3] Bantuan Warga",
                CozyGuiTheme.HintStyle);
        }
    }
}


using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace JejakAsa
{
    public enum MissionOutcome
    {
        InProgress,
        Success,        // 3/3 paket terantar tepat waktu (U08)
        PartialSuccess, // 1-2 paket terantar, waktu habis (U08)
        Failed          // 0 paket terantar, waktu habis (U08)
    }

    [DefaultExecutionOrder(-10)]
    [DisallowMultipleComponent]
    [RequireComponent(typeof(PlayerController))]
    public sealed class PlayerInteraction : MonoBehaviour
    {
        [Header("Interaction Settings")]
        [SerializeField, Min(0.01f)] private float interactionDistance = 1.8f;
        [SerializeField] private GameObject promptPanel;
        [SerializeField] private TMP_Text promptText;
        [SerializeField] private GameObject dialoguePanel;
        [SerializeField] private TMP_Text dialogueTitle;
        [SerializeField] private TMP_Text dialogueBody;
        [SerializeField] private Button closeButton;

        [Header("U04/U05 — Bansos & Delivery Settings")]
        [SerializeField] private int initialPackages = 3;

        [Header("U06 — Mission Timer Settings")]
        [Tooltip("Durasi waktu misi dalam detik (default 180 detik / 03:00)")]
        [SerializeField, Min(1f)] private float initialTimeSeconds = 180f;

        private PlayerController movement;
        private bool movementWasEnabled;

        // Spawn point awal pemain untuk U17 / Retry Bersih
        private Vector3 initialSpawnPosition;
        private Quaternion initialSpawnRotation;

        // Runtime Tracking Data (U04, U05, U06, U07, U08)
        [Header("Status Misi Bansos")]
        [SerializeField] private bool isMissionActive = true;
        private int packagesRemaining = 3;
        private int deliveredCount = 0;
        private int trust = 0;
        private float timeRemaining = 180f;

        public bool IsMissionActive => isMissionActive;
        public event System.Action MissionReset;

        // U07 & U08: State Hasil Misi & Layar Hasil
        private MissionOutcome outcome = MissionOutcome.InProgress;
        private string outcomeReason = "";
        private bool isOutcomeOpen = false;
        private bool isSystemDialogue = false;
        private DialogueLine[] dialogueLines;
        private int dialogueLineIndex;
        private int dialogueAdvanceFrame = -1;
        private DialoguePresentation dialoguePresentation;
        private int outcomeOpenedFrame = -1;
        private float outcomeOpenedTime = -10f;
        private bool returningToPosko;
        private Texture2D missionReportArt;

        private bool ReturnsToPosko => outcome == MissionOutcome.Success && GameFlowManager.Instance.CurrentPhase == DayPhase.DiLevel;

        public InteractionTarget CurrentTarget { get; private set; }
        public InteractionTarget DialogueTarget { get; private set; }
        public bool IsDialogueOpen { get; private set; }

        public int PackagesRemaining => packagesRemaining;
        public int DeliveredCount => deliveredCount;
        public int Trust => trust;
        public float TimeRemaining => timeRemaining;
        public bool IsTimeExpired => timeRemaining <= 0f;
        public bool IsMissionCompleted => deliveredCount >= initialPackages;

        public MissionOutcome Outcome => outcome;
        public string OutcomeReason => outcomeReason;
        public bool IsOutcomeOpen => isOutcomeOpen;
        public bool IsMissionEnded => outcome != MissionOutcome.InProgress;

        public string FormattedTimeRemaining
        {
            get
            {
                int minutes = Mathf.FloorToInt(timeRemaining / 60f);
                int seconds = Mathf.FloorToInt(timeRemaining % 60f);
                return $"{minutes:00}:{seconds:00}";
            }
        }

        private void Awake()
        {
            movement = GetComponent<PlayerController>();
            missionReportArt = Resources.Load<Texture2D>("JejakAsaUI/Generated/MissionReport");
            if (promptPanel == null || promptText == null || dialoguePanel == null
                || dialogueTitle == null || dialogueBody == null || closeButton == null)
            {
                Debug.LogError("PlayerInteraction requires its prompt and dialogue UI references.", this);
                enabled = false;
                return;
            }

            initialSpawnPosition = transform.position;
            initialSpawnRotation = transform.rotation;

            // Jika dalam alur campaign dan pemain belum mengambil paket di Posko,
            // misi pengantaran dan timer belum dimulai sampai mengambil paket di Posko
            if (GameFlowManager.Instance != null && !GameFlowManager.Instance.Progress.hasPickedUpBansos && (GameFlowManager.Instance.CurrentPhase == DayPhase.PoskoBriefing || GameFlowManager.Instance.LastSpawnPoint == "RumahVicky"))
            {
                isMissionActive = false;
                packagesRemaining = 0;
            }
            else
            {
                isMissionActive = true;
                packagesRemaining = initialPackages;
            }

            deliveredCount = 0;
            trust = 0;
            timeRemaining = initialTimeSeconds;
            outcome = MissionOutcome.InProgress;
            outcomeReason = "";
            isOutcomeOpen = false;

            DialoguePresentation.StylePrompt(promptPanel, promptText);
            promptPanel.SetActive(false);
            dialoguePanel.SetActive(false);
            closeButton.onClick.AddListener(AdvanceDialogue);
        }

        private void Update()
        {
            if (SessionMenu.InputBlocked)
                return;

            // Beri jeda agar tombol penutup dialog tidak langsung mengonfirmasi hasil.
            if (isOutcomeOpen)
            {
                if (Time.frameCount > outcomeOpenedFrame && (Time.unscaledTime - outcomeOpenedTime) >= 0.4f)
                {
                    Keyboard kb = Keyboard.current;
                    if (kb != null && (kb.enterKey.wasPressedThisFrame || kb.spaceKey.wasPressedThisFrame))
                    {
                        AcknowledgeOutcome();
                    }
                }
                return;
            }

            if (StoryNpcController.AnyDialogueOpen)
            {
                CurrentTarget = null;
                promptPanel.SetActive(false);
                return;
            }

            // U06 & U07: Timer hanya berkurang saat eksplorasi aktif, misi aktif & misi masih berlangsung
            if (!IsDialogueOpen && outcome == MissionOutcome.InProgress && isMissionActive)
            {
                if (IsMissionCompleted)
                {
                    // U07: Penyerahan ke-3 sah -> Berhasil (tidak boleh ditimpa gagal/parsial)
                    SetOutcome(MissionOutcome.Success, "Semua 3 paket bantuan berhasil disalurkan tepat waktu!");
                }
                else if (timeRemaining > 0f)
                {
                    SetTimeRemaining(timeRemaining - Time.deltaTime);
                }
            }

            Keyboard keyboard = Keyboard.current;
            if (IsDialogueOpen)
            {
                bool invalidTarget = !isSystemDialogue && (DialogueTarget == null || !DialogueTarget.isActiveAndEnabled);
                if (invalidTarget)
                    CloseDialogue();
                else if (keyboard != null && (keyboard.escapeKey.wasPressedThisFrame || keyboard.enterKey.wasPressedThisFrame || keyboard.spaceKey.wasPressedThisFrame))
                    AdvanceDialogue();
                return;
            }

            InteractionTarget nearest = FindNearestTarget();
            if (!ReferenceEquals(nearest, CurrentTarget))
            {
                CurrentTarget = nearest;
                promptPanel.SetActive(nearest != null);
                if (nearest != null)
                {
                    // Menyesuaikan teks prompt interaksi
                    if (nearest.IsRecipient && !nearest.IsDelivered && packagesRemaining > 0 && outcome == MissionOutcome.InProgress && timeRemaining > 0f)
                    {
                        promptText.text = "Serahkan bantuan ke " + nearest.DisplayName;
                    }
                    else
                    {
                        promptText.text = "Bicara dengan " + nearest.DisplayName;
                    }
                }
            }

            if (keyboard != null && keyboard.eKey.wasPressedThisFrame && CurrentTarget != null)
            {
                OpenDialogueWith(CurrentTarget);
            }
        }

        /// <summary>
        /// Saat pemain menekan E di depan warga:
        /// Jika warga belum menerima bansos dan misi masih aktif, bansos langsung diserahkan (U04)
        /// dan percakapan terima kasih warga langsung muncul di layar.
        /// </summary>
        private void OpenDialogueWith(InteractionTarget target)
        {
            DialogueTarget = target;
            CurrentTarget = null;
            IsDialogueOpen = true;
            movementWasEnabled = movement.enabled;
            movement.enabled = false;
            promptPanel.SetActive(false);

            dialogueTitle.text = DialogueTarget.DisplayName;

            // Logika Penyerahan Bansos Otomatis (U04, U05, U06, U07)
            // U07: Penyerahan sah hanya jika waktu masih tersisa (> 0) dan misi masih InProgress
            if (DialogueTarget.IsRecipient && !DialogueTarget.IsDelivered && packagesRemaining > 0 && timeRemaining > 0f && outcome == MissionOutcome.InProgress)
            {
                DialogueTarget.MarkAsDelivered();
                packagesRemaining--;
                deliveredCount++;
                trust++;

                Debug.Log($"[U04/U05/U06/U07] Bansos berhasil diserahkan ke {DialogueTarget.DisplayName}! Paket: {packagesRemaining}/3, Kepercayaan: {trust}/3, Sisa Waktu: {FormattedTimeRemaining}");

                if (IsMissionCompleted)
                {
                    SetOutcome(MissionOutcome.Success, "Semua 3 paket bantuan berhasil disalurkan tepat waktu!");
                    Debug.Log("[U05/U07/U08] SELURUH 3 PENERIMA SELESAI! Misi pengantaran bansos berhasil diselesaikan.");
                }
            }

            dialogueLines = DialogueTarget.DialogueLines;
            dialogueLineIndex = 0;
            dialogueAdvanceFrame = -1;
            if (dialogueLines != null && dialogueLines.Length > 0)
                ShowDialogueLine();
            else
                dialogueBody.text = DialogueTarget.Dialogue;
            dialoguePanel.SetActive(true);
            PresentDialogue();

            if (dialogueLines != null && dialogueLines.Length > 1 && UnityEngine.EventSystems.EventSystem.current != null)
                UnityEngine.EventSystems.EventSystem.current.SetSelectedGameObject(null);
            else
                closeButton.Select();
        }

        /// <summary>
        /// U07 & U08: Menetapkan hasil akhir misi dengan penjagaan prioritas.
        /// Hasil sah (Success) tidak boleh ditimpa gagal/parsial.
        /// </summary>
        public void SetOutcome(MissionOutcome newOutcome, string reason)
        {
            if (newOutcome == MissionOutcome.InProgress)
            {
                outcome = MissionOutcome.InProgress;
                outcomeReason = "";
                isOutcomeOpen = false;
                return;
            }

            // Hasil akhir yang sudah ditetapkan tidak boleh ditimpa.
            if (outcome != MissionOutcome.InProgress)
            {
                return;
            }

            outcome = newOutcome;
            outcomeReason = reason;

            // Jika sedang tidak ada dialog, langsung buka layar hasil
            if (!IsDialogueOpen)
            {
                OpenOutcome();
            }
        }

        public void OpenOutcome()
        {
            isOutcomeOpen = true;
            outcomeOpenedFrame = Time.frameCount;
            outcomeOpenedTime = Time.unscaledTime;
            if (UnityEngine.EventSystems.EventSystem.current != null)
                UnityEngine.EventSystems.EventSystem.current.SetSelectedGameObject(null);
            if (movement != null)
                movement.enabled = false;
            if (promptPanel != null)
                promptPanel.SetActive(false);
            CurrentTarget = null;
        }

        private void AcknowledgeOutcome()
        {
            if (returningToPosko)
                return;

            if (ReturnsToPosko)
            {
                returningToPosko = true;
                if (GameFlowManager.Instance.OnLevelSuccess())
                    SceneManager.LoadScene("Assets/_Project/Scenes/Posko.unity", LoadSceneMode.Single);
                else
                    returningToPosko = false;
                return;
            }

            ResetMissionSession();
        }

        public void CloseOutcome()
        {
            isOutcomeOpen = false;
            if (movement != null)
                movement.enabled = true;
        }

        /// <summary>
        /// Mengambil daftar nama warga penerima yang berhasil dibantu dalam sesi ini (U08).
        /// </summary>
        public System.Collections.Generic.List<string> GetDeliveredRecipientNames()
        {
            var list = new System.Collections.Generic.List<string>();
            foreach (var target in InteractionTarget.Active)
            {
                if (target != null && target.IsRecipient && target.IsDelivered)
                {
                    list.Add(target.DisplayName);
                }
            }
            return list;
        }

        /// <summary>
        /// Mengambil paket bansos dari Posko Bantuan dan memulai timer misi penyaluran.
        /// </summary>
        public void StartDeliveryMissionFromPosko()
        {
            packagesRemaining = initialPackages;
            deliveredCount = 0;
            trust = 0;
            timeRemaining = initialTimeSeconds;
            isMissionActive = true;
            outcome = MissionOutcome.InProgress;
            outcomeReason = "";
            Debug.Log($"[Posko] Paket bansos diambil! Misi penyaluran ({packagesRemaining} paket) resmi dimulai.");
        }

        /// <summary>
        /// Reset seluruh sesi pengantaran bansos ke kondisi awal bersih (U05, U06, U08, U17).
        /// Pemain kembali ke pos awal, waktu 180s, 3 paket, kepercayaan 0.
        /// </summary>
        public void ResetMissionSession()
        {
            packagesRemaining = initialPackages;
            deliveredCount = 0;
            trust = 0;
            timeRemaining = initialTimeSeconds;

            foreach (var target in InteractionTarget.Active)
            {
                if (target != null)
                {
                    target.ResetDelivery();
                }
            }

            // Kembalikan posisi pemain ke pos awal bersih (U17)
            CharacterController cc = GetComponent<CharacterController>();
            if (cc != null) cc.enabled = false;
            transform.position = initialSpawnPosition;
            transform.rotation = initialSpawnRotation;
            if (cc != null) cc.enabled = true;
            Physics.SyncTransforms();

            outcome = MissionOutcome.InProgress;
            outcomeReason = "";
            isOutcomeOpen = false;

            CloseDialogue();
            if (movement != null)
                movement.enabled = true;
            MissionReset?.Invoke();

            Debug.Log("[U05/U06/U08/U17] Sesi misi telah direset bersih.");
        }

        public void SetTimeRemaining(float seconds)
        {
            timeRemaining = Mathf.Max(0f, seconds);
            if (timeRemaining <= 0f && outcome == MissionOutcome.InProgress)
            {
                if (IsMissionCompleted)
                {
                    SetOutcome(MissionOutcome.Success, "Semua 3 paket bantuan berhasil disalurkan tepat waktu!");
                }
                else if (deliveredCount > 0)
                {
                    SetOutcome(MissionOutcome.PartialSuccess, "Waktu pengantaran telah habis.");
                }
                else
                {
                    SetOutcome(MissionOutcome.Failed, "Waktu pengantaran telah habis sebelum bantuan sempat disalurkan.");
                }
            }
        }

        public bool TryDeliverBansos()
        {
            // U07: Tolak jika warga bukan penerima, sudah selesai, paket habis, waktu habis, atau misi selesai
            if (DialogueTarget == null || !DialogueTarget.IsRecipient || DialogueTarget.IsDelivered
                || packagesRemaining <= 0 || timeRemaining <= 0f || outcome != MissionOutcome.InProgress)
            {
                return false;
            }

            DialogueTarget.MarkAsDelivered();
            packagesRemaining--;
            deliveredCount++;
            trust++;

            if (dialogueBody != null)
            {
                dialogueBody.text = DialogueTarget.Dialogue;
            }

            if (IsMissionCompleted)
            {
                SetOutcome(MissionOutcome.Success, "Semua 3 paket bantuan berhasil disalurkan tepat waktu!");
            }

            return true;
        }

        private InteractionTarget FindNearestTarget()
        {
            InteractionTarget nearest = null;
            float nearestDistance = interactionDistance * interactionDistance;
            Vector3 origin = transform.position;
            foreach (InteractionTarget candidate in InteractionTarget.Active)
            {
                if (candidate == null || !candidate.isActiveAndEnabled)
                    continue;
                Vector3 offset = candidate.transform.position - origin;
                offset.y = 0f;
                float distance = offset.sqrMagnitude;
                if (distance <= nearestDistance && (nearest == null || distance < nearestDistance))
                {
                    nearest = candidate;
                    nearestDistance = distance;
                }
            }
            return nearest;
        }

        public void OpenSystemDialogue(string title, string body)
        {
            if (dialoguePanel == null || dialogueTitle == null || dialogueBody == null || closeButton == null)
                return;

            DialogueTarget = null;
            CurrentTarget = null;
            isSystemDialogue = true;
            dialogueLines = null;
            dialogueAdvanceFrame = -1;
            IsDialogueOpen = true;
            movementWasEnabled = true;

            if (movement != null)
                movement.enabled = false;
            if (promptPanel != null)
                promptPanel.SetActive(false);

            dialogueTitle.text = title;
            dialogueBody.text = body;
            dialoguePanel.SetActive(true);
            PresentDialogue();
            closeButton.Select();
        }

        private void ShowDialogueLine()
        {
            DialogueLine line = dialogueLines[dialogueLineIndex];
            dialogueTitle.text = string.IsNullOrEmpty(line.speaker) ? DialogueTarget.DisplayName : line.speaker;
            dialogueBody.text = line.text;
            TMP_Text label = closeButton.GetComponentInChildren<TMP_Text>(true);
            if (label != null && dialogueLines.Length > 1)
                label.text = dialogueLineIndex + 1 < dialogueLines.Length ? "Lanjut" : "Tutup";
            if (dialoguePresentation != null)
                dialoguePresentation.Line(dialogueTitle.text);
        }

        private void PresentDialogue()
        {
            if (dialoguePresentation == null)
            {
                dialoguePresentation = dialoguePanel.GetComponent<DialoguePresentation>();
                if (dialoguePresentation == null)
                    dialoguePresentation = dialoguePanel.AddComponent<DialoguePresentation>();
                dialoguePresentation.Setup(dialogueTitle, dialogueBody, closeButton);
            }
            dialoguePresentation.Open(dialogueTitle.text);
        }

        private void AdvanceDialogue()
        {
            if (!IsDialogueOpen || dialogueAdvanceFrame == Time.frameCount)
                return;
            dialogueAdvanceFrame = Time.frameCount;
            if (dialogueLines != null && dialogueLineIndex + 1 < dialogueLines.Length)
            {
                dialogueLineIndex++;
                ShowDialogueLine();
                return;
            }
            CloseDialogue();
        }

        private void CloseDialogue()
        {
            if (!IsDialogueOpen)
                return;
            IsDialogueOpen = false;
            isSystemDialogue = false;
            if (dialogueLines != null && dialogueLines.Length > 1 && closeButton != null)
            {
                TMP_Text label = closeButton.GetComponentInChildren<TMP_Text>(true);
                if (label != null)
                    label.text = "Tutup";
            }
            dialogueLines = null;
            DialogueTarget = null;
            if (dialoguePanel != null)
                dialoguePanel.SetActive(false);

            // U08: Jika misi sudah berakhir saat dialog ditutup, buka layar hasil
            if (outcome != MissionOutcome.InProgress)
            {
                OpenOutcome();
            }
            else if (movement != null)
            {
                movement.enabled = movementWasEnabled;
            }
        }

        private void OnDisable()
        {
            CloseDialogue();
            CurrentTarget = null;
            if (promptPanel != null)
                promptPanel.SetActive(false);
        }

        private void OnDestroy()
        {
            if (closeButton != null)
                closeButton.onClick.RemoveListener(AdvanceDialogue);
        }

        private void OnGUI()
        {
            if (SessionMenu.InputBlocked || !isOutcomeOpen) return;
            CozyGuiTheme.EnsureInitialized();
            Matrix4x4 old = GUI.matrix; float scale = CozyGuiTheme.Scale;
            GUI.matrix = old * Matrix4x4.Scale(new Vector3(scale, scale, 1));
            float sw = Screen.width / scale, sh = Screen.height / scale;
            GUI.depth = -100;
            GUI.DrawTexture(new Rect(0, 0, sw, sh), CozyGuiTheme.DimTex);
            bool success = outcome == MissionOutcome.Success;
            Rect r = new Rect((sw - 620) / 2, (sh - 602) / 2, 620, 602);
            CozyGuiTheme.Header(r, "LAPORAN PENGANTARAN / HARI " + GameFlowManager.Instance.CurrentDay,
                success ? "Harapan telah sampai." : "Masih ada kesempatan.", outcomeReason, missionReportArt);
            GUI.Label(new Rect(r.x + 28, r.y + 210, 564, 27), success ? "MISI SELESAI" : outcome == MissionOutcome.PartialSuccess ? "SEBAGIAN BANTUAN TERANTAR" : "WAKTU PENGANTARAN BERAKHIR", CozyGuiTheme.Eyebrow);
            Rect stats = new Rect(r.x + 28, r.y + 250, 564, 172);
            CozyGuiTheme.Card(stats, true);
            string[] names = { "Bantuan tersalurkan", "Warga terbantu", "Kepercayaan warga", "Sisa waktu" };
            string[] values = { deliveredCount + " / " + initialPackages + " paket", deliveredCount + " / 3 warga", trust + " / 3", FormattedTimeRemaining };
            for (int i = 0; i < 4; i++)
            {
                GUI.Label(new Rect(stats.x + 20, stats.y + 14 + 36 * i, 330, 30), names[i], CozyGuiTheme.StatLeftStyle);
                GUI.Label(new Rect(stats.x + 350, stats.y + 14 + 36 * i, 194, 30), values[i], CozyGuiTheme.StatRightStyle);
            }
            string recipients = deliveredCount > 0 ? "Terima kasih sudah hadir untuk warga hari ini." : "Susun kembali rute, lalu coba sekali lagi.";
            GUI.Label(new Rect(r.x + 28, r.y + 432, 564, 42), recipients, CozyGuiTheme.Small);
            bool returnToPosko = success && GameFlowManager.Instance.CurrentPhase == DayPhase.DiLevel;
            string action = returnToPosko ? "Kembali ke posko" : "Coba pengantaran lagi";
            if (CozyGuiTheme.Button(new Rect(r.x + 28, r.y + 492, 564, 54), action + "    >", CozyGuiTheme.TealBtnStyle)) AcknowledgeOutcome();
            GUI.Label(new Rect(r.x + 28, r.y + 561, 564, 20), "Enter / Spasi  " + action, CozyGuiTheme.HintStyle);
            GUI.matrix = old;
        }
    }
}

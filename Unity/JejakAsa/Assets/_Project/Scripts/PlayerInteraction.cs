using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;

namespace JejakAsa
{
    public enum MissionOutcome
    {
        InProgress,
        Success,        // 3/3 paket terantar tepat waktu (U08)
        PartialSuccess, // 1-2 paket terantar, waktu habis / kalah battle (U08)
        Failed          // 0 paket terantar, waktu habis / kalah battle (U08)
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
        private int packagesRemaining = 3;
        private int deliveredCount = 0;
        private int trust = 0;
        private float timeRemaining = 180f;

        // U07 & U08: State Hasil Misi & Layar Hasil
        private MissionOutcome outcome = MissionOutcome.InProgress;
        private string outcomeReason = "";
        private bool isOutcomeOpen = false;
        private bool isSystemDialogue = false;
        private int outcomeOpenedFrame = -1;
        private float outcomeOpenedTime = -10f;

        public InteractionTarget CurrentTarget { get; private set; }
        public InteractionTarget DialogueTarget { get; private set; }
        public bool IsDialogueOpen { get; private set; }
        public bool IsEncounterOpen => EncounterController.AnyOpen;
        public bool IsInBattle => BattleTransition.Active != null && BattleTransition.Active.IsInBattle;

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
            if (promptPanel == null || promptText == null || dialoguePanel == null
                || dialogueTitle == null || dialogueBody == null || closeButton == null)
            {
                Debug.LogError("PlayerInteraction requires its prompt and dialogue UI references.", this);
                enabled = false;
                return;
            }

            initialSpawnPosition = transform.position;
            initialSpawnRotation = transform.rotation;

            packagesRemaining = initialPackages;
            deliveredCount = 0;
            trust = 0;
            timeRemaining = initialTimeSeconds;
            outcome = MissionOutcome.InProgress;
            outcomeReason = "";
            isOutcomeOpen = false;

            promptPanel.SetActive(false);
            dialoguePanel.SetActive(false);
            closeButton.onClick.AddListener(CloseDialogue);
        }

        private void Update()
        {
            if (SessionMenu.InputBlocked)
                return;

            // Jika Layar Hasil Misi sedang terbuka, beri jeda 0.4 detik agar tombol Enter penutup dialog tidak langsung mereset misi di frame yang sama
            if (isOutcomeOpen)
            {
                if (Time.frameCount > outcomeOpenedFrame && (Time.unscaledTime - outcomeOpenedTime) >= 0.4f)
                {
                    Keyboard kb = Keyboard.current;
                    if (kb != null && (kb.enterKey.wasPressedThisFrame || kb.spaceKey.wasPressedThisFrame))
                    {
                        ResetMissionSession();
                    }
                }
                return;
            }

            // U06 & U07: Timer hanya berkurang saat eksplorasi aktif & misi masih berlangsung
            if (!IsDialogueOpen && !IsEncounterOpen && !IsInBattle && outcome == MissionOutcome.InProgress)
            {
                if (IsMissionCompleted)
                {
                    // U07: Penyerahan ke-3 sah -> Berhasil (tidak boleh ditimpa gagal/parsial)
                    SetOutcome(MissionOutcome.Success, "Semua 3 paket bantuan berhasil disalurkan tepat waktu!");
                }
                else if (timeRemaining > 0f)
                {
                    timeRemaining = Mathf.Max(0f, timeRemaining - Time.deltaTime);
                    if (timeRemaining <= 0f)
                    {
                        // U07 & U08: Waktu habis saat misi berlangsung
                        if (deliveredCount > 0)
                        {
                            SetOutcome(MissionOutcome.PartialSuccess, "Waktu pengantaran telah habis.");
                        }
                        else
                        {
                            SetOutcome(MissionOutcome.Failed, "Waktu pengantaran telah habis sebelum bantuan sempat disalurkan.");
                        }
                    }
                }
            }

            if (IsEncounterOpen || IsInBattle)
            {
                CurrentTarget = null;
                if (promptPanel != null && promptPanel.activeSelf)
                    promptPanel.SetActive(false);
                return;
            }

            Keyboard keyboard = Keyboard.current;
            if (IsDialogueOpen)
            {
                bool invalidTarget = !isSystemDialogue && (DialogueTarget == null || !DialogueTarget.isActiveAndEnabled);
                if (invalidTarget
                    || (keyboard != null && (keyboard.escapeKey.wasPressedThisFrame || keyboard.enterKey.wasPressedThisFrame)))
                {
                    CloseDialogue();
                }
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
                        promptText.text = "[E] Serahkan Bansos ke " + nearest.DisplayName;
                    }
                    else
                    {
                        promptText.text = "[E] Bicara dengan " + nearest.DisplayName;
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

            // Tampilkan percakapan warga
            dialogueBody.text = DialogueTarget.Dialogue;
            dialoguePanel.SetActive(true);

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

            // U07 Guard: Jika sudah berhasil sah (Success), event waktu habis tidak boleh menimpa
            if (outcome == MissionOutcome.Success)
            {
                return;
            }

            // Jika sudah ada status akhir (Partial/Failed), jangan timpa lagi
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

            for (int i = 0; i < EncounterController.Active.Count; i++)
            {
                EncounterController.Active[i]?.ResetEncounter();
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

            Debug.Log("[U05/U06/U08/U17] Sesi misi telah direset bersih.");
        }

        public void SetTimeRemaining(float seconds)
        {
            timeRemaining = Mathf.Max(0f, seconds);
            if (timeRemaining <= 0f && outcome == MissionOutcome.InProgress)
            {
                if (deliveredCount > 0)
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

        public void SetEncounterMovementLock(bool locked)
        {
            if (locked)
            {
                CurrentTarget = null;
                if (promptPanel != null)
                    promptPanel.SetActive(false);
                if (movement != null)
                    movement.enabled = false;
            }
            else if (movement != null && !IsDialogueOpen && !isOutcomeOpen && !IsEncounterOpen && !IsInBattle)
            {
                movement.enabled = true;
            }
        }

        public void OpenSystemDialogue(string title, string body)
        {
            if (dialoguePanel == null || dialogueTitle == null || dialogueBody == null || closeButton == null)
                return;

            DialogueTarget = null;
            CurrentTarget = null;
            isSystemDialogue = true;
            IsDialogueOpen = true;
            movementWasEnabled = true;

            if (movement != null)
                movement.enabled = false;
            if (promptPanel != null)
                promptPanel.SetActive(false);

            dialogueTitle.text = title;
            dialogueBody.text = body;
            dialoguePanel.SetActive(true);
            closeButton.Select();
        }

        private void CloseDialogue()
        {
            if (!IsDialogueOpen)
                return;
            IsDialogueOpen = false;
            isSystemDialogue = false;
            DialogueTarget = null;
            if (dialoguePanel != null)
                dialoguePanel.SetActive(false);

            // U08: Jika misi sudah berakhir saat dialog ditutup, buka layar hasil
            if (outcome != MissionOutcome.InProgress)
            {
                OpenOutcome();
            }
            else if (movement != null && !IsEncounterOpen)
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
                closeButton.onClick.RemoveListener(CloseDialogue);
        }

        private void OnGUI()
        {
            if (IsInBattle || SessionMenu.InputBlocked)
                return;

            CozyGuiTheme.EnsureInitialized();
            float scale = CozyGuiTheme.Scale;
            Matrix4x4 previousMatrix = GUI.matrix;
            GUI.matrix = previousMatrix * Matrix4x4.Scale(new Vector3(scale, scale, 1f));
            float screenWidth = Screen.width / scale;
            float screenHeight = Screen.height / scale;

            float cardX = 20f;
            float cardY = 26f;
            float cardW = 276f;
            float cardH = 114f;

            // 1. Bayangan kartun lembut di belakang kartu
            GUI.DrawTexture(new Rect(cardX + 3, cardY + 4, cardW, cardH), CozyGuiTheme.ShadowTex);

            // 2. Kartu Stiker Utama (Cream Hangat dengan Outline Toska)
            GUI.Box(new Rect(cardX, cardY, cardW, cardH), GUIContent.none, CozyGuiTheme.CardStyle);

            // 3. Pita Judul Mungil di Atas Kiri
            GUI.Label(new Rect(cardX + 12, cardY - 12, 114, 24), "MISI BANSOS", CozyGuiTheme.RibbonStyle);

            // 4. Pil Timer Bulat di Atas Kanan
            if (timeRemaining <= 15f)
                CozyGuiTheme.TimerStyle.normal.background = CozyGuiTheme.TimerDangerTex;
            else if (timeRemaining <= 45f)
                CozyGuiTheme.TimerStyle.normal.background = CozyGuiTheme.TimerWarnTex;
            else
                CozyGuiTheme.TimerStyle.normal.background = CozyGuiTheme.TimerNormalTex;

            GUI.Label(new Rect(cardX + cardW - 84, cardY - 12, 72, 24), FormattedTimeRemaining, CozyGuiTheme.TimerStyle);

            // 5. Baris Utama: Warga Terbantu + 3 Stempel Lingkaran
            GUI.Label(new Rect(cardX + 16, cardY + 22, 140, 24), "Warga Terbantu", CozyGuiTheme.LabelStyle);

            for (int i = 0; i < 3; i++)
            {
                Rect stampRect = new Rect(cardX + 184 + (i * 26), cardY + 23, 22, 22);
                bool done = i < deliveredCount;
                GUI.DrawTexture(stampRect, done ? CozyGuiTheme.StampDoneTex : CozyGuiTheme.StampEmptyTex);
            }

            // 6. Info Paket & Kepercayaan
            GUI.Label(new Rect(cardX + 16, cardY + 50, 125, 20), $"Paket: <b><color=#0F766E>{packagesRemaining}</color></b> tersisa", CozyGuiTheme.SubLabelStyle);
            GUI.Label(new Rect(cardX + 148, cardY + 50, 120, 20), $"Kepercayaan: <b><color=#B45309>{trust}/3</color></b>", CozyGuiTheme.SubLabelStyle);

            // 7. Gelembung Pesan Penyemangat di Bagian Bawah
            string cheerText;
            if (IsMissionCompleted)
            {
                CozyGuiTheme.BannerStyle.normal.background = CozyGuiTheme.BannerSuccessTex;
                CozyGuiTheme.BannerStyle.normal.textColor = new Color(0.05f, 0.45f, 0.30f);
                cheerText = "Horeee! Semua warga terbantu!";
            }
            else if (timeRemaining <= 0f)
            {
                CozyGuiTheme.BannerStyle.normal.background = CozyGuiTheme.BannerFailTex;
                CozyGuiTheme.BannerStyle.normal.textColor = new Color(0.65f, 0.12f, 0.12f);
                cheerText = "Waktu habis! Yuk coba lagi!";
            }
            else
            {
                CozyGuiTheme.BannerStyle.normal.background = CozyGuiTheme.BannerInfoTex;
                CozyGuiTheme.BannerStyle.normal.textColor = new Color(0.55f, 0.35f, 0.05f);
                cheerText = deliveredCount switch
                {
                    0 => "Yuk antar bansos ke 3 warga!",
                    1 => "Bagus! Tinggal 2 warga lagi!",
                    _ => "Semangat! 1 warga terakhir!"
                };
            }

            GUI.Label(new Rect(cardX + 14, cardY + 76, cardW - 28, 25), cheerText, CozyGuiTheme.BannerStyle);

            // =========================================================================
            // U08: LAYAR HASIL MISI (OUTCOME OVERLAY POPUP)
            // =========================================================================
            if (isOutcomeOpen)
            {
                GUI.depth = -100;

                GUI.DrawTexture(new Rect(0, 0, screenWidth, screenHeight), CozyGuiTheme.DimTex);

                float popW = 360f;
                float popH = 286f;
                float popX = (screenWidth - popW) * 0.5f;
                float popY = Mathf.Max(20f, (screenHeight - popH) * 0.5f);

                GUI.DrawTexture(new Rect(popX + 4, popY + 6, popW, popH), CozyGuiTheme.ShadowTex);
                GUI.Box(new Rect(popX, popY, popW, popH), GUIContent.none, CozyGuiTheme.CardStyle);

                string outcomeBadgeText;
                Texture2D outcomeBadgeBg;
                switch (outcome)
                {
                    case MissionOutcome.Success:
                        outcomeBadgeText = "MISI BERHASIL!";
                        outcomeBadgeBg = CozyGuiTheme.OutcomeSuccessTex;
                        break;
                    case MissionOutcome.PartialSuccess:
                        outcomeBadgeText = "BERHASIL SEBAGIAN";
                        outcomeBadgeBg = CozyGuiTheme.OutcomePartialTex;
                        break;
                    default:
                        outcomeBadgeText = "MISI BELUM BERHASIL";
                        outcomeBadgeBg = CozyGuiTheme.OutcomeFailTex;
                        break;
                }
                CozyGuiTheme.OutcomeHeaderStyle.normal.background = outcomeBadgeBg;
                GUI.Label(new Rect(popX + (popW - 210) * 0.5f, popY - 13, 210, 26), outcomeBadgeText, CozyGuiTheme.OutcomeHeaderStyle);

                GUI.Label(new Rect(popX + 20, popY + 18, popW - 40, 30), outcomeReason, CozyGuiTheme.OutcomeDescStyle);

                Rect innerRect = new Rect(popX + 20, popY + 52, popW - 40, 152);
                GUI.Box(innerRect, GUIContent.none, CozyGuiTheme.InnerBoxStyle);

                float lineY = innerRect.y + 10;
                float lineH = 22f;
                float labelW = 145f;
                float valW = innerRect.width - labelW - 24;

                GUI.Label(new Rect(innerRect.x + 14, lineY, labelW, lineH), "Paket Tersalurkan", CozyGuiTheme.StatLeftStyle);
                GUI.Label(new Rect(innerRect.x + 14 + labelW, lineY, valW, lineH), $"<b>{deliveredCount} / {initialPackages}</b> Paket", CozyGuiTheme.StatRightStyle);

                GUI.Label(new Rect(innerRect.x + 14, lineY + lineH, labelW, lineH), "Warga Terbantu", CozyGuiTheme.StatLeftStyle);
                GUI.Label(new Rect(innerRect.x + 14 + labelW, lineY + lineH, valW, lineH), $"<b><color=#0F766E>{deliveredCount} / 3</color></b> Orang", CozyGuiTheme.StatRightStyle);

                GUI.Label(new Rect(innerRect.x + 14, lineY + lineH * 2, labelW, lineH), "Kepercayaan Warga", CozyGuiTheme.StatLeftStyle);
                GUI.Label(new Rect(innerRect.x + 14 + labelW, lineY + lineH * 2, valW, lineH), $"<b><color=#B45309>{trust} / 3</color></b>", CozyGuiTheme.StatRightStyle);

                GUI.Label(new Rect(innerRect.x + 14, lineY + lineH * 3, labelW, lineH), "Sisa Waktu Akhir", CozyGuiTheme.StatLeftStyle);
                GUI.Label(new Rect(innerRect.x + 14 + labelW, lineY + lineH * 3, valW, lineH), $"<b>{FormattedTimeRemaining}</b>", CozyGuiTheme.StatRightStyle);

                var names = GetDeliveredRecipientNames();
                string namesStr = names.Count > 0 ? string.Join(", ", names) : "(Belum ada)";
                GUI.Label(new Rect(innerRect.x + 14, lineY + lineH * 4 + 2, innerRect.width - 28, 36), $"Penerima Selesai: <b><color=#0F766E>{namesStr}</color></b>", CozyGuiTheme.SubLabelStyle);

                Rect btnRect = new Rect(popX + (popW - 170) * 0.5f, popY + popH - 64, 170, 34);
                if (GUI.Button(btnRect, "Ulang Misi", CozyGuiTheme.TealBtnStyle))
                {
                    ResetMissionSession();
                }

                GUI.Label(new Rect(popX + 20, popY + popH - 25, popW - 40, 18), "Tekan Enter atau Space untuk Ulang Misi", CozyGuiTheme.HintStyle);
            }
            GUI.matrix = previousMatrix;
        }
    }
}

using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;

namespace JejakAsa
{
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

        // Runtime Tracking Data (U04, U05, U06)
        private int packagesRemaining = 3;
        private int deliveredCount = 0;
        private int trust = 0;
        private float timeRemaining = 180f;

        public InteractionTarget CurrentTarget { get; private set; }
        public InteractionTarget DialogueTarget { get; private set; }
        public bool IsDialogueOpen { get; private set; }

        public int PackagesRemaining => packagesRemaining;
        public int DeliveredCount => deliveredCount;
        public int Trust => trust;
        public float TimeRemaining => timeRemaining;
        public bool IsTimeExpired => timeRemaining <= 0f;
        public bool IsMissionCompleted => deliveredCount >= initialPackages;

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

            packagesRemaining = initialPackages;
            deliveredCount = 0;
            trust = 0;
            timeRemaining = initialTimeSeconds;

            promptPanel.SetActive(false);
            dialoguePanel.SetActive(false);
            closeButton.onClick.AddListener(CloseDialogue);
        }

        private void Update()
        {
            // U06: Timer hanya berkurang saat eksplorasi aktif (berhenti saat dialog terbuka & saat misi selesai)
            if (!IsDialogueOpen && !IsMissionCompleted && timeRemaining > 0f)
            {
                timeRemaining = Mathf.Max(0f, timeRemaining - Time.deltaTime);
            }

            Keyboard keyboard = Keyboard.current;
            if (IsDialogueOpen)
            {
                if (DialogueTarget == null || !DialogueTarget.isActiveAndEnabled
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
                    if (nearest.IsRecipient && !nearest.IsDelivered && packagesRemaining > 0)
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
        /// Jika warga belum menerima bansos, bansos langsung diserahkan (U04)
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

            // Logika Penyerahan Bansos Otomatis (U04, U05, U06)
            // Penyerahan sah hanya jika waktu masih tersisa (> 0) (U06 & U07)
            if (DialogueTarget.IsRecipient && !DialogueTarget.IsDelivered && packagesRemaining > 0 && timeRemaining > 0f)
            {
                DialogueTarget.MarkAsDelivered();
                packagesRemaining--;
                deliveredCount++;
                trust++;

                Debug.Log($"[U04/U05/U06] Bansos berhasil diserahkan ke {DialogueTarget.DisplayName}! Paket tersisa: {packagesRemaining}/3, Kepercayaan: {trust}/3, Sisa Waktu: {FormattedTimeRemaining}");

                if (IsMissionCompleted)
                {
                    Debug.Log("[U05] SELURUH 3 PENERIMA SELESAI! Misi pengantaran bansos berhasil diselesaikan.");
                }
            }

            // Tampilkan percakapan warga
            dialogueBody.text = DialogueTarget.Dialogue;
            dialoguePanel.SetActive(true);

            closeButton.Select();
        }

        /// <summary>
        /// Reset seluruh sesi pengantaran bansos ke kondisi awal bersih (U05, U06, U17).
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

            CloseDialogue();
            Debug.Log("[U05/U06] Sesi misi telah direset bersih.");
        }

        public void SetTimeRemaining(float seconds)
        {
            timeRemaining = Mathf.Max(0f, seconds);
        }

        public bool TryDeliverBansos()
        {
            if (DialogueTarget == null || !DialogueTarget.IsRecipient || DialogueTarget.IsDelivered || packagesRemaining <= 0 || timeRemaining <= 0f)
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

        private void CloseDialogue()
        {
            if (!IsDialogueOpen)
                return;
            IsDialogueOpen = false;
            DialogueTarget = null;
            if (dialoguePanel != null)
                dialoguePanel.SetActive(false);
            if (movement != null)
                movement.enabled = movementWasEnabled;
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

        // Tampilan HUD Status Bansos & Timer di Pojok Kiri Atas (U06)
        private void OnGUI()
        {
            float boxH = (IsMissionCompleted || timeRemaining <= 0f) ? 142 : 120;
            GUI.Box(new Rect(20, 20, 260, boxH), "JEJAK ASA — STATUS MISI");

            string timerStatus = IsDialogueOpen ? " <color=#FFFF55>[JEDA]</color>" : "";
            string timerColor = timeRemaining <= 30f ? (timeRemaining <= 0f ? "red" : "#FFAA00") : "white";
            GUI.Label(new Rect(32, 45, 230, 22), $"⏱️ Sisa Waktu: <b><color={timerColor}>{FormattedTimeRemaining}</color></b>{timerStatus}");

            GUI.Label(new Rect(32, 65, 230, 22), $"📦 Paket Tersisa: <b>{packagesRemaining}/3</b>");
            GUI.Label(new Rect(32, 85, 230, 22), $"✅ Warga Terbantu: <b>{deliveredCount}/3</b>");
            GUI.Label(new Rect(32, 105, 230, 22), $"🤝 Kepercayaan: <b>{trust}/3</b>");

            if (IsMissionCompleted)
            {
                GUI.Label(new Rect(32, 124, 230, 20), "<b><color=#55FF55>🎉 SEMUA 3 BANSOS TERKIRIM!</color></b>");
            }
            else if (timeRemaining <= 0f)
            {
                GUI.Label(new Rect(32, 124, 230, 20), "<b><color=#FF5555>⚠️ WAKTU MISI HABIS!</color></b>");
            }
        }
    }
}

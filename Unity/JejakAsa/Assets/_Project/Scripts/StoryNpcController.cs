using UnityEngine;
using UnityEngine.InputSystem;

namespace JejakAsa
{
    public enum StoryNpcType
    {
        Jonathan,
        Ophelia,
        Ibu
    }

    /// <summary>
    /// Pengendali dialog dan pemicu progres naratif untuk Jonathan, Ophelia, dan Ibu (R02, R07, R08).
    /// </summary>
    [DisallowMultipleComponent]
    public class StoryNpcController : MonoBehaviour
    {
        [Header("Identitas NPC")]
        public StoryNpcType npcType = StoryNpcType.Jonathan;
        public string characterName = "Pak Jonathan";
        public float interactionRadius = 2.0f;
        [SerializeField] private DialogueLine[] dialogueLines;

        [Header("Referensi")]
        public Transform playerTransform;

        private bool isPlayerInRange;
        private bool isDialogueOpen;
        private string currentDialogueText = string.Empty;
        private int dialogueLineIndex;
        private int dialogueAdvanceFrame = -1;
        private DayPhase dialoguePhase;
        private PlayerController movement;
        private bool movementWasEnabled;
        private static StoryNpcController activeDialogue;
        private DialogueLine[] activeLines;
        private System.Action sequenceCompleted;
        private float dialogueOpenedAt;
        private float dialogueLineAt;
        public static event System.Action<string> SpeakerChanged;
        private static int escapeConsumedFrame = -1;
        internal static bool EscapeConsumedThisFrame => escapeConsumedFrame == Time.frameCount;
        internal static bool AnyDialogueOpen => activeDialogue != null;
        internal bool IsDialogueOpen => isDialogueOpen;

        private void Start()
        {
            if (playerTransform == null)
            {
                var p = GameObject.FindWithTag("Player");
                if (p != null)
                    playerTransform = p.transform;
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

            if (GameFlowManager.Instance.CurrentPhase != DayPhase.Prolog && isPlayerInRange && !isDialogueOpen && !AnyDialogueOpen && kb.eKey.wasPressedThisFrame && PoskoDistributionTrigger.IsNearestInteraction(this, playerTransform))
            {
                OpenDialogue();
            }
            else if (isDialogueOpen && (kb.enterKey.wasPressedThisFrame || kb.spaceKey.wasPressedThisFrame || kb.escapeKey.wasPressedThisFrame))
            {
                if (kb.escapeKey.wasPressedThisFrame)
                    escapeConsumedFrame = Time.frameCount;
                CloseDialogue();
            }
        }

        public void OpenDialogue()
        {
            OpenDialogue(null, null);
        }

        public bool OpenSequence(DialogueLine[] lines, System.Action onCompleted)
        {
            return OpenDialogue(lines, onCompleted);
        }

        private bool OpenDialogue(DialogueLine[] lines, System.Action onCompleted)
        {
            if (isDialogueOpen || AnyDialogueOpen)
                return false;
            var interaction = playerTransform != null ? playerTransform.GetComponent<PlayerInteraction>() : null;
            if (interaction != null && (interaction.IsDialogueOpen || interaction.IsOutcomeOpen))
                return false;

            isDialogueOpen = true;
            activeDialogue = this;
            activeLines = lines ?? dialogueLines;
            sequenceCompleted = onCompleted;
            movement = playerTransform != null ? playerTransform.GetComponent<PlayerController>() : null;
            if (movement != null)
            {
                movementWasEnabled = movement.enabled;
                movement.enabled = false;
            }
            int day = GameFlowManager.Instance.CurrentDay;
            DayPhase phase = GameFlowManager.Instance.CurrentPhase;
            dialoguePhase = phase;
            dialogueLineIndex = 0;
            dialogueOpenedAt = dialogueLineAt = Time.unscaledTime;
            dialogueAdvanceFrame = -1;

            switch (npcType)
            {
                case StoryNpcType.Jonathan:
                    if (phase == DayPhase.PoskoBriefing)
                    {
                        currentDialogueText = $"\"Selamat pagi Vicky. Untuk Hari ke-{day}, kita punya sembako yang harus sampai ke 3 keluarga di kampung. Pastikan catatannya rapi dan waspada bila ada preman yang menghalangi.\"";
                        if (dialogueLines == null || dialogueLines.Length <= 1)
                            GameFlowManager.Instance.CompleteBriefing();
                    }
                    else if (phase == DayPhase.PoskoLaporan)
                    {
                        currentDialogueText = $"\"Kerja bagus Vicky! Semua laporan penyaluran bansos Hari ke-{day} sudah kuterima dengan baik. Jangan lupa sapa Ophelia sebelum kamu pulang istirahat.\"";
                        if (dialogueLines == null || dialogueLines.Length <= 1)
                            GameFlowManager.Instance.CompleteReport();
                    }
                    else
                    {
                        currentDialogueText = "\"Utamakan keselamatan warga dan relawan. Terima kasih atas dedikasimu, Vicky.\"";
                    }
                    break;

                case StoryNpcType.Ophelia:
                    if (phase == DayPhase.PoskoBriefing)
                    {
                        currentDialogueText = $"\"Hai Vicky! Semangat ya untuk distribusi bansos hari ini. Jangan lupa minum air, cuaca di luar terik sekali.\"";
                    }
                    else if (phase == DayPhase.PoskoLaporan)
                    {
                        currentDialogueText = "\"Vicky, kamu hebat bisa selesaikan rute hari ini! Istirahatlah dengan baik ya nanti di rumah.\"";
                        if (dialogueLines == null || dialogueLines.Length <= 1)
                            GameFlowManager.Instance.CompleteOpheliaTalk();
                    }
                    else
                    {
                        currentDialogueText = "\"Senang melihat warga mulai tersenyum kembali setelah posko ini berjalan.\"";
                    }
                    break;

                case StoryNpcType.Ibu:
                    if (phase == DayPhase.RumahPagi)
                    {
                        currentDialogueText = $"\"Vicky, sarapan dulu sebelum berangkat ke posko ya. Hati-hati di jalan dan jangan lupa tersenyum pada tetangga.\"";
                    }
                    else if (phase == DayPhase.RumahMalam)
                    {
                        currentDialogueText = $"\"Alhamdulillah kamu sudah pulang nak. Mandi lalu istirahatlah di kasurmu. Besok adalah hari perjuangan yang baru.\"";
                    }
                    else
                    {
                        currentDialogueText = "\"Ibu selalu mendoakan yang terbaik untuk niat tulusmu membantu warga.\"";
                    }
                    break;
            }
            if (activeLines != null && activeLines.Length > 0)
                SpeakerChanged?.Invoke(activeLines[0].speaker);
            return true;
        }

        public void CloseDialogue()
        {
            if (!isDialogueOpen || dialogueAdvanceFrame == Time.frameCount)
                return;
            dialogueAdvanceFrame = Time.frameCount;
            if (activeLines != null && dialogueLineIndex + 1 < activeLines.Length)
            {
                dialogueLineIndex++;
                dialogueLineAt = Time.unscaledTime;
                SpeakerChanged?.Invoke(activeLines[dialogueLineIndex].speaker);
                return;
            }
            if (sequenceCompleted == null && activeLines != null && activeLines.Length > 1)
            {
                if (npcType == StoryNpcType.Jonathan && dialoguePhase == DayPhase.PoskoBriefing)
                    GameFlowManager.Instance.CompleteBriefing();
                else if (npcType == StoryNpcType.Jonathan && dialoguePhase == DayPhase.PoskoLaporan)
                    GameFlowManager.Instance.CompleteReport();
                else if (npcType == StoryNpcType.Ophelia && dialoguePhase == DayPhase.PoskoLaporan)
                    GameFlowManager.Instance.CompleteOpheliaTalk();
            }
            var completed = sequenceCompleted;
            EndDialogue();
            completed?.Invoke();
        }

        public void CancelSequence()
        {
            if (sequenceCompleted != null)
                EndDialogue();
        }

        private void EndDialogue()
        {
            if (!isDialogueOpen)
                return;
            isDialogueOpen = false;
            activeDialogue = null;
            activeLines = null;
            sequenceCompleted = null;
            SpeakerChanged?.Invoke(null);
            if (movement != null)
                movement.enabled = movementWasEnabled;
        }

        private void OnDisable()
        {
            EndDialogue();
        }

        private void OnGUI()
        {
            if (SessionMenu.InputBlocked) return;
            CozyGuiTheme.EnsureInitialized();
            if (isPlayerInRange && !isDialogueOpen && !AnyDialogueOpen && PoskoDistributionTrigger.IsNearestInteraction(this, playerTransform))
                CozyGuiTheme.Prompt("Bicara dengan " + characterName);
            if (!isDialogueOpen) return;
            GUI.depth = -100;
            Matrix4x4 old = GUI.matrix; float scale = CozyGuiTheme.Scale;
            GUI.matrix = old * Matrix4x4.Scale(new Vector3(scale, scale, 1));
            float sw = Screen.width / scale, sh = Screen.height / scale;
            bool configured = activeLines != null && activeLines.Length > 0;
            string speaker = configured && !string.IsNullOrEmpty(activeLines[dialogueLineIndex].speaker) ? activeLines[dialogueLineIndex].speaker : characterName;
            string text = configured ? activeLines[dialogueLineIndex].text : currentDialogueText;
            Rect r = new Rect((sw - 1120) / 2, sh - 268, 1120, 236);
            r.y += 12 * (1 - Mathf.SmoothStep(0, 1, Mathf.Clamp01((Time.unscaledTime - dialogueOpenedAt) / .2f)));
            CozyGuiTheme.Card(r);

            var portrait = DialoguePresentation.Portrait(speaker);
            if (portrait != null) GUI.DrawTexture(new Rect(r.x + 16, r.y - 22, 168, 242), portrait, ScaleMode.ScaleToFit, true);
            CozyGuiTheme.Fill(new Rect(r.x + 206, r.y + 46, 20, 3), CozyGuiTheme.Clay);
            GUI.Label(new Rect(r.x + 240, r.y + 35, 620, 32), speaker, CozyGuiTheme.LabelStyle);
            Color color = GUI.color; GUI.color = new Color(1, 1, 1, Mathf.Lerp(.7f, 1, Mathf.Clamp01((Time.unscaledTime - dialogueLineAt) / .16f)));
            GUI.Label(new Rect(r.x + 206, r.y + 82, 850, 82), text, CozyGuiTheme.DialogueTextStyle);
            GUI.color = color;
            CozyGuiTheme.Rule(r.x + 206, r.y + 175, 886);
            bool next = configured && dialogueLineIndex + 1 < activeLines.Length;
            string counter = configured ? (dialogueLineIndex + 1) + " / " + activeLines.Length + "     ·     " : "";
            GUI.Label(new Rect(r.x + 206, r.y + 190, 680, 24), counter + "Enter / Spasi / Esc  Lanjut", CozyGuiTheme.Small);
            if (CozyGuiTheme.Button(new Rect(r.xMax - 174, r.y + 187, 146, 34), next ? "Lanjut  >" : "Selesai", CozyGuiTheme.TealBtnStyle)) CloseDialogue();
            GUI.matrix = old;
        }
    }
}

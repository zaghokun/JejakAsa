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
        private int dialogueDay;
        private PlayerController movement;
        private bool movementWasEnabled;
        private static StoryNpcController activeDialogue;
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

            if (isPlayerInRange && !isDialogueOpen && !AnyDialogueOpen && kb.eKey.wasPressedThisFrame && PoskoDistributionTrigger.IsNearestInteraction(this, playerTransform))
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
            if (isDialogueOpen || AnyDialogueOpen)
                return;
            var interaction = playerTransform != null ? playerTransform.GetComponent<PlayerInteraction>() : null;
            if (interaction != null && (interaction.IsDialogueOpen || interaction.IsEncounterOpen || interaction.IsOutcomeOpen || interaction.IsInBattle))
                return;

            isDialogueOpen = true;
            activeDialogue = this;
            movement = playerTransform != null ? playerTransform.GetComponent<PlayerController>() : null;
            if (movement != null)
            {
                movementWasEnabled = movement.enabled;
                movement.enabled = false;
            }
            int day = GameFlowManager.Instance.CurrentDay;
            DayPhase phase = GameFlowManager.Instance.CurrentPhase;
            dialogueDay = day;
            dialoguePhase = phase;
            dialogueLineIndex = 0;
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
                        {
                            GameFlowManager.Instance.CompleteReport();
                            GameFlowManager.Instance.ClaimLevelReward($"BekalLogistik_Hari{day}");
                        }
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
                        currentDialogueText = $"\"Vicky, kamu hebat bisa selesaikan rute hari ini! Ini ada bekal dan catatan tambahan untukmu. Istirahatlah dengan baik ya nanti di rumah.\"";
                        if (dialogueLines == null || dialogueLines.Length <= 1)
                        {
                            GameFlowManager.Instance.CompleteOpheliaTalk();
                            GameFlowManager.Instance.ClaimLevelReward($"BekalLogistik_Hari{day}");
                        }
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
        }

        public void CloseDialogue()
        {
            if (!isDialogueOpen || dialogueAdvanceFrame == Time.frameCount)
                return;
            dialogueAdvanceFrame = Time.frameCount;
            if (dialogueLines != null && dialogueLineIndex + 1 < dialogueLines.Length)
            {
                dialogueLineIndex++;
                return;
            }
            if (dialogueLines != null && dialogueLines.Length > 1)
            {
                if (npcType == StoryNpcType.Jonathan && dialoguePhase == DayPhase.PoskoBriefing)
                    GameFlowManager.Instance.CompleteBriefing();
                else if (npcType == StoryNpcType.Jonathan && dialoguePhase == DayPhase.PoskoLaporan)
                {
                    GameFlowManager.Instance.CompleteReport();
                    GameFlowManager.Instance.ClaimLevelReward($"BekalLogistik_Hari{dialogueDay}");
                }
                else if (npcType == StoryNpcType.Ophelia && dialoguePhase == DayPhase.PoskoLaporan)
                {
                    GameFlowManager.Instance.CompleteOpheliaTalk();
                    GameFlowManager.Instance.ClaimLevelReward($"BekalLogistik_Hari{dialogueDay}");
                }
            }
            EndDialogue();
        }

        private void EndDialogue()
        {
            if (!isDialogueOpen)
                return;
            isDialogueOpen = false;
            activeDialogue = null;
            if (movement != null)
                movement.enabled = movementWasEnabled;
        }

        private void OnDisable()
        {
            EndDialogue();
        }

        private void OnGUI()
        {
            if (SessionMenu.InputBlocked)
                return;

            CozyGuiTheme.EnsureInitialized();

            // Prompt interaksi
            if (isPlayerInRange && !isDialogueOpen)
            {
                float pW = 280f;
                float pH = 44f;
                float pX = (Screen.width - pW) * 0.5f;
                float pY = Screen.height - 110f;

                GUI.DrawTexture(new Rect(pX + 2, pY + 2, pW, pH), CozyGuiTheme.ShadowTex);
                GUI.Box(new Rect(pX, pY, pW, pH), GUIContent.none, CozyGuiTheme.CardStyle);
                GUI.Label(new Rect(pX, pY + 10, pW, 24), $"Tekan [E] Bicara dengan {characterName}", CozyGuiTheme.HintStyle);
                return;
            }

            // Tampilan Dialog Box
            if (isDialogueOpen)
            {
                GUI.depth = -100;
                float scale = CozyGuiTheme.Scale;
                Matrix4x4 prevMatrix = GUI.matrix;
                GUI.matrix = prevMatrix * Matrix4x4.Scale(new Vector3(scale, scale, 1f));

                float sw = Screen.width / scale;
                float sh = Screen.height / scale;

                float cardW = Mathf.Min(560f, sw - 40f);
                float cardH = 160f;
                float cardX = (sw - cardW) * 0.5f;
                float cardY = sh - cardH - 30f;

                GUI.DrawTexture(new Rect(cardX + 4, cardY + 6, cardW, cardH), CozyGuiTheme.ShadowTex);
                GUI.Box(new Rect(cardX, cardY, cardW, cardH), GUIContent.none, CozyGuiTheme.CardStyle);

                bool configured = dialogueLines != null && dialogueLines.Length > 0;
                string speaker = configured && !string.IsNullOrEmpty(dialogueLines[dialogueLineIndex].speaker)
                    ? dialogueLines[dialogueLineIndex].speaker : characterName;
                string text = configured ? dialogueLines[dialogueLineIndex].text : currentDialogueText;
                GUI.Label(new Rect(cardX + 18, cardY + 14, 160, 24), speaker.ToUpper(), CozyGuiTheme.RibbonStyle);
                GUI.Label(new Rect(cardX + 18, cardY + 44, cardW - 36, 75), text, CozyGuiTheme.DialogueTextStyle);
                bool hasNext = configured && dialogueLineIndex + 1 < dialogueLines.Length;
                GUI.Label(new Rect(cardX + 18, cardY + cardH - 24, cardW - 150, 18), "Enter / Spasi / Esc", CozyGuiTheme.HintStyle);
                if (GUI.Button(new Rect(cardX + cardW - 125, cardY + cardH - 30, 105, 24), hasNext ? "Lanjut" : "Tutup"))
                    CloseDialogue();

                GUI.matrix = prevMatrix;
            }
        }
    }
}


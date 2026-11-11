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

        // Tampilan Modern HUD Status Bansos & Timer (U06)
        private Texture2D hudCardTex;
        private Texture2D hudAccentTex;
        private Texture2D hudLineTex;
        private Texture2D hudBarBgTex;
        private Texture2D hudBarFillTex;
        private Texture2D hudBadgeTex;
        private GUIStyle hudBoxStyle;
        private GUIStyle hudHeaderStyle;
        private GUIStyle hudSubHeaderStyle;
        private GUIStyle hudLabelStyle;
        private GUIStyle hudValueStyle;
        private GUIStyle hudBadgeStyle;
        private bool hudStylesInitialized = false;

        private void InitHudStyles()
        {
            if (hudStylesInitialized) return;

            // Card background: Deep slate glassmorphism dengan border rounded
            hudCardTex = MakeRoundedRectTex(32, 32, 8, new Color(0.07f, 0.10f, 0.15f, 0.88f), new Color(0.24f, 0.38f, 0.48f, 0.55f), 1);
            hudAccentTex = MakeTex(2, 2, new Color(0.18f, 0.76f, 0.72f, 0.95f)); // Toska cerah
            hudLineTex = MakeTex(2, 2, new Color(1f, 1f, 1f, 0.08f));
            hudBarBgTex = MakeTex(2, 2, new Color(1f, 1f, 1f, 0.12f));
            hudBarFillTex = MakeTex(2, 2, Color.white);
            hudBadgeTex = MakeRoundedRectTex(24, 24, 6, new Color(0.10f, 0.18f, 0.22f, 0.95f), new Color(0.18f, 0.76f, 0.72f, 0.8f), 1);

            hudBoxStyle = new GUIStyle(GUI.skin.box)
            {
                normal = { background = hudCardTex },
                border = new RectOffset(10, 10, 10, 10),
                padding = new RectOffset(14, 14, 12, 12)
            };

            hudHeaderStyle = new GUIStyle(GUI.skin.label)
            {
                fontSize = 11,
                fontStyle = FontStyle.Bold,
                alignment = TextAnchor.MiddleLeft,
                richText = true
            };
            hudHeaderStyle.normal.textColor = new Color(0.40f, 0.85f, 0.82f);

            hudSubHeaderStyle = new GUIStyle(GUI.skin.label)
            {
                fontSize = 9,
                fontStyle = FontStyle.Normal,
                alignment = TextAnchor.MiddleRight,
                richText = true
            };
            hudSubHeaderStyle.normal.textColor = new Color(0.60f, 0.70f, 0.80f, 0.8f);

            hudLabelStyle = new GUIStyle(GUI.skin.label)
            {
                fontSize = 12,
                alignment = TextAnchor.MiddleLeft,
                richText = true
            };
            hudLabelStyle.normal.textColor = new Color(0.85f, 0.90f, 0.95f);

            hudValueStyle = new GUIStyle(GUI.skin.label)
            {
                fontSize = 12,
                fontStyle = FontStyle.Bold,
                alignment = TextAnchor.MiddleRight,
                richText = true
            };
            hudValueStyle.normal.textColor = Color.white;

            hudBadgeStyle = new GUIStyle(GUI.skin.label)
            {
                normal = { background = hudBadgeTex },
                border = new RectOffset(8, 8, 8, 8),
                fontSize = 11,
                fontStyle = FontStyle.Bold,
                alignment = TextAnchor.MiddleCenter,
                richText = true
            };

            hudStylesInitialized = true;
        }

        private Texture2D MakeTex(int width, int height, Color col)
        {
            Color[] pix = new Color[width * height];
            for (int i = 0; i < pix.Length; ++i)
                pix[i] = col;
            Texture2D result = new Texture2D(width, height);
            result.SetPixels(pix);
            result.Apply();
            return result;
        }

        private Texture2D MakeRoundedRectTex(int width, int height, int radius, Color fillColor, Color borderColor, int borderWidth = 1)
        {
            Texture2D tex = new Texture2D(width, height, TextureFormat.RGBA32, false)
            {
                filterMode = FilterMode.Bilinear
            };
            Color[] colors = new Color[width * height];

            for (int y = 0; y < height; y++)
            {
                for (int x = 0; x < width; x++)
                {
                    float cx = (x < radius) ? radius : (x >= width - radius) ? (width - 1 - radius) : x;
                    float cy = (y < radius) ? radius : (y >= height - radius) ? (height - 1 - radius) : y;
                    float dist = Vector2.Distance(new Vector2(x, y), new Vector2(cx, cy));

                    int index = y * width + x;
                    if (dist > radius)
                    {
                        float alpha = Mathf.Clamp01(1f - (dist - radius));
                        colors[index] = new Color(borderColor.r, borderColor.g, borderColor.b, borderColor.a * alpha);
                    }
                    else if (dist > radius - borderWidth)
                    {
                        colors[index] = borderColor;
                    }
                    else
                    {
                        colors[index] = fillColor;
                    }
                }
            }
            tex.SetPixels(colors);
            tex.Apply();
            return tex;
        }

        private void OnGUI()
        {
            InitHudStyles();

            float panelW = 260f;
            float panelH = (IsMissionCompleted || timeRemaining <= 0f) ? 168f : 142f;
            Rect panelRect = new Rect(20, 20, panelW, panelH);

            // 1. Kartu Utama (Glassmorphism Dark Navy dengan Rounded Border)
            GUI.Box(panelRect, GUIContent.none, hudBoxStyle);

            // 2. Aksen Toska Halus di Garis Paling Atas
            GUI.DrawTexture(new Rect(panelRect.x + 12, panelRect.y + 1, panelRect.width - 24, 2f), hudAccentTex);

            // 3. Header & Sub-header
            GUI.Label(new Rect(panelRect.x + 14, panelRect.y + 8, 140, 18), "◆  <b>STATUS MISI</b>", hudHeaderStyle);
            GUI.Label(new Rect(panelRect.x + panelW - 90, panelRect.y + 8, 76, 18), "JEJAK ASA", hudSubHeaderStyle);

            // 4. Garis Pemisah Halus
            GUI.DrawTexture(new Rect(panelRect.x + 14, panelRect.y + 28, panelRect.width - 28, 1f), hudLineTex);

            float startY = panelRect.y + 34;
            float rowH = 22f;

            // Baris 1: Sisa Waktu (Tanpa teks [JEDA] sama sekali)
            Color timerBarColor;
            string timerHex;
            if (timeRemaining <= 30f)
            {
                timerHex = timeRemaining <= 0f ? "#EF4444" : "#F59E0B";
                timerBarColor = timeRemaining <= 0f ? new Color(0.94f, 0.27f, 0.27f) : new Color(0.96f, 0.62f, 0.04f);
            }
            else
            {
                timerHex = "#F8FAFC";
                timerBarColor = new Color(0.18f, 0.76f, 0.72f);
            }

            GUI.Label(new Rect(panelRect.x + 14, startY, 130, rowH), "⏱ Sisa Waktu", hudLabelStyle);
            GUI.Label(new Rect(panelRect.x + panelW - 104, startY, 90, rowH), $"<color={timerHex}>{FormattedTimeRemaining}</color>", hudValueStyle);

            // Mini Progress Bar Sisa Waktu (Tampak modern & responsif)
            float timerProgress = Mathf.Clamp01(timeRemaining / Mathf.Max(1f, initialTimeSeconds));
            float barW = panelRect.width - 28;
            float barY = startY + 20;
            GUI.DrawTexture(new Rect(panelRect.x + 14, barY, barW, 3f), hudBarBgTex);
            Color prevColor = GUI.color;
            GUI.color = timerBarColor;
            GUI.DrawTexture(new Rect(panelRect.x + 14, barY, barW * timerProgress, 3f), hudBarFillTex);
            GUI.color = prevColor;

            startY += 6; // Kompensasi mini bar

            // Baris 2: Paket Bansos
            string packagePips = packagesRemaining switch
            {
                3 => "<color=#38BDF8>■ ■ ■</color>",
                2 => "<color=#38BDF8>■ ■</color> <color=#475569>□</color>",
                1 => "<color=#38BDF8>■</color> <color=#475569>□ □</color>",
                _ => "<color=#475569>□ □ □</color>"
            };
            GUI.Label(new Rect(panelRect.x + 14, startY + rowH, 120, rowH), "📦 Paket Bansos", hudLabelStyle);
            GUI.Label(new Rect(panelRect.x + panelW - 120, startY + rowH, 106, rowH), $"{packagePips}  <b>{packagesRemaining}/3</b>", hudValueStyle);

            // Baris 3: Warga Terbantu
            GUI.Label(new Rect(panelRect.x + 14, startY + rowH * 2, 120, rowH), "👥 Warga Terbantu", hudLabelStyle);
            GUI.Label(new Rect(panelRect.x + panelW - 120, startY + rowH * 2, 106, rowH), $"<color=#34D399><b>{deliveredCount}</b></color> <color=#94A3B8>/ 3 Warga</color>", hudValueStyle);

            // Baris 4: Tingkat Kepercayaan
            string trustStars = trust switch
            {
                3 => "<color=#FBBF24>★★★</color>",
                2 => "<color=#FBBF24>★★</color><color=#475569>☆</color>",
                1 => "<color=#FBBF24>★</color><color=#475569>☆☆</color>",
                _ => "<color=#475569>☆☆☆</color>"
            };
            GUI.Label(new Rect(panelRect.x + 14, startY + rowH * 3, 120, rowH), "🤝 Kepercayaan", hudLabelStyle);
            GUI.Label(new Rect(panelRect.x + panelW - 120, startY + rowH * 3, 106, rowH), $"{trustStars}  <color=#FBBF24><b>{trust}/3</b></color>", hudValueStyle);

            // Badge Banner Status Misi (Hanya muncul jika Selesai atau Waktu Habis)
            if (IsMissionCompleted)
            {
                hudBadgeStyle.normal.textColor = new Color(0.20f, 0.90f, 0.58f);
                GUI.Label(new Rect(panelRect.x + 14, startY + rowH * 4 + 4, panelRect.width - 28, 22), "★ SEMUA 3 BANSOS TERKIRIM ★", hudBadgeStyle);
            }
            else if (timeRemaining <= 0f)
            {
                hudBadgeStyle.normal.textColor = new Color(0.95f, 0.35f, 0.35f);
                GUI.Label(new Rect(panelRect.x + 14, startY + rowH * 4 + 4, panelRect.width - 28, 22), "⚠️ WAKTU MISI TELAH HABIS", hudBadgeStyle);
            }
        }
    }
}

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
        private int outcomeOpenedFrame = -1;
        private float outcomeOpenedTime = -10f;

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
            if (!IsDialogueOpen && outcome == MissionOutcome.InProgress)
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

            // Kembalikan posisi pemain ke pos awal bersih (U17)
            CharacterController cc = GetComponent<CharacterController>();
            if (cc != null) cc.enabled = false;
            transform.position = initialSpawnPosition;
            transform.rotation = initialSpawnRotation;
            if (cc != null) cc.enabled = true;

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

        private void CloseDialogue()
        {
            if (!IsDialogueOpen)
                return;
            IsDialogueOpen = false;
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
                closeButton.onClick.RemoveListener(CloseDialogue);
        }

        // Tampilan HUD Lucu & Cozy untuk Anak-anak (Gaya Nintendo / Animal Crossing / Overcooked)
        private Texture2D cuteShadowTex;
        private Texture2D cuteCardTex;
        private Texture2D cuteRibbonTex;
        private Texture2D cuteTimerNormalTex;
        private Texture2D cuteTimerWarnTex;
        private Texture2D cuteTimerDangerTex;
        private Texture2D cuteStampDoneTex;
        private Texture2D cuteStampEmptyTex;
        private Texture2D cuteBannerInfoTex;
        private Texture2D cuteBannerSuccessTex;
        private Texture2D cuteBannerFailTex;

        // U08: Aset UI Layar Hasil Misi
        private Texture2D cuteDimTex;
        private Texture2D cuteInnerBoxTex;
        private Texture2D cuteBtnNormalTex;
        private Texture2D cuteBtnHoverTex;
        private Texture2D cuteOutcomeSuccessTex;
        private Texture2D cuteOutcomePartialTex;
        private Texture2D cuteOutcomeFailTex;

        private GUIStyle cuteCardStyle;
        private GUIStyle cuteRibbonStyle;
        private GUIStyle cuteTimerStyle;
        private GUIStyle cuteLabelStyle;
        private GUIStyle cuteSubLabelStyle;
        private GUIStyle cuteCheckStyle;
        private GUIStyle cuteBannerStyle;

        private GUIStyle cuteOutcomeHeaderStyle;
        private GUIStyle cuteOutcomeDescStyle;
        private GUIStyle cuteInnerBoxStyle;
        private GUIStyle cuteStatLeftStyle;
        private GUIStyle cuteStatRightStyle;
        private GUIStyle cuteBtnStyle;
        private GUIStyle cuteHintStyle;
        private bool cuteHudInitialized = false;

        private void InitCuteHudStyles()
        {
            if (cuteHudInitialized) return;

            // Warna cozy & ramah anak: Cream hangat, Toska lembut, Kuning cerah, Hijau mint
            cuteShadowTex = MakeRoundedRectTex(32, 32, 14, new Color(0.10f, 0.08f, 0.06f, 0.25f), new Color(0f, 0f, 0f, 0f), 0);
            cuteCardTex = MakeRoundedRectTex(32, 32, 14, new Color(1.0f, 0.98f, 0.94f, 0.97f), new Color(0.08f, 0.56f, 0.52f, 1.0f), 2);
            cuteRibbonTex = MakeRoundedRectTex(24, 24, 10, new Color(0.08f, 0.56f, 0.52f, 1.0f), new Color(1.0f, 0.98f, 0.94f, 1.0f), 2);
            cuteTimerNormalTex = MakeRoundedRectTex(24, 24, 10, new Color(0.96f, 0.62f, 0.04f, 1.0f), new Color(1.0f, 0.98f, 0.94f, 1.0f), 2);
            cuteTimerWarnTex = MakeRoundedRectTex(24, 24, 10, new Color(0.98f, 0.45f, 0.09f, 1.0f), new Color(1.0f, 0.98f, 0.94f, 1.0f), 2);
            cuteTimerDangerTex = MakeRoundedRectTex(24, 24, 10, new Color(0.90f, 0.25f, 0.25f, 1.0f), new Color(1.0f, 0.98f, 0.94f, 1.0f), 2);

            // Stempel lingkaran anti-aliased (gantikan emoji HP, checkmark digambar vektor langsung pada tekstur)
            cuteStampDoneTex = MakeCircleTex(32, new Color(0.10f, 0.72f, 0.50f, 1.0f), new Color(0.05f, 0.50f, 0.35f, 1.0f), 2, drawCheckmark: true);
            cuteStampEmptyTex = MakeCircleTex(32, new Color(0.92f, 0.89f, 0.84f, 1.0f), new Color(0.80f, 0.76f, 0.70f, 1.0f), 2, drawCheckmark: false);

            // Banner status bagian bawah
            cuteBannerInfoTex = MakeRoundedRectTex(24, 24, 8, new Color(0.99f, 0.94f, 0.78f, 1.0f), new Color(0.96f, 0.78f, 0.35f, 0.6f), 1);
            cuteBannerSuccessTex = MakeRoundedRectTex(24, 24, 8, new Color(0.82f, 0.98f, 0.90f, 1.0f), new Color(0.10f, 0.72f, 0.50f, 0.6f), 1);
            cuteBannerFailTex = MakeRoundedRectTex(24, 24, 8, new Color(0.99f, 0.86f, 0.86f, 1.0f), new Color(0.90f, 0.25f, 0.25f, 0.6f), 1);

            // Aset Layar Hasil (U08)
            cuteDimTex = MakeRoundedRectTex(4, 4, 0, new Color(0.05f, 0.08f, 0.12f, 0.65f), Color.clear, 0);
            cuteInnerBoxTex = MakeRoundedRectTex(24, 24, 10, new Color(0.96f, 0.93f, 0.86f, 1.0f), new Color(0.85f, 0.78f, 0.68f, 1.0f), 1);
            cuteBtnNormalTex = MakeRoundedRectTex(24, 24, 12, new Color(0.08f, 0.56f, 0.52f, 1.0f), new Color(0.05f, 0.40f, 0.38f, 1.0f), 2);
            cuteBtnHoverTex = MakeRoundedRectTex(24, 24, 12, new Color(0.10f, 0.68f, 0.62f, 1.0f), new Color(0.05f, 0.45f, 0.42f, 1.0f), 2);
            cuteOutcomeSuccessTex = MakeRoundedRectTex(24, 24, 12, new Color(0.06f, 0.65f, 0.42f, 1.0f), new Color(1.0f, 0.98f, 0.94f, 1.0f), 2);
            cuteOutcomePartialTex = MakeRoundedRectTex(24, 24, 12, new Color(0.90f, 0.55f, 0.04f, 1.0f), new Color(1.0f, 0.98f, 0.94f, 1.0f), 2);
            cuteOutcomeFailTex = MakeRoundedRectTex(24, 24, 12, new Color(0.85f, 0.22f, 0.22f, 1.0f), new Color(1.0f, 0.98f, 0.94f, 1.0f), 2);

            cuteCardStyle = new GUIStyle(GUI.skin.box)
            {
                normal = { background = cuteCardTex },
                border = new RectOffset(14, 14, 14, 14)
            };

            cuteRibbonStyle = new GUIStyle(GUI.skin.label)
            {
                normal = { background = cuteRibbonTex, textColor = Color.white },
                border = new RectOffset(10, 10, 10, 10),
                fontSize = 11,
                fontStyle = FontStyle.Bold,
                alignment = TextAnchor.MiddleCenter
            };

            cuteTimerStyle = new GUIStyle(GUI.skin.label)
            {
                normal = { background = cuteTimerNormalTex, textColor = Color.white },
                border = new RectOffset(10, 10, 10, 10),
                fontSize = 12,
                fontStyle = FontStyle.Bold,
                alignment = TextAnchor.MiddleCenter
            };

            cuteLabelStyle = new GUIStyle(GUI.skin.label)
            {
                fontSize = 13,
                fontStyle = FontStyle.Bold,
                alignment = TextAnchor.MiddleLeft,
                richText = true
            };
            cuteLabelStyle.normal.textColor = new Color(0.24f, 0.19f, 0.16f); // Cokelat hangat ramah mata

            cuteSubLabelStyle = new GUIStyle(GUI.skin.label)
            {
                fontSize = 11,
                fontStyle = FontStyle.Normal,
                alignment = TextAnchor.MiddleLeft,
                richText = true
            };
            cuteSubLabelStyle.normal.textColor = new Color(0.42f, 0.36f, 0.32f);

            cuteCheckStyle = new GUIStyle(GUI.skin.label)
            {
                fontSize = 12,
                fontStyle = FontStyle.Bold,
                alignment = TextAnchor.MiddleCenter
            };
            cuteCheckStyle.normal.textColor = Color.white;

            cuteBannerStyle = new GUIStyle(GUI.skin.label)
            {
                border = new RectOffset(8, 8, 8, 8),
                fontSize = 11,
                fontStyle = FontStyle.Bold,
                alignment = TextAnchor.MiddleCenter
            };

            cuteOutcomeHeaderStyle = new GUIStyle(GUI.skin.label)
            {
                normal = { textColor = Color.white },
                border = new RectOffset(12, 12, 12, 12),
                fontSize = 14,
                fontStyle = FontStyle.Bold,
                alignment = TextAnchor.MiddleCenter
            };

            cuteOutcomeDescStyle = new GUIStyle(GUI.skin.label)
            {
                fontSize = 12,
                wordWrap = true,
                alignment = TextAnchor.MiddleCenter,
                richText = true
            };
            cuteOutcomeDescStyle.normal.textColor = new Color(0.32f, 0.25f, 0.20f);

            cuteInnerBoxStyle = new GUIStyle(GUI.skin.box)
            {
                normal = { background = cuteInnerBoxTex },
                border = new RectOffset(10, 10, 10, 10)
            };

            cuteStatLeftStyle = new GUIStyle(GUI.skin.label)
            {
                fontSize = 12,
                alignment = TextAnchor.MiddleLeft
            };
            cuteStatLeftStyle.normal.textColor = new Color(0.40f, 0.33f, 0.28f);

            cuteStatRightStyle = new GUIStyle(GUI.skin.label)
            {
                fontSize = 12,
                fontStyle = FontStyle.Bold,
                alignment = TextAnchor.MiddleRight,
                richText = true
            };
            cuteStatRightStyle.normal.textColor = new Color(0.18f, 0.14f, 0.10f);

            cuteBtnStyle = new GUIStyle(GUI.skin.button)
            {
                normal = { background = cuteBtnNormalTex, textColor = Color.white },
                hover = { background = cuteBtnHoverTex, textColor = Color.white },
                active = { background = cuteBtnHoverTex, textColor = Color.white },
                border = new RectOffset(12, 12, 12, 12),
                fontSize = 13,
                fontStyle = FontStyle.Bold,
                alignment = TextAnchor.MiddleCenter
            };

            cuteHintStyle = new GUIStyle(GUI.skin.label)
            {
                fontSize = 10,
                alignment = TextAnchor.MiddleCenter
            };
            cuteHintStyle.normal.textColor = new Color(0.55f, 0.48f, 0.42f);

            cuteHudInitialized = true;
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
                        colors[index] = borderWidth > 0
                            ? new Color(borderColor.r, borderColor.g, borderColor.b, borderColor.a * alpha)
                            : new Color(fillColor.r, fillColor.g, fillColor.b, fillColor.a * alpha);
                    }
                    else if (borderWidth > 0 && dist > radius - borderWidth)
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

        private float DistToSegment(Vector2 p, Vector2 a, Vector2 b)
        {
            Vector2 ab = b - a;
            float t = Mathf.Clamp01(Vector2.Dot(p - a, ab) / Mathf.Max(0.0001f, ab.sqrMagnitude));
            return Vector2.Distance(p, a + t * ab);
        }

        private Texture2D MakeCircleTex(int size, Color fillColor, Color borderColor, int borderWidth = 2, bool drawCheckmark = false)
        {
            Texture2D tex = new Texture2D(size, size, TextureFormat.RGBA32, false)
            {
                filterMode = FilterMode.Bilinear
            };
            Color[] colors = new Color[size * size];
            float center = (size - 1) * 0.5f;
            float radius = size * 0.5f - 1f;

            // Koordinat checkmark proporsional untuk size 32
            Vector2 p1 = new Vector2(size * 0.28f, size * 0.48f);
            Vector2 p2 = new Vector2(size * 0.44f, size * 0.33f);
            Vector2 p3 = new Vector2(size * 0.72f, size * 0.65f);

            for (int y = 0; y < size; y++)
            {
                for (int x = 0; x < size; x++)
                {
                    float dist = Vector2.Distance(new Vector2(x, y), new Vector2(center, center));
                    int idx = y * size + x;
                    Color baseCol;

                    if (dist > radius)
                    {
                        float a = Mathf.Clamp01(1f - (dist - radius));
                        baseCol = new Color(borderColor.r, borderColor.g, borderColor.b, borderColor.a * a);
                    }
                    else if (dist > radius - borderWidth)
                    {
                        baseCol = borderColor;
                    }
                    else
                    {
                        baseCol = fillColor;
                        if (drawCheckmark)
                        {
                            Vector2 pt = new Vector2(x, y);
                            float d1 = DistToSegment(pt, p1, p2);
                            float d2 = DistToSegment(pt, p2, p3);
                            float checkDist = Mathf.Min(d1, d2);
                            float thickness = size * 0.065f;
                            if (checkDist < thickness + 1f)
                            {
                                float checkAlpha = Mathf.Clamp01(1f - (checkDist - thickness));
                                baseCol = Color.Lerp(fillColor, Color.white, checkAlpha);
                            }
                        }
                    }
                    colors[idx] = baseCol;
                }
            }
            tex.SetPixels(colors);
            tex.Apply();
            return tex;
        }

        private void OnGUI()
        {
            InitCuteHudStyles();

            float cardX = 20f;
            float cardY = 26f;
            float cardW = 276f;
            float cardH = 114f;

            // 1. Bayangan kartun lembut di belakang kartu
            GUI.DrawTexture(new Rect(cardX + 3, cardY + 4, cardW, cardH), cuteShadowTex);

            // 2. Kartu Stiker Utama (Cream Hangat dengan Outline Toska)
            GUI.Box(new Rect(cardX, cardY, cardW, cardH), GUIContent.none, cuteCardStyle);

            // 3. Pita Judul Mungil di Atas Kiri (Overlapping style ala game Nintendo)
            GUI.Label(new Rect(cardX + 12, cardY - 12, 114, 24), "MISI BANSOS", cuteRibbonStyle);

            // 4. Pil Timer Bulat di Atas Kanan
            if (timeRemaining <= 15f)
                cuteTimerStyle.normal.background = cuteTimerDangerTex;
            else if (timeRemaining <= 45f)
                cuteTimerStyle.normal.background = cuteTimerWarnTex;
            else
                cuteTimerStyle.normal.background = cuteTimerNormalTex;

            GUI.Label(new Rect(cardX + cardW - 84, cardY - 12, 72, 24), FormattedTimeRemaining, cuteTimerStyle);

            // 5. Baris Utama: Warga Terbantu + 3 Stempel Lingkaran Lucu
            GUI.Label(new Rect(cardX + 16, cardY + 22, 140, 24), "Warga Terbantu", cuteLabelStyle);

            for (int i = 0; i < 3; i++)
            {
                Rect stampRect = new Rect(cardX + 184 + (i * 26), cardY + 23, 22, 22);
                bool done = i < deliveredCount;
                GUI.DrawTexture(stampRect, done ? cuteStampDoneTex : cuteStampEmptyTex);
            }

            // 6. Info Paket & Kepercayaan
            GUI.Label(new Rect(cardX + 16, cardY + 50, 125, 20), $"Paket: <b><color=#0F766E>{packagesRemaining}</color></b> tersisa", cuteSubLabelStyle);
            GUI.Label(new Rect(cardX + 148, cardY + 50, 120, 20), $"Kepercayaan: <b><color=#B45309>{trust}/3</color></b>", cuteSubLabelStyle);

            // 7. Gelembung Pesan Penyemangat di Bagian Bawah
            string cheerText;
            if (IsMissionCompleted)
            {
                cuteBannerStyle.normal.background = cuteBannerSuccessTex;
                cuteBannerStyle.normal.textColor = new Color(0.05f, 0.45f, 0.30f);
                cheerText = "Horeee! Semua warga terbantu!";
            }
            else if (timeRemaining <= 0f)
            {
                cuteBannerStyle.normal.background = cuteBannerFailTex;
                cuteBannerStyle.normal.textColor = new Color(0.65f, 0.12f, 0.12f);
                cheerText = "Waktu habis! Yuk coba lagi!";
            }
            else
            {
                cuteBannerStyle.normal.background = cuteBannerInfoTex;
                cuteBannerStyle.normal.textColor = new Color(0.55f, 0.35f, 0.05f);
                cheerText = deliveredCount switch
                {
                    0 => "Yuk antar bansos ke 3 warga!",
                    1 => "Bagus! Tinggal 2 warga lagi!",
                    _ => "Semangat! 1 warga terakhir!"
                };
            }

            GUI.Label(new Rect(cardX + 14, cardY + 76, cardW - 28, 25), cheerText, cuteBannerStyle);

            // =========================================================================
            // U08: LAYAR HASIL MISI (OUTCOME OVERLAY POPUP)
            // =========================================================================
            if (isOutcomeOpen)
            {
                GUI.depth = -100;

                // 1. Dim Backdrop seluruh layar
                GUI.DrawTexture(new Rect(0, 0, Screen.width, Screen.height), cuteDimTex);

                float popW = 360f;
                float popH = 286f;
                float popX = (Screen.width - popW) * 0.5f;
                float popY = Mathf.Max(20f, (Screen.height - popH) * 0.5f);

                // 2. Bayangan lembut kartu popup
                GUI.DrawTexture(new Rect(popX + 4, popY + 6, popW, popH), cuteShadowTex);

                // 3. Kartu Popup Utama (Cream Hangat + Border Toska)
                GUI.Box(new Rect(popX, popY, popW, popH), GUIContent.none, cuteCardStyle);

                // 4. Badge Header Hasil Misi
                string outcomeBadgeText;
                Texture2D outcomeBadgeBg;
                switch (outcome)
                {
                    case MissionOutcome.Success:
                        outcomeBadgeText = "MISI BERHASIL!";
                        outcomeBadgeBg = cuteOutcomeSuccessTex;
                        break;
                    case MissionOutcome.PartialSuccess:
                        outcomeBadgeText = "BERHASIL SEBAGIAN";
                        outcomeBadgeBg = cuteOutcomePartialTex;
                        break;
                    default:
                        outcomeBadgeText = "MISI BELUM BERHASIL";
                        outcomeBadgeBg = cuteOutcomeFailTex;
                        break;
                }
                cuteOutcomeHeaderStyle.normal.background = outcomeBadgeBg;
                GUI.Label(new Rect(popX + (popW - 210) * 0.5f, popY - 13, 210, 26), outcomeBadgeText, cuteOutcomeHeaderStyle);

                // 5. Deskripsi Alasan Akhir Misi
                GUI.Label(new Rect(popX + 20, popY + 18, popW - 40, 30), outcomeReason, cuteOutcomeDescStyle);

                // 6. Kotak Rincian Statistik (Inner Plate)
                Rect innerRect = new Rect(popX + 20, popY + 52, popW - 40, 152);
                GUI.Box(innerRect, GUIContent.none, cuteInnerBoxStyle);

                float lineY = innerRect.y + 10;
                float lineH = 22f;
                float labelW = 145f;
                float valW = innerRect.width - labelW - 24;

                // Baris 1: Paket Terbagi
                GUI.Label(new Rect(innerRect.x + 14, lineY, labelW, lineH), "Paket Tersalurkan", cuteStatLeftStyle);
                GUI.Label(new Rect(innerRect.x + 14 + labelW, lineY, valW, lineH), $"<b>{deliveredCount} / {initialPackages}</b> Paket", cuteStatRightStyle);

                // Baris 2: Warga Terbantu
                GUI.Label(new Rect(innerRect.x + 14, lineY + lineH, labelW, lineH), "Warga Terbantu", cuteStatLeftStyle);
                GUI.Label(new Rect(innerRect.x + 14 + labelW, lineY + lineH, valW, lineH), $"<b><color=#0F766E>{deliveredCount} / 3</color></b> Orang", cuteStatRightStyle);

                // Baris 3: Kepercayaan Warga
                GUI.Label(new Rect(innerRect.x + 14, lineY + lineH * 2, labelW, lineH), "Kepercayaan Warga", cuteStatLeftStyle);
                GUI.Label(new Rect(innerRect.x + 14 + labelW, lineY + lineH * 2, valW, lineH), $"<b><color=#B45309>{trust} / 3</color></b>", cuteStatRightStyle);

                // Baris 4: Sisa Waktu
                GUI.Label(new Rect(innerRect.x + 14, lineY + lineH * 3, labelW, lineH), "Sisa Waktu Akhir", cuteStatLeftStyle);
                GUI.Label(new Rect(innerRect.x + 14 + labelW, lineY + lineH * 3, valW, lineH), $"<b>{FormattedTimeRemaining}</b>", cuteStatRightStyle);

                // Baris 5: Daftar Nama Penerima
                var names = GetDeliveredRecipientNames();
                string namesStr = names.Count > 0 ? string.Join(", ", names) : "(Belum ada)";
                GUI.Label(new Rect(innerRect.x + 14, lineY + lineH * 4 + 2, innerRect.width - 28, 36), $"Penerima Selesai: <b><color=#0F766E>{namesStr}</color></b>", cuteSubLabelStyle);

                // 7. Tombol Ulang Misi (Retry)
                Rect btnRect = new Rect(popX + (popW - 170) * 0.5f, popY + popH - 64, 170, 34);
                if (GUI.Button(btnRect, "Ulang Misi", cuteBtnStyle))
                {
                    ResetMissionSession();
                }

                // 8. Petunjuk Keyboard
                GUI.Label(new Rect(popX + 20, popY + popH - 25, popW - 40, 18), "Tekan Enter atau Space untuk Ulang Misi", cuteHintStyle);
            }
        }
    }
}

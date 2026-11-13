using UnityEngine;
using UnityEngine.InputSystem;

namespace JejakAsa
{
    /// <summary>
    /// Pengendali interaksi pengambilan paket bansos di Posko Bantuan (Posko.unity / Kampung.unity).
    /// Pemain mendekati tumpukan kotak bansos dan menekan [E] untuk mengambil 3 paket bansos.
    /// </summary>
    [DisallowMultipleComponent]
    public class PoskoDistributionTrigger : MonoBehaviour
    {
        [Header("Konfigurasi")]
        public float interactionRadius = 2.5f;

        private Transform playerTransform;
        private PlayerInteraction playerInteraction;
        private bool isPlayerNear;
        private string feedbackText = string.Empty;
        private float feedbackTimer;

        private void Start()
        {
            FindPlayer();
        }

        private void FindPlayer()
        {
            if (playerTransform == null)
            {
                var playerObj = GameObject.FindWithTag("Player");
                if (playerObj != null)
                {
                    playerTransform = playerObj.transform;
                    playerInteraction = playerObj.GetComponent<PlayerInteraction>();
                }
            }
        }

        private void Update()
        {
            if (playerTransform == null)
            {
                FindPlayer();
                if (playerTransform == null)
                    return;
            }

            if (feedbackTimer > 0f)
            {
                feedbackTimer -= Time.deltaTime;
                if (feedbackTimer <= 0f)
                    feedbackText = string.Empty;
            }

            if (SessionMenu.InputBlocked)
                return;

            float dist = Vector3.Distance(transform.position, playerTransform.position);
            isPlayerNear = dist <= interactionRadius;

            if (isPlayerNear)
            {
                Keyboard kb = Keyboard.current;
                if (kb != null && kb.eKey.wasPressedThisFrame)
                {
                    TryTakeBansos();
                }
            }
        }

        private void TryTakeBansos()
        {
            bool alreadyPickedUp = (GameFlowManager.Instance != null && GameFlowManager.Instance.Progress.hasPickedUpBansos)
                || (playerInteraction != null && playerInteraction.IsMissionActive);

            if (!alreadyPickedUp)
            {
                if (GameFlowManager.Instance != null)
                {
                    GameFlowManager.Instance.Progress.hasPickedUpBansos = true;
                    GameFlowManager.Instance.CompleteBriefing();
                }

                if (playerInteraction != null)
                {
                    playerInteraction.StartDeliveryMissionFromPosko();
                }

                feedbackText = "3 Paket bansos diambil! Silakan keluar posko untuk membagikan ke warga.";
                feedbackTimer = 3.5f;
            }
            else
            {
                int remaining = playerInteraction != null ? playerInteraction.PackagesRemaining : 3;
                if (playerInteraction != null && playerInteraction.IsMissionCompleted)
                {
                    feedbackText = "Semua bansos telah tersalurkan! Bicaralah dengan Pak Jonathan untuk laporan.";
                }
                else
                {
                    feedbackText = $"Bansos sudah diambil ({remaining} paket). Bawa keluar posko untuk dibagikan!";
                }
                feedbackTimer = 2.5f;
            }
        }

        private void OnGUI()
        {
            if (SessionMenu.InputBlocked)
                return;

            if (feedbackTimer > 0f && !string.IsNullOrEmpty(feedbackText))
            {
                float nbW = 480f;
                float nbH = 46f;
                float nbX = (Screen.width - nbW) * 0.5f;
                float nbY = 150f;
                GUI.Box(new Rect(nbX, nbY, nbW, nbH), feedbackText);
            }

            if (!isPlayerNear)
                return;

            float w = 280f;
            float h = 42f;
            float x = (Screen.width - w) * 0.5f;
            float y = Screen.height - 110f;

            bool alreadyPickedUp = (GameFlowManager.Instance != null && GameFlowManager.Instance.Progress.hasPickedUpBansos)
                || (playerInteraction != null && playerInteraction.IsMissionActive);

            if (!alreadyPickedUp)
            {
                GUI.Box(new Rect(x, y, w, h), "Tekan [E] Ambil Paket Bansos");
            }
            else if (playerInteraction != null && playerInteraction.IsMissionCompleted)
            {
                GUI.Box(new Rect(x, y, w, h), "Bicara dengan Pak Jonathan (Laporan)");
            }
        }

        private void OnDrawGizmosSelected()
        {
            Gizmos.color = Color.yellow;
            Gizmos.DrawWireSphere(transform.position, interactionRadius);
        }
    }
}

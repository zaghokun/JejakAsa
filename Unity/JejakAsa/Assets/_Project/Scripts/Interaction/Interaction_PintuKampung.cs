using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;

namespace JejakAsa
{
    /// <summary>
    /// Trigger pintu transisi scene di Kampung (Rumah Vicky dan Posko Bantuan).
    /// Mengatur pemain masuk ke interior scene dan mengatur posisi spawn saat keluar.
    /// </summary>
    [DefaultExecutionOrder(-50)]
    [DisallowMultipleComponent]
    public class Interaction_PintuKampung : MonoBehaviour
    {
        [Header("Konfigurasi Pintu")]
        public string targetScenePath = "Assets/_Project/Scenes/Rumah.unity";
        public string promptLabel = "Masuk ke Rumah";
        public string spawnPointKey = "RumahVicky";
        public Vector3 spawnOffset = new Vector3(0f, -0.5f, 3.8f);
        public Vector3 interactionOffset = new Vector3(0f, 0f, 2.6f);
        public float interactionRadius = 3.5f;

        private Transform playerTransform;
        private bool isPlayerNear;

        public Vector3 TriggerCenter => transform.position + interactionOffset;

        private void Awake()
        {
            FindPlayer();
            TryRepositionPlayer();
        }

        private void Start()
        {
            FindPlayer();
            TryRepositionPlayer();
        }

        private void FindPlayer()
        {
            if (playerTransform != null)
                return;

            var pc = Object.FindFirstObjectByType<Player_Gerak>();
            if (pc != null)
            {
                playerTransform = pc.transform;
                return;
            }

            var pi = Object.FindFirstObjectByType<Mission_PengantaranBantuan>();
            if (pi != null)
            {
                playerTransform = pi.transform;
                return;
            }

            var p = GameObject.FindWithTag("Player") ?? GameObject.Find("Player");
            if (p != null)
            {
                playerTransform = p.transform;
            }
        }

        private void TryRepositionPlayer()
        {
            if (StoryFlow_PengaturAlur.Instance == null || string.IsNullOrEmpty(spawnPointKey) ||
                StoryFlow_PengaturAlur.Instance.LastSpawnPoint != spawnPointKey || playerTransform == null)
                return;

            var cc = playerTransform.GetComponent<CharacterController>();
            if (cc != null) cc.enabled = false;

            Vector3 targetPos = transform.position + spawnOffset;
            playerTransform.position = targetPos;
            playerTransform.rotation = Quaternion.LookRotation(Vector3.forward);

            if (cc != null) cc.enabled = true;
            Physics.SyncTransforms();

            StoryFlow_PengaturAlur.Instance.LastSpawnPoint = null;
            Debug.Log($"[Interaction_PintuKampung] Pemain berhasil di-spawn di depan {spawnPointKey} pada posisi {targetPos}");
        }

        private void Update()
        {
            if (playerTransform == null)
            {
                FindPlayer();
                if (playerTransform == null)
                    return;
                TryRepositionPlayer();
            }

            if (UI_MenuSesi.InputBlocked)
                return;

            float dist = Vector3.Distance(TriggerCenter, playerTransform.position);
            isPlayerNear = dist <= interactionRadius;

            if (isPlayerNear)
            {
                Keyboard kb = Keyboard.current;
                if (kb != null && kb.eKey.wasPressedThisFrame)
                {
                    EnterScene();
                }
            }
        }

        private void EnterScene()
        {
            if (string.IsNullOrEmpty(targetScenePath))
            {
                Debug.LogError($"[Interaction_PintuKampung] targetScenePath kosong pada {gameObject.name}!");
                return;
            }

            if (StoryFlow_PengaturAlur.Instance != null && !string.IsNullOrEmpty(spawnPointKey))
            {
                StoryFlow_PengaturAlur.Instance.LastSpawnPoint = spawnPointKey;
            }

            Debug.Log($"[Interaction_PintuKampung] Memuat scene {targetScenePath}...");
            SceneManager.LoadScene(targetScenePath, LoadSceneMode.Single);
        }

        private void OnGUI()
        {
            if (!isPlayerNear || UI_MenuSesi.InputBlocked)
                return;

            UI_TemaVisual.EnsureInitialized();

            float pW = 280f;
            float pH = 44f;
            float pX = (Screen.width - pW) * 0.5f;
            float pY = Screen.height - 110f;

            GUI.DrawTexture(new Rect(pX + 2, pY + 2, pW, pH), UI_TemaVisual.ShadowTex);
            GUI.Box(new Rect(pX, pY, pW, pH), GUIContent.none, UI_TemaVisual.CardStyle);
            GUI.Label(new Rect(pX, pY + 10, pW, 24), $"Tekan [E] {promptLabel}", UI_TemaVisual.HintStyle);
        }

        private void OnDrawGizmosSelected()
        {
            Gizmos.color = Color.green;
            Gizmos.DrawWireSphere(TriggerCenter, interactionRadius);
        }
    }
}

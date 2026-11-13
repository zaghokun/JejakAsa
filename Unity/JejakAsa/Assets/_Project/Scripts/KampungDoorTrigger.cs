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
    public class KampungDoorTrigger : MonoBehaviour
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

            var pc = Object.FindFirstObjectByType<PlayerController>();
            if (pc != null)
            {
                playerTransform = pc.transform;
                return;
            }

            var pi = Object.FindFirstObjectByType<PlayerInteraction>();
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
            if (GameFlowManager.Instance != null && !string.IsNullOrEmpty(spawnPointKey) && GameFlowManager.Instance.LastSpawnPoint == spawnPointKey)
            {
                if (playerTransform != null)
                {
                    var cc = playerTransform.GetComponent<CharacterController>();
                    if (cc != null) cc.enabled = false;

                    Vector3 targetPos = transform.position + spawnOffset;
                    playerTransform.position = targetPos;
                    playerTransform.rotation = Quaternion.LookRotation(Vector3.forward);

                    if (cc != null) cc.enabled = true;
                    Physics.SyncTransforms();

                    GameFlowManager.Instance.LastSpawnPoint = null;
                    Debug.Log($"[KampungDoorTrigger] Pemain berhasil di-spawn di depan {spawnPointKey} pada posisi {targetPos}");
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
                TryRepositionPlayer();
            }

            if (SessionMenu.InputBlocked)
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
                Debug.LogError($"[KampungDoorTrigger] targetScenePath kosong pada {gameObject.name}!");
                return;
            }

            if (GameFlowManager.Instance != null && !string.IsNullOrEmpty(spawnPointKey))
            {
                GameFlowManager.Instance.LastSpawnPoint = spawnPointKey;
            }

            Debug.Log($"[KampungDoorTrigger] Memuat scene {targetScenePath}...");
            SceneManager.LoadScene(targetScenePath, LoadSceneMode.Single);
        }

        private void OnGUI()
        {
            if (!isPlayerNear || SessionMenu.InputBlocked)
                return;

            CozyGuiTheme.EnsureInitialized();

            float pW = 280f;
            float pH = 44f;
            float pX = (Screen.width - pW) * 0.5f;
            float pY = Screen.height - 110f;

            GUI.DrawTexture(new Rect(pX + 2, pY + 2, pW, pH), CozyGuiTheme.ShadowTex);
            GUI.Box(new Rect(pX, pY, pW, pH), GUIContent.none, CozyGuiTheme.CardStyle);
            GUI.Label(new Rect(pX, pY + 10, pW, 24), $"Tekan [E] {promptLabel}", CozyGuiTheme.HintStyle);
        }

        private void OnDrawGizmosSelected()
        {
            Gizmos.color = Color.green;
            Gizmos.DrawWireSphere(TriggerCenter, interactionRadius);
        }
    }
}

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;

namespace JejakAsa
{
    /// <summary>
    /// Mengelola transisi masuk/keluar arena Battle secara additive (U15).
    /// Dipasang pada objek Player di Kampung yang tetap aktif selama battle;
    /// visual/kamera dunia Kampung dinonaktifkan sementara, lalu dipulihkan tanpa mereset misi.
    /// </summary>
    [DisallowMultipleComponent]
    public sealed class BattleTransition : MonoBehaviour
    {
        [SerializeField] private string battleScenePath = "Assets/_Project/Scenes/Battle.unity";
        [SerializeField] private PlayerInteraction playerInteraction;
        [SerializeField] private GameObject kampungRoot;
        [SerializeField] private Camera kampungCamera;
        [SerializeField, Min(0f)] private float postBattleDelay = 1.2f;

        private readonly HashSet<EncounterController> subscribedEncounters = new HashSet<EncounterController>();
        private readonly List<GameObject> hiddenWorldObjects = new List<GameObject>();
        private readonly List<Renderer> hiddenPlayerRenderers = new List<Renderer>();

        private EncounterController sourceEncounter;
        private Scene loadedBattleScene;
        private bool isInBattle;
        private bool hiddenCameraWasEnabled;
        private AudioListener hiddenListener;
        private AudioSource pausedKampungAudio;


        public bool IsInBattle => isInBattle;

        public static BattleTransition Active { get; private set; }

        private void Awake()
        {
            Active = this;
        }

        private void Start()
        {
            ResolveSceneReferences();
            SubscribeAllActiveEncounters();
        }

        private void OnEnable()
        {
            if (Active == null)
                Active = this;
            SubscribeAllActiveEncounters();
        }

        private void OnDisable()
        {
            foreach (EncounterController enc in subscribedEncounters)
            {
                if (enc != null)
                    enc.BattleRequested -= OnBattleRequested;
            }
            subscribedEncounters.Clear();
        }

        private void OnDestroy()
        {
            if (Active == this)
                Active = null;
        }

        /// <summary>
        /// Mendaftarkan EncounterController agar tombol Bertengkar selalu terhubung tanpa peduli urutan inisialisasi.
        /// </summary>
        public void RegisterEncounter(EncounterController encounter)
        {
            if (encounter == null || subscribedEncounters.Contains(encounter))
                return;

            encounter.BattleRequested -= OnBattleRequested;
            encounter.BattleRequested += OnBattleRequested;
            subscribedEncounters.Add(encounter);
        }

        private void SubscribeAllActiveEncounters()
        {
            for (int i = 0; i < EncounterController.Active.Count; i++)
            {
                RegisterEncounter(EncounterController.Active[i]);
            }
        }

        private void ResolveSceneReferences()
        {
            if (playerInteraction == null)
                playerInteraction = GetComponentInParent<PlayerInteraction>() ?? FindFirstObjectByType<PlayerInteraction>();

            if (kampungRoot == null)
                kampungRoot = FindObjectInScene("U02_Graybox");

            if (kampungCamera == null)
                kampungCamera = FindCameraInScene();
        }

        private GameObject FindObjectInScene(string objectName)
        {
            GameObject[] roots = gameObject.scene.GetRootGameObjects();
            for (int i = 0; i < roots.Length; i++)
            {
                if (roots[i].name == objectName)
                    return roots[i];

                Transform found = FindChildRecursive(roots[i].transform, objectName);
                if (found != null)
                    return found.gameObject;
            }
            return null;
        }

        private static Transform FindChildRecursive(Transform parent, string targetName)
        {
            for (int i = 0; i < parent.childCount; i++)
            {
                Transform child = parent.GetChild(i);
                if (child.name == targetName)
                    return child;
                Transform nested = FindChildRecursive(child, targetName);
                if (nested != null)
                    return nested;
            }
            return null;
        }

        private Camera FindCameraInScene()
        {
            GameObject[] roots = gameObject.scene.GetRootGameObjects();
            for (int i = 0; i < roots.Length; i++)
            {
                Camera cam = roots[i].GetComponentInChildren<Camera>(true);
                if (cam != null)
                    return cam;
            }
            return Camera.main;
        }

        private void OnBattleRequested(EncounterController encounter)
        {
            if (isInBattle)
                return;

            sourceEncounter = encounter;
            StartCoroutine(BattleFlow());
        }

        private IEnumerator BattleFlow()
        {
            ResolveSceneReferences();
            isInBattle = true;

            if (playerInteraction != null)
                playerInteraction.SetEncounterMovementLock(true);

            HideKampungVisualsAndCamera();

            AsyncOperation op = SceneManager.LoadSceneAsync(battleScenePath, LoadSceneMode.Additive);
            if (op == null)
            {
                Debug.LogError($"[BattleTransition] Gagal memuat scene battle: {battleScenePath}");
                AbortBattle();
                yield break;
            }
            yield return op;

            loadedBattleScene = SceneManager.GetSceneByPath(battleScenePath);
            if (!loadedBattleScene.IsValid() || !loadedBattleScene.isLoaded)
            {
                Debug.LogError($"[BattleTransition] Scene battle tidak valid setelah dimuat: {battleScenePath}");
                AbortBattle();
                yield break;
            }

            BattleController battle = null;
            GameObject[] battleRoots = loadedBattleScene.GetRootGameObjects();
            for (int i = 0; i < battleRoots.Length; i++)
            {
                battle = battleRoots[i].GetComponentInChildren<BattleController>(true);
                if (battle != null)
                    break;
            }

            if (battle == null)
            {
                Debug.LogError("[BattleTransition] BattleController tidak ditemukan di Battle.unity!");
                yield return SceneManager.UnloadSceneAsync(loadedBattleScene);
                AbortBattle();
                yield break;
            }

            battle.StartBattle();

            bool? battleResult = null;
            battle.OnBattleEnd += (won) => battleResult = won;

            while (battleResult == null)
                yield return null;

            // Beri jeda sejenak agar pemain sempat membaca hasil battle (bisa dipercepat klik Lanjut / Enter / Spasi).
            float elapsed = 0f;
            while (elapsed < postBattleDelay && !battle.IsDismissRequested)
            {
                if (SessionMenu.InputBlocked)
                {
                    yield return null;
                    continue;
                }
                elapsed += Time.deltaTime;
                Keyboard kb = Keyboard.current;
                if (elapsed >= 0.25f && kb != null && (kb.enterKey.wasPressedThisFrame || kb.spaceKey.wasPressedThisFrame))
                    break;
                yield return null;
            }
            yield return new WaitWhile(() => SessionMenu.InputBlocked);


            if (loadedBattleScene.IsValid() && loadedBattleScene.isLoaded)
                yield return SceneManager.UnloadSceneAsync(loadedBattleScene);

            RestoreKampungVisualsAndCamera();

            if (battleResult == true)
            {
                sourceEncounter?.ResolveFromBattleVictory();

                GameObject returnPoint = FindObjectInScene("BattleReturnPoint");
                if (returnPoint != null && playerInteraction != null)
                {
                    CharacterController cc = playerInteraction.GetComponent<CharacterController>();
                    if (cc != null) cc.enabled = false;
                    playerInteraction.transform.position = returnPoint.transform.position;
                    if (cc != null) cc.enabled = true;
                    Physics.SyncTransforms();
                }

                isInBattle = false;
                sourceEncounter = null;

                playerInteraction?.OpenSystemDialogue(
                    "Menang Battle!",
                    "Kamu berhasil mengalahkan preman!\n\nGang utama sekarang aman. Lanjutkan misi pengantaran.");

                if (playerInteraction != null && !playerInteraction.IsDialogueOpen)
                    playerInteraction.SetEncounterMovementLock(false);
            }
            else
            {
                sourceEncounter?.CancelPendingBattle();
                isInBattle = false;
                sourceEncounter = null;

                if (playerInteraction != null)
                {
                    if (playerInteraction.DeliveredCount > 0)
                        playerInteraction.SetOutcome(MissionOutcome.PartialSuccess, "Kalah battle melawan preman.");
                    else
                        playerInteraction.SetOutcome(MissionOutcome.Failed, "Kalah battle melawan preman.");
                }
            }
        }

        private void AbortBattle()
        {
            RestoreKampungVisualsAndCamera();
            sourceEncounter?.CancelPendingBattle();
            isInBattle = false;
            sourceEncounter = null;
            playerInteraction?.SetEncounterMovementLock(false);
        }

        private void HideKampungVisualsAndCamera()
        {
            hiddenWorldObjects.Clear();
            hiddenPlayerRenderers.Clear();

            string[] worldObjectNames =
            {
                "U02_Graybox",
                "Plane",
                "PosBantuan",
                "Directional Light",
                "InteractionCanvas"
            };

            for (int i = 0; i < worldObjectNames.Length; i++)
            {
                GameObject obj = FindObjectInScene(worldObjectNames[i]);
                TryHideObject(obj);
            }

            if (kampungRoot != null)
                TryHideObject(kampungRoot);

            if (kampungCamera != null && kampungCamera.gameObject.activeSelf)
            {
                AudioSource audioSource = kampungCamera.GetComponent<AudioSource>();
                if (audioSource != null && audioSource.isPlaying)
                {
                    audioSource.Pause();
                    pausedKampungAudio = audioSource;
                }
                hiddenCameraWasEnabled = kampungCamera.enabled;
                kampungCamera.enabled = false;
                AudioListener listener = kampungCamera.GetComponent<AudioListener>();
                if (listener != null && listener.enabled)
                {
                    listener.enabled = false;
                    hiddenListener = listener;
                }
            }

            if (playerInteraction != null)
            {
                Renderer[] renderers = playerInteraction.GetComponentsInChildren<Renderer>(false);
                for (int i = 0; i < renderers.Length; i++)
                {
                    if (renderers[i].enabled)
                    {
                        renderers[i].enabled = false;
                        hiddenPlayerRenderers.Add(renderers[i]);
                    }
                }
            }
        }

        private void TryHideObject(GameObject obj)
        {
            if (obj == null || !obj.activeSelf || obj == gameObject || hiddenWorldObjects.Contains(obj))
                return;

            obj.SetActive(false);
            hiddenWorldObjects.Add(obj);
        }

        private void RestoreKampungVisualsAndCamera()
        {
            for (int i = 0; i < hiddenWorldObjects.Count; i++)
            {
                if (hiddenWorldObjects[i] != null)
                    hiddenWorldObjects[i].SetActive(true);
            }
            hiddenWorldObjects.Clear();

            if (hiddenCameraWasEnabled && kampungCamera != null)
                kampungCamera.enabled = true;
            if (hiddenListener != null)
                hiddenListener.enabled = true;
            hiddenListener = null;
            if (pausedKampungAudio != null)
            {
                pausedKampungAudio.UnPause();
                pausedKampungAudio = null;
            }
            hiddenCameraWasEnabled = false;

            for (int i = 0; i < hiddenPlayerRenderers.Count; i++)
            {
                if (hiddenPlayerRenderers[i] != null)
                    hiddenPlayerRenderers[i].enabled = true;
            }
            hiddenPlayerRenderers.Clear();
        }

        /// <summary>
        /// Untuk PlayMode test: atur jeda pasca-battle tanpa menimpa referensi scene.
        /// </summary>
        public void SetPostBattleDelayForTests(float delaySeconds)
        {
            postBattleDelay = Mathf.Max(0f, delaySeconds);
        }

        /// <summary>
        /// Untuk PlayMode test: setup referensi dan jeda pasca-battle secara programatik.
        /// </summary>
        public void ConfigureForTests(PlayerInteraction interaction, GameObject root, Camera cam, string scenePath = null, float delaySeconds = 0f)
        {
            playerInteraction = interaction;
            kampungRoot = root;
            kampungCamera = cam;
            postBattleDelay = delaySeconds;
            if (scenePath != null)
                battleScenePath = scenePath;
        }
    }
}

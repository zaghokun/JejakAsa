using System.Collections;
using UnityEngine;

namespace JejakAsa
{
    [DisallowMultipleComponent]
    public sealed class P01_VickyPulang : MonoBehaviour
    {
        private static readonly DialogueLine[] Lines =
        {
            new DialogueLine { speaker = "Vicky", text = "Akhirnya pulang juga. Rasanya gang ini lebih sempit daripada yang kuingat." },
            new DialogueLine { speaker = "Ibu", text = "Gang-nya tidak mengecil. Kamu saja yang sudah lama tidak pulang." },
            new DialogueLine { speaker = "Vicky", text = "Bisa jadi, Bu. Bagaimana keadaan warga sekarang?" },
            new DialogueLine { speaker = "Ibu", text = "Banyak yang sedang berusaha mencukupi kebutuhan sehari-hari. Tetangga juga saling membantu, tapi kadang bantuan yang datang belum sampai ke semua orang yang membutuhkan." },
            new DialogueLine { speaker = "Vicky", text = "Aku ingin cari tahu apa yang benar-benar mereka perlukan. Aku tidak mau datang lalu asal membagikan sesuatu." },
            new DialogueLine { speaker = "Ibu", text = "Bagus. Mulai dengan mendengarkan mereka, ya." }
        };

        [SerializeField] private PlayerController playerReference;
        [SerializeField] private StoryDialog_InteraksiNPC ibuReference;
        [SerializeField] private SceneStartFade fadeReference;
        [SerializeField] private Camera cameraReference;

        private PlayerController player;
        private StoryDialog_InteraksiNPC ibu;
        private bool movementWasEnabled;
        private bool started;
        private bool sequenceStarted;
        private bool finishing;
        private bool cameraCaptured;
        private FixedIsometricCamera cameraFollow;
        private bool followWasEnabled;
        private Vector3 originalCameraPosition;
        private Quaternion originalCameraRotation;
        private float originalCameraSize;

        private void Start()
        {
            started = true;
            StartCoroutine(PlayOpening());
        }

        private void OnEnable()
        {
            if (started)
                StartCoroutine(PlayOpening());
        }

        private IEnumerator PlayOpening()
        {
            if (StoryFlow_PengaturAlur.Instance.CurrentPhase != DayPhase.Prolog)
                yield break;

            player = playerReference != null ? playerReference : FindFirstObjectByType<PlayerController>();
            ibu = ibuReference;
            if (ibu == null)
                foreach (var npc in FindObjectsByType<StoryDialog_InteraksiNPC>(FindObjectsSortMode.None))
                    if (npc.npcType == StoryNpcType.Ibu)
                    {
                        ibu = npc;
                        break;
                    }
            if (player == null || ibu == null)
            {
                Debug.LogError("P01 requires a PlayerController and an Ibu StoryDialog_InteraksiNPC.", this);
                yield break;
            }

            movementWasEnabled = player.enabled;
            player.enabled = false;
            ibu.playerTransform = player.transform;
            if (cameraReference != null)
            {
                originalCameraPosition = cameraReference.transform.position;
                originalCameraRotation = cameraReference.transform.rotation;
                originalCameraSize = cameraReference.orthographicSize;
                cameraFollow = cameraReference.GetComponent<FixedIsometricCamera>();
                if (cameraFollow != null)
                {
                    followWasEnabled = cameraFollow.enabled;
                    cameraFollow.enabled = false;
                }
                cameraCaptured = true;
                Vector3 focus = (player.transform.position + ibu.transform.position) * 0.5f;
                focus.y = -0.6f;
                cameraReference.transform.position = focus - cameraReference.transform.forward * 18f;
                cameraReference.orthographicSize = 3.5f;
            }
            var fade = fadeReference;
            while (fade != null && !fade.IsFinished && StoryFlow_PengaturAlur.Instance.CurrentPhase == DayPhase.Prolog)
                yield return null;

            while (StoryFlow_PengaturAlur.Instance.CurrentPhase == DayPhase.Prolog)
            {
                if (!sequenceStarted && !finishing && ibu.isActiveAndEnabled)
                    sequenceStarted = ibu.OpenSequence(Lines, CompleteOpening);
                if (sequenceStarted && !ibu.IsDialogueOpen)
                    sequenceStarted = false;
                yield return null;
            }
            if (sequenceStarted && ibu != null)
                ibu.CancelSequence();
            sequenceStarted = false;
            RestoreCamera();
            RestoreMovement();
        }

        private void CompleteOpening()
        {
            sequenceStarted = false;
            finishing = true;
            StartCoroutine(ReturnToGameplay());
        }

        private IEnumerator ReturnToGameplay()
        {
            if (cameraCaptured && cameraReference != null)
            {
                Vector3 from = cameraReference.transform.position;
                float fromSize = cameraReference.orthographicSize;
                float elapsed = 0f;
                while (elapsed < 0.4f)
                {
                    elapsed += Time.unscaledDeltaTime;
                    float t = Mathf.SmoothStep(0f, 1f, elapsed / 0.4f);
                    cameraReference.transform.position = Vector3.Lerp(from, originalCameraPosition, t);
                    cameraReference.orthographicSize = Mathf.Lerp(fromSize, originalCameraSize, t);
                    yield return null;
                }
            }
            StoryFlow_PengaturAlur.Instance.CompleteProlog();
            finishing = false;
            RestoreCamera();
            RestoreMovement();
        }

        private void RestoreCamera()
        {
            if (!cameraCaptured || cameraReference == null)
                return;
            cameraReference.transform.SetPositionAndRotation(originalCameraPosition, originalCameraRotation);
            cameraReference.orthographicSize = originalCameraSize;
            if (cameraFollow != null) cameraFollow.enabled = followWasEnabled;
            cameraCaptured = false;
        }

        private void OnDisable()
        {
            StopAllCoroutines();
            if (sequenceStarted && ibu != null)
                ibu.CancelSequence();
            sequenceStarted = false;
            finishing = false;
            RestoreCamera();
            RestoreMovement();
        }

        private void RestoreMovement()
        {
            if (player != null)
                player.enabled = movementWasEnabled;
            player = null;
        }
    }
}

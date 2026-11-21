using System.Collections;
using UnityEngine;

namespace JejakAsa
{
    public enum PoskoIntroStage { None, Listening, Sponsor, Reunion, Threat }

    [DefaultExecutionOrder(-300), DisallowMultipleComponent]
    public sealed class P03_SponsorDanPosko : MonoBehaviour
    {
        [SerializeField] private StoryDialog_InteraksiNPC jonathan, ophelia, resident;
        [SerializeField] private GameObject logisticsVisuals, registrationDetails;
        [SerializeField] private Transform communityHelper;
        [SerializeField] private P05_BayanganAncaman threat;
        [SerializeField, Min(.5f)] private float setupSeconds = 3.4f;
        private static P03_SponsorDanPosko active;
        private bool settingUp, actorsPlaced;
        private float fade;
        private PlayerController lockedPlayer;
        private bool movementBeforeSetup;
        private Camera setupCamera;
        private FixedIsometricCamera setupFollow;
        private bool followBeforeSetup;
        private Vector3 cameraPosition;
        private Quaternion cameraRotation;
        private float cameraSize;
        public static bool PresentationActive => (active != null && active.settingUp) || P05_BayanganAncaman.IsActive;
        public bool IsSettingUp => settingUp;

        private static readonly DialogueLine[] SponsorLines =
        {
            new DialogueLine {speaker="Pak Jonathan",text="Vicky? Saya Jonathan. Saya mendapat ceritamu dari warga. Saya menjadi penghubung dengan sponsor yang bersedia menyediakan bantuan untuk program ini."},
            new DialogueLine {speaker="Vicky",text="Terima kasih, Pak. Saya ingin bantuan sampai ke rumah yang sudah didata warga. Tapi saya belum pernah mengelola program seperti ini."},
            new DialogueLine {speaker="Pak Jonathan",text="Kita kerjakan bersama. Warga membantu menyusun kebutuhan, sponsor membantu persediaan, dan kita mencatat apa yang masuk serta keluar. Jangan mengambil keputusan untuk warga tanpa berbicara dengan mereka."},
            new DialogueLine {speaker="Vicky",text="Baik, Pak. Tempat ini bisa kita jadikan posko. Saya akan bertanggung jawab atas pengantarannya."},
            new DialogueLine {speaker="Bu Rina",text="Kami bisa membantu memastikan alamat dan menyampaikan kalau ada kebutuhan yang berubah."},
            new DialogueLine {speaker="Pak Jonathan",text="Saya juga akan mendampingimu. Kalau ada masalah di lapangan, laporkan. Kita cari jalan keluar bersama."}
        };


        public static PoskoIntroStage GetStage(GameProgressData data)
        {
            if (!data.requiresP02Listening || data.currentDay != 1 || data.currentPhase != DayPhase.PoskoBriefing)
                return PoskoIntroStage.None;
            // Earlier saves that already started delivery must not be rewound.
            if (data.poskoPrologueVersion == 0 && (data.isBriefingDone || data.hasPickedUpBansos || data.isLevelCompleted))
                return PoskoIntroStage.None;
            if (!data.isP02ListeningDone) return PoskoIntroStage.Listening;
            if (!data.isPoskoEstablished) return PoskoIntroStage.Sponsor;
            if (!data.isOpheliaReunionDone) return PoskoIntroStage.Reunion;
            return data.isThreatForeshadowDone ? PoskoIntroStage.None : PoskoIntroStage.Threat;
        }

        public static string Objective(GameProgressData data)
        {
            switch (GetStage(data))
            {
                case PoskoIntroStage.Listening: return "Dengarkan kebutuhan warga bersama Bu Rina";
                case PoskoIntroStage.Sponsor: return "Temui Pak Jonathan untuk menyiapkan posko";
                case PoskoIntroStage.Reunion: return "Sapa Ophelia di dekat bangku posko";
                case PoskoIntroStage.Threat: return "Selesaikan pengenalan posko";
                default: return "Temui Pak Jonathan";
            }
        }

        private void OnEnable() { active = this; }
        private void Start() { RefreshAvailability(); }
        private void Update()
        {
            RefreshAvailability();
            if (GetStage(StoryFlow_PengaturAlur.Instance.Progress) == PoskoIntroStage.Threat && !PresentationActive &&
                !StoryDialog_InteraksiNPC.AnyDialogueOpen && !SessionMenu.InputBlocked && threat != null && threat.isActiveAndEnabled)
                threat.Play();
        }

        public void RefreshAvailability()
        {
            if (jonathan == null || ophelia == null || resident == null) return;
            var progress = StoryFlow_PengaturAlur.Instance.Progress;
            var stage = GetStage(progress);
            bool available = !PresentationActive;
            jonathan.enabled = available && (stage == PoskoIntroStage.None || stage == PoskoIntroStage.Sponsor);
            ophelia.enabled = available && (stage == PoskoIntroStage.None || stage == PoskoIntroStage.Reunion);
            resident.enabled = available && (stage == PoskoIntroStage.None || stage == PoskoIntroStage.Listening);
            bool furnished = settingUp || stage == PoskoIntroStage.None || progress.isPoskoEstablished;
            if (logisticsVisuals != null) logisticsVisuals.SetActive(furnished);
            if (registrationDetails != null) registrationDetails.SetActive(furnished);
            if (progress.isPoskoEstablished && !settingUp && !actorsPlaced) PlaceResidents();
        }

        public static bool TryOpenInteraction(StoryDialog_InteraksiNPC npc)
        {
            var sceneIntro = active;
            if (sceneIntro == null || sceneIntro.gameObject.scene != npc.gameObject.scene) return false;
            var stage = GetStage(StoryFlow_PengaturAlur.Instance.Progress);
            if (stage == PoskoIntroStage.None) return false;
            if (stage == PoskoIntroStage.Listening && npc == sceneIntro.resident) return false;
            if (PresentationActive || SessionMenu.InputBlocked) return true;
            if (stage == PoskoIntroStage.Sponsor && npc == sceneIntro.jonathan)
            {
                FaceConversation(npc);
                npc.OpenSequence(SponsorLines, () => sceneIntro.StartCoroutine(sceneIntro.EstablishPosko()));
            }
            else if (stage == PoskoIntroStage.Reunion && npc == sceneIntro.ophelia)
            {
                FaceConversation(npc);
                P04_PertemuanOphelia.Open(npc);
            }
            return true;
        }
        private static void FaceConversation(StoryDialog_InteraksiNPC npc)
        {
            if (npc.playerTransform == null) return;
            Vector3 direction = npc.playerTransform.position - npc.transform.position; direction.y = 0;
            if (direction.sqrMagnitude < .001f) return;
            npc.transform.rotation = Quaternion.LookRotation(direction);
            npc.playerTransform.rotation = Quaternion.LookRotation(-direction);
        }

        private IEnumerator EstablishPosko()
        {
            settingUp = true;
            lockedPlayer = jonathan.playerTransform.GetComponent<PlayerController>();
            movementBeforeSetup = lockedPlayer != null && lockedPlayer.enabled;
            if (lockedPlayer != null) lockedPlayer.enabled = false;
            setupCamera = Camera.main;
            if (setupCamera != null)
            {
                setupFollow = setupCamera.GetComponent<FixedIsometricCamera>();
                followBeforeSetup = setupFollow != null && setupFollow.enabled;
                if (setupFollow != null) setupFollow.enabled = false;
                cameraPosition = setupCamera.transform.position; cameraRotation = setupCamera.transform.rotation;
                cameraSize = setupCamera.orthographicSize;
            }
            RefreshAvailability();
            yield return FadeTo(1, .35f);
            if (setupCamera != null)
            {
                setupCamera.orthographicSize = 5.5f;
                setupCamera.transform.position = new Vector3(-3, .1f, 3.8f) - setupCamera.transform.forward * 20;
            }
            if (logisticsVisuals != null) logisticsVisuals.SetActive(true);
            if (registrationDetails != null) registrationDetails.SetActive(true);
            Vector3 fromRina = resident.transform.position;
            Vector3 fromHelper = communityHelper != null ? communityHelper.position : Vector3.zero;
            yield return FadeTo(0, .45f);
            float elapsed = 0;
            while (elapsed < setupSeconds)
            {
                elapsed += Time.unscaledDeltaTime;
                float t = Mathf.SmoothStep(0, 1, elapsed / setupSeconds);
                MoveResident(resident.transform, Vector3.Lerp(fromRina, new Vector3(-.8f, 1, 2.5f), t));
                if (communityHelper != null) MoveResident(communityHelper, Vector3.Lerp(fromHelper, new Vector3(2, 1, 5.4f), t));
                yield return null;
            }
            yield return FadeTo(1, .35f);
            StoryFlow_PengaturAlur.Instance.Progress.isPoskoEstablished = true;
            PlaceResidents();
            RestoreSetupCamera();
            yield return FadeTo(0, .4f);
            settingUp = false;
            if (lockedPlayer != null) lockedPlayer.enabled = movementBeforeSetup;
            RefreshAvailability();
        }
        private static void MoveResident(Transform actor, Vector3 position)
        {
            Vector3 direction = position - actor.position; direction.y = 0;
            if (direction.sqrMagnitude > .00001f) actor.rotation = Quaternion.LookRotation(direction);
            actor.position = position;
        }
        private void PlaceResidents()
        {
            resident.transform.position = new Vector3(-.8f, 1, 2.5f);
            resident.transform.rotation = Quaternion.Euler(0, 180, 0);
            if (communityHelper != null) communityHelper.position = new Vector3(2, 1, 5.4f);
            actorsPlaced = true;
        }
        private IEnumerator FadeTo(float target, float seconds)
        {
            float from = fade;
            for (float t = 0; t < seconds; t += Time.unscaledDeltaTime)
            { fade = Mathf.Lerp(from, target, t / seconds); yield return null; }
            fade = target;
        }
        private void RestoreSetupCamera()
        {
            if (setupCamera != null) { setupCamera.transform.SetPositionAndRotation(cameraPosition, cameraRotation); setupCamera.orthographicSize = cameraSize; }
            if (setupFollow != null) setupFollow.enabled = followBeforeSetup;
        }
        private void OnDisable()
        {
            if (settingUp)
            {
                StopAllCoroutines(); RestoreSetupCamera();
                if (lockedPlayer != null) lockedPlayer.enabled = movementBeforeSetup;
                settingUp = false; fade = 0;
            }
            if (active == this) active = null;
        }
        private void OnGUI()
        {
            if (!settingUp) return;
            GUI.depth = -2000;
            if (fade > 0) CozyGuiTheme.Fill(new Rect(0, 0, Screen.width, Screen.height), new Color(0, 0, 0, fade));
            if (fade < .5f)
            {
                CozyGuiTheme.EnsureInitialized();
                GUI.Label(new Rect(24, Screen.height - 65, Screen.width - 48, 40), "Warga menyiapkan posko bersama.", CozyGuiTheme.White);
            }
        }
    }
}

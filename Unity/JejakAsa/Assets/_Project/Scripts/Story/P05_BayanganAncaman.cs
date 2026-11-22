using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;

namespace JejakAsa
{
    [DisallowMultipleComponent]
    public sealed class P05_BayanganAncaman : MonoBehaviour
    {
        [SerializeField] private StoryDialog_InteraksiNPC narrator;
        [SerializeField] private Transform player;
        [SerializeField] private GameObject cinematicSets;
        [SerializeField] private Transform deddyCamera, deddyLookAt, gasCamera, gasLookAt;
        [SerializeField] private AudioSource tensionAudio;
        [SerializeField, Min(.2f)] private float minimumLineSeconds = .85f;
        private static P05_BayanganAncaman active;
        public static bool IsActive => active != null && active.playing;
        public bool IsPlaying => playing;
        private bool playing, gasShot, ending;
        private float fade, shotStartedAt, startedAt;
        private Camera cameraReference;
        private FixedIsometricCamera follow;
        private PlayerController movement;
        private bool followEnabled, movementEnabled, orthographic;
        private Vector3 oldPosition;
        private Quaternion oldRotation;
        private float oldSize, oldFov;
        private CameraClearFlags oldClear;
        private Color oldBackground, oldAmbient, oldFogColor;
        private AmbientMode oldAmbientMode;
        private float oldAmbientIntensity, oldFogDensity;
        private FogMode oldFogMode;
        private bool oldFog;
        private readonly List<Light> dayLights = new List<Light>();
        private readonly List<float> lightLevels = new List<float>();
        private readonly List<AudioSource> sceneAudio = new List<AudioSource>();
        private readonly List<float> sceneAudioLevels = new List<float>();

        private static readonly DialogueLine[] Lines =
        {
            new DialogueLine {speaker="Mr. Deddy",text="Posko baru? Kalau mereka mulai melewati gang itu, orang-orang akan lupa siapa yang selama ini mengatur jalan di sini."},
            new DialogueLine {speaker="Anak buah",text="Mau kami hentikan sekarang, Pak?"},
            new DialogueLine {speaker="Mr. Deddy",text="Awasi dulu. Aku ingin tahu seberapa jauh mereka berani melangkah."},
            new DialogueLine {speaker="Mr. Gas Oke",text="Program bantuan tanpa kendaliku? Menarik. Biarkan mereka mulai. Nanti kita bicarakan siapa yang berhak menentukan jalurnya."}
        };

        public bool Play()
        {
            if (!isActiveAndEnabled || playing || narrator == null || cinematicSets == null || player == null || Camera.main == null ||
                P03_SponsorDanPosko.GetStage(StoryFlow_PengaturAlur.Instance.Progress) != PoskoIntroStage.Threat || StoryDialog_InteraksiNPC.AnyDialogueOpen)
                return false;
            active = this; playing = true; ending = false; gasShot = false; fade = 0;
            startedAt = Time.unscaledTime;
            CaptureState();
            StoryDialog_InteraksiNPC.SpeakerChanged += OnSpeaker;
            StartCoroutine(Begin());
            return true;
        }

        private void CaptureState()
        {
            cameraReference = Camera.main;
            oldPosition = cameraReference.transform.position; oldRotation = cameraReference.transform.rotation;
            oldSize = cameraReference.orthographicSize; oldFov = cameraReference.fieldOfView;
            orthographic = cameraReference.orthographic; oldClear = cameraReference.clearFlags; oldBackground = cameraReference.backgroundColor;
            follow = cameraReference.GetComponent<FixedIsometricCamera>(); followEnabled = follow != null && follow.enabled;
            if (follow != null) follow.enabled = false;
            movement = player.GetComponent<PlayerController>(); movementEnabled = movement != null && movement.enabled;
            if (movement != null) movement.enabled = false;
            oldAmbientMode = RenderSettings.ambientMode; oldAmbient = RenderSettings.ambientLight;
            oldAmbientIntensity = RenderSettings.ambientIntensity;
            oldFog = RenderSettings.fog; oldFogColor = RenderSettings.fogColor; oldFogMode = RenderSettings.fogMode; oldFogDensity = RenderSettings.fogDensity;
            dayLights.Clear(); lightLevels.Clear(); sceneAudio.Clear(); sceneAudioLevels.Clear();
            foreach (var light in FindObjectsByType<Light>(FindObjectsSortMode.None))
                if (light.gameObject.scene == gameObject.scene && !light.transform.IsChildOf(cinematicSets.transform))
                { dayLights.Add(light); lightLevels.Add(light.intensity); }
            foreach (var audio in FindObjectsByType<AudioSource>(FindObjectsSortMode.None))
                if (audio.gameObject.scene == gameObject.scene && audio != tensionAudio)
                { sceneAudio.Add(audio); sceneAudioLevels.Add(audio.volume); }
        }

        private IEnumerator Begin()
        {
            yield return FadeTo(1, .75f);
            cinematicSets.SetActive(true);
            foreach (var light in dayLights) if (light != null) light.intensity = 0;
            RenderSettings.ambientMode = AmbientMode.Flat; RenderSettings.ambientLight = new Color(.10f,.12f,.16f); RenderSettings.ambientIntensity = .5f;
            RenderSettings.fog = true; RenderSettings.fogMode = FogMode.ExponentialSquared; RenderSettings.fogDensity = .03f; RenderSettings.fogColor = new Color(.012f,.018f,.028f);
            cameraReference.orthographic = false; cameraReference.fieldOfView = 40;
            cameraReference.clearFlags = CameraClearFlags.SolidColor; cameraReference.backgroundColor = new Color(.008f,.012f,.021f);
            shotStartedAt = Time.unscaledTime; ApplyCamera();
            if (tensionAudio != null) { tensionAudio.volume = 0; tensionAudio.Play(); }
            yield return FadeTo(0, .8f);
            yield return new WaitForSecondsRealtime(.75f);
            if (!narrator.OpenSequence(Lines, () => StartCoroutine(Complete())))
            { Restore(); yield break; }
            narrator.HidePresentation = true;
            narrator.EarliestAdvanceTime = Time.unscaledTime + minimumLineSeconds;
        }

        private void OnSpeaker(string speaker)
        {
            if (!playing || string.IsNullOrEmpty(speaker)) return;
            if (speaker == "Mr. Gas Oke" && !gasShot)
            {
                gasShot = true; shotStartedAt = Time.unscaledTime;
                // A brief cut through black separates the observer in the alley
                // from the person planning the routes at the desk.
                fade = 1;
                StartCoroutine(FadeTo(0, .55f));
            }
            narrator.EarliestAdvanceTime = Time.unscaledTime + minimumLineSeconds;
        }
        private void LateUpdate()
        {
            if (!playing || !cinematicSets.activeSelf) return;
            ApplyCamera();
            for (int i = 0; i < sceneAudio.Count; i++) if (sceneAudio[i] != null) sceneAudio[i].volume = sceneAudioLevels[i] * .08f;
            if (tensionAudio != null)
                tensionAudio.volume = .42f * Mathf.Clamp01((Time.unscaledTime - startedAt) / 2.5f) * (1 - fade);
        }
        private void ApplyCamera()
        {
            Transform anchor = gasShot ? gasCamera : deddyCamera;
            Transform look = gasShot ? gasLookAt : deddyLookAt;
            if (anchor == null || look == null || cameraReference == null) return;
            float push = Mathf.SmoothStep(0, 1, Mathf.Clamp01((Time.unscaledTime - shotStartedAt) / 12f));
            Vector3 direction = (look.position - anchor.position).normalized;
            cameraReference.transform.position = anchor.position + direction * push * .55f;
            cameraReference.transform.rotation = Quaternion.LookRotation(look.position - cameraReference.transform.position);
        }
        private IEnumerator Complete()
        {
            if (ending) yield break;
            ending = true;
            yield return new WaitForSecondsRealtime(.55f);
            yield return FadeTo(1, .8f);
            RestoreWorldAndCamera();
            yield return new WaitForSecondsRealtime(.25f);
            yield return FadeTo(0, .65f);
            StoryFlow_PengaturAlur.Instance.Progress.isThreatForeshadowDone = true;
            Finish();
        }
        private IEnumerator FadeTo(float target, float seconds)
        {
            float from = fade;
            for (float t = 0; t < seconds; t += Time.unscaledDeltaTime)
            { fade = Mathf.Lerp(from, target, t / seconds); yield return null; }
            fade = target;
        }
        private void RestoreWorldAndCamera()
        {
            if (cinematicSets != null) cinematicSets.SetActive(false);
            if (tensionAudio != null) tensionAudio.Stop();
            for (int i = 0; i < dayLights.Count; i++) if (dayLights[i] != null) dayLights[i].intensity = lightLevels[i];
            for (int i = 0; i < sceneAudio.Count; i++) if (sceneAudio[i] != null) sceneAudio[i].volume = sceneAudioLevels[i];
            RenderSettings.ambientMode = oldAmbientMode; RenderSettings.ambientLight = oldAmbient; RenderSettings.ambientIntensity = oldAmbientIntensity;
            RenderSettings.fog = oldFog; RenderSettings.fogColor = oldFogColor; RenderSettings.fogMode = oldFogMode; RenderSettings.fogDensity = oldFogDensity;
            if (cameraReference != null)
            {
                cameraReference.transform.SetPositionAndRotation(oldPosition, oldRotation);
                cameraReference.orthographic = orthographic; cameraReference.orthographicSize = oldSize; cameraReference.fieldOfView = oldFov;
                cameraReference.clearFlags = oldClear; cameraReference.backgroundColor = oldBackground;
            }
        }
        private void Finish()
        {
            StoryDialog_InteraksiNPC.SpeakerChanged -= OnSpeaker;
            if (follow != null) follow.enabled = followEnabled;
            if (movement != null) movement.enabled = movementEnabled;
            playing = false; ending = false; fade = 0;
            if (active == this) active = null;
        }
        private void Restore()
        {
            if (narrator != null) narrator.CancelSequence();
            RestoreWorldAndCamera(); Finish();
        }
        private void OnDisable()
        {
            if (!playing) return;
            StopAllCoroutines(); Restore();
        }
        private GUIStyle subtitleStyle, speakerStyle, hintStyle;
        private void OnGUI()
        {
            if (!playing) return;
            GUI.depth = -2500;
            float scale = Mathf.Max(.1f, Mathf.Min(Screen.width / 1280f, Screen.height / 720f));
            Matrix4x4 old = GUI.matrix; GUI.matrix = Matrix4x4.Scale(new Vector3(scale,scale,1));
            float w = Screen.width / scale, h = Screen.height / scale;
            CozyGuiTheme.Fill(new Rect(0,0,w,58), Color.black);
            CozyGuiTheme.Fill(new Rect(0,h-166,w,166), new Color(.008f,.011f,.018f,1));
            if (narrator != null && narrator.IsDialogueOpen && fade < .8f)
            {
                CozyGuiTheme.EnsureInitialized();
                if (subtitleStyle == null)
                {
                    subtitleStyle = CozyGuiTheme.Text(23,new Color(.92f,.91f,.86f));
                    speakerStyle = CozyGuiTheme.Text(15,new Color(.78f,.56f,.33f),true);
                    hintStyle = CozyGuiTheme.Text(13,new Color(.58f,.61f,.63f),false,TextAnchor.MiddleRight);
                }
                GUI.Label(new Rect(76,h-151,w-152,24),narrator.ActiveSpeaker.ToUpperInvariant(),speakerStyle);
                GUI.Label(new Rect(76,h-120,w-152,82),narrator.ActiveText,subtitleStyle);
                if (Time.unscaledTime >= narrator.EarliestAdvanceTime && GUI.Button(new Rect(w-320,h-32,244,24),"Enter / Klik untuk lanjut  >",hintStyle))
                    narrator.CloseDialogue();
            }
            if (fade > 0) CozyGuiTheme.Fill(new Rect(0,0,w,h),new Color(0,0,0,fade));
            GUI.matrix = old;
        }
    }
}

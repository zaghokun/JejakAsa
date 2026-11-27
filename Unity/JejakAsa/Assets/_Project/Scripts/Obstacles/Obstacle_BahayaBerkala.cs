using System.Collections.Generic;
using UnityEngine;

namespace JejakAsa
{
    public enum TimedHazardState
    {
        Safe,
        Warning,
        Active
    }

    /// <summary>
    /// Base behavior for cyclic environmental hazards (shorted wires, gas leaks)
    /// that cycle Safe -> Warning -> Active and share one uncluttered HUD warning channel.
    /// </summary>
    [DisallowMultipleComponent]
    public abstract class Obstacle_BahayaBerkala : MonoBehaviour
    {
        private static readonly List<Obstacle_BahayaBerkala> ActiveInstances = new List<Obstacle_BahayaBerkala>();
        private static Obstacle_BahayaBerkala lastTriggeredHazard;
        private static float lastHitUiTime = -100f;
        private static float lastHitPenalty;

        [SerializeField, Min(0.1f)] private float hazardRadius = 1.45f;
        [SerializeField, Min(0.1f)] private float warningRadius = 4.2f;
        [SerializeField, Min(0.2f)] private float safeDuration = 2.1f;
        [SerializeField, Min(0.2f)] private float warningDuration = 0.85f;
        [SerializeField, Min(0.2f)] private float activeDuration = 1.35f;
        [SerializeField, Min(0f)] private float phaseOffset;
        [SerializeField, Min(0f)] private float timePenalty = 3f;
        [SerializeField, Range(0.1f, 1f)] private float shockSlowMultiplier = 0.30f;
        [SerializeField, Min(0.1f)] private float shockSlowDuration = 1.0f;
        [SerializeField, Min(0.1f)] private float shockCooldown = 1.7f;
        [SerializeField, Min(0f)] private float pushDistance = 0.55f;

        [SerializeField] private Renderer ringRenderer;
        [SerializeField] private GameObject sparkVisual;
        [SerializeField] private Light pulseLight;
        [SerializeField] private Material safeMaterial;
        [SerializeField] private Material warningMaterial;
        [SerializeField] private Material activeMaterial;

        private Mission_PengantaranBantuan player;
        private Player_Gerak movement;
        private float elapsedCycleTime;
        private float cooldownRemaining;
        private bool playerNearDanger;

        public TimedHazardState CurrentState
        {
            get
            {
                float total = safeDuration + warningDuration + activeDuration;
                if (total <= 0.001f) return TimedHazardState.Safe;
                float t = Mathf.Repeat(elapsedCycleTime, total);
                if (t < safeDuration) return TimedHazardState.Safe;
                return t < safeDuration + warningDuration ? TimedHazardState.Warning : TimedHazardState.Active;
            }
        }

        public float HazardRadius => hazardRadius;
        public bool IsDangerWindow => CurrentState != TimedHazardState.Safe;

        protected virtual Color WarningLightColor => new Color(1.0f, 0.72f, 0.16f);
        protected virtual Color ActiveLightColor => new Color(0.28f, 0.90f, 1.0f);
        protected virtual Color ScreenRimColor => new Color(0.22f, 0.84f, 1.0f);
        protected virtual Color HudAccentColor => UI_TemaVisual.Gold;
        protected virtual string NearWarningText => "Awas! Bahaya berkala di depan — tunggu reda";
        protected virtual string HitTitleFormat => "-{0:0} DETIK! KENA BAHAYA!";
        protected virtual string HitSubtitleText => "Tunggu fase aman sebelum melintas.";

        protected virtual void AnimateActiveVisual(Transform visualTransform, float cycleTime)
        {
            visualTransform.localRotation = Quaternion.Euler(0f, cycleTime * 280f, 0f);
        }

        private void Awake()
        {
            elapsedCycleTime = phaseOffset;
            player = FindFirstObjectByType<Mission_PengantaranBantuan>();
            if (player != null)
            {
                movement = player.GetComponent<Player_Gerak>();
                player.MissionReset += ResetHazard;
            }
            ApplyVisualState(CurrentState);
        }

        private void OnEnable()
        {
            if (!ActiveInstances.Contains(this))
                ActiveInstances.Add(this);
        }

        private void OnDisable()
        {
            ActiveInstances.Remove(this);
            playerNearDanger = false;
            if (lastTriggeredHazard == this)
                lastTriggeredHazard = null;
        }

        private void OnDestroy()
        {
            if (player != null)
                player.MissionReset -= ResetHazard;
        }

        private void Update()
        {
            playerNearDanger = false;
            if (player == null || !player.isActiveAndEnabled || !player.IsMissionActive || player.IsMissionEnded
                || player.IsDialogueOpen || StoryDialog_InteraksiNPC.AnyDialogueOpen || UI_MenuSesi.InputBlocked || Time.timeScale <= 0f)
                return;

            elapsedCycleTime += Time.deltaTime;
            cooldownRemaining = Mathf.Max(0f, cooldownRemaining - Time.deltaTime);

            TimedHazardState state = CurrentState;
            ApplyVisualState(state);

            Vector3 offset = player.transform.position - transform.position;
            offset.y = 0f;
            float sqrDistance = offset.sqrMagnitude;
            playerNearDanger = state != TimedHazardState.Safe && sqrDistance <= warningRadius * warningRadius;

            if (state == TimedHazardState.Active && cooldownRemaining <= 0f && sqrDistance <= hazardRadius * hazardRadius)
                TriggerPenalty(offset);
        }

        private void TriggerPenalty(Vector3 offsetFromCenter)
        {
            cooldownRemaining = shockCooldown;
            if (player == null) return;

            player.SetTimeRemaining(player.TimeRemaining - timePenalty);
            if (movement != null)
            {
                Vector3 pushDir = offsetFromCenter.sqrMagnitude > 0.001f ? offsetFromCenter.normalized : -player.transform.forward;
                movement.ApplyShockEffect(shockSlowMultiplier, shockSlowDuration, pushDir * pushDistance);
            }

            lastTriggeredHazard = this;
            lastHitUiTime = Time.unscaledTime;
            lastHitPenalty = Mathf.Round(timePenalty);
        }

        private void ApplyVisualState(TimedHazardState state)
        {
            if (ringRenderer != null)
            {
                Material mat = state == TimedHazardState.Active
                    ? activeMaterial
                    : state == TimedHazardState.Warning ? warningMaterial : safeMaterial;
                if (mat != null && ringRenderer.sharedMaterial != mat)
                    ringRenderer.sharedMaterial = mat;
            }

            if (sparkVisual != null)
            {
                bool showActive = state == TimedHazardState.Active;
                if (sparkVisual.activeSelf != showActive)
                    sparkVisual.SetActive(showActive);
                if (showActive)
                    AnimateActiveVisual(sparkVisual.transform, elapsedCycleTime);
            }

            if (pulseLight != null)
            {
                if (state == TimedHazardState.Safe)
                {
                    pulseLight.enabled = false;
                }
                else if (state == TimedHazardState.Warning)
                {
                    pulseLight.enabled = ((int)(elapsedCycleTime * 10f) % 2) == 0;
                    pulseLight.color = WarningLightColor;
                    pulseLight.intensity = 18f;
                }
                else
                {
                    pulseLight.enabled = true;
                    pulseLight.color = ActiveLightColor;
                    pulseLight.intensity = 32f + 12f * Mathf.Sin(elapsedCycleTime * 28f);
                }
            }
        }

        private void ResetHazard()
        {
            elapsedCycleTime = phaseOffset;
            cooldownRemaining = 0f;
            playerNearDanger = false;
            lastHitUiTime = -100f;
            lastTriggeredHazard = null;
            ApplyVisualState(CurrentState);
        }

        private void OnGUI()
        {
            if (ActiveInstances.Count == 0 || ActiveInstances[0] != this) return;
            if (player == null || UI_MenuSesi.InputBlocked || player.IsDialogueOpen || player.IsOutcomeOpen) return;

            bool recentlyHit = Time.unscaledTime - lastHitUiTime < 1.8f && lastTriggeredHazard != null;
            Obstacle_BahayaBerkala nearHazard = null;
            for (int i = 0; i < ActiveInstances.Count; i++)
            {
                if (ActiveInstances[i] != null && ActiveInstances[i].playerNearDanger)
                {
                    nearHazard = ActiveInstances[i];
                    break;
                }
            }
            if (!recentlyHit && nearHazard == null) return;

            UI_TemaVisual.EnsureInitialized();
            GUI.depth = -58;
            Matrix4x4 old = GUI.matrix;
            float scale = UI_TemaVisual.Scale;
            GUI.matrix = old * Matrix4x4.Scale(new Vector3(scale, scale, 1));
            float sw = Screen.width / scale;
            float sh = Screen.height / scale;

            if (recentlyHit)
            {
                float alpha = 1f - (Time.unscaledTime - lastHitUiTime) / 1.8f;
                Color rim = lastTriggeredHazard.ScreenRimColor;
                rim.a = 0.35f * alpha;
                UI_TemaVisual.Fill(new Rect(0, 0, sw, 12), rim);
                UI_TemaVisual.Fill(new Rect(0, sh - 12, sw, 12), rim);
                UI_TemaVisual.Fill(new Rect(0, 0, 12, sh), rim);
                UI_TemaVisual.Fill(new Rect(sw - 12, 0, 12, sh), rim);

                Rect card = new Rect((sw - 440) / 2, 198, 440, 70);
                UI_TemaVisual.Card(card);
                GUIStyle big = new GUIStyle(UI_TemaVisual.Title)
                {
                    fontSize = 26,
                    alignment = TextAnchor.MiddleCenter
                };
                big.normal.textColor = lastTriggeredHazard.HudAccentColor;
                GUI.Label(new Rect(card.x, card.y + 6, card.width, 32), string.Format(lastTriggeredHazard.HitTitleFormat, lastHitPenalty), big);
                GUIStyle small = new GUIStyle(UI_TemaVisual.Small) { alignment = TextAnchor.MiddleCenter };
                GUI.Label(new Rect(card.x, card.y + 38, card.width, 20), lastTriggeredHazard.HitSubtitleText, small);
            }
            else
            {
                float pulse = 0.78f + Mathf.Sin(Time.unscaledTime * 8f) * 0.22f;
                Rect banner = new Rect((sw - 400) / 2, 100, 400, 48);
                Color prev = GUI.color;
                GUI.color = new Color(1, 1, 1, pulse);
                UI_TemaVisual.Card(banner);
                GUI.color = prev;
                UI_TemaVisual.Ikon(new Rect(banner.x + 16, banner.y + 13, 20, 20), "awas", nearHazard.HudAccentColor);
                GUIStyle text = new GUIStyle(UI_TemaVisual.LabelStyle);
                text.normal.textColor = nearHazard.HudAccentColor;
                GUI.Label(new Rect(banner.x + 46, banner.y + 5, banner.width - 58, 36), nearHazard.NearWarningText, text);
            }

            GUI.matrix = old;
        }
    }
}

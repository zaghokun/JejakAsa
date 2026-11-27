using System.Collections.Generic;
using UnityEngine;

namespace JejakAsa
{
    public enum ElectricHazardState
    {
        Safe,
        Warning,
        Active
    }

    [DisallowMultipleComponent]
    public sealed class Obstacle_KorsletingListrik : MonoBehaviour
    {
        private static readonly List<Obstacle_KorsletingListrik> ActiveInstances = new List<Obstacle_KorsletingListrik>();
        private static float lastShockUiTime = -100f;
        private static float lastShockPenalty;

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

        public ElectricHazardState CurrentState
        {
            get
            {
                float total = safeDuration + warningDuration + activeDuration;
                if (total <= 0.001f) return ElectricHazardState.Safe;
                float t = Mathf.Repeat(elapsedCycleTime, total);
                if (t < safeDuration) return ElectricHazardState.Safe;
                return t < safeDuration + warningDuration ? ElectricHazardState.Warning : ElectricHazardState.Active;
            }
        }

        public float HazardRadius => hazardRadius;
        public bool IsDangerWindow => CurrentState != ElectricHazardState.Safe;

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

            ElectricHazardState state = CurrentState;
            ApplyVisualState(state);

            Vector3 offset = player.transform.position - transform.position;
            offset.y = 0f;
            float sqrDistance = offset.sqrMagnitude;
            playerNearDanger = state != ElectricHazardState.Safe && sqrDistance <= warningRadius * warningRadius;

            if (state == ElectricHazardState.Active && cooldownRemaining <= 0f && sqrDistance <= hazardRadius * hazardRadius)
                TriggerShock(offset);
        }

        private void TriggerShock(Vector3 offsetFromCenter)
        {
            cooldownRemaining = shockCooldown;
            if (player == null) return;

            player.SetTimeRemaining(player.TimeRemaining - timePenalty);
            if (movement != null)
            {
                Vector3 pushDir = offsetFromCenter.sqrMagnitude > 0.001f ? offsetFromCenter.normalized : -player.transform.forward;
                movement.ApplyShockEffect(shockSlowMultiplier, shockSlowDuration, pushDir * pushDistance);
            }

            lastShockUiTime = Time.unscaledTime;
            lastShockPenalty = Mathf.Round(timePenalty);
        }

        private void ApplyVisualState(ElectricHazardState state)
        {
            if (ringRenderer != null)
            {
                Material mat = state == ElectricHazardState.Active
                    ? activeMaterial
                    : state == ElectricHazardState.Warning ? warningMaterial : safeMaterial;
                if (mat != null && ringRenderer.sharedMaterial != mat)
                    ringRenderer.sharedMaterial = mat;
            }

            if (sparkVisual != null)
            {
                bool showSparks = state == ElectricHazardState.Active;
                if (sparkVisual.activeSelf != showSparks)
                    sparkVisual.SetActive(showSparks);
                if (showSparks)
                    sparkVisual.transform.localRotation = Quaternion.Euler(0f, elapsedCycleTime * 280f, 0f);
            }

            if (pulseLight != null)
            {
                if (state == ElectricHazardState.Safe)
                {
                    pulseLight.enabled = false;
                }
                else if (state == ElectricHazardState.Warning)
                {
                    pulseLight.enabled = ((int)(elapsedCycleTime * 10f) % 2) == 0;
                    pulseLight.color = new Color(1.0f, 0.72f, 0.16f);
                    pulseLight.intensity = 18f;
                }
                else
                {
                    pulseLight.enabled = true;
                    pulseLight.color = new Color(0.28f, 0.90f, 1.0f);
                    pulseLight.intensity = 32f + 12f * Mathf.Sin(elapsedCycleTime * 28f);
                }
            }
        }

        private void ResetHazard()
        {
            elapsedCycleTime = phaseOffset;
            cooldownRemaining = 0f;
            playerNearDanger = false;
            lastShockUiTime = -100f;
            ApplyVisualState(CurrentState);
        }

        private void OnGUI()
        {
            if (ActiveInstances.Count == 0 || ActiveInstances[0] != this) return;
            if (player == null || UI_MenuSesi.InputBlocked || player.IsDialogueOpen || player.IsOutcomeOpen) return;

            bool shocked = Time.unscaledTime - lastShockUiTime < 1.8f;
            bool nearbyDanger = false;
            for (int i = 0; i < ActiveInstances.Count; i++)
            {
                if (ActiveInstances[i] != null && ActiveInstances[i].playerNearDanger)
                {
                    nearbyDanger = true;
                    break;
                }
            }
            if (!shocked && !nearbyDanger) return;

            UI_TemaVisual.EnsureInitialized();
            GUI.depth = -58;
            Matrix4x4 old = GUI.matrix;
            float scale = UI_TemaVisual.Scale;
            GUI.matrix = old * Matrix4x4.Scale(new Vector3(scale, scale, 1));
            float sw = Screen.width / scale;
            float sh = Screen.height / scale;

            if (shocked)
            {
                float alpha = 1f - (Time.unscaledTime - lastShockUiTime) / 1.8f;
                Color electricRim = new Color(0.22f, 0.84f, 1.0f, 0.35f * alpha);
                UI_TemaVisual.Fill(new Rect(0, 0, sw, 12), electricRim);
                UI_TemaVisual.Fill(new Rect(0, sh - 12, sw, 12), electricRim);
                UI_TemaVisual.Fill(new Rect(0, 0, 12, sh), electricRim);
                UI_TemaVisual.Fill(new Rect(sw - 12, 0, 12, sh), electricRim);

                Rect card = new Rect((sw - 440) / 2, 198, 440, 70);
                UI_TemaVisual.Card(card);
                GUIStyle big = new GUIStyle(UI_TemaVisual.Title)
                {
                    fontSize = 26,
                    alignment = TextAnchor.MiddleCenter
                };
                big.normal.textColor = UI_TemaVisual.Gold;
                GUI.Label(new Rect(card.x, card.y + 6, card.width, 32), $"-{lastShockPenalty:0} DETIK! KORSLET!", big);
                GUIStyle small = new GUIStyle(UI_TemaVisual.Small) { alignment = TextAnchor.MiddleCenter };
                GUI.Label(new Rect(card.x, card.y + 38, card.width, 20), "Tunggu percikan listrik padam sebelum melintas.", small);
            }
            else
            {
                float pulse = 0.78f + Mathf.Sin(Time.unscaledTime * 8f) * 0.22f;
                Rect banner = new Rect((sw - 400) / 2, 100, 400, 48);
                Color prev = GUI.color;
                GUI.color = new Color(1, 1, 1, pulse);
                UI_TemaVisual.Card(banner);
                GUI.color = prev;
                UI_TemaVisual.Ikon(new Rect(banner.x + 16, banner.y + 13, 20, 20), "awas", UI_TemaVisual.Gold);
                GUIStyle text = new GUIStyle(UI_TemaVisual.LabelStyle);
                text.normal.textColor = UI_TemaVisual.Gold;
                GUI.Label(new Rect(banner.x + 46, banner.y + 5, banner.width - 58, 36), "Awas! Kabel korslet — tunggu padam", text);
            }

            GUI.matrix = old;
        }
    }
}

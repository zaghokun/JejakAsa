using UnityEngine;

namespace JejakAsa
{
    public sealed class Obstacle_KebocoranGas : Obstacle_BahayaBerkala
    {
        protected override Color WarningLightColor => new Color(0.95f, 0.65f, 0.18f);
        protected override Color ActiveLightColor => new Color(0.32f, 0.92f, 0.42f);
        protected override Color ScreenRimColor => new Color(0.28f, 0.82f, 0.36f);
        protected override Color HudAccentColor => UI_TemaVisual.Clay;
        protected override string NearWarningText => "Awas! Tabung gas bocor — tunggu reda";
        protected override string HitTitleFormat => "-{0:0} DETIK! SESAK GAS!";
        protected override string HitSubtitleText => "Tunggu semburan tabung gas berhenti sebelum melintas.";

        protected override void AnimateActiveVisual(Transform visualTransform, float cycleTime)
        {
            float pulse = 0.88f + 0.18f * Mathf.Sin(cycleTime * 14f);
            visualTransform.localScale = new Vector3(pulse, 0.85f + 0.25f * Mathf.Abs(Mathf.Sin(cycleTime * 9f)), pulse);
            visualTransform.localRotation = Quaternion.Euler(0f, cycleTime * 95f, 0f);
        }
    }
}

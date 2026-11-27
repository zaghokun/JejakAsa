using UnityEngine;

namespace JejakAsa
{
    public sealed class Obstacle_KorsletingListrik : Obstacle_BahayaBerkala
    {
        protected override Color WarningLightColor => new Color(1.0f, 0.72f, 0.16f);
        protected override Color ActiveLightColor => new Color(0.28f, 0.90f, 1.0f);
        protected override Color ScreenRimColor => new Color(0.22f, 0.84f, 1.0f);
        protected override Color HudAccentColor => UI_TemaVisual.Gold;
        protected override string NearWarningText => "Awas! Kabel korslet — tunggu padam";
        protected override string HitTitleFormat => "-{0:0} DETIK! KORSLET!";
        protected override string HitSubtitleText => "Tunggu percikan listrik padam sebelum melintas.";
    }
}

using UnityEngine;

namespace JejakAsa
{
    [DisallowMultipleComponent]
    public sealed class UI_TujuanDanProgres : MonoBehaviour
    {
        private Mission_PengantaranBantuan interaction;
        private string objective;
        private float changedAt;

        private void Awake()
        {
            interaction = GetComponent<Mission_PengantaranBantuan>();
        }

        private void Update()
        {
            string next = GetObjective(StoryFlow_PengaturAlur.Instance.Progress);
            if (next != objective)
            {
                objective = next;
                changedAt = Time.unscaledTime;
            }
        }

        internal static string GetObjective(GameProgressData progress)
        {
            if (P03_SponsorDanPosko.GetStage(progress) != PoskoIntroStage.None) return P03_SponsorDanPosko.Objective(progress);
            switch (progress.currentPhase)
            {
                case DayPhase.Prolog: return "Bicara dengan Ibu";
                case DayPhase.RumahPagi: return "Pergi ke posko";
                case DayPhase.PoskoBriefing:
                    if (!progress.isBriefingDone) return "Temui Pak Jonathan";
                    return progress.hasPickedUpBansos ? "Berangkat ke wilayah" : "Ambil paket bantuan";
                case DayPhase.DiLevel: return "Antarkan bantuan ke warga";
                case DayPhase.PoskoLaporan:
                    if (!progress.isReportDone) return "Laporkan hasil ke Pak Jonathan";
                    return progress.isOpheliaTalkDone ? "Pulang ke rumah" : "Bicara dengan Ophelia";
                case DayPhase.RumahMalam: return "Istirahat di rumah";
                default: return "Perjalanan selesai";
            }
        }

        private void OnGUI()
        {
            if (UI_MenuSesi.InputBlocked || Interaction_Rumah.AnyMenuOpen || Interaction_GerbangPosko.AnyMenuOpen || StoryDialog_InteraksiNPC.AnyDialogueOpen ||
                (interaction != null && (interaction.IsDialogueOpen || interaction.IsOutcomeOpen))) return;
            UI_TemaVisual.EnsureInitialized();
            Matrix4x4 old = GUI.matrix;
            float scale = UI_TemaVisual.Scale;
            GUI.matrix = old * Matrix4x4.Scale(new Vector3(scale, scale, 1));
            float sw = Screen.width / scale, sh = Screen.height / scale;
            var progress = StoryFlow_PengaturAlur.Instance.Progress;
            bool inLevel = progress.currentPhase == DayPhase.DiLevel && interaction != null;
            GUI.depth = -50;
            float entrance = 10 * (1 - Mathf.SmoothStep(0, 1, Mathf.Clamp01((Time.unscaledTime - changedAt) / .22f)));
            Rect r = new Rect(28 - entrance, 28, 354, inLevel ? 188 : 132);
            UI_TemaVisual.Card(r);
            UI_TemaVisual.Fill(new Rect(r.x, r.y + 18, 4, r.height - 36), UI_TemaVisual.Clay);
            GUI.Label(new Rect(r.x + 24, r.y + 18, 306, 20), inLevel ? "MISI PENGANTARAN" : "CATATAN RELAWAN / TUJUAN", UI_TemaVisual.Eyebrow);
            GUI.Label(new Rect(r.x + 24, r.y + 50, 306, 60), objective ?? GetObjective(progress), UI_TemaVisual.LabelStyle);
            if (inLevel)
            {
                UI_TemaVisual.Rule(r.x + 24, r.y + 116, 306);
                UI_TemaVisual.Ikon(new Rect(r.x + 24, r.y + 134, 18, 18), "paket", UI_TemaVisual.Clay); // ikon paket
                GUI.Label(new Rect(r.x + 48, r.y + 133, 282, 22), "Terantar " + interaction.DeliveredCount + " / 3     ·     Bekal " + interaction.PackagesRemaining + " paket", UI_TemaVisual.Small);
                for (int i = 0; i < 3; i++) UI_TemaVisual.Fill(new Rect(r.x + 24 + i * 104, r.y + 165, 96, 5), i < interaction.DeliveredCount ? UI_TemaVisual.Green : UI_TemaVisual.Linen);
            }
            Rect day = new Rect(sw - 250, 28, 222, 84);
            UI_TemaVisual.Card(day);
            GUI.Label(new Rect(day.x + 20, day.y + 12, 180, 20), "HARI " + progress.currentDay + " / 5", UI_TemaVisual.Eyebrow);
            GUI.Label(new Rect(day.x + 20, day.y + 38, 184, 28), inLevel ? "Wilayah " + progress.currentLevel : UI_TemaVisual.PhaseName(progress.currentPhase), UI_TemaVisual.Body);
            if (inLevel && interaction.IsMissionActive)
            {
                Rect timer = new Rect(sw - 250, 124, 222, 104);
                UI_TemaVisual.Card(timer);
                bool urgent = interaction.TimeRemaining <= 30;
                UI_TemaVisual.Ikon(new Rect(timer.x + 20, timer.y + 15, 18, 18), "timer", urgent ? UI_TemaVisual.Clay : UI_TemaVisual.Green); // ikon timer
                GUI.Label(new Rect(timer.x + 44, timer.y + 14, 160, 20), urgent ? "SEGERA SELESAIKAN" : "SISA WAKTU", UI_TemaVisual.Eyebrow);
                GUIStyle timerStyle = urgent ? dangerTime : UI_TemaVisual.TimerStyle;
                if (timerStyle == null) timerStyle = dangerTime = UI_TemaVisual.Text(30, UI_TemaVisual.Clay, true, TextAnchor.MiddleCenter);
                GUI.Label(new Rect(timer.x + 20, timer.y + 40, 182, 48), interaction.FormattedTimeRemaining, timerStyle);
            }
            GUI.Label(new Rect(28, sh - 34, 500, 20), "WASD  Bergerak     Shift  Berlari     E  Interaksi", UI_TemaVisual.Small);
            GUI.Label(new Rect(sw - 128, sh - 34, 100, 20), "Esc  Jeda", UI_TemaVisual.Small);
            GUI.matrix = old;
        }
        private GUIStyle dangerTime;
    }
}

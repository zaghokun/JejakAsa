using System;
using System.Collections.Generic;
using UnityEngine;

namespace JejakAsa
{
    /// <summary>
    /// Fase alur permainan dalam satu siklus harian (R01 / AB-D01).
    /// </summary>
    public enum DayPhase
    {
        Prolog,         // Pengenalan cerita awal (hari 1)
        RumahPagi,      // Di dalam rumah (pagi), bersiap menuju ke posko
        PoskoBriefing,  // Di posko, menunggu briefing Jonathan & dialog Ophelia
        DiLevel,        // Sedang menjalankan misi level aktif (Level 1–5)
        PoskoLaporan,   // Kembali ke posko setelah misi, melapor ke Jonathan & Ophelia
        RumahMalam,     // Di dalam rumah (malam), save game dan istirahat tidur
        Tamat           // Selesai seluruh level 5 dan penutup cerita
    }

    /// <summary>
    /// Model data progres permainan yang dipersistensikan lintas scene dan save slot (R01 / R13).
    /// </summary>
    [Serializable]
    public class GameProgressData
    {
        public int currentDay = 1;
        public int currentLevel = 1;
        public DayPhase currentPhase = DayPhase.RumahPagi;

        // Flag aktivitas harian
        public bool isBriefingDone;
        public bool hasPickedUpBansos;
        public bool isLevelCompleted;
        public bool isReportDone;
        public bool isOpheliaTalkDone;
        public bool hasClaimedReward;

        // Progres inventori dan level
        public List<string> inventory = new List<string>();
        public string equippedWeapon = "";
        public List<int> completedLevels = new List<int>();

        public GameProgressData Clone()
        {
            GameProgressData copy = (GameProgressData)MemberwiseClone();
            copy.inventory = new List<string>(inventory);
            copy.completedLevels = new List<int>(completedLevels);
            return copy;
        }

        public void ResetDailyFlags()
        {
            isBriefingDone = false;
            hasPickedUpBansos = false;
            isLevelCompleted = false;
            isReportDone = false;
            isOpheliaTalkDone = false;
            hasClaimedReward = false;
        }
    }

    /// <summary>
    /// Metadata ringkas untuk slot penyimpanan (Save Slot 1..3).
    /// </summary>
    [Serializable]
    public struct SaveSlotMeta
    {
        public int slotIndex;
        public bool exists;
        public int day;
        public int level;
        public string phaseName;
        public string timestamp;
    }
}


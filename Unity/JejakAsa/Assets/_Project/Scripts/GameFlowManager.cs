using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace JejakAsa
{
    /// <summary>
    /// Pengelola alur siklus harian, validasi gerbang, dan persistensi progres antar scene (R01 / AB-D01).
    /// </summary>
    [DisallowMultipleComponent]
    [DefaultExecutionOrder(-900)]
    public class GameFlowManager : MonoBehaviour
    {
        private const string SaveKeyPrefix = "JejakAsa_SaveSlot_";
        public const int MaxSaveSlots = 3;

        private static GameFlowManager instance;
        public static GameFlowManager Instance
        {
            get
            {
                if (instance == null)
                {
                    GameObject go = new GameObject("GameFlowManager");
                    instance = go.AddComponent<GameFlowManager>();
                    if (Application.isPlaying)
                        DontDestroyOnLoad(go);
                }
                return instance;
            }
        }

        [SerializeField]
        private GameProgressData data = new GameProgressData();

        public GameProgressData Progress => data;
        public DayPhase CurrentPhase => data.currentPhase;
        public int CurrentDay => data.currentDay;
        public int CurrentLevel => data.currentLevel;

        /// <summary>
        /// Nama titik spawn tujuan saat berpindah scene (misal: "RumahVicky").
        /// </summary>
        public string LastSpawnPoint { get; set; }

        public event Action<DayPhase> OnPhaseChanged;
        public event Action<int> OnDayAdvanced;

        private void Awake()
        {
            if (instance == null)
            {
                instance = this;
                if (transform.parent == null && Application.isPlaying)
                    DontDestroyOnLoad(gameObject);
            }
            else if (instance != this)
            {
                Destroy(gameObject);
            }
        }

        /// <summary>
        /// Memulai sesi petualangan baru dari Hari 1.
        /// </summary>
        public void StartNewCampaign(bool startAtProlog = false)
        {
            data = new GameProgressData
            {
                currentDay = 1,
                currentLevel = 1,
                currentPhase = startAtProlog ? DayPhase.Prolog : DayPhase.RumahPagi
            };
            OnPhaseChanged?.Invoke(data.currentPhase);
        }

        /// <summary>
        /// Mengatur state kustom secara langsung (berguna untuk pengujian unit deterministik).
        /// </summary>
        public void SetStateForTesting(GameProgressData customData)
        {
            data = customData != null ? customData.Clone() : new GameProgressData();
            OnPhaseChanged?.Invoke(data.currentPhase);
        }

        // ─── Navigasi Rumah -> Posko ─────────────────────────────────────

        public bool CanGoToPoskoFromHome(out string reason)
        {
            if (data.currentPhase == DayPhase.RumahMalam)
            {
                reason = "Sudah larut malam. Istirahatlah di tempat tidur sebelum kembali ke posko besok pagi.";
                return false;
            }
            if (data.currentPhase == DayPhase.Tamat)
            {
                reason = "Kelima level sudah selesai. Kembali ke menu utama melalui jeda.";
                return false;
            }

            reason = string.Empty;
            return true;
        }

        public bool GoToPoskoFromHome()
        {
            if (!CanGoToPoskoFromHome(out _))
                return false;

            if (data.currentPhase == DayPhase.Prolog || data.currentPhase == DayPhase.RumahPagi)
            {
                data.currentPhase = DayPhase.PoskoBriefing;
                OnPhaseChanged?.Invoke(data.currentPhase);
            }
            return true;
        }

        // ─── Aktivitas di Posko (Pagi: Briefing Jonathan & Ophelia) ─────

        public bool CompleteBriefing()
        {
            if (data.currentPhase != DayPhase.PoskoBriefing)
                return false;

            data.isBriefingDone = true;
            return true;
        }

        // ─── Navigasi Posko -> Map Level ────────────────────────────────

        public bool CanDepartToLevel(out string reason)
        {
            if (data.currentPhase != DayPhase.PoskoBriefing)
            {
                reason = "Bukan waktu keberangkatan misi level.";
                return false;
            }

            if (!data.isBriefingDone)
            {
                reason = "Bicaralah dengan Pak Jonathan terlebih dahulu untuk mendapatkan briefing misi hari ini!";
                return false;
            }

            if (!data.hasPickedUpBansos)
            {
                reason = "Ambil paket bansos di posko sebelum berangkat.";
                return false;
            }

            reason = string.Empty;
            return true;
        }

        public bool DepartToLevel()
        {
            if (!CanDepartToLevel(out _))
                return false;

            data.currentPhase = DayPhase.DiLevel;
            OnPhaseChanged?.Invoke(data.currentPhase);
            return true;
        }

        // ─── Hasil Level & Retry ────────────────────────────────────────

        public bool OnLevelSuccess()
        {
            if (data.currentPhase != DayPhase.DiLevel)
                return false;

            data.isLevelCompleted = true;
            if (!data.completedLevels.Contains(data.currentLevel))
                data.completedLevels.Add(data.currentLevel);

            data.currentPhase = DayPhase.PoskoLaporan;
            OnPhaseChanged?.Invoke(data.currentPhase);
            return true;
        }

        public bool OnLevelFailureRetry()
        {
            if (data.currentPhase != DayPhase.DiLevel)
                return false;

            // Retry mempertahankan level aktif dan progres hari sebelumnya
            data.isLevelCompleted = false;
            return true;
        }

        // ─── Aktivitas di Posko (Sore: Laporan & Ophelia & Reward) ───────

        public bool CompleteReport()
        {
            if (data.currentPhase != DayPhase.PoskoLaporan)
                return false;

            data.isReportDone = true;
            return true;
        }

        public bool CompleteOpheliaTalk()
        {
            if (data.currentPhase != DayPhase.PoskoLaporan)
                return false;

            data.isOpheliaTalkDone = true;
            return true;
        }

        public bool ClaimLevelReward(string rewardId)
        {
            if (data.currentPhase != DayPhase.PoskoLaporan || data.hasClaimedReward)
                return false;

            if (!data.isReportDone || !data.isOpheliaTalkDone)
                return false;

            if (!string.IsNullOrEmpty(rewardId))
                data.inventory.Add(rewardId);
            data.inventory.Add(data.currentLevel == 2 ? BattleController.BasicWeaponId : BattleController.HealingItemId);

            data.hasClaimedReward = true;
            return true;
        }

        // ─── Navigasi Posko -> Rumah (Pulang Malam) ──────────────────────

        public bool CanGoHomeFromPosko(out string reason)
        {
            if (data.currentPhase == DayPhase.PoskoBriefing)
            {
                reason = "Kamu belum menyelesaikan tugas penyaluran bansos hari ini.";
                return false;
            }

            if (data.currentPhase == DayPhase.PoskoLaporan)
            {
                if (!data.isReportDone)
                {
                    reason = "Lapor ke Pak Jonathan sebelum meninggalkan posko!";
                    return false;
                }
                if (!data.isOpheliaTalkDone)
                {
                    reason = "Bicaralah dengan Ophelia sebelum pulang!";
                    return false;
                }

                reason = string.Empty;
                return true;
            }

            reason = "Tidak dapat pulang ke rumah dari kondisi saat ini.";
            return false;
        }

        public bool GoHomeFromPosko()
        {
            if (!CanGoHomeFromPosko(out _))
                return false;

            data.currentPhase = DayPhase.RumahMalam;
            OnPhaseChanged?.Invoke(data.currentPhase);
            return true;
        }

        // ─── Istirahat Tidur di Rumah & Memajukan Hari ───────────────────

        public bool CanRestAndAdvanceDay(out string reason)
        {
            if (data.currentPhase != DayPhase.RumahMalam)
            {
                reason = "Belum saatnya tidur dan mengakhiri hari.";
                return false;
            }

            reason = string.Empty;
            return true;
        }

        public bool RestAndAdvanceDay()
        {
            if (!CanRestAndAdvanceDay(out _))
                return false;

            if (data.currentDay >= 5)
            {
                data.currentPhase = DayPhase.Tamat;
                OnPhaseChanged?.Invoke(data.currentPhase);
                return true;
            }

            data.currentDay++;
            data.currentLevel = data.currentDay;
            data.ResetDailyFlags();
            data.currentPhase = DayPhase.RumahPagi;

            OnDayAdvanced?.Invoke(data.currentDay);
            OnPhaseChanged?.Invoke(data.currentPhase);
            return true;
        }

        // ─── Sistem Save & Load (3 Slot Manual di Rumah) ─────────────────

        public bool SaveGame(int slotIndex)
        {
            if (slotIndex < 1 || slotIndex > MaxSaveSlots)
                return false;

            string json = JsonUtility.ToJson(data);
            PlayerPrefs.SetString(SaveKeyPrefix + slotIndex, json);
            PlayerPrefs.SetString(SaveKeyPrefix + slotIndex + "_Time", DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"));
            PlayerPrefs.Save();
            return true;
        }

        public bool LoadGame(int slotIndex)
        {
            if (!TryReadSave(slotIndex, out GameProgressData loaded))
                return false;

            data = loaded;
            OnPhaseChanged?.Invoke(data.currentPhase);
            return true;
        }
        internal static bool TryReadSave(int slotIndex, out GameProgressData loaded)
        {
            loaded = null;
            if (slotIndex < 1 || slotIndex > MaxSaveSlots || !HasSave(slotIndex))
                return false;
            string json = PlayerPrefs.GetString(SaveKeyPrefix + slotIndex);
            string[] required = { "currentDay", "currentLevel", "currentPhase", "isBriefingDone", "hasPickedUpBansos",
                "isLevelCompleted", "isReportDone", "isOpheliaTalkDone", "hasClaimedReward", "inventory", "completedLevels" };
            foreach (string field in required)
                if (!json.Contains("\"" + field + "\""))
                    return false;
            try
            {
                GameProgressData candidate = JsonUtility.FromJson<GameProgressData>(json);
                if (candidate == null || candidate.currentDay < 1 || candidate.currentDay > 5 ||
                    candidate.currentLevel < 1 || candidate.currentLevel > 5 ||
                    !Enum.IsDefined(typeof(DayPhase), candidate.currentPhase) ||
                    candidate.inventory == null || candidate.completedLevels == null)
                    return false;
                loaded = candidate;
                return true;
            }
            catch (ArgumentException)
            {
                return false;
            }
        }


        public static bool HasSave(int slotIndex)
        {
            return PlayerPrefs.HasKey(SaveKeyPrefix + slotIndex);
        }

        public static SaveSlotMeta GetSlotMeta(int slotIndex)
        {
            SaveSlotMeta meta = new SaveSlotMeta
            {
                slotIndex = slotIndex,
                exists = HasSave(slotIndex)
            };

            if (meta.exists)
            {
                if (TryReadSave(slotIndex, out GameProgressData d))
                {
                    meta.day = d.currentDay;
                    meta.level = d.currentLevel;
                    meta.phaseName = d.currentPhase.ToString();
                }
                meta.timestamp = PlayerPrefs.GetString(SaveKeyPrefix + slotIndex + "_Time", "-");
            }
            return meta;
        }

        public static void DeleteSave(int slotIndex)
        {
            PlayerPrefs.DeleteKey(SaveKeyPrefix + slotIndex);
            PlayerPrefs.DeleteKey(SaveKeyPrefix + slotIndex + "_Time");
            PlayerPrefs.Save();
        }
    }
}


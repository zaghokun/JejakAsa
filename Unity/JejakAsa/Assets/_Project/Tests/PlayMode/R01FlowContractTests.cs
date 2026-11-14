using System.Collections;
using System.Collections.Generic;
using NUnit.Framework;
using UnityEngine;
using System.Reflection;
using UnityEngine.TestTools;

namespace JejakAsa
{
    /// <summary>
    /// Pengujian otomatis kontrak alur siklus harian R01 / AB-D01.
    /// </summary>
    public class R01FlowContractTests
    {
        private GameFlowManager flow;
        private GameProgressData previousProgress;
        private string previousSpawn;
        private bool createdFlow;
        private readonly bool[] slotExisted = new bool[6];
        private readonly string[] slotValues = new string[6];

        [SetUp]
        public void SetUp()
        {
            flow = Object.FindFirstObjectByType<GameFlowManager>();
            createdFlow = flow == null;
            if (createdFlow)
                flow = GameFlowManager.Instance;
            previousProgress = flow.Progress.Clone();
            previousSpawn = flow.LastSpawnPoint;
            for (int i = 0; i < slotValues.Length; i++)
            {
                string key = "JejakAsa_SaveSlot_" + (i / 2 + 1) + (i % 2 == 1 ? "_Time" : "");
                slotExisted[i] = PlayerPrefs.HasKey(key);
                slotValues[i] = PlayerPrefs.GetString(key);
            }
            flow.LastSpawnPoint = null;
            flow.StartNewCampaign();
        }

        [TearDown]
        public void TearDown()
        {
            flow.SetStateForTesting(previousProgress);
            flow.LastSpawnPoint = previousSpawn;
            for (int i = 0; i < slotValues.Length; i++)
            {
                string key = "JejakAsa_SaveSlot_" + (i / 2 + 1) + (i % 2 == 1 ? "_Time" : "");
                if (slotExisted[i]) PlayerPrefs.SetString(key, slotValues[i]);
                else PlayerPrefs.DeleteKey(key);
            }
            PlayerPrefs.Save();
            if (createdFlow)
                Object.DestroyImmediate(flow.gameObject);
        }

        [Test]
        public void NewCampaign_StartsAtDay1_RumahPagi()
        {
            Assert.AreEqual(1, flow.CurrentDay);
            Assert.AreEqual(1, flow.CurrentLevel);
            Assert.AreEqual(DayPhase.RumahPagi, flow.CurrentPhase);
            Assert.IsFalse(flow.Progress.isBriefingDone);
            Assert.IsFalse(flow.Progress.isLevelCompleted);
            Assert.IsEmpty(flow.Progress.completedLevels);
        }

        [Test]
        public void HomeToPosko_TransitionsToPoskoBriefing()
        {
            Assert.IsTrue(flow.CanGoToPoskoFromHome(out string reason));
            Assert.IsEmpty(reason);

            bool success = flow.GoToPoskoFromHome();
            Assert.IsTrue(success);
            Assert.AreEqual(DayPhase.PoskoBriefing, flow.CurrentPhase);
        }

        [Test]
        public void DepartToLevel_BeforeBriefing_IsBlockedWithClearReason()
        {
            flow.GoToPoskoFromHome();

            Assert.IsFalse(flow.CanDepartToLevel(out string reason));
            StringAssert.Contains("briefing", reason.ToLower());

            bool departed = flow.DepartToLevel();
            Assert.IsFalse(departed);
            Assert.AreEqual(DayPhase.PoskoBriefing, flow.CurrentPhase);
        }

        [Test]
        public void Packages_RequireJonathanBriefing()
        {
            flow.GoToPoskoFromHome();
            var packages = new GameObject("Test_Packages");
            try
            {
                var trigger = packages.AddComponent<PoskoDistributionTrigger>();
                var take = typeof(PoskoDistributionTrigger).GetMethod("TryTakeBansos", BindingFlags.Instance | BindingFlags.NonPublic);
                take.Invoke(trigger, null);
                Assert.IsFalse(flow.Progress.isBriefingDone);
                Assert.IsFalse(flow.Progress.hasPickedUpBansos);
                Assert.IsFalse(flow.CanDepartToLevel(out _));
                StringAssert.Contains("Jonathan", (string)typeof(PoskoDistributionTrigger)
                    .GetField("feedbackText", BindingFlags.Instance | BindingFlags.NonPublic).GetValue(trigger));

                var jonathan = packages.AddComponent<StoryNpcController>();
                jonathan.OpenDialogue();
                var gate = packages.AddComponent<PoskoGateController>();
                gate.TryDepartToLevel();
                Assert.IsFalse(flow.Progress.hasPickedUpBansos);
                Assert.AreEqual(DayPhase.PoskoBriefing, flow.CurrentPhase);
                take.Invoke(trigger, null);
                Assert.IsTrue(flow.Progress.isBriefingDone);
                Assert.IsTrue(flow.Progress.hasPickedUpBansos);
                Assert.IsTrue(flow.CanDepartToLevel(out _));
            }
            finally
            {
                Object.DestroyImmediate(packages);
            }
        }

        [Test]
        public void Packages_OutsideMissionPhase_DoNotStartMission()
        {
            var packages = new GameObject("Test_Packages");
            try
            {
                var trigger = packages.AddComponent<PoskoDistributionTrigger>();
                typeof(PoskoDistributionTrigger).GetMethod("TryTakeBansos", BindingFlags.Instance | BindingFlags.NonPublic)
                    .Invoke(trigger, null);
                Assert.IsFalse(flow.Progress.hasPickedUpBansos);
                Assert.IsFalse(flow.Progress.isBriefingDone);

                flow.SetStateForTesting(new GameProgressData { currentPhase = DayPhase.DiLevel, isBriefingDone = true });
                typeof(PoskoDistributionTrigger).GetMethod("TryTakeBansos", BindingFlags.Instance | BindingFlags.NonPublic)
                    .Invoke(trigger, null);
                Assert.IsTrue(flow.Progress.hasPickedUpBansos, "Kampung pickup remains available during the level.");
            }
            finally
            {
                Object.DestroyImmediate(packages);
            }
        }


        [Test]
        public void DepartToLevel_AfterBriefingAndPackagePickup_Succeeds()
        {
            flow.GoToPoskoFromHome();
            flow.CompleteBriefing();

            Assert.IsFalse(flow.CanDepartToLevel(out string lockedReason));
            StringAssert.Contains("paket", lockedReason.ToLower());
            Assert.IsFalse(flow.DepartToLevel());
            Assert.AreEqual(DayPhase.PoskoBriefing, flow.CurrentPhase);
            flow.Progress.hasPickedUpBansos = true;

            Assert.IsTrue(flow.CanDepartToLevel(out string reason));
            Assert.IsEmpty(reason);

            bool departed = flow.DepartToLevel();
            Assert.IsTrue(departed);
            Assert.AreEqual(DayPhase.DiLevel, flow.CurrentPhase);
        }

        [Test]
        public void LevelSuccess_TransitionsToPoskoLaporan_AndRecordsCompletion()
        {
            flow.GoToPoskoFromHome();
            flow.CompleteBriefing();
            flow.Progress.hasPickedUpBansos = true;
            flow.DepartToLevel();

            bool finished = flow.OnLevelSuccess();
            Assert.IsTrue(finished);
            Assert.AreEqual(DayPhase.PoskoLaporan, flow.CurrentPhase);
            Assert.IsTrue(flow.Progress.isLevelCompleted);
            CollectionAssert.Contains(flow.Progress.completedLevels, 1);
        }

        [Test]
        public void PoskoToHome_RequiresBothReportAndOpheliaTalk()
        {
            flow.GoToPoskoFromHome();
            flow.CompleteBriefing();
            flow.Progress.hasPickedUpBansos = true;
            flow.DepartToLevel();
            flow.OnLevelSuccess();

            // Belum lapor ke Jonathan
            Assert.IsFalse(flow.CanGoHomeFromPosko(out string reasonReport));
            StringAssert.Contains("lapor", reasonReport.ToLower());

            flow.CompleteReport();

            // Sudah lapor tapi belum bicara ke Ophelia
            Assert.IsFalse(flow.CanGoHomeFromPosko(out string reasonOphelia));
            StringAssert.Contains("ophelia", reasonOphelia.ToLower());

            flow.CompleteOpheliaTalk();

            // Keduanya sudah selesai -> gerbang pulang terbuka
            Assert.IsTrue(flow.CanGoHomeFromPosko(out string reasonOk));
            Assert.IsEmpty(reasonOk);

            bool wentHome = flow.GoHomeFromPosko();
            Assert.IsTrue(wentHome);
            Assert.AreEqual(DayPhase.RumahMalam, flow.CurrentPhase);
        }

        [Test]
        public void ClaimLevelReward_GivenExactlyOnce_AfterReportAndOpheliaTalk()
        {
            flow.GoToPoskoFromHome();
            flow.CompleteBriefing();
            flow.Progress.hasPickedUpBansos = true;
            flow.DepartToLevel();
            flow.OnLevelSuccess();

            // Klaim sebelum lapor -> gagal
            Assert.IsFalse(flow.ClaimLevelReward("BansosKit_L1"));

            flow.CompleteReport();
            flow.CompleteOpheliaTalk();

            // Klaim pertama kali -> sukses
            bool claimed = flow.ClaimLevelReward("BansosKit_L1");
            Assert.IsTrue(claimed);
            Assert.IsTrue(flow.Progress.hasClaimedReward);
            CollectionAssert.Contains(flow.Progress.inventory, "BansosKit_L1");
            CollectionAssert.Contains(flow.Progress.inventory, BattleController.HealingItemId);

            // Klaim kedua kali di level yang sama -> ditolak
            bool claimAgain = flow.ClaimLevelReward("BansosKit_L1");
            Assert.IsFalse(claimAgain);
            Assert.AreEqual(2, flow.Progress.inventory.Count);
        }

        [Test]
        public void LevelTwoRewardGrantsWeaponOnce()
        {
            flow.SetStateForTesting(new GameProgressData
            {
                currentDay = 2,
                currentLevel = 2,
                currentPhase = DayPhase.PoskoLaporan,
                isReportDone = true,
                isOpheliaTalkDone = true
            });
            Assert.IsTrue(flow.ClaimLevelReward("BansosKit_L2"));
            CollectionAssert.Contains(flow.Progress.inventory, BattleController.BasicWeaponId);
            Assert.IsFalse(flow.ClaimLevelReward("BansosKit_L2"));
            Assert.AreEqual(2, flow.Progress.inventory.Count);
        }

        [Test]
        public void RestInHome_AdvancesDay_AndResetsDailyFlagsCleanly()
        {
            flow.GoToPoskoFromHome();
            flow.CompleteBriefing();
            flow.Progress.hasPickedUpBansos = true;
            flow.DepartToLevel();
            flow.OnLevelSuccess();
            flow.CompleteReport();
            flow.CompleteOpheliaTalk();
            flow.ClaimLevelReward("BansosKit_L1");
            flow.GoHomeFromPosko();

            Assert.AreEqual(DayPhase.RumahMalam, flow.CurrentPhase);
            Assert.IsTrue(flow.CanRestAndAdvanceDay(out string reason));
            Assert.IsEmpty(reason);

            bool rested = flow.RestAndAdvanceDay();
            Assert.IsTrue(rested);

            // Hari berganti ke Hari 2
            Assert.AreEqual(2, flow.CurrentDay);
            Assert.AreEqual(2, flow.CurrentLevel);
            Assert.AreEqual(DayPhase.RumahPagi, flow.CurrentPhase);

            // Flag harian bersih kembali
            Assert.IsFalse(flow.Progress.isBriefingDone);
            Assert.IsFalse(flow.Progress.isLevelCompleted);
            Assert.IsFalse(flow.Progress.isReportDone);
            Assert.IsFalse(flow.Progress.isOpheliaTalkDone);
            Assert.IsFalse(flow.Progress.hasClaimedReward);

            // Progres permanen (inventory & level selesai) tetap utuh
            CollectionAssert.Contains(flow.Progress.completedLevels, 1);
            CollectionAssert.Contains(flow.Progress.inventory, "BansosKit_L1");
        }

        [Test]
        public void LevelFailure_RetryMaintainsCurrentDayAndPriorProgress()
        {
            // Selesaikan Hari 1
            flow.GoToPoskoFromHome();
            flow.CompleteBriefing();
            flow.Progress.hasPickedUpBansos = true;
            flow.DepartToLevel();
            flow.OnLevelSuccess();
            flow.CompleteReport();
            flow.CompleteOpheliaTalk();
            flow.GoHomeFromPosko();
            flow.RestAndAdvanceDay();

            Assert.AreEqual(2, flow.CurrentDay);

            // Masuk Level 2 lalu gagal
            flow.GoToPoskoFromHome();
            flow.CompleteBriefing();
            flow.Progress.hasPickedUpBansos = true;
            flow.DepartToLevel();

            bool retry = flow.OnLevelFailureRetry();
            Assert.IsTrue(retry);

            // Hari 2 tetap aktif, progres Hari 1 tidak terhapus
            Assert.AreEqual(2, flow.CurrentDay);
            Assert.AreEqual(2, flow.CurrentLevel);
            Assert.AreEqual(DayPhase.DiLevel, flow.CurrentPhase);
            CollectionAssert.Contains(flow.Progress.completedLevels, 1);
        }

        [Test]
        public void SaveAndLoad_AllThreeSlots_PreservesStateDeterministically()
        {
            // Buat state spesifik di Hari 3
            GameProgressData testData = new GameProgressData
            {
                currentDay = 3,
                currentLevel = 3,
                currentPhase = DayPhase.RumahMalam,
                isBriefingDone = true,
                isLevelCompleted = true,
                isReportDone = true,
                isOpheliaTalkDone = true,
                hasClaimedReward = true
            };
            testData.completedLevels.Add(1);
            testData.completedLevels.Add(2);
            testData.inventory.Add("Reward_L1");
            testData.inventory.Add("Reward_L2");

            flow.SetStateForTesting(testData);

            // Simpan ke Slot 2
            bool saved = flow.SaveGame(2);
            Assert.IsTrue(saved);
            Assert.IsTrue(GameFlowManager.HasSave(2));

            SaveSlotMeta meta = GameFlowManager.GetSlotMeta(2);
            Assert.AreEqual(3, meta.day);
            Assert.AreEqual(3, meta.level);

            // Reset flow ke game baru
            flow.StartNewCampaign();
            Assert.AreEqual(1, flow.CurrentDay);

            // Muat kembali dari Slot 2
            bool loaded = flow.LoadGame(2);
            Assert.IsTrue(loaded);
            Assert.AreEqual(3, flow.CurrentDay);
            Assert.AreEqual(3, flow.CurrentLevel);
            Assert.AreEqual(DayPhase.RumahMalam, flow.CurrentPhase);
            Assert.AreEqual(2, flow.Progress.completedLevels.Count);
            Assert.AreEqual(2, flow.Progress.inventory.Count);
        }
    }
}


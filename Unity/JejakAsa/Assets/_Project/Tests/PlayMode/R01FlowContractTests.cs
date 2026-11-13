using System.Collections;
using System.Collections.Generic;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;

namespace JejakAsa
{
    /// <summary>
    /// Pengujian otomatis kontrak alur siklus harian R01 / AB-D01.
    /// </summary>
    public class R01FlowContractTests
    {
        private GameObject flowManagerGo;
        private GameFlowManager flow;

        [SetUp]
        public void SetUp()
        {
            flowManagerGo = new GameObject("Test_GameFlowManager");
            flow = flowManagerGo.AddComponent<GameFlowManager>();
            flow.StartNewCampaign();
        }

        [TearDown]
        public void TearDown()
        {
            for (int i = 1; i <= GameFlowManager.MaxSaveSlots; i++)
                GameFlowManager.DeleteSave(i);

            if (flowManagerGo != null)
                Object.DestroyImmediate(flowManagerGo);
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
        public void DepartToLevel_AfterBriefing_Succeeds()
        {
            flow.GoToPoskoFromHome();
            flow.CompleteBriefing();

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

            // Klaim kedua kali di level yang sama -> ditolak
            bool claimAgain = flow.ClaimLevelReward("BansosKit_L1");
            Assert.IsFalse(claimAgain);
            Assert.AreEqual(1, flow.Progress.inventory.Count);
        }

        [Test]
        public void RestInHome_AdvancesDay_AndResetsDailyFlagsCleanly()
        {
            flow.GoToPoskoFromHome();
            flow.CompleteBriefing();
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


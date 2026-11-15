using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;

namespace JejakAsa
{
    public class P01OpeningTests
    {
        private GameFlowManager flow;
        private GameProgressData previous;
        private GameObject playerObject;
        private GameObject npcObject;
        private GameObject openingObject;
        private GameObject cameraObject;
        private PlayerController player;
        private StoryNpcController ibu;
        private readonly List<P01Opening> previousOpenings = new List<P01Opening>();
        private readonly List<string> speakers = new List<string>();

        [SetUp]
        public void SetUp()
        {
            speakers.Clear();
            previousOpenings.Clear();
            foreach (var existing in Object.FindObjectsByType<P01Opening>(FindObjectsSortMode.None))
                if (existing.enabled) { previousOpenings.Add(existing); existing.enabled = false; }
            flow = GameFlowManager.Instance;
            previous = flow.Progress.Clone();
            flow.StartNewCampaign(true);
            cameraObject = new GameObject("P01 test camera");
            playerObject = new GameObject("P01 test player");
            playerObject.SetActive(false);
            playerObject.AddComponent<CharacterController>();
            player = playerObject.AddComponent<PlayerController>();
            typeof(PlayerController).GetField("movementCamera", BindingFlags.Instance | BindingFlags.NonPublic)
                .SetValue(player, cameraObject.transform);
            playerObject.SetActive(true);
            npcObject = new GameObject("P01 test Ibu");
            ibu = npcObject.AddComponent<StoryNpcController>();
            ibu.npcType = StoryNpcType.Ibu;
            openingObject = new GameObject("P01 test opening");
            openingObject.SetActive(false);
            StoryNpcController.SpeakerChanged += OnSpeakerChanged;
            var opening = openingObject.AddComponent<P01Opening>();
            typeof(P01Opening).GetField("playerReference", BindingFlags.Instance | BindingFlags.NonPublic).SetValue(opening, player);
            typeof(P01Opening).GetField("ibuReference", BindingFlags.Instance | BindingFlags.NonPublic).SetValue(opening, ibu);
        }

        [TearDown]
        public void TearDown()
        {
            StoryNpcController.SpeakerChanged -= OnSpeakerChanged;
            Object.DestroyImmediate(openingObject);
            Object.DestroyImmediate(npcObject);
            Object.DestroyImmediate(playerObject);
            Object.DestroyImmediate(cameraObject);
            flow.SetStateForTesting(previous);
            foreach (var existing in previousOpenings)
                if (existing != null) existing.enabled = true;
        }

        private void OnSpeakerChanged(string speaker) => speakers.Add(speaker);

        [UnityTest]
        public IEnumerator OpeningBlocksExitAndCompletesAllSixLines()
        {
            Assert.AreEqual(DayPhase.Prolog, flow.CurrentPhase);
            Assert.IsFalse(flow.GoToPoskoFromHome());
            openingObject.SetActive(true);
            yield return null;
            Assert.IsFalse(player.enabled);
            var lines = (DialogueLine[])typeof(StoryNpcController)
                .GetField("activeLines", BindingFlags.Instance | BindingFlags.NonPublic).GetValue(ibu);
            CollectionAssert.AreEqual(new[]
            {
                "Akhirnya pulang juga. Rasanya gang ini lebih sempit daripada yang kuingat.",
                "Gang-nya tidak mengecil. Kamu saja yang sudah lama tidak pulang.",
                "Bisa jadi, Bu. Bagaimana keadaan warga sekarang?",
                "Banyak yang sedang berusaha mencukupi kebutuhan sehari-hari. Tetangga juga saling membantu, tapi kadang bantuan yang datang belum sampai ke semua orang yang membutuhkan.",
                "Aku ingin cari tahu apa yang benar-benar mereka perlukan. Aku tidak mau datang lalu asal membagikan sesuatu.",
                "Bagus. Mulai dengan mendengarkan mereka, ya."
            }, System.Array.ConvertAll(lines, line => line.text));
            for (int i = 0; i < 6; i++)
            {
                Assert.IsFalse(flow.GoToPoskoFromHome());
                ibu.CloseDialogue();
                yield return null;
            }
            CollectionAssert.AreEqual(new[] { "Vicky", "Ibu", "Vicky", "Ibu", "Vicky", "Ibu", null }, speakers);
            Assert.AreEqual(DayPhase.RumahPagi, flow.CurrentPhase);
            Assert.IsTrue(player.enabled);
            Assert.IsTrue(flow.GoToPoskoFromHome());
        }

        [UnityTest]
        public IEnumerator OpeningWaitsForFade()
        {
            var fade = cameraObject.AddComponent<SceneStartFade>();
            typeof(P01Opening).GetField("fadeReference", BindingFlags.Instance | BindingFlags.NonPublic)
                .SetValue(openingObject.GetComponent<P01Opening>(), fade);
            openingObject.SetActive(true);
            yield return null;
            Assert.IsFalse(player.enabled);
            Assert.IsEmpty(speakers);
            Assert.IsFalse(flow.GoToPoskoFromHome());
            typeof(SceneStartFade).GetProperty("IsFinished").GetSetMethod(true).Invoke(fade, new object[] { true });
            yield return null;
            Assert.AreEqual("Vicky", speakers[0]);
        }

        [UnityTest]
        public IEnumerator CompletedPhaseDoesNotReplayOpening()
        {
            flow.CompleteProlog();
            openingObject.SetActive(true);
            yield return null;
            Assert.IsEmpty(speakers);
            Assert.IsTrue(player.enabled);
            Assert.AreEqual(DayPhase.RumahPagi, flow.CurrentPhase);
        }

        [UnityTest]
        public IEnumerator InterruptedOpeningRestoresMovementAndReplays()
        {
            openingObject.SetActive(true);
            yield return null;
            Assert.IsFalse(player.enabled);
            openingObject.SetActive(false);
            Assert.IsTrue(player.enabled);
            Assert.AreEqual(DayPhase.Prolog, flow.CurrentPhase);
            openingObject.SetActive(true);
            yield return null;
            Assert.IsFalse(player.enabled);
            Assert.AreEqual("Vicky", speakers[speakers.Count - 1]);
        }
    }
}

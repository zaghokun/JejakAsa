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
        private StoryFlow_PengaturAlur flow;
        private GameProgressData previous;
        private GameObject playerObject;
        private GameObject npcObject;
        private GameObject openingObject;
        private GameObject cameraObject;
        private Player_Gerak player;
        private StoryDialog_InteraksiNPC ibu;
        private readonly List<P01_VickyPulang> previousOpenings = new List<P01_VickyPulang>();
        private readonly List<string> speakers = new List<string>();

        [SetUp]
        public void SetUp()
        {
            speakers.Clear();
            previousOpenings.Clear();
            foreach (var existing in Object.FindObjectsByType<P01_VickyPulang>(FindObjectsSortMode.None))
                if (existing.enabled) { previousOpenings.Add(existing); existing.enabled = false; }
            flow = StoryFlow_PengaturAlur.Instance;
            previous = flow.Progress.Clone();
            flow.StartNewCampaign(true);
            cameraObject = new GameObject("P01 test camera");
            playerObject = new GameObject("P01 test player");
            playerObject.SetActive(false);
            playerObject.AddComponent<CharacterController>();
            player = playerObject.AddComponent<Player_Gerak>();
            typeof(Player_Gerak).GetField("movementCamera", BindingFlags.Instance | BindingFlags.NonPublic)
                .SetValue(player, cameraObject.transform);
            playerObject.SetActive(true);
            npcObject = new GameObject("P01 test Ibu");
            ibu = npcObject.AddComponent<StoryDialog_InteraksiNPC>();
            ibu.npcType = StoryNpcType.Ibu;
            openingObject = new GameObject("P01 test opening");
            openingObject.SetActive(false);
            StoryDialog_InteraksiNPC.SpeakerChanged += OnSpeakerChanged;
            var opening = openingObject.AddComponent<P01_VickyPulang>();
            typeof(P01_VickyPulang).GetField("playerReference", BindingFlags.Instance | BindingFlags.NonPublic).SetValue(opening, player);
            typeof(P01_VickyPulang).GetField("ibuReference", BindingFlags.Instance | BindingFlags.NonPublic).SetValue(opening, ibu);
        }

        [TearDown]
        public void TearDown()
        {
            StoryDialog_InteraksiNPC.SpeakerChanged -= OnSpeakerChanged;
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
            var lines = (DialogueLine[])typeof(StoryDialog_InteraksiNPC)
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
            var portrait = typeof(StoryDialog_InteraksiNPC).Assembly.GetType("JejakAsa.UI_DialogDanPotret")
                .GetMethod("Portrait", BindingFlags.Static | BindingFlags.NonPublic);
            for (int i = 0; i < 6; i++)
            {
                string speaker = i % 2 == 0 ? "Vicky" : "Ibu";
                Assert.AreSame(Resources.Load<Texture2D>("JejakAsaUI/Generated/Portraits/" + speaker),
                    portrait.Invoke(null, new object[] { lines[i].speaker }));
                Assert.IsNotNull(portrait.Invoke(null, new object[] { lines[i].speaker }));
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
            var fade = cameraObject.AddComponent<UI_TransisiMasukScene>();
            typeof(P01_VickyPulang).GetField("fadeReference", BindingFlags.Instance | BindingFlags.NonPublic)
                .SetValue(openingObject.GetComponent<P01_VickyPulang>(), fade);
            openingObject.SetActive(true);
            yield return null;
            Assert.IsFalse(player.enabled);
            Assert.IsEmpty(speakers);
            Assert.IsFalse(flow.GoToPoskoFromHome());
            typeof(UI_TransisiMasukScene).GetProperty("IsFinished").GetSetMethod(true).Invoke(fade, new object[] { true });
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

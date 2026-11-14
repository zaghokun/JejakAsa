#if UNITY_EDITOR
using System.Collections;
using JejakAsa;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.LowLevel;
using UnityEngine.SceneManagement;
using UnityEngine.TestTools;

public sealed class PoskoFlowIntegrationTests : InputTestFixture
{
    private Keyboard keyboard;
    private float previousTimeScale;

    public override void Setup()
    {
        base.Setup();
        previousTimeScale = Time.timeScale;
        Time.timeScale = 1f;
        keyboard = InputSystem.AddDevice<Keyboard>();
    }

    public override void TearDown()
    {
        Time.timeScale = previousTimeScale;
        base.TearDown();
    }

    [UnityTearDown]
    public IEnumerator CleanUp()
    {
        Time.timeScale = 1f;
        Scene current = SceneManager.GetActiveScene();
        Scene empty = SceneManager.CreateScene("PoskoFlowCleanup");
        SceneManager.SetActiveScene(empty);
        if (current.IsValid() && current.isLoaded)
            yield return SceneManager.UnloadSceneAsync(current);
    }

    [UnityTest]
    public IEnumerator GateLoadsEachLevelWithItsOwnLayoutAndBoss()
    {
        GameFlowManager flow = GameFlowManager.Instance;
        GameProgressData previous = flow.Progress.Clone();
        string previousSpawn = flow.LastSpawnPoint;
        try
        {
            var housePositions = new System.Collections.Generic.HashSet<Vector3>();
            for (int level = 1; level <= 5; level++)
            {
                flow.SetStateForTesting(new GameProgressData
                {
                    currentDay = level,
                    currentLevel = level,
                    currentPhase = DayPhase.PoskoBriefing,
                    isBriefingDone = true,
                    hasPickedUpBansos = true
                });
                yield return SceneManager.LoadSceneAsync("Assets/_Project/Scenes/Posko.unity", LoadSceneMode.Single);
                Object.FindFirstObjectByType<PoskoGateController>().TryDepartToLevel();
                yield return null;
                string expected = level == 1 ? "Kampung" : $"Kampung_L{level}";
                Assert.That(SceneManager.GetActiveScene().name, Is.EqualTo(expected));
                Assert.That(flow.CurrentPhase, Is.EqualTo(DayPhase.DiLevel));
                var house = GameObject.Find("Rumah_01");
                Assert.That(house, Is.Not.Null);
                Assert.That(housePositions.Add(house.transform.position), Is.True, "Each level must have its own layout.");
                var encounters = Object.FindObjectsByType<EncounterController>(FindObjectsSortMode.None);
                Assert.That(System.Array.FindAll(encounters, e => e.IsBoss).Length, Is.EqualTo(level == 3 || level == 5 ? 1 : 0));
            }
        }
        finally
        {
            flow.SetStateForTesting(previous);
            flow.LastSpawnPoint = previousSpawn;
        }
    }

    [UnityTest]
    public IEnumerator OverlappingTargetsOnlyTriggerTheClosest()
    {
        GameFlowManager flow = GameFlowManager.Instance;
        GameProgressData previous = flow.Progress.Clone();
        try
        {
            flow.StartNewCampaign();
            flow.GoToPoskoFromHome();
            yield return SceneManager.LoadSceneAsync("Assets/_Project/Scenes/Posko.unity", LoadSceneMode.Single);
            var player = GameObject.FindGameObjectWithTag("Player").transform;
            var npcs = Object.FindObjectsByType<StoryNpcController>(FindObjectsSortMode.None);
            var jonathan = System.Array.Find(npcs, n => n.npcType == StoryNpcType.Jonathan);
            var packages = Object.FindFirstObjectByType<PoskoDistributionTrigger>();
            jonathan.transform.position = new Vector3(0.5f, 1f, 0f);
            packages.transform.position = new Vector3(0f, 0.7f, 0f);
            MoveNear(player, packages.transform.position);
            yield return null;
            yield return Press(Key.E);
            Assert.That(flow.Progress.isBriefingDone, Is.False, "The nearby package must not open Jonathan's dialogue.");
            Assert.That(flow.Progress.hasPickedUpBansos, Is.False);

            MoveNear(player, jonathan.transform.position + Vector3.right);
            yield return null;
            yield return Press(Key.E);
            Assert.That(flow.Progress.isBriefingDone, Is.True);
            Assert.That(flow.Progress.hasPickedUpBansos, Is.False, "Briefing must not also take the package.");
        }
        finally
        {
            flow.SetStateForTesting(previous);
        }
    }

    [UnityTest]
    public IEnumerator OutdoorPoskoRoutesReachInteractionsWithShift()
    {
        yield return SceneManager.LoadSceneAsync("Assets/_Project/Scenes/Posko.unity", LoadSceneMode.Single);
        var player = GameObject.FindGameObjectWithTag("Player").transform;
        yield return WalkTo(player, new Vector2(-5f, -5f));
        yield return WalkTo(player, new Vector2(-5f, 3.1f));
        var npcs = Object.FindObjectsByType<StoryNpcController>(FindObjectsSortMode.None);
        var jonathan = System.Array.Find(npcs, n => n.npcType == StoryNpcType.Jonathan);
        Assert.That(Vector3.Distance(player.position, jonathan.transform.position), Is.LessThan(jonathan.interactionRadius));
        yield return WalkTo(player, new Vector2(-5f, -2.5f));
        yield return WalkTo(player, new Vector2(-9f, -2.5f));
        var packages = Object.FindFirstObjectByType<PoskoDistributionTrigger>();
        Assert.That(Vector3.Distance(player.position, packages.transform.position), Is.LessThan(packages.interactionRadius));
        yield return WalkTo(player, new Vector2(5f, -2.5f));
        yield return WalkTo(player, new Vector2(5f, 3.1f));
        var ophelia = System.Array.Find(npcs, n => n.npcType == StoryNpcType.Ophelia);
        Assert.That(Vector3.Distance(player.position, ophelia.transform.position), Is.LessThan(ophelia.interactionRadius));
        yield return WalkTo(player, new Vector2(0f, 3.1f));
        yield return WalkTo(player, new Vector2(0f, -8.8f));
        var gate = Object.FindFirstObjectByType<PoskoGateController>();
        Assert.That(Vector3.Distance(player.position, gate.transform.position), Is.LessThan(gate.interactionRadius));
    }

    private IEnumerator WalkTo(Transform player, Vector2 target)
    {
        float deadline = Time.time + 8f;
        while (Vector2.Distance(new Vector2(player.position.x, player.position.z), target) > 0.25f && Time.time < deadline)
        {
            Vector2 delta = target - new Vector2(player.position.x, player.position.z);
            Key horizontal = delta.x > 0.1f ? Key.D : delta.x < -0.1f ? Key.A : Key.None;
            Key vertical = delta.y > 0.1f ? Key.W : delta.y < -0.1f ? Key.S : Key.None;
            InputSystem.QueueStateEvent(keyboard, new KeyboardState(horizontal, vertical, Key.LeftShift));
            yield return null;
        }
        InputSystem.QueueStateEvent(keyboard, new KeyboardState());
        Assert.That(Vector2.Distance(new Vector2(player.position.x, player.position.z), target), Is.LessThanOrEqualTo(0.25f), "Route blocked");
    }

    [UnityTest]
    public IEnumerator RumahPoskoDeliveryReportAndReturnHome()
    {
        GameFlowManager flow = GameFlowManager.Instance;
        GameProgressData previous = flow.Progress.Clone();
        string previousSpawn = flow.LastSpawnPoint;
        try
        {
            flow.StartNewCampaign();
            flow.LastSpawnPoint = null;
            yield return SceneManager.LoadSceneAsync("Assets/_Project/Scenes/Rumah.unity", LoadSceneMode.Single);
            Object.FindFirstObjectByType<HomeInteractionController>().TryExitToPosko();
            yield return null;
            Assert.That(SceneManager.GetActiveScene().name, Is.EqualTo("Posko"));
            Assert.That(flow.CurrentPhase, Is.EqualTo(DayPhase.PoskoBriefing));

            var player = GameObject.FindGameObjectWithTag("Player").transform;
            var packages = Object.FindFirstObjectByType<PoskoDistributionTrigger>();
            MoveNear(player, packages.transform.position);
            yield return null;
            yield return Press(Key.E);
            Assert.That(flow.Progress.hasPickedUpBansos, Is.False);
            Assert.That(flow.Progress.isBriefingDone, Is.False);

            var npcs = Object.FindObjectsByType<StoryNpcController>(FindObjectsSortMode.None);
            StoryNpcController jonathan = System.Array.Find(npcs, n => n.npcType == StoryNpcType.Jonathan);
            StoryNpcController ophelia = System.Array.Find(npcs, n => n.npcType == StoryNpcType.Ophelia);
            Assert.That(jonathan, Is.Not.Null);
            Assert.That(ophelia, Is.Not.Null);
            MoveNear(player, jonathan.transform.position);
            yield return null;
            yield return Press(Key.E);
            Assert.That(flow.Progress.isBriefingDone, Is.True);
            yield return Press(Key.Enter);
            MoveNear(player, packages.transform.position);
            yield return null;
            yield return Press(Key.E);
            Assert.That(flow.Progress.hasPickedUpBansos, Is.True);

            var gate = Object.FindFirstObjectByType<PoskoGateController>();
            MoveNear(player, gate.transform.position);
            yield return null;
            yield return Press(Key.E);
            yield return Press(Key.Digit1);
            yield return null;
            Assert.That(SceneManager.GetActiveScene().name, Is.EqualTo("Kampung"));
            Assert.That(flow.CurrentPhase, Is.EqualTo(DayPhase.DiLevel));
            var delivery = Object.FindFirstObjectByType<PlayerInteraction>();
            Assert.That(delivery.PackagesRemaining, Is.EqualTo(3));
            var recipients = Object.FindObjectsByType<InteractionTarget>(FindObjectsSortMode.None);
            int delivered = 0;
            foreach (var recipient in recipients)
            {
                if (!recipient.IsRecipient) continue;
                MoveNear(delivery.transform, recipient.transform.position);
                yield return null;
                yield return Press(Key.E);
                Assert.That(recipient.IsDelivered, Is.True, recipient.name);
                yield return Press(Key.Enter);
                delivered++;
            }
            Assert.That(delivered, Is.EqualTo(3));
            Assert.That(delivery.Outcome, Is.EqualTo(MissionOutcome.Success));
            yield return new WaitForSecondsRealtime(0.5f);
            yield return Press(Key.Enter);
            yield return null;
            Assert.That(SceneManager.GetActiveScene().name, Is.EqualTo("Posko"));
            Assert.That(flow.CurrentPhase, Is.EqualTo(DayPhase.PoskoLaporan));

            player = GameObject.FindGameObjectWithTag("Player").transform;
            npcs = Object.FindObjectsByType<StoryNpcController>(FindObjectsSortMode.None);
            jonathan = System.Array.Find(npcs, n => n.npcType == StoryNpcType.Jonathan);
            ophelia = System.Array.Find(npcs, n => n.npcType == StoryNpcType.Ophelia);
            MoveNear(player, jonathan.transform.position);
            yield return null;
            yield return Press(Key.E);
            yield return Press(Key.Enter);
            MoveNear(player, ophelia.transform.position);
            yield return null;
            yield return Press(Key.E);
            yield return Press(Key.Enter);
            Assert.That(flow.Progress.isReportDone, Is.True);
            Assert.That(flow.Progress.isOpheliaTalkDone, Is.True);
            gate = Object.FindFirstObjectByType<PoskoGateController>();
            MoveNear(player, gate.transform.position);
            yield return null;
            yield return Press(Key.E);
            yield return Press(Key.Digit2);
            yield return null;
            Assert.That(SceneManager.GetActiveScene().name, Is.EqualTo("Rumah"));
            Assert.That(flow.CurrentPhase, Is.EqualTo(DayPhase.RumahMalam));
        }
        finally
        {
            flow.SetStateForTesting(previous);
            flow.LastSpawnPoint = previousSpawn;
        }
    }

    private static void MoveNear(Transform player, Vector3 target)
    {
        var controller = player.GetComponent<CharacterController>();
        controller.enabled = false;
        player.position = new Vector3(target.x, 1f, target.z - 1.3f);
        controller.enabled = true;
        Physics.SyncTransforms();
    }

    private IEnumerator Press(Key key)
    {
        InputSystem.QueueStateEvent(keyboard, new KeyboardState(key));
        yield return null;
        InputSystem.QueueStateEvent(keyboard, new KeyboardState());
        yield return null;
    }
}
#endif

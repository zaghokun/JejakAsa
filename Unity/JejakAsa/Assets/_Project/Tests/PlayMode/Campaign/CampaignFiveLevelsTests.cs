#if UNITY_EDITOR
using System.Collections;
using JejakAsa;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.LowLevel;
using UnityEngine.SceneManagement;
using UnityEngine.TestTools;

public sealed class CampaignFiveLevelsTests : InputTestFixture
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
        Scene empty = SceneManager.CreateScene("CampaignTestCleanup");
        SceneManager.SetActiveScene(empty);
        if (current.IsValid() && current.isLoaded)
            yield return SceneManager.UnloadSceneAsync(current);
    }

    [UnityTest]
    public IEnumerator AllFiveLevelsHaveReachableRecipients()
    {
        var flow = StoryFlow_PengaturAlur.Instance;
        var previous = flow.Progress.Clone();
        string previousSpawn = flow.LastSpawnPoint;
        try
        {
            for (int level = 1; level <= 5; level++)
            {
                flow.SetStateForTesting(new GameProgressData
                {
                    currentDay = level, currentLevel = level, currentPhase = DayPhase.DiLevel,
                    isBriefingDone = true, hasPickedUpBansos = true
                });
                flow.LastSpawnPoint = null;
                string scene = level == 1 ? "Kampung" : $"Kampung_L{level}";
                yield return SceneManager.LoadSceneAsync($"Assets/_Project/Scenes/{scene}.unity", LoadSceneMode.Single);
                var delivery = Object.FindFirstObjectByType<Mission_PengantaranBantuan>();
                Vector3 spawn = delivery.transform.position;
                var residents = Object.FindObjectsByType<Mission_PenerimaBantuan>(FindObjectsSortMode.None);
                int count = 0;
                foreach (var resident in residents)
                {
                    if (!resident.IsRecipient) continue;
                    var controller = delivery.GetComponent<CharacterController>();
                    controller.enabled = false;
                    delivery.transform.position = spawn;
                    controller.enabled = true;
                    Physics.SyncTransforms();
                    yield return null;
                    if (level == 1)
                    {
                        yield return Level1DryRoute(delivery.transform,resident.transform.position.x);
                        Vector3 approach = resident.transform.position-delivery.transform.position; approach.y=0;
                        Assert.That(approach.magnitude,Is.LessThan(1.8f),resident.name);
                        count++;
                        continue;
                    }
                    if (level == 2)
                    {
                        yield return Level2BridgeAndDryRoute(delivery.transform, resident.transform.position.x);
                        Vector3 approach = resident.transform.position - delivery.transform.position; approach.y = 0;
                        Assert.That(approach.magnitude, Is.LessThan(1.8f), resident.name);
                        count++;
                        continue;
                    }
                    if (level == 3)
                    {
                        yield return Level3BlockadeRoute(delivery.transform, resident.transform.position.x);
                        Vector3 approach = resident.transform.position - delivery.transform.position; approach.y = 0;
                        Assert.That(approach.magnitude, Is.LessThan(1.8f), resident.name);
                        count++;
                        continue;
                    }
                    yield return WalkTo(delivery.transform, new Vector2(-12f, -3f));
                    float approachZ = resident.transform.position.z - 1.2f;
                    if (Mathf.Abs(resident.transform.position.x) < 7.5f && approachZ > -1f)
                    {
                        yield return WalkTo(delivery.transform, new Vector2(-8f, -3f));
                        yield return WalkTo(delivery.transform, new Vector2(-8f, approachZ));
                    }
                    else
                        yield return WalkTo(delivery.transform, new Vector2(resident.transform.position.x, -3f));
                    yield return WalkTo(delivery.transform, new Vector2(resident.transform.position.x, resident.transform.position.z - 1.2f));
                    Vector3 delta = resident.transform.position - delivery.transform.position;
                    delta.y = 0f;
                    Assert.That(delta.magnitude, Is.LessThan(1.8f), $"{scene}: {resident.name}");
                    count++;
                }
                Assert.That(count, Is.EqualTo(3));
            }
        }
        finally
        {
            flow.SetStateForTesting(previous);
            flow.LastSpawnPoint = previousSpawn;
        }
    }

    [UnityTest]
    public IEnumerator Level1DryRouteDeliversAllPackagesWithinItsConfiguredBudget()
    {
        var flow=StoryFlow_PengaturAlur.Instance;var previous=flow.Progress.Clone();string previousSpawn=flow.LastSpawnPoint;
        try
        {
            flow.SetStateForTesting(new GameProgressData{currentPhase=DayPhase.DiLevel,isBriefingDone=true,hasPickedUpBansos=true});flow.LastSpawnPoint=null;
            yield return SceneManager.LoadSceneAsync("Assets/_Project/Scenes/Kampung.unity");
            var delivery=Object.FindFirstObjectByType<Mission_PengantaranBantuan>();float budget=delivery.TimeRemaining;
            yield return WalkTo(delivery.transform,new Vector2(-12,-2.4f));yield return Press(Key.E);yield return Press(Key.Enter);
            foreach(var point in new[]{new Vector2(-16.8f,-2.4f),new Vector2(-16.8f,7.5f),new Vector2(-9.4f,7.5f),new Vector2(-9.4f,16.6f),new Vector2(-5.6f,16.6f),new Vector2(-5.6f,4.8f),new Vector2(-.5f,4.8f)})yield return WalkTo(delivery.transform,point);
            yield return Press(Key.E);yield return Press(Key.Enter);
            foreach(var point in new[]{new Vector2(-3.7f,4.7f),new Vector2(-3.7f,12),new Vector2(5.5f,12),new Vector2(5.5f,16.6f),new Vector2(17.3f,16.6f),new Vector2(17.3f,4.3f),new Vector2(13,4.3f)})yield return WalkTo(delivery.transform,point);
            yield return Press(Key.E);yield return Press(Key.Enter);
            Assert.That(delivery.DeliveredCount,Is.EqualTo(3));Assert.That(delivery.Outcome,Is.EqualTo(MissionOutcome.Success));Assert.That(delivery.TimeRemaining,Is.GreaterThan(0));
            string result="Level1 dry-route keyboard test: spent "+(budget-delivery.TimeRemaining).ToString("F2")+" / "+budget.ToString("F0")+" seconds; remaining "+delivery.TimeRemaining.ToString("F2");
            Debug.Log(result);System.IO.Directory.CreateDirectory("Library/Level1Review");System.IO.File.WriteAllText("Library/Level1Review/dry-route-baseline.txt",result);
        }
        finally{flow.SetStateForTesting(previous);flow.LastSpawnPoint=previousSpawn;}
    }

    private IEnumerator Level1DryRoute(Transform player,float recipientX)
    {
        yield return WalkTo(player,new Vector2(-12,-2.4f));
        if(recipientX < -6) yield break;
        foreach(var point in new[]{new Vector2(-16.8f,-2.4f),new Vector2(-16.8f,7.5f),new Vector2(-9.4f,7.5f),new Vector2(-9.4f,16.6f),new Vector2(-5.6f,16.6f),new Vector2(-5.6f,4.8f),new Vector2(-.5f,4.8f)})
            yield return WalkTo(player,point);
        if(recipientX < 6) yield break;
        foreach(var point in new[]{new Vector2(-3.7f,4.7f),new Vector2(-3.7f,12),new Vector2(5.5f,12),new Vector2(5.5f,16.6f),new Vector2(17.3f,16.6f),new Vector2(17.3f,4.3f),new Vector2(13.0f,4.3f)})
            yield return WalkTo(player,point);
    }

    [UnityTest]
    public IEnumerator Level2BridgeRouteDeliversAllPackagesWithinItsConfiguredBudget()
    {
        var flow = StoryFlow_PengaturAlur.Instance; var previous = flow.Progress.Clone(); string previousSpawn = flow.LastSpawnPoint;
        try
        {
            flow.SetStateForTesting(new GameProgressData { currentDay = 2, currentLevel = 2, currentPhase = DayPhase.DiLevel, isBriefingDone = true, hasPickedUpBansos = true });
            flow.LastSpawnPoint = null;
            yield return SceneManager.LoadSceneAsync("Assets/_Project/Scenes/Kampung_L2.unity");
            var delivery = Object.FindFirstObjectByType<Mission_PengantaranBantuan>();
            yield return WalkTo(delivery.transform, new Vector2(-12f, -3.5f));
            yield return WalkTo(delivery.transform, new Vector2(-14f, -3.5f));
            yield return WalkTo(delivery.transform, new Vector2(-14f, 0.3f));
            yield return Press(Key.E); yield return Press(Key.Enter);
            foreach (var point in new[] { new Vector2(-14f, -3.5f), new Vector2(-10f, -3.5f), new Vector2(-10f, 5.2f), new Vector2(2f, 5.2f) })
                yield return WalkTo(delivery.transform, point);
            yield return Press(Key.E); yield return Press(Key.Enter);
            foreach (var point in new[] { new Vector2(8.2f, 5.2f), new Vector2(8.2f, 2.5f), new Vector2(13f, 2.5f) })
                yield return WalkTo(delivery.transform, point);
            yield return Press(Key.E); yield return Press(Key.Enter);
            Assert.That(delivery.DeliveredCount, Is.EqualTo(3));
            Assert.That(delivery.Outcome, Is.EqualTo(MissionOutcome.Success));
            Assert.That(delivery.TimeRemaining, Is.GreaterThan(0f));
        }
        finally { flow.SetStateForTesting(previous); flow.LastSpawnPoint = previousSpawn; }
    }

    private IEnumerator Level2BridgeAndDryRoute(Transform player, float recipientX)
    {
        yield return WalkTo(player, new Vector2(-12f, -3.5f));
        if (recipientX < -6f)
        {
            yield return WalkTo(player, new Vector2(-14f, -3.5f));
            yield return WalkTo(player, new Vector2(-14f, 0.3f));
            yield break;
        }
        foreach (var point in new[] { new Vector2(-10f, -3.5f), new Vector2(-10f, 5.2f), new Vector2(2f, 5.2f) })
            yield return WalkTo(player, point);
        if (recipientX < 6f) yield break;
        foreach (var point in new[] { new Vector2(8.2f, 5.2f), new Vector2(8.2f, 2.5f), new Vector2(13f, 2.5f) })
            yield return WalkTo(player, point);
    }

    [UnityTest]
    public IEnumerator Level3BlockadeRouteDeliversAllPackagesWithinItsConfiguredBudget()
    {
        var flow = StoryFlow_PengaturAlur.Instance; var previous = flow.Progress.Clone(); string previousSpawn = flow.LastSpawnPoint;
        try
        {
            flow.SetStateForTesting(new GameProgressData { currentDay = 3, currentLevel = 3, currentPhase = DayPhase.DiLevel, isBriefingDone = true, hasPickedUpBansos = true });
            flow.LastSpawnPoint = null;
            yield return SceneManager.LoadSceneAsync("Assets/_Project/Scenes/Kampung_L3.unity");
            var delivery = Object.FindFirstObjectByType<Mission_PengantaranBantuan>();
            foreach (var point in new[] { new Vector2(-13.2f, -3.8f), new Vector2(-13.2f, 1.7f), new Vector2(-9.5f, 1.7f), new Vector2(-9.5f, 4.8f), new Vector2(-10.8f, 4.8f) })
                yield return WalkTo(delivery.transform, point);
            yield return Press(Key.E); yield return Press(Key.Enter);
            foreach (var point in new[] { new Vector2(-8.5f, 5.8f), new Vector2(-2.5f, 6.2f), new Vector2(3.0f, 6.2f) })
                yield return WalkTo(delivery.transform, point);
            yield return Press(Key.E); yield return Press(Key.Enter);
            foreach (var point in new[] { new Vector2(10.4f, 6.2f), new Vector2(10.4f, 2.8f), new Vector2(13.2f, 2.8f) })
                yield return WalkTo(delivery.transform, point);
            yield return Press(Key.E); yield return Press(Key.Enter);
            Assert.That(delivery.DeliveredCount, Is.EqualTo(3));
            Assert.That(delivery.Outcome, Is.EqualTo(MissionOutcome.Success));
            Assert.That(delivery.TimeRemaining, Is.GreaterThan(0f));
        }
        finally { flow.SetStateForTesting(previous); flow.LastSpawnPoint = previousSpawn; }
    }

    private IEnumerator Level3BlockadeRoute(Transform player, float recipientX)
    {
        foreach (var point in new[] { new Vector2(-13.2f, -3.8f), new Vector2(-13.2f, 1.7f), new Vector2(-9.5f, 1.7f), new Vector2(-9.5f, 4.8f), new Vector2(-10.8f, 4.8f) })
            yield return WalkTo(player, point);
        if (recipientX < -6f) yield break;
        foreach (var point in new[] { new Vector2(-8.5f, 5.8f), new Vector2(-2.5f, 6.2f), new Vector2(3.0f, 6.2f) })
            yield return WalkTo(player, point);
        if (recipientX < 8f) yield break;
        foreach (var point in new[] { new Vector2(10.4f, 6.2f), new Vector2(10.4f, 2.8f), new Vector2(13.2f, 2.8f) })
            yield return WalkTo(player, point);
    }

    private IEnumerator WalkTo(Transform player, Vector2 target)
    {
        float deadline = Time.time + 12f;
        Vector2 position = new Vector2(player.position.x, player.position.z);
        while (Vector2.Distance(position, target) > 0.3f && Time.time < deadline)
        {
            Vector2 remaining = target - position;
            Key[] keys;
            if (Mathf.Abs(remaining.x) > 0.15f)
                keys = remaining.x > 0f ? new[] { Key.S, Key.D, Key.LeftShift } : new[] { Key.W, Key.A, Key.LeftShift };
            else
                keys = remaining.y > 0f ? new[] { Key.W, Key.D, Key.LeftShift } : new[] { Key.S, Key.A, Key.LeftShift };
            InputSystem.QueueStateEvent(keyboard, new KeyboardState(keys));
            yield return null;
            position = new Vector2(player.position.x, player.position.z);
        }
        InputSystem.QueueStateEvent(keyboard, new KeyboardState());
        Assert.That(Vector2.Distance(position, target), Is.LessThanOrEqualTo(0.3f), $"Blocked at {position} toward {target}");
        yield return null;
    }

    [UnityTest]
    public IEnumerator FiveDaysDeliverAndFinish()
    {
        var flow = StoryFlow_PengaturAlur.Instance;
        var previous = flow.Progress.Clone();
        string previousSpawn = flow.LastSpawnPoint;
        try
        {
            flow.StartNewCampaign();
            flow.LastSpawnPoint = null;
            for (int day = 1; day <= 5; day++)
            {
                Assert.That(flow.CurrentDay, Is.EqualTo(day));
                Assert.That(flow.GoToPoskoFromHome(), Is.True);
                yield return SceneManager.LoadSceneAsync("Assets/_Project/Scenes/Posko.unity", LoadSceneMode.Single);
                var npcs = Object.FindObjectsByType<StoryDialog_InteraksiNPC>(FindObjectsSortMode.None);
                var jonathan = System.Array.Find(npcs, n => n.npcType == StoryNpcType.Jonathan);
                var ophelia = System.Array.Find(npcs, n => n.npcType == StoryNpcType.Ophelia);
                var player = GameObject.FindGameObjectWithTag("Player").transform;
                MoveNear(player, jonathan.transform.position);
                yield return null;
                yield return Press(Key.E);
                yield return Press(Key.Enter);
                Assert.That(flow.Progress.isBriefingDone, Is.True);
                var packages = Object.FindFirstObjectByType<Interaction_AmbilPaketPosko>();
                MoveNear(player, packages.transform.position);
                yield return null;
                yield return Press(Key.E);
                Assert.That(flow.Progress.hasPickedUpBansos, Is.True);
                Object.FindFirstObjectByType<Interaction_GerbangPosko>().TryDepartToLevel();
                yield return null;
                Assert.That(SceneManager.GetActiveScene().name, Is.EqualTo(day == 1 ? "Kampung" : $"Kampung_L{day}"));
                var delivery = Object.FindFirstObjectByType<Mission_PengantaranBantuan>();
                Assert.That(delivery.PackagesRemaining, Is.EqualTo(3));
                yield return null;

                var residents = Object.FindObjectsByType<Mission_PenerimaBantuan>(FindObjectsSortMode.None);
                int count = 0;
                foreach (var resident in residents)
                {
                    if (!resident.IsRecipient) continue;
                    MoveNear(delivery.transform, resident.transform.position);
                    yield return null;
                    yield return Press(Key.E);
                    Assert.That(resident.IsDelivered, Is.True, resident.name);
                    yield return Press(Key.Enter);
                    count++;
                }
                Assert.That(count, Is.EqualTo(3));
                Assert.That(delivery.Outcome, Is.EqualTo(MissionOutcome.Success));
                yield return new WaitForSecondsRealtime(0.5f);
                yield return Press(Key.Enter);
                yield return null;
                Assert.That(SceneManager.GetActiveScene().name, Is.EqualTo("Posko"));
                Assert.That(flow.Progress.completedLevels, Does.Contain(day));
                npcs = Object.FindObjectsByType<StoryDialog_InteraksiNPC>(FindObjectsSortMode.None);
                jonathan = System.Array.Find(npcs, n => n.npcType == StoryNpcType.Jonathan);
                ophelia = System.Array.Find(npcs, n => n.npcType == StoryNpcType.Ophelia);
                player = GameObject.FindGameObjectWithTag("Player").transform;
                MoveNear(player, jonathan.transform.position);
                yield return null;
                yield return Press(Key.E);
                yield return Press(Key.Enter);
                MoveNear(player, ophelia.transform.position);
                yield return null;
                yield return Press(Key.E);
                yield return Press(Key.Enter);
                Object.FindFirstObjectByType<Interaction_GerbangPosko>().TryGoHome();
                yield return null;
                Assert.That(SceneManager.GetActiveScene().name, Is.EqualTo("Rumah"));
                Object.FindFirstObjectByType<Interaction_Rumah>().TryRestAndAdvanceDay();
                if (day < 5)
                    Assert.That(flow.CurrentPhase, Is.EqualTo(DayPhase.RumahPagi));
            }
            Assert.That(flow.CurrentPhase, Is.EqualTo(DayPhase.Tamat));
            Assert.That(flow.Progress.completedLevels, Is.EquivalentTo(new[] { 1, 2, 3, 4, 5 }));
            Assert.That(flow.CanGoToPoskoFromHome(out string reason), Is.False);
            Assert.That(reason, Does.Contain("selesai"));
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

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
        var flow = GameFlowManager.Instance;
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
                var delivery = Object.FindFirstObjectByType<PlayerInteraction>();
                Vector3 spawn = delivery.transform.position;
                var encounters = Object.FindObjectsByType<EncounterController>(FindObjectsSortMode.None);
                foreach (var encounter in encounters)
                    encounter.ResolveFromBattleVictory();
                var residents = Object.FindObjectsByType<InteractionTarget>(FindObjectsSortMode.None);
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
    public IEnumerator FiveDaysDeliverFightBossesReceiveRewardsAndFinish()
    {
        var flow = GameFlowManager.Instance;
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
                var npcs = Object.FindObjectsByType<StoryNpcController>(FindObjectsSortMode.None);
                var jonathan = System.Array.Find(npcs, n => n.npcType == StoryNpcType.Jonathan);
                var ophelia = System.Array.Find(npcs, n => n.npcType == StoryNpcType.Ophelia);
                var player = GameObject.FindGameObjectWithTag("Player").transform;
                MoveNear(player, jonathan.transform.position);
                yield return null;
                yield return Press(Key.E);
                yield return Press(Key.Enter);
                Assert.That(flow.Progress.isBriefingDone, Is.True);
                var packages = Object.FindFirstObjectByType<PoskoDistributionTrigger>();
                MoveNear(player, packages.transform.position);
                yield return null;
                yield return Press(Key.E);
                Assert.That(flow.Progress.hasPickedUpBansos, Is.True);
                Object.FindFirstObjectByType<PoskoGateController>().TryDepartToLevel();
                yield return null;
                Assert.That(SceneManager.GetActiveScene().name, Is.EqualTo(day == 1 ? "Kampung" : $"Kampung_L{day}"));
                var delivery = Object.FindFirstObjectByType<PlayerInteraction>();
                Assert.That(delivery.PackagesRemaining, Is.EqualTo(3));
                yield return null;

                if (day == 3 || day == 5)
                {
                    var boss = System.Array.Find(Object.FindObjectsByType<EncounterController>(FindObjectsSortMode.None), e => e.IsBoss);
                    Assert.That(boss, Is.Not.Null);
                    boss.OpenEncounter();
                    Assert.That(boss.TryRequestBattle(), Is.True);
                    float deadline = Time.realtimeSinceStartup + 15f;
                    while ((BattleController.Active == null || BattleController.Active.BossLevel != day) && Time.realtimeSinceStartup < deadline)
                        yield return null;
                    var battle = BattleController.Active;
                    Assert.That(battle, Is.Not.Null);
                    Assert.That(battle.BossLevel, Is.EqualTo(day));
                    if (flow.Progress.equippedWeapon != BattleController.BasicWeaponId)
                        Assert.That(battle.EquipBasicWeapon(), Is.True);
                    int turns = 0;
                    while (!battle.IsBattleOver && turns++ < 20)
                    {
                        if (battle.NextEnemyAction == BattleController.EnemyAction.Kuat)
                            Assert.That(battle.PlayerDefend(), Is.True);
                        else
                            Assert.That(battle.PlayerAttack(), Is.True);
                    }
                    Assert.That(battle.PlayerWon, Is.True);
                    deadline = Time.realtimeSinceStartup + 15f;
                    while ((!boss.IsResolved || !delivery.IsDialogueOpen) && Time.realtimeSinceStartup < deadline)
                        yield return null;
                    Assert.That(boss.IsResolved, Is.True);
                    Assert.That(delivery.IsDialogueOpen, Is.True);
                    yield return Press(Key.Escape);
                }

                var residents = Object.FindObjectsByType<InteractionTarget>(FindObjectsSortMode.None);
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
                npcs = Object.FindObjectsByType<StoryNpcController>(FindObjectsSortMode.None);
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
                Assert.That(flow.Progress.hasClaimedReward, Is.True);
                Object.FindFirstObjectByType<PoskoGateController>().TryGoHome();
                yield return null;
                Assert.That(SceneManager.GetActiveScene().name, Is.EqualTo("Rumah"));
                Object.FindFirstObjectByType<HomeInteractionController>().TryRestAndAdvanceDay();
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

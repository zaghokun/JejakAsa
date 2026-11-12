#if UNITY_EDITOR
using System.Collections;
using System.Collections.Generic;
using JejakAsa;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.LowLevel;
using UnityEngine.SceneManagement;
using UnityEngine.TestTools;

public sealed class U17RetryIntegrationTests : InputTestFixture
{
    private const string KampungPath = "Assets/_Project/Scenes/Kampung.unity";
    private const string BattlePath = "Assets/_Project/Scenes/Battle.unity";
    private readonly List<Behaviour> suspended = new List<Behaviour>();
    private Keyboard keyboard;
    private float previousTimeScale;
    private bool battleWasLoaded;
    private Scene kampung;

    public override void Setup()
    {
        base.Setup();
        previousTimeScale = Time.timeScale;
        battleWasLoaded = SceneManager.GetSceneByPath(BattlePath).isLoaded;
        Time.timeScale = 1f;
        keyboard = InputSystem.AddDevice<Keyboard>();

        // Isolate the fixture from gameplay and cameras in the test runner's open scenes.
        Suspend<EncounterController>();
        Suspend<BattleTransition>();
        Suspend<PlayerInteraction>();
        Suspend<PlayerController>();
        Suspend<Camera>();
        Suspend<AudioListener>();
    }

    public override void TearDown()
    {
        Time.timeScale = previousTimeScale;
        base.TearDown();
    }

    [UnityTearDown]
    public IEnumerator UnloadScenes()
    {
        // Stop the transition coroutine before unloading a battle it may have started.
        if (kampung.IsValid() && kampung.isLoaded)
            yield return SceneManager.UnloadSceneAsync(kampung);
        Scene battle = SceneManager.GetSceneByPath(BattlePath);
        if (!battleWasLoaded && battle.IsValid() && battle.isLoaded)
            yield return SceneManager.UnloadSceneAsync(battle);

        for (int i = suspended.Count - 1; i >= 0; i--)
            if (suspended[i] != null)
                suspended[i].enabled = true;
        suspended.Clear();
    }

    [UnityTest]
    public IEnumerator ThreeLossesAndRetriesRestoreTheCompleteMissionSession()
    {
        Assert.That(battleWasLoaded, Is.False, "The battle scene must not already be loaded by another session.");
        yield return SceneManager.LoadSceneAsync(KampungPath, LoadSceneMode.Additive);
        kampung = SceneManager.GetSceneAt(SceneManager.sceneCount - 1);
        PlayerInteraction player = null;
        BattleTransition transition = null;
        EncounterController encounter = null;
        InteractionTarget[] targets = null;
        foreach (GameObject root in kampung.GetRootGameObjects())
        {
            if (root.TryGetComponent(out PlayerInteraction found))
            {
                player = found;
                transition = root.GetComponent<BattleTransition>();
            }
            if (root.name == "U02_Graybox")
            {
                encounter = root.GetComponentInChildren<EncounterController>(true);
                targets = root.GetComponentsInChildren<InteractionTarget>(true);
            }
        }
        Assert.That(player, Is.Not.Null);
        Assert.That(transition, Is.Not.Null);
        Assert.That(encounter, Is.Not.Null);
        Assert.That(targets, Is.Not.Null);
        InteractionTarget recipient = System.Array.Find(targets, target => target.IsRecipient);
        Assert.That(recipient, Is.Not.Null);
        Vector3 spawn = player.transform.position;
        Quaternion spawnRotation = player.transform.rotation;
        transition.SetPostBattleDelayForTests(0f);
        yield return null;
        for (int session = 0; session <= 3; session++)
        {
            Assert.That(player.Outcome, Is.EqualTo(MissionOutcome.InProgress));
            Assert.That(player.IsOutcomeOpen, Is.False);
            Assert.That(player.PackagesRemaining, Is.EqualTo(3));
            Assert.That(player.DeliveredCount, Is.Zero);
            Assert.That(player.Trust, Is.Zero);
            Assert.That(player.TimeRemaining, Is.GreaterThan(178f).And.LessThanOrEqualTo(180f));
            foreach (InteractionTarget target in targets)
                if (target.IsRecipient) Assert.That(target.IsDelivered, Is.False);
            Assert.That(encounter.IsResolved, Is.False);
            Assert.That(encounter.IsEncounterOpen, Is.False);
            Assert.That(encounter.IsAwaitingExit, Is.False);
            Assert.That(encounter.HasBattleRequestPending, Is.False);
            Assert.That(encounter.ObstacleObject.activeSelf, Is.True);
            Assert.That(encounter.PremanVisual.activeSelf, Is.True);
            Assert.That(player.GetComponent<PlayerController>().enabled, Is.True);
            AssertSingleCameraAndListener();
            if (session == 3)
                break;

            Teleport(player, new Vector3(recipient.transform.position.x, 1f, recipient.transform.position.z - 1.5f));
            yield return null;
            yield return Press(Key.E);
            Assert.That(recipient.IsDelivered, Is.True);
            Assert.That(player.IsDialogueOpen, Is.True);
            Assert.That(player.DeliveredCount, Is.EqualTo(1));
            Assert.That(player.PackagesRemaining, Is.EqualTo(2));
            Assert.That(player.Trust, Is.EqualTo(1));
            yield return Press(Key.Escape);

            Teleport(player, new Vector3(encounter.transform.position.x, 1f, encounter.transform.position.z - 1.6f));
            yield return null;
            Assert.That(encounter.IsEncounterOpen, Is.True);
            float frozenTime = player.TimeRemaining;
            yield return Press(Key.Digit2);
            float deadline = Time.realtimeSinceStartup + 5f;
            while ((BattleController.Active == null || !BattleController.Active.IsPlayerTurn) && Time.realtimeSinceStartup < deadline)
                yield return null;
            BattleController battle = BattleController.Active;
            Assert.That(battle, Is.Not.Null);
            Assert.That(transition.IsInBattle, Is.True);
            AssertSingleCameraAndListener();
            yield return new WaitForSecondsRealtime(0.1f);
            Assert.That(player.TimeRemaining, Is.EqualTo(frozenTime));
            for (int attack = 0; attack < 4; attack++)
                Assert.That(battle.PlayerAttack(), Is.True);
            Assert.That(battle.IsBattleOver, Is.True);
            Assert.That(battle.PlayerWon, Is.False);

            deadline = Time.realtimeSinceStartup + 5f;
            while (transition.IsInBattle && Time.realtimeSinceStartup < deadline)
                yield return null;
            Assert.That(transition.IsInBattle, Is.False);
            Assert.That(SceneManager.GetSceneByPath(BattlePath).isLoaded, Is.False);
            AssertSingleCameraAndListener();
            Assert.That(player.Outcome, Is.EqualTo(MissionOutcome.PartialSuccess));
            Assert.That(player.OutcomeReason, Does.Contain("Kalah battle"));
            Assert.That(player.IsOutcomeOpen, Is.True);
            Assert.That(player.DeliveredCount, Is.EqualTo(1));
            Assert.That(player.PackagesRemaining, Is.EqualTo(2));
            Assert.That(player.Trust, Is.EqualTo(1));
            Assert.That(recipient.IsDelivered, Is.True);
            Assert.That(player.TimeRemaining, Is.EqualTo(frozenTime).Within(0.1f));

            yield return new WaitForSecondsRealtime(0.45f);
            yield return Press(Key.Enter);
            Assert.That(player.Outcome, Is.EqualTo(MissionOutcome.InProgress));
            Assert.That(player.OutcomeReason, Is.Empty);
            Assert.That(Vector3.Distance(player.transform.position, spawn), Is.LessThan(0.15f));
            Assert.That(Quaternion.Angle(player.transform.rotation, spawnRotation), Is.LessThan(0.1f));
        }
    }

    private void Suspend<T>() where T : Behaviour
    {
        foreach (T component in Object.FindObjectsByType<T>(FindObjectsInactive.Exclude, FindObjectsSortMode.None))
        {
            if (!component.enabled) continue;
            suspended.Add(component);
            component.enabled = false;
        }
    }

    private static void Teleport(PlayerInteraction player, Vector3 position)
    {
        CharacterController controller = player.GetComponent<CharacterController>();
        controller.enabled = false;
        player.transform.position = position;
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

    private static void AssertSingleCameraAndListener()
    {
        Assert.That(CountEnabled<Camera>(), Is.EqualTo(1));
        Assert.That(CountEnabled<AudioListener>(), Is.EqualTo(1));
    }

    private static int CountEnabled<T>() where T : Behaviour
    {
        int count = 0;
        foreach (T component in Object.FindObjectsByType<T>(FindObjectsInactive.Exclude, FindObjectsSortMode.None))
            if (component.isActiveAndEnabled) count++;
        return count;
    }
}
#endif

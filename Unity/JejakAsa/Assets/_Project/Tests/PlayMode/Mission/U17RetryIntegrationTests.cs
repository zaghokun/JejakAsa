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
    private readonly List<Behaviour> suspended = new List<Behaviour>();
    private Keyboard keyboard;
    private float previousTimeScale;
    private Scene kampung;
    private GameProgressData previousProgress;
    private string previousSpawn;

    public override void Setup()
    {
        base.Setup();
        previousTimeScale = Time.timeScale;
        var flow = StoryFlow_PengaturAlur.Instance;
        previousProgress = flow.Progress.Clone();
        previousSpawn = flow.LastSpawnPoint;
        flow.SetStateForTesting(new GameProgressData
        {
            currentDay = 1, currentLevel = 1, currentPhase = DayPhase.DiLevel,
            isBriefingDone = true, hasPickedUpBansos = true
        });
        flow.LastSpawnPoint = null;
        Time.timeScale = 1f;
        keyboard = InputSystem.AddDevice<Keyboard>();

        // Isolate the fixture from gameplay and cameras in the test runner's open scenes.
        Suspend<Mission_PengantaranBantuan>();
        Suspend<Player_Gerak>();
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
        if (kampung.IsValid() && kampung.isLoaded)
            yield return SceneManager.UnloadSceneAsync(kampung);

        for (int i = suspended.Count - 1; i >= 0; i--)
            if (suspended[i] != null)
                suspended[i].enabled = true;
        suspended.Clear();
        StoryFlow_PengaturAlur.Instance.SetStateForTesting(previousProgress);
        StoryFlow_PengaturAlur.Instance.LastSpawnPoint = previousSpawn;
    }

    [UnityTest]
    public IEnumerator ThreeTimeoutsAndRetriesRestoreTheCompleteMissionSession()
    {
        yield return SceneManager.LoadSceneAsync(KampungPath, LoadSceneMode.Additive);
        kampung = SceneManager.GetSceneAt(SceneManager.sceneCount - 1);
        Mission_PengantaranBantuan player = null;
        Mission_PenerimaBantuan[] targets = null;
        foreach (GameObject root in kampung.GetRootGameObjects())
        {
            if (root.TryGetComponent(out Mission_PengantaranBantuan found))
            {
                player = found;
            }
            if (root.name == "U02_Graybox")
            {
                targets = root.GetComponentsInChildren<Mission_PenerimaBantuan>(true);
            }
        }
        Assert.That(player, Is.Not.Null);
        Assert.That(targets, Is.Not.Null);
        Mission_PenerimaBantuan recipient = System.Array.Find(targets, target => target.IsRecipient);
        Assert.That(recipient, Is.Not.Null);
        Vector3 spawn = player.transform.position;
        Quaternion spawnRotation = player.transform.rotation;
        string progress = JsonUtility.ToJson(StoryFlow_PengaturAlur.Instance.Progress);
        float configuredTime = (float)typeof(Mission_PengantaranBantuan).GetField("initialTimeSeconds",System.Reflection.BindingFlags.Instance|System.Reflection.BindingFlags.NonPublic).GetValue(player);
        yield return null;
        for (int session = 0; session <= 3; session++)
        {
            Assert.That(player.Outcome, Is.EqualTo(MissionOutcome.InProgress));
            Assert.That(player.IsOutcomeOpen, Is.False);
            Assert.That(player.PackagesRemaining, Is.EqualTo(3));
            Assert.That(player.DeliveredCount, Is.Zero);
            Assert.That(player.Trust, Is.Zero);
            Assert.That(player.TimeRemaining, Is.GreaterThan(configuredTime-2f).And.LessThanOrEqualTo(configuredTime));
            foreach (Mission_PenerimaBantuan target in targets)
                if (target.IsRecipient) Assert.That(target.IsDelivered, Is.False);
            Assert.That(player.GetComponent<Player_Gerak>().enabled, Is.True);
            Assert.That(JsonUtility.ToJson(StoryFlow_PengaturAlur.Instance.Progress), Is.EqualTo(progress));
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

            player.SetTimeRemaining(0f);
            Assert.That(player.Outcome, Is.EqualTo(MissionOutcome.PartialSuccess));
            Assert.That(player.OutcomeReason, Does.Contain("Waktu"));
            Assert.That(player.IsOutcomeOpen, Is.True);
            Assert.That(player.DeliveredCount, Is.EqualTo(1));
            Assert.That(player.PackagesRemaining, Is.EqualTo(2));
            Assert.That(player.Trust, Is.EqualTo(1));
            Assert.That(recipient.IsDelivered, Is.True);
            Assert.That(player.TimeRemaining, Is.Zero);

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

    private static void Teleport(Mission_PengantaranBantuan player, Vector3 position)
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

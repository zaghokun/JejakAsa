#if UNITY_EDITOR
using System.Collections;
using System.Collections.Generic;
using JejakAsa;
using NUnit.Framework;
using UnityEditor;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.LowLevel;
using UnityEngine.SceneManagement;
using UnityEngine.TestTools;

public sealed class P01SceneIntegrationTests : InputTestFixture
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
        if (current.name == "P01SceneTestCleanup")
            yield break;
        Scene empty = SceneManager.CreateScene("P01SceneTestCleanup");
        SceneManager.SetActiveScene(empty);
        if (current.IsValid() && current.isLoaded)
            yield return SceneManager.UnloadSceneAsync(current);
    }

    [UnityTest]
    public IEnumerator VickyAnimatesWalkingRunningAndStopping()
    {
        var flow = GameFlowManager.Instance;
        var previous = flow.Progress.Clone();
        string previousSpawn = flow.LastSpawnPoint;
        try
        {
            flow.SetStateForTesting(new GameProgressData { currentPhase = DayPhase.RumahPagi });
            yield return SceneManager.LoadSceneAsync("Assets/_Project/Scenes/Rumah.unity", LoadSceneMode.Single);
            var animator = GameObject.Find("Vicky_Visual").GetComponent<Animator>();
            InputSystem.QueueStateEvent(keyboard, new KeyboardState(Key.W));
            yield return new WaitForSeconds(0.35f);
            Assert.That(animator.GetCurrentAnimatorStateInfo(0).IsName("Walk"), Is.True, "Gerakan berjalan harus terlihat.");
            InputSystem.QueueStateEvent(keyboard, new KeyboardState(Key.W, Key.LeftShift));
            yield return new WaitForSeconds(0.35f);
            Assert.That(animator.GetCurrentAnimatorStateInfo(0).IsName("Run"), Is.True, "Shift harus memainkan lari.");
            InputSystem.QueueStateEvent(keyboard, new KeyboardState());
            yield return new WaitForSeconds(0.35f);
            Assert.That(animator.GetCurrentAnimatorStateInfo(0).IsName("Idle"), Is.True, "Karakter harus berhenti saat input dilepas.");
        }
        finally
        {
            flow.SetStateForTesting(previous);
            flow.LastSpawnPoint = previousSpawn;
        }
    }
    [UnityTest]
    public IEnumerator LocomotionKeepsLoopingAcrossChangingFrameRates()
    {
        int previousCaptureFramerate = Time.captureFramerate;
        var prefab = AssetDatabase.LoadAssetAtPath<GameObject>("Assets/_Project/Art/Characters/P01/Vicky.fbx");
        var character = Object.Instantiate(prefab);
        try
        {
            var animator = character.GetComponent<Animator>();
            animator.runtimeAnimatorController = AssetDatabase.LoadAssetAtPath<RuntimeAnimatorController>(
                "Assets/_Project/Art/Characters/P01/P01Character.controller");
            animator.applyRootMotion = false;
            animator.cullingMode = AnimatorCullingMode.AlwaysAnimate;
            character.AddComponent<P01CharacterAnimator>();
            var mover = character.AddComponent<P01AnimationTestMover>();

            foreach (var clip in new[] { "Vicky", "Run" })
            {
                var importer = (ModelImporter)AssetImporter.GetAtPath("Assets/_Project/Art/Characters/P01/" + clip + ".fbx");
                Assert.That(importer.clipAnimations[0].loopTime, Is.True);
                Assert.That(importer.clipAnimations[0].loopPose, Is.True);
            }

            foreach (var state in new[] { "Walk", "Run" })
            {
                mover.speed = state == "Walk" ? 3.5f : 7f;
                Time.captureFramerate = 60;
                for (int i = 0; i < 30; i++)
                    yield return null;
                var initial = animator.GetCurrentAnimatorStateInfo(0);
                Assert.That(initial.IsName(state), Is.True);
                float previousTime = initial.normalizedTime;
                for (int i = 0; i < 150; i++)
                {
                    Time.captureFramerate = i % 2 == 0 ? 30 : 144;
                    yield return null;
                    var current = animator.GetCurrentAnimatorStateInfo(0);
                    Assert.That(current.IsName(state), Is.True);
                    Assert.That(animator.IsInTransition(0), Is.False);
                    Assert.That(current.normalizedTime, Is.GreaterThan(previousTime));
                    previousTime = current.normalizedTime;
                }
                Assert.That(previousTime - initial.normalizedTime, Is.GreaterThan(1f));
            }
        }
        finally
        {
            Time.captureFramerate = previousCaptureFramerate;
            Object.Destroy(character);
        }
    }


    [UnityTest]
    public IEnumerator FurnishedHomeKeepsBedroomAndExitReachable()
    {
        var flow = GameFlowManager.Instance;
        var previous = flow.Progress.Clone();
        string previousSpawn = flow.LastSpawnPoint;
        try
        {
            flow.SetStateForTesting(new GameProgressData { currentPhase = DayPhase.RumahPagi });
            yield return SceneManager.LoadSceneAsync("Assets/_Project/Scenes/Rumah.unity", LoadSceneMode.Single);
            var player = GameObject.FindGameObjectWithTag("Player").transform;
            yield return WalkTo(player, new Vector2(-1.5f, -2.4f));
            yield return WalkTo(player, new Vector2(-1.5f, 0.45f));
            yield return WalkTo(player, new Vector2(-3.6f, 0.45f));
            yield return WalkTo(player, new Vector2(-4.7f, 0.35f));
            var home = Object.FindFirstObjectByType<HomeInteractionController>();
            Assert.That(Vector3.Distance(player.position, home.bedTransform.position), Is.LessThan(home.interactionRadius));
            yield return Press(Key.E);
            yield return Press(Key.Digit5);
            yield return WalkTo(player, new Vector2(-3.6f, 0.45f));
            yield return WalkTo(player, new Vector2(-1.5f, 0.45f));
            yield return WalkTo(player, new Vector2(-1.5f, -2.4f));
            yield return WalkTo(player, new Vector2(2f, -3.65f));
            Assert.That(Vector3.Distance(player.position, home.doorTransform.position), Is.LessThan(home.interactionRadius));
            yield return Press(Key.E);
            yield return null;
            Assert.That(SceneManager.GetActiveScene().name, Is.EqualTo("Posko"));
        }
        finally
        {
            flow.SetStateForTesting(previous);
            flow.LastSpawnPoint = previousSpawn;
        }
    }

    private IEnumerator WalkTo(Transform player, Vector2 target)
    {
        float deadline = Time.time + 8f;
        Vector2 position = new Vector2(player.position.x, player.position.z);
        while (Vector2.Distance(position, target) > 0.25f && Time.time < deadline)
        {
            Vector2 remaining = target - position;
            Key[] keys;
            if (Mathf.Abs(remaining.x) > 0.1f)
                keys = remaining.x > 0f ? new[] { Key.S, Key.D } : new[] { Key.W, Key.A };
            else
                keys = remaining.y > 0f ? new[] { Key.W, Key.D } : new[] { Key.S, Key.A };
            InputSystem.QueueStateEvent(keyboard, new KeyboardState(keys));
            yield return null;
            position = new Vector2(player.position.x, player.position.z);
        }
        InputSystem.QueueStateEvent(keyboard, new KeyboardState());
        Assert.That(Vector2.Distance(position, target), Is.LessThanOrEqualTo(0.25f), $"Jalur tertutup di {position} menuju {target}");
        yield return null;
    }

    [UnityTest]
    public IEnumerator NewGameFadesThenPlaysP01OnceAndProgressSurvivesReload()
    {
        var flow = GameFlowManager.Instance;
        var previous = flow.Progress.Clone();
        string previousSpawn = flow.LastSpawnPoint;
        const string slotKey = "JejakAsa_SaveSlot_3";
        const string timeKey = "JejakAsa_SaveSlot_3_Time";
        bool hadSlot = PlayerPrefs.HasKey(slotKey);
        bool hadTime = PlayerPrefs.HasKey(timeKey);
        string oldSlot = PlayerPrefs.GetString(slotKey);
        string oldTime = PlayerPrefs.GetString(timeKey);
        var speakers = new List<string>();
        System.Action<string> onSpeaker = speaker => speakers.Add(speaker);
        StoryNpcController.SpeakerChanged += onSpeaker;
        try
        {
            yield return SceneManager.LoadSceneAsync("Assets/_Project/Scenes/MainMenu.unity", LoadSceneMode.Single);
            Object.FindFirstObjectByType<SessionMenu>().StartCampaignFromHome();
            yield return null;
            Assert.That(SceneManager.GetActiveScene().name, Is.EqualTo("Rumah"));
            Assert.That(flow.CurrentPhase, Is.EqualTo(DayPhase.Prolog));
            var fade = Object.FindFirstObjectByType<SceneStartFade>();
            Assert.That(fade, Is.Not.Null);
            Assert.That(fade.IsFinished, Is.False);
            yield return null;
            var player = GameObject.FindGameObjectWithTag("Player").GetComponent<PlayerController>();
            Assert.That(player.enabled, Is.False);
            Assert.That(flow.CanGoToPoskoFromHome(out _), Is.False);
            yield return Press(Key.Enter);
            Assert.That(flow.CurrentPhase, Is.EqualTo(DayPhase.Prolog));
            Assert.That(speakers, Is.Empty, "Dialog tidak boleh terlewati saat fade.");
            float deadline = Time.realtimeSinceStartup + 4f;
            while (speakers.Count == 0 && Time.realtimeSinceStartup < deadline)
                yield return null;
            Assert.That(fade.IsFinished, Is.True);
            Assert.That(speakers, Is.EqualTo(new[] { "Vicky" }));
            foreach (string name in new[] { "Vicky_Visual", "Ibu_Visual" })
            {
                var visual = GameObject.Find(name);
                Assert.That(visual, Is.Not.Null, name);
                var animator = visual.GetComponent<Animator>();
                Assert.That(animator.avatar.isValid && animator.avatar.isHuman, Is.True);
                Assert.That(visual.GetComponentInChildren<SkinnedMeshRenderer>().sharedMaterial.mainTexture, Is.Not.Null);
            }
            yield return new WaitForSecondsRealtime(0.25f);
            var vicky = GameObject.Find("Vicky_Visual").GetComponent<Animator>();
            Assert.That(vicky.GetCurrentAnimatorStateInfo(0).IsName("TalkStart") ||
                vicky.GetNextAnimatorStateInfo(0).IsName("TalkStart"), Is.True);
            yield return Press(Key.Enter);
            yield return new WaitForSecondsRealtime(0.25f);
            var ibu = GameObject.Find("Ibu_Visual").GetComponent<Animator>();
            Assert.That(ibu.GetCurrentAnimatorStateInfo(0).IsName("TalkStart") ||
                ibu.GetNextAnimatorStateInfo(0).IsName("TalkStart"), Is.True);
            for (int i = 1; i < 6; i++)
            {
                Assert.That(flow.CurrentPhase, Is.EqualTo(DayPhase.Prolog));
                yield return Press(Key.Enter);
            }
            yield return new WaitForSecondsRealtime(0.5f);
            Assert.That(speakers, Is.EqualTo(new[] { "Vicky", "Ibu", "Vicky", "Ibu", "Vicky", "Ibu", null }));
            Assert.That(flow.CurrentPhase, Is.EqualTo(DayPhase.RumahPagi));
            Assert.That(player.enabled, Is.True);
            Assert.That(Camera.main.orthographicSize, Is.EqualTo(5.6f).Within(0.01f), "Opening must restore the closer Rumah gameplay framing.");
            Assert.That(Camera.main.GetComponent<FixedIsometricCamera>().enabled, Is.True);
            Assert.That(flow.CanGoToPoskoFromHome(out _), Is.True);
            Assert.That(flow.SaveGame(3), Is.True);
            flow.StartNewCampaign(true);
            Assert.That(flow.LoadGame(3), Is.True);
            yield return SceneManager.LoadSceneAsync("Assets/_Project/Scenes/Rumah.unity", LoadSceneMode.Single);
            yield return new WaitForSecondsRealtime(0.2f);
            Assert.That(flow.CurrentPhase, Is.EqualTo(DayPhase.RumahPagi));
            Assert.That(speakers.Count, Is.EqualTo(7), "P01 tidak boleh terulang sesudah load.");
            Assert.That(GameObject.FindGameObjectWithTag("Player").GetComponent<PlayerController>().enabled, Is.True);
        }
        finally
        {
            StoryNpcController.SpeakerChanged -= onSpeaker;
            if (hadSlot) PlayerPrefs.SetString(slotKey, oldSlot);
            else PlayerPrefs.DeleteKey(slotKey);
            if (hadTime) PlayerPrefs.SetString(timeKey, oldTime);
            else PlayerPrefs.DeleteKey(timeKey);
            PlayerPrefs.Save();
            flow.SetStateForTesting(previous);
            flow.LastSpawnPoint = previousSpawn;
        }
    }

    private IEnumerator Press(Key key)
    {
        InputSystem.QueueStateEvent(keyboard, new KeyboardState(key));
        yield return null;
        InputSystem.QueueStateEvent(keyboard, new KeyboardState());
        yield return null;
    }
}

public sealed class P01AnimationTestMover : MonoBehaviour
{
    public float speed;

    private void Update()
    {
        transform.position += Vector3.forward * (speed * Time.deltaTime);
    }
}
#endif

#if UNITY_EDITOR
using System.Collections;
using System.Reflection;
using JejakAsa;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.TestTools;

public sealed class Level2ChallengeTests
{
    private GameProgressData before;
    private string spawn;
    private const BindingFlags Private = BindingFlags.Instance | BindingFlags.NonPublic;

    [UnitySetUp]
    public IEnumerator Setup()
    {
        before = StoryFlow_PengaturAlur.Instance.Progress.Clone();
        spawn = StoryFlow_PengaturAlur.Instance.LastSpawnPoint;
        StoryFlow_PengaturAlur.Instance.SetStateForTesting(new GameProgressData
        {
            currentDay = 2,
            currentLevel = 2,
            currentPhase = DayPhase.DiLevel,
            isBriefingDone = true,
            hasPickedUpBansos = true
        });
        StoryFlow_PengaturAlur.Instance.LastSpawnPoint = null;
        yield return SceneManager.LoadSceneAsync("Assets/_Project/Scenes/Kampung_L2.unity");
        yield return null;
        GameObject.Find("Player").GetComponent<Player_Gerak>().enabled = false;
    }

    [UnityTearDown]
    public IEnumerator Cleanup()
    {
        var scene = SceneManager.GetActiveScene();
        SceneManager.SetActiveScene(SceneManager.CreateScene("Level2ChallengeCleanup"));
        yield return SceneManager.UnloadSceneAsync(scene);
        StoryFlow_PengaturAlur.Instance.SetStateForTesting(before);
        StoryFlow_PengaturAlur.Instance.LastSpawnPoint = spawn;
    }

    private static void Warp(Vector3 position)
    {
        var g = GameObject.Find("Player");
        var cc = g.GetComponent<CharacterController>();
        cc.enabled = false;
        g.transform.position = position;
        cc.enabled = true;
        Physics.SyncTransforms();
    }

    [UnityTest]
    public IEnumerator PlankBridgeKeepsCenterDryWhileFlanksSlowAndBarricadeBlocksCenter()
    {
        var leftMud = GameObject.Find("Genangan_L2_Barat_Kiri").GetComponent<Obstacle_Genangan>();
        var rightMud = GameObject.Find("Genangan_L2_Barat_Kanan").GetComponent<Obstacle_Genangan>();
        var eastMud = GameObject.Find("Genangan_L2_Timur").GetComponent<Obstacle_Genangan>();
        var cc = GameObject.Find("Player").GetComponent<CharacterController>();
        var overlaps = typeof(Obstacle_Genangan).GetMethod("Overlaps", Private);

        // Center of wooden plank bridge stays dry (neither flanking puddle overlaps)
        Warp(new Vector3(-10.0f, 1f, 0.5f));
        yield return null;
        Assert.That((bool)overlaps.Invoke(leftMud, new object[] { cc }), Is.False, "Plank bridge center must stay out of left mud.");
        Assert.That((bool)overlaps.Invoke(rightMud, new object[] { cc }), Is.False, "Plank bridge center must stay out of right mud.");

        // Slipping off left or right hits mud slowdown
        Warp(new Vector3(-11.6f, 1f, 0.5f));
        yield return null;
        Assert.That((bool)overlaps.Invoke(leftMud, new object[] { cc }), Is.True, "Stepping left off planks must trigger mud slowdown.");

        Warp(new Vector3(-8.4f, 1f, 0.5f));
        yield return null;
        Assert.That((bool)overlaps.Invoke(rightMud, new object[] { cc }), Is.True, "Stepping right off planks must trigger mud slowdown.");

        // Organic PBR textures assigned to mud banks (same visual quality as Level 1)
        var bank = eastMud.transform.Find("Genangan_L2_Timur_Lumpur").GetComponent<MeshRenderer>();
        Assert.That(bank.sharedMaterial.GetTexture("_BaseMap"), Is.Not.Null);
        Assert.That(bank.sharedMaterial.GetTexture("_BumpMap"), Is.Not.Null);

        // Barricade blocks old Level 1 center gang and two guards patrol Level 2
        Assert.That(GameObject.Find("Barikade_Banjir_Tengah").GetComponent<BoxCollider>(), Is.Not.Null);
        Assert.That(GameObject.Find("Genangan_L2_AlasJembatan"), Is.Not.Null);
        Assert.That(Object.FindObjectsByType<Obstacle_Preman>(FindObjectsSortMode.None).Length, Is.EqualTo(2));
        Assert.That(Object.FindObjectsByType<Obstacle_Genangan>(FindObjectsSortMode.None).Length, Is.EqualTo(4));
    }
}
#endif

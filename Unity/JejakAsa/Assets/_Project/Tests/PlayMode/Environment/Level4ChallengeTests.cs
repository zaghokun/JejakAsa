#if UNITY_EDITOR
using System.Collections;
using System.Reflection;
using JejakAsa;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.TestTools;

public sealed class Level4ChallengeTests
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
            currentDay = 4,
            currentLevel = 4,
            currentPhase = DayPhase.DiLevel,
            isBriefingDone = true,
            hasPickedUpBansos = true
        });
        StoryFlow_PengaturAlur.Instance.LastSpawnPoint = null;
        yield return SceneManager.LoadSceneAsync("Assets/_Project/Scenes/Kampung_L4.unity");
        yield return null;
        GameObject.Find("Player").GetComponent<Player_Gerak>().enabled = false;
    }

    [UnityTearDown]
    public IEnumerator Cleanup()
    {
        var scene = SceneManager.GetActiveScene();
        SceneManager.SetActiveScene(SceneManager.CreateScene("Level4ChallengeCleanup"));
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

    private static T Read<T>(object obj, string name) => (T)obj.GetType().GetField(name, Private).GetValue(obj);

    [UnityTest]
    public IEnumerator MultiHazardCourseHasDryBridgesTimedElectricTrapsFallenTreesAndZeroGrayBoxes()
    {
        var northLeftMud = GameObject.Find("Genangan_L4_Utara_Kiri").GetComponent<Obstacle_Genangan>();
        var northTopMud = GameObject.Find("Genangan_L4_Utara_Atas").GetComponent<Obstacle_Genangan>();
        var rightMud = GameObject.Find("Genangan_L4_Timur_Kanan").GetComponent<Obstacle_Genangan>();
        var innerMud = GameObject.Find("Genangan_L4_Timur_Dalam").GetComponent<Obstacle_Genangan>();
        var topMud = GameObject.Find("Genangan_L4_Timur_Atas").GetComponent<Obstacle_Genangan>();
        var cc = GameObject.Find("Player").GetComponent<CharacterController>();
        var overlaps = typeof(Obstacle_Genangan).GetMethod("Overlaps", Private);

        // North spillway narrow bridge stays dry while flanking mud overlaps
        Warp(new Vector3(-7.6f, 1f, 5.6f));
        yield return null;
        Assert.That((bool)overlaps.Invoke(northLeftMud, new object[] { cc }), Is.False);

        Warp(new Vector3(-6.2f, 1f, 6.7f));
        yield return null;
        Assert.That((bool)overlaps.Invoke(northLeftMud, new object[] { cc }), Is.False);
        Assert.That((bool)overlaps.Invoke(northTopMud, new object[] { cc }), Is.False);

        Warp(new Vector3(-9.8f, 1f, 6.7f));
        yield return null;
        Assert.That((bool)overlaps.Invoke(northLeftMud, new object[] { cc }), Is.True);

        // East narrow L-bridge stays dry along centerline while flanks slow
        Warp(new Vector3(14.2f, 1f, 1.9f));
        yield return null;
        Assert.That((bool)overlaps.Invoke(rightMud, new object[] { cc }), Is.False);
        Assert.That((bool)overlaps.Invoke(innerMud, new object[] { cc }), Is.False);

        Warp(new Vector3(12.6f, 1f, 3.5f));
        yield return null;
        Assert.That((bool)overlaps.Invoke(innerMud, new object[] { cc }), Is.False);
        Assert.That((bool)overlaps.Invoke(topMud, new object[] { cc }), Is.False);

        Warp(new Vector3(15.7f, 1f, 1.9f));
        yield return null;
        Assert.That((bool)overlaps.Invoke(rightMud, new object[] { cc }), Is.True);

        Warp(new Vector3(12.1f, 1f, 1.6f));
        yield return null;
        Assert.That((bool)overlaps.Invoke(innerMud, new object[] { cc }), Is.True);

        // Authentic obstacles exist & legacy blind-spot gray boxes are gone
        Assert.That(GameObject.Find("Pulau_BlindSpot_Barat"), Is.Null);
        Assert.That(GameObject.Find("Pulau_BlindSpot_Tengah"), Is.Null);
        Assert.That(GameObject.Find("Pulau_BlindSpot_Timur"), Is.Null);
        Assert.That(GameObject.Find("Tenda_Darurat_Barat").GetComponent<BoxCollider>(), Is.Not.Null);
        Assert.That(GameObject.Find("Pos_Logistik_Tengah").GetComponent<BoxCollider>(), Is.Not.Null);
        Assert.That(GameObject.Find("Pohon_Tumbang_BaratUtara").GetComponent<BoxCollider>(), Is.Not.Null);
        Assert.That(GameObject.Find("Pohon_Tumbang_Utara").GetComponent<BoxCollider>(), Is.Not.Null);
        Assert.That(GameObject.Find("Genangan_L4_AlasUtara"), Is.Not.Null);
        Assert.That(GameObject.Find("Genangan_L4_AlasJembatanTimur"), Is.Not.Null);

        // Variety counts: 2 guards, 2 periodic electric traps, 8 interactive puddles
        Assert.That(Object.FindObjectsByType<Obstacle_Preman>(FindObjectsSortMode.None).Length, Is.EqualTo(2));
        Assert.That(Object.FindObjectsByType<Obstacle_KorsletingListrik>(FindObjectsSortMode.None).Length, Is.EqualTo(2));
        Assert.That(Object.FindObjectsByType<Obstacle_Genangan>(FindObjectsSortMode.None).Length, Is.EqualTo(8));

        var eastGuard = GameObject.Find("Preman_02_JagaJembatanTimur").GetComponent<Obstacle_Preman>();
        Assert.That(Read<float>(eastGuard, "timePenalty"), Is.EqualTo(12f));
    }

    [UnityTest]
    public IEnumerator ElectricShortHazardShocksOnlyDuringActivePhase()
    {
        var mission = Object.FindFirstObjectByType<Mission_PengantaranBantuan>();
        var electric = GameObject.Find("Korslet_Utara_Barat").GetComponent<Obstacle_KorsletingListrik>();

        // Stand safely inside center during initial Safe window: zero shock penalty
        Warp(new Vector3(-7.4f, 1f, 1.6f));
        Assert.That(electric.CurrentState, Is.EqualTo(TimedHazardState.Safe));
        float beforeWait = mission.TimeRemaining;
        yield return new WaitForSeconds(0.5f);
        float passiveElapsed = beforeWait - mission.TimeRemaining;
        Assert.That(passiveElapsed, Is.LessThan(1.0f));

        // Wait until Active phase arrives and shock fires (-3s penalty)
        float deadline = Time.time + 5f;
        float preActiveTime = mission.TimeRemaining;
        while (electric.CurrentState != TimedHazardState.Active && Time.time < deadline)
        {
            preActiveTime = mission.TimeRemaining;
            yield return null;
        }
        Assert.That(electric.CurrentState, Is.EqualTo(TimedHazardState.Active));
        yield return null;
        yield return null;
        Assert.That(preActiveTime - mission.TimeRemaining, Is.GreaterThanOrEqualTo(2.9f));
    }
}
#endif

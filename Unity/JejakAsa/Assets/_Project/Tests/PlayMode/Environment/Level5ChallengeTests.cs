#if UNITY_EDITOR
using System.Collections;
using System.Reflection;
using JejakAsa;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.TestTools;

public sealed class Level5ChallengeTests
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
            currentDay = 5,
            currentLevel = 5,
            currentPhase = DayPhase.DiLevel,
            isBriefingDone = true,
            hasPickedUpBansos = true
        });
        StoryFlow_PengaturAlur.Instance.LastSpawnPoint = null;
        yield return SceneManager.LoadSceneAsync("Assets/_Project/Scenes/Kampung_L5.unity");
        yield return null;
        GameObject.Find("Player").GetComponent<Player_Gerak>().enabled = false;
    }

    [UnityTearDown]
    public IEnumerator Cleanup()
    {
        var scene = SceneManager.GetActiveScene();
        SceneManager.SetActiveScene(SceneManager.CreateScene("Level5ChallengeCleanup"));
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
    public IEnumerator FinaleMazeHasDryBridgesGasLeaksElectricShortBossMrGasOkeAndZeroGrayBoxes()
    {
        var westLeftMud = GameObject.Find("Genangan_L5_Barat_Kiri").GetComponent<Obstacle_Genangan>();
        var westRightMud = GameObject.Find("Genangan_L5_Barat_Kanan").GetComponent<Obstacle_Genangan>();
        var eastInnerMud = GameObject.Find("Genangan_L5_Timur_Dalam").GetComponent<Obstacle_Genangan>();
        var eastRightMud = GameObject.Find("Genangan_L5_Timur_Kanan").GetComponent<Obstacle_Genangan>();
        var eastTopMud = GameObject.Find("Genangan_L5_Timur_Atas").GetComponent<Obstacle_Genangan>();
        var cc = GameObject.Find("Player").GetComponent<CharacterController>();
        var overlaps = typeof(Obstacle_Genangan).GetMethod("Overlaps", Private);

        // West narrow plank bridge stays dry along centerline while flanks slow
        Warp(new Vector3(-12.0f, 1f, 1.5f));
        yield return null;
        Assert.That((bool)overlaps.Invoke(westLeftMud, new object[] { cc }), Is.False);
        Assert.That((bool)overlaps.Invoke(westRightMud, new object[] { cc }), Is.False);

        Warp(new Vector3(-13.8f, 1f, 1.5f));
        yield return null;
        Assert.That((bool)overlaps.Invoke(westLeftMud, new object[] { cc }), Is.True);

        // East L-bridge stays dry along centerline while flanks slow
        Warp(new Vector3(12.6f, 1f, 2.0f));
        yield return null;
        Assert.That((bool)overlaps.Invoke(eastInnerMud, new object[] { cc }), Is.False);
        Assert.That((bool)overlaps.Invoke(eastRightMud, new object[] { cc }), Is.False);

        Warp(new Vector3(14.2f, 1f, 5.2f));
        yield return null;
        Assert.That((bool)overlaps.Invoke(eastRightMud, new object[] { cc }), Is.False);
        Assert.That((bool)overlaps.Invoke(eastTopMud, new object[] { cc }), Is.False);

        Warp(new Vector3(10.8f, 1f, 2.0f));
        yield return null;
        Assert.That((bool)overlaps.Invoke(eastInnerMud, new object[] { cc }), Is.True);

        // Authentic shelters, trees & boss exist with no placeholder blind-spot cubes
        Assert.That(GameObject.Find("Tenda_Penimbunan_Barat").GetComponent<BoxCollider>(), Is.Not.Null);
        Assert.That(GameObject.Find("Pos_Logistik_Tengah").GetComponent<BoxCollider>(), Is.Not.Null);
        Assert.That(GameObject.Find("Pos_Timbunan_Timur").GetComponent<BoxCollider>(), Is.Not.Null);
        Assert.That(GameObject.Find("Pohon_Tumbang_Barat").GetComponent<BoxCollider>(), Is.Not.Null);
        Assert.That(GameObject.Find("Pohon_Tumbang_Timur").GetComponent<BoxCollider>(), Is.Not.Null);
        Assert.That(GameObject.Find("Genangan_L5_BaratBanjir"), Is.Not.Null);
        Assert.That(GameObject.Find("Genangan_L5_AlasTimur"), Is.Not.Null);

        Assert.That(Object.FindObjectsByType<Obstacle_KebocoranGas>(FindObjectsSortMode.None).Length, Is.EqualTo(2));
        Assert.That(Object.FindObjectsByType<Obstacle_KorsletingListrik>(FindObjectsSortMode.None).Length, Is.EqualTo(1));
        Assert.That(Object.FindObjectsByType<Obstacle_Preman>(FindObjectsSortMode.None).Length, Is.EqualTo(3));
        Assert.That(Object.FindObjectsByType<Obstacle_Genangan>(FindObjectsSortMode.None).Length, Is.EqualTo(7));

        var boss = GameObject.Find("Mr_Gas_Oke_Boss").GetComponent<Obstacle_Preman>();
        Assert.That(Read<float>(boss, "timePenalty"), Is.EqualTo(15f));
        Assert.That(Read<float>(boss, "moveSpeed"), Is.GreaterThanOrEqualTo(4.7f));
    }

    [UnityTest]
    public IEnumerator GasLeakHazardPenalizesOnlyDuringActivePlumePhase()
    {
        var mission = Object.FindFirstObjectByType<Mission_PengantaranBantuan>();
        var gas = GameObject.Find("KebocoranGas_Barat").GetComponent<Obstacle_KebocoranGas>();

        Warp(new Vector3(-12.0f, 1f, -2.0f));
        Assert.That(gas.CurrentState, Is.EqualTo(TimedHazardState.Safe));
        float beforeWait = mission.TimeRemaining;
        yield return new WaitForSeconds(0.5f);
        Assert.That(beforeWait - mission.TimeRemaining, Is.LessThan(1.0f));

        float deadline = Time.time + 5f;
        float preActiveTime = mission.TimeRemaining;
        while (gas.CurrentState != TimedHazardState.Active && Time.time < deadline)
        {
            preActiveTime = mission.TimeRemaining;
            yield return null;
        }
        Assert.That(gas.CurrentState, Is.EqualTo(TimedHazardState.Active));
        yield return null;
        yield return null;
        Assert.That(preActiveTime - mission.TimeRemaining, Is.GreaterThanOrEqualTo(3.9f));
    }
}
#endif

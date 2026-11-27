#if UNITY_EDITOR
using System.Collections;
using System.Reflection;
using JejakAsa;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.TestTools;

public sealed class Level3ChallengeTests
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
            currentDay = 3,
            currentLevel = 3,
            currentPhase = DayPhase.DiLevel,
            isBriefingDone = true,
            hasPickedUpBansos = true
        });
        StoryFlow_PengaturAlur.Instance.LastSpawnPoint = null;
        yield return SceneManager.LoadSceneAsync("Assets/_Project/Scenes/Kampung_L3.unity");
        yield return null;
        GameObject.Find("Player").GetComponent<Player_Gerak>().enabled = false;
    }

    [UnityTearDown]
    public IEnumerator Cleanup()
    {
        var scene = SceneManager.GetActiveScene();
        SceneManager.SetActiveScene(SceneManager.CreateScene("Level3ChallengeCleanup"));
        yield return SceneManager.UnloadSceneAsync(scene);
        StoryFlow_PengaturAlur.Instance.SetStateForTesting(before);
        StoryFlow_PengaturAlur.Instance.LastSpawnPoint = spawn;
    }

    private static T Read<T>(object obj, string name) => (T)obj.GetType().GetField(name, Private).GetValue(obj);

    [UnityTest]
    public IEnumerator MrDeddyEnforcerDealsHeavyPenaltyWithThreeTrianglePatrolsAndHeavyMud()
    {
        var deddyGo = GameObject.Find("Mr_Deddy_Boss");
        Assert.That(deddyGo, Is.Not.Null, "Mr_Deddy_Boss must be present in Level 3.");
        var deddyGuard = deddyGo.GetComponent<Obstacle_Preman>();
        Assert.That(Read<float>(deddyGuard, "timePenalty"), Is.EqualTo(15f));
        Assert.That(Read<float>(deddyGuard, "moveSpeed"), Is.GreaterThan(4.7f));
        Assert.That(Read<Vector3[]>(deddyGuard, "patrolPoints").Length, Is.EqualTo(4));

        var westGuard = GameObject.Find("Preman_Graybox").GetComponent<Obstacle_Preman>();
        Assert.That(Read<Vector3[]>(westGuard, "patrolPoints").Length, Is.EqualTo(3));

        Assert.That(Object.FindObjectsByType<Obstacle_Preman>(FindObjectsSortMode.None).Length, Is.EqualTo(4));
        Assert.That(Object.FindObjectsByType<Obstacle_Genangan>(FindObjectsSortMode.None).Length, Is.EqualTo(5));
        Assert.That(GameObject.Find("Sekat_Chicane_Tengah").GetComponent<BoxCollider>(), Is.Not.Null);
        Assert.That(GameObject.Find("Pagar_Tutup_BaratLuar").GetComponent<BoxCollider>(), Is.Not.Null);

        var heavyMud = GameObject.Find("Genangan_L3_Utama_Pekat").GetComponent<Obstacle_Genangan>();
        Assert.That(heavyMud.transform.Find("Genangan_L3_Utama_Pekat_Lumpur").GetComponent<MeshRenderer>().sharedMaterial.GetTexture("_BaseMap"), Is.Not.Null);
        yield return null;
    }
}
#endif

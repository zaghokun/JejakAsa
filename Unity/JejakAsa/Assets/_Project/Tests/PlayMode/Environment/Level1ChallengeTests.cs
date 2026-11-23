#if UNITY_EDITOR
using System.Collections;
using System.Reflection;
using JejakAsa;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.TestTools;
using UnityEngine.UI;

public sealed class Level1ChallengeTests
{
    GameProgressData before;string spawn;
    const BindingFlags Private=BindingFlags.Instance|BindingFlags.NonPublic;
    [UnitySetUp] public IEnumerator Setup()
    {
        before=StoryFlow_PengaturAlur.Instance.Progress.Clone();spawn=StoryFlow_PengaturAlur.Instance.LastSpawnPoint;
        StoryFlow_PengaturAlur.Instance.SetStateForTesting(new GameProgressData{currentPhase=DayPhase.DiLevel,isBriefingDone=true,hasPickedUpBansos=true});
        StoryFlow_PengaturAlur.Instance.LastSpawnPoint=null;
        yield return SceneManager.LoadSceneAsync("Assets/_Project/Scenes/Kampung.unity");
        yield return null;
        GameObject.Find("Player").GetComponent<Player_Gerak>().enabled=false;
    }
    [UnityTearDown] public IEnumerator Cleanup()
    {
        Time.timeScale=1;
        var scene=SceneManager.GetActiveScene();SceneManager.SetActiveScene(SceneManager.CreateScene("ChallengeCleanup"));
        yield return SceneManager.UnloadSceneAsync(scene);
        StoryFlow_PengaturAlur.Instance.SetStateForTesting(before);StoryFlow_PengaturAlur.Instance.LastSpawnPoint=spawn;
    }
    static void Set(object obj,string name,object value)=>obj.GetType().GetField(name,Private).SetValue(obj,value);
    static T Read<T>(object obj,string name)=>(T)obj.GetType().GetField(name,Private).GetValue(obj);
    static void Warp(Vector3 position)
    {var g=GameObject.Find("Player");var cc=g.GetComponent<CharacterController>();cc.enabled=false;g.transform.position=position;cc.enabled=true;Physics.SyncTransforms();}

    [UnityTest] public IEnumerator PatrolTurnsAndChaseUsesRunAnimationThenPauseAndRetryRestoreState()
    {
        var guard=GameObject.Find("Preman_Graybox").GetComponent<Obstacle_Preman>();
        var player=GameObject.Find("Player").GetComponent<Mission_PengantaranBantuan>();
        Vector3 start=Read<Vector3>(guard,"initialPosition");Quaternion original=Read<Quaternion>(guard,"initialRotation");
        Assert.That(Object.FindObjectsByType<Obstacle_Preman>(FindObjectsSortMode.None).Length,Is.EqualTo(2));
        yield return new WaitForSeconds(1.8f);
        Assert.That(Vector3.Distance(start,guard.transform.position),Is.GreaterThan(.2f));
        Assert.That(Quaternion.Angle(original,guard.transform.rotation),Is.GreaterThan(90),"Patrol must face north after its southern waypoint.");
        player.ResetMissionSession();
        player.GetComponent<Player_Gerak>().enabled=false;
        Warp(new Vector3(2.8f,1,-3));
        Vector3 from=guard.transform.position;
        yield return new WaitForSeconds(.25f);
        Vector3 delta=guard.transform.position-from;delta.y=0;
        Assert.That(Vector3.Dot(guard.transform.forward,delta.normalized),Is.GreaterThan(.6f),"The visible actor must face its travel direction.");
        var animator=guard.GetComponentInChildren<Animator>();
        Assert.That(animator.GetCurrentAnimatorStateInfo(0).IsName("Run")||animator.GetNextAnimatorStateInfo(0).IsName("Run"),Is.True);
        player.OpenSystemDialogue("Jeda uji","Patroli berhenti selama dialog.");
        Vector3 stopped=guard.transform.position;
        yield return new WaitForSeconds(.25f);Assert.That(guard.transform.position,Is.EqualTo(stopped));
        Read<Button>(player,"closeButton").onClick.Invoke();
        Time.timeScale=0;stopped=guard.transform.position;
        yield return new WaitForSecondsRealtime(.2f);Assert.That(guard.transform.position,Is.EqualTo(stopped));
        Time.timeScale=1;player.ResetMissionSession();
        Assert.That(Vector3.Distance(guard.transform.position,start),Is.LessThan(.05));
        Assert.That(Quaternion.Angle(guard.transform.rotation,original),Is.LessThan(.1));
        var second=GameObject.Find("Preman_02_PatroliTimur");
        Assert.That(second.transform.position.x,Is.EqualTo(11.25f).Within(.05));
        Assert.That(player.TimeRemaining,Is.EqualTo(Read<float>(player,"initialTimeSeconds")).Within(.1));
    }

    [UnityTest] public IEnumerator PuddleFootprintKeepsDryCornersDryAndImportedMapsAreAssigned()
    {
        var zone=GameObject.Find("Genangan_Gang_Timur").GetComponent<Obstacle_Genangan>();
        var cc=GameObject.Find("Player").GetComponent<CharacterController>();
        var overlaps=typeof(Obstacle_Genangan).GetMethod("Overlaps",Private);
        Warp(new Vector3(13.2f,1,3.9f));yield return null;
        Assert.That((bool)overlaps.Invoke(zone,new object[]{cc}),Is.False,"Dry trigger corners must not slow the player.");
        Warp(new Vector3(11.25f,1,.5f));yield return null;
        Assert.That((bool)overlaps.Invoke(zone,new object[]{cc}),Is.True);
        var bank=zone.transform.Find("Genangan_Gang_Timur_Lumpur").GetComponent<MeshRenderer>();
        Assert.That(bank.sharedMaterial.GetTexture("_BaseMap"),Is.Not.Null);
        Assert.That(bank.sharedMaterial.GetTexture("_BumpMap"),Is.Not.Null);
        Assert.That(Object.FindObjectsByType<Obstacle_Genangan>(FindObjectsSortMode.None).Length,Is.EqualTo(3));
    }

    [UnityTest] public IEnumerator RepeatedContactsKeepGuardPresentAndTwoGuardsShareGracePeriod()
    {
        var guard=GameObject.Find("Preman_Graybox").GetComponent<Obstacle_Preman>();
        var player=GameObject.Find("Player").GetComponent<Mission_PengantaranBantuan>();
        Set(guard,"moveSpeed",0f);Set(guard,"patrolSpeed",0f);Set(guard,"contactCooldown",.03f);Set(guard,"sharedContactGraceSeconds",0f);Set(guard,"retreatAfterContactSeconds",0f);
        Warp(guard.transform.position);
        float deadline=Time.realtimeSinceStartup+1;
        while(Read<int>(guard,"contactCount")<4&&Time.realtimeSinceStartup<deadline)yield return null;
        Assert.That(Read<int>(guard,"contactCount"),Is.GreaterThanOrEqualTo(4));
        Assert.That(guard.gameObject.activeSelf,Is.True,"Taking hits must not remove the level's challenge.");
        player.ResetMissionSession();guard.enabled=false;
        var penalty=typeof(Mission_PengantaranBantuan).GetMethod("TryApplyPremanPenalty",Private);
        Assert.That((bool)penalty.Invoke(player,new object[]{10f,1.1f}),Is.True);
        float after=player.TimeRemaining;
        Assert.That((bool)penalty.Invoke(player,new object[]{10f,1.1f}),Is.False);
        Assert.That(player.TimeRemaining,Is.EqualTo(after),"Two simultaneous contacts must not double the penalty.");
    }
}
#endif

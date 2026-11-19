#if UNITY_EDITOR
using System.Collections;
using JejakAsa;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.TestTools;

public sealed class ExitSafetyTests
{
    GameProgressData before;string spawn;
    [SetUp] public void Setup(){before=GameFlowManager.Instance.Progress.Clone();spawn=GameFlowManager.Instance.LastSpawnPoint;}
    [UnityTearDown] public IEnumerator Cleanup()
    {
        GameFlowManager.Instance.SetStateForTesting(before);GameFlowManager.Instance.LastSpawnPoint=spawn;
        var scene=SceneManager.GetActiveScene();SceneManager.SetActiveScene(SceneManager.CreateScene("ExitSafetyCleanup"));
        if(scene.IsValid()&&scene.isLoaded)yield return SceneManager.UnloadSceneAsync(scene);
    }
    [UnityTest] public IEnumerator RunningThroughPoskoGateStopsOnSolidGroundAndInteractionStillWorks()
    {
        GameFlowManager.Instance.SetStateForTesting(new GameProgressData{currentPhase=DayPhase.PoskoBriefing});
        yield return SceneManager.LoadSceneAsync("Assets/_Project/Scenes/Posko.unity");
        var player=GameObject.FindGameObjectWithTag("Player");player.GetComponent<PlayerController>().enabled=false;
        var controller=player.GetComponent<CharacterController>();
        foreach(float x in new[]{-1.4f,0f,1.4f})
        {
            yield return HoldAgainstExit(controller,new Vector3(x,1,-9.5f),new Vector3(0,-.05f,-.16f),-12);
            var gate=Object.FindFirstObjectByType<PoskoGateController>();
            Assert.That(Vector3.Distance(gate.transform.position,player.transform.position),Is.LessThan(gate.interactionRadius),"Physical stop must remain inside the E interaction area.");
            gate.OpenGateMenu();gate.CloseGateMenu();
        }
        foreach(var point in new[]{new Vector3(0,2,-12.5f),new Vector3(0,2,-16),new Vector3(8,2,-20)})AssertGround(point);
    }
    [UnityTest] public IEnumerator RunningAtHomeDoorJambsCannotLeaveFloorAndGardenHasCollision()
    {
        GameFlowManager.Instance.SetStateForTesting(new GameProgressData{currentPhase=DayPhase.RumahPagi});
        yield return SceneManager.LoadSceneAsync("Assets/_Project/Scenes/Rumah.unity");
        var player=GameObject.FindGameObjectWithTag("Player");player.GetComponent<PlayerController>().enabled=false;
        var controller=player.GetComponent<CharacterController>();
        foreach(float x in new[]{.6f,1.1f,2f,2.9f,3.4f})
            yield return HoldAgainstExit(controller,new Vector3(x,1,-3.65f),new Vector3((x<2?.015f:-.015f),-.05f,-.16f),-5);
        foreach(var point in new[]{new Vector3(2,2,-6),new Vector3(2,2,-9),new Vector3(9,2,-7)})AssertGround(point);
    }
    static IEnumerator HoldAgainstExit(CharacterController controller,Vector3 start,Vector3 motion,float limit)
    {
        controller.enabled=false;controller.transform.position=start;controller.enabled=true;Physics.SyncTransforms();
        bool hitSide=false;
        for(int i=0;i<90;i++)
        {
            var flags=controller.Move(motion);hitSide|=(flags&CollisionFlags.Sides)!=0;
            Assert.That(controller.transform.position.y,Is.GreaterThan(.8f),"Player fell while holding movement into the exit.");
            Assert.That(controller.transform.position.z,Is.GreaterThan(limit),"Player passed the physical exit boundary.");
            if(i%6==0)yield return null;
        }
        Assert.That(hitSide,Is.True,"Exit must block movement with a solid collider.");
    }
    static void AssertGround(Vector3 origin)
    {
        RaycastHit hit;Assert.That(Physics.Raycast(origin,Vector3.down,out hit,4,~0,QueryTriggerInteraction.Ignore),Is.True,"Visible earth has no physical support at "+origin);
        Assert.That(hit.point.y,Is.GreaterThan(-.12f));Assert.That(hit.normal.y,Is.GreaterThan(.95f));
    }
}
#endif

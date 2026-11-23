#if UNITY_EDITOR
using System.Collections;
using System.Collections.Generic;
using JejakAsa;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.TestTools;

public sealed class P02HomeNavigationTests
{
    [UnityTest]
    public IEnumerator DressingKeepsRoutesToMotherBedAndExitOpen()
    {
        var flow=StoryFlow_PengaturAlur.Instance;var before=flow.Progress.Clone();var spawn=flow.LastSpawnPoint;
        try
        {
            flow.SetStateForTesting(new GameProgressData { currentPhase=DayPhase.RumahPagi });
            yield return SceneManager.LoadSceneAsync("Assets/_Project/Scenes/Rumah.unity");
            var root=GameObject.Find("P02_Home_Dressing");
            Assert.That(root,Is.Not.Null);
            Assert.That(root.GetComponentsInChildren<BoxCollider>().Length,Is.EqualTo(8));
            Assert.That(root.GetComponentsInChildren<MeshCollider>(),Is.Empty);
            var player=GameObject.FindGameObjectWithTag("Player");
            player.GetComponent<CharacterController>().enabled=false;
            player.GetComponent<Player_Gerak>().enabled=false;
            Physics.SyncTransforms();
            foreach(var target in new[]{new Vector2(-4.7f,.35f),new Vector2(2f,-3.65f),new Vector2(3f,-2.55f)})
                Assert.That(CanReach(new Vector2(player.transform.position.x,player.transform.position.z),target),Is.True,"No clear player-width route to "+target);
        }
        finally {flow.SetStateForTesting(before);flow.LastSpawnPoint=spawn;}
    }
    private static bool CanReach(Vector2 start,Vector2 goal)
    {
        const float step=.25f;
        var first=new Vector2Int(Mathf.RoundToInt(start.x/step),Mathf.RoundToInt(start.y/step));
        var queue=new Queue<Vector2Int>();var seen=new HashSet<Vector2Int>();queue.Enqueue(first);seen.Add(first);
        var directions=new[]{Vector2Int.up,Vector2Int.down,Vector2Int.left,Vector2Int.right};
        while(queue.Count>0)
        {
            var p=queue.Dequeue();Vector2 pos=new Vector2(p.x*step,p.y*step);
            if(Vector2.Distance(pos,goal)<.28f)return true;
            foreach(var d in directions)
            {
                var n=p+d;if(seen.Contains(n))continue;seen.Add(n);
                float x=n.x*step,z=n.y*step;
                if(x< -6.4f || x>6.4f || z< -4.4f || z>4.4f)continue;
                if(Physics.CheckCapsule(new Vector3(x,.52f,z),new Vector3(x,1.48f,z),.48f,~0,QueryTriggerInteraction.Ignore))continue;
                queue.Enqueue(n);
            }
        }
        return false;
    }
    [UnityTearDown] public IEnumerator Cleanup()
    {
        var scene=SceneManager.GetActiveScene();SceneManager.SetActiveScene(SceneManager.CreateScene("P02Cleanup"));
        if(scene.IsValid()&&scene.isLoaded)yield return SceneManager.UnloadSceneAsync(scene);
    }
}
#endif

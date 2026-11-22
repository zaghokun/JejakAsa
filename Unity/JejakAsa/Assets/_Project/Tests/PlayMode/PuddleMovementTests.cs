#if UNITY_EDITOR
using System.Collections;
using JejakAsa;
using NUnit.Framework;
using UnityEditor;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.LowLevel;
using UnityEngine.TestTools;

public sealed class PuddleMovementTests : InputTestFixture
{
    private GameObject root;
    private Transform player;
    private PlayerController movement;
    private Keyboard keyboard;
    private float previousTimeScale;

    public override void Setup()
    {
        base.Setup();
        previousTimeScale = Time.timeScale;
        Time.timeScale = 1f;
        keyboard = InputSystem.AddDevice<Keyboard>();
        root = new GameObject("Puddle test world");
        GameObject floor = GameObject.CreatePrimitive(PrimitiveType.Cube);
        floor.transform.SetParent(root.transform);
        floor.transform.position = new Vector3(0f, -0.5f, 0f);
        floor.transform.localScale = new Vector3(100f, 1f, 100f);

        GameObject actor = new GameObject("Player");
        actor.SetActive(false);
        actor.transform.SetParent(root.transform);
        actor.transform.position = Vector3.up;
        player = actor.transform;
        actor.AddComponent<CharacterController>();
        Transform view = new GameObject("View").transform;
        view.SetParent(root.transform);
        movement = actor.AddComponent<PlayerController>();
        SerializedObject fields = new SerializedObject(movement);
        fields.FindProperty("movementCamera").objectReferenceValue = view;
        fields.ApplyModifiedPropertiesWithoutUndo();
        actor.SetActive(true);
    }

    public override void TearDown()
    {
        Object.DestroyImmediate(root);
        Time.timeScale = previousTimeScale;
        base.TearDown();
    }

    private PuddleZone CreatePuddle(float multiplier)
    {
        GameObject zone = new GameObject("Puddle");
        zone.transform.SetParent(root.transform);
        BoxCollider box = zone.AddComponent<BoxCollider>();
        box.size = new Vector3(30f, 4f, 30f);
        PuddleZone puddle = zone.AddComponent<PuddleZone>();
        SerializedObject fields = new SerializedObject(puddle);
        fields.FindProperty("speedMultiplier").floatValue = multiplier;
        fields.ApplyModifiedPropertiesWithoutUndo();
        return puddle;
    }

    private IEnumerator CheckSpeed(float expected, bool run = false)
    {
        InputSystem.QueueStateEvent(keyboard, run
            ? new KeyboardState(Key.W, Key.LeftShift) : new KeyboardState(Key.W));
        yield return null;
        Vector3 start = player.position;
        float started = Time.time;
        yield return new WaitForSeconds(0.2f);
        float distance = Vector3.ProjectOnPlane(player.position - start, Vector3.up).magnitude;
        Assert.That(distance, Is.EqualTo(expected * (Time.time - started)).Within(0.05f));
        InputSystem.QueueStateEvent(keyboard, new KeyboardState());
        yield return null;
    }

    [UnityTest]
    public IEnumerator OverlappingPuddlesSlowWalkingAndRunningUntilExitOrDisable()
    {
        PuddleZone first = CreatePuddle(0.5f);
        PuddleZone second = CreatePuddle(0.25f);
        Physics.SyncTransforms();
        yield return new WaitForFixedUpdate();
        yield return CheckSpeed(0.875f);
        yield return CheckSpeed(1.75f, true);

        second.transform.position = Vector3.right * 100f;
        Physics.SyncTransforms();
        yield return new WaitForFixedUpdate();
        yield return CheckSpeed(1.75f);
        first.enabled = false;
        yield return CheckSpeed(3.5f);
        first.enabled = true;
        yield return new WaitForFixedUpdate();
        yield return CheckSpeed(1.75f);
        Object.Destroy(first.gameObject);
        yield return null;
        yield return CheckSpeed(7f, true);
    }

    [UnityTest]
    public IEnumerator DisablingMovementDoesNotLeaveStaleSlowdownAfterExit()
    {
        PuddleZone puddle = CreatePuddle(0.5f);
        Physics.SyncTransforms();
        yield return new WaitForFixedUpdate();
        yield return CheckSpeed(1.75f);
        movement.enabled = false;
        puddle.transform.position = Vector3.right * 100f;
        Physics.SyncTransforms();
        yield return new WaitForFixedUpdate();
        movement.enabled = true;
        yield return CheckSpeed(3.5f);
    }
    [UnityTest]
    public IEnumerator TeleportingWithControllerDisabledReconcilesPuddles()
    {
        CreatePuddle(0.5f);
        PuddleZone other = CreatePuddle(0.25f);
        other.transform.position = Vector3.right * 50f;
        Physics.SyncTransforms();
        yield return new WaitForFixedUpdate();
        yield return CheckSpeed(1.75f);

        CharacterController controller = player.GetComponent<CharacterController>();
        controller.enabled = false;
        player.position = new Vector3(25f, 1f, 0f);
        controller.enabled = true;
        yield return CheckSpeed(3.5f);

        controller.enabled = false;
        player.position = new Vector3(50f, 1f, 0f);
        controller.enabled = true;
        yield return CheckSpeed(0.875f);

        controller.enabled = false;
        player.position = new Vector3(51f, 1f, 0f);
        controller.enabled = true;
        yield return CheckSpeed(0.875f);
    }
}
#endif

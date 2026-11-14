#if UNITY_EDITOR
using System.Collections;
using JejakAsa;
using NUnit.Framework;
using UnityEditor;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.LowLevel;
using UnityEngine.TestTools;

public sealed class U02MovementTests : InputTestFixture
{
    private GameObject root;
    private Transform player;
    private Transform view;
    private CharacterController controller;
    private Keyboard keyboard;
    private float previousTimeScale;

    public override void Setup()
    {
        base.Setup();
        previousTimeScale = Time.timeScale;
        Time.timeScale = 1f;
        keyboard = InputSystem.AddDevice<Keyboard>();
        root = new GameObject("U02 test world");
        GameObject floor = GameObject.CreatePrimitive(PrimitiveType.Cube);
        floor.transform.SetParent(root.transform);
        floor.transform.position = new Vector3(0f, -0.5f, 0f);
        floor.transform.localScale = new Vector3(100f, 1f, 100f);

        GameObject actor = new GameObject("Test player");
        actor.SetActive(false);
        actor.transform.SetParent(root.transform);
        actor.transform.position = new Vector3(0f, 1f, 0f);
        player = actor.transform;
        controller = actor.AddComponent<CharacterController>();
        controller.height = 2f;
        controller.radius = 0.5f;
        controller.skinWidth = 0.03f;

        GameObject cameraObject = new GameObject("Test view");
        cameraObject.SetActive(false);
        cameraObject.transform.SetParent(root.transform);
        view = cameraObject.transform;
        view.position = new Vector3(12f, 13f, -12f);
        view.rotation = Quaternion.Euler(35.264f, -45f, 0f);

        PlayerController movement = actor.AddComponent<PlayerController>();
        SerializedObject movementFields = new SerializedObject(movement);
        movementFields.FindProperty("movementCamera").objectReferenceValue = view;
        movementFields.ApplyModifiedPropertiesWithoutUndo();
        FixedIsometricCamera follow = cameraObject.AddComponent<FixedIsometricCamera>();
        SerializedObject cameraFields = new SerializedObject(follow);
        cameraFields.FindProperty("target").objectReferenceValue = player;
        cameraFields.ApplyModifiedPropertiesWithoutUndo();
        actor.SetActive(true);
        cameraObject.SetActive(true);
        Physics.SyncTransforms();
    }

    public override void TearDown()
    {
        Object.DestroyImmediate(root);
        Time.timeScale = previousTimeScale;
        base.TearDown();
    }

    [UnityTest]
    public IEnumerator StraightAndDiagonalHaveTheSameSpeedAndCameraStaysFixed()
    {
        yield return null;
        yield return new WaitForEndOfFrame();
        Vector3 offset = view.position - player.position;
        Quaternion orientation = view.rotation;
        Vector3 start = player.position;
        float started = Time.time;
        SetKeys(Key.W);
        yield return new WaitForSeconds(0.5f);
        float elapsed = Time.time - started;
        Vector3 straight = Vector3.ProjectOnPlane(player.position - start, Vector3.up);
        Assert.That(straight.magnitude, Is.EqualTo(3.5f * elapsed).Within(0.03f));
        Assert.That(Vector3.Dot(straight.normalized, new Vector3(-1f, 0f, 1f).normalized), Is.GreaterThan(0.999f));
        SetKeys();
        yield return null;

        controller.enabled = false;
        player.position = new Vector3(0f, 1f, 0f);
        controller.enabled = true;
        yield return null;
        start = player.position;
        started = Time.time;
        SetKeys(Key.W, Key.D);
        yield return new WaitForSeconds(0.5f);
        elapsed = Time.time - started;
        Vector3 diagonal = Vector3.ProjectOnPlane(player.position - start, Vector3.up);
        Assert.That(diagonal.magnitude, Is.EqualTo(3.5f * elapsed).Within(0.03f));
        Assert.That(Mathf.Abs(diagonal.x), Is.LessThan(0.02f));
        SetKeys();
        yield return null;
        yield return new WaitForEndOfFrame();
        Assert.That(Quaternion.Angle(view.rotation, orientation), Is.LessThan(0.01f));
        Assert.That((view.position - player.position - offset).magnitude, Is.LessThan(0.02f));
    }

    [UnityTest]
    public IEnumerator EitherShiftRunsAndReleasingReturnsToWalking()
    {
        yield return null;
        foreach (Key shift in new[] { Key.LeftShift, Key.RightShift })
        {
            Vector3 start = player.position;
            float started = Time.time;
            SetKeys(Key.W, shift);
            yield return new WaitForSeconds(0.3f);
            float elapsed = Time.time - started;
            Assert.That(Vector3.ProjectOnPlane(player.position - start, Vector3.up).magnitude,
                Is.EqualTo(7f * elapsed).Within(0.03f));
        }

        Vector3 walkStart = player.position;
        float walkStarted = Time.time;
        SetKeys(Key.W);
        yield return new WaitForSeconds(0.3f);
        Assert.That(Vector3.ProjectOnPlane(player.position - walkStart, Vector3.up).magnitude,
            Is.EqualTo(3.5f * (Time.time - walkStarted)).Within(0.03f));
    }

    [UnityTest]
    public IEnumerator DuplicateBindingsDoNotAddSpeedAndOppositeDirectionsCancel()
    {
        yield return null;
        Vector3 start = player.position;
        float started = Time.time;
        SetKeys(Key.W, Key.UpArrow);
        yield return new WaitForSeconds(0.3f);
        float elapsed = Time.time - started;
        Assert.That(Vector3.ProjectOnPlane(player.position - start, Vector3.up).magnitude,
            Is.EqualTo(3.5f * elapsed).Within(0.03f));
        SetKeys();
        yield return null;

        start = player.position;
        SetKeys(Key.W, Key.S, Key.A, Key.D);
        yield return new WaitForSeconds(0.3f);
        Assert.That(Vector3.ProjectOnPlane(player.position - start, Vector3.up).magnitude, Is.LessThan(0.01f));
    }

    [UnityTest]
    public IEnumerator HouseColliderStopsThePlayerAndTheGroundSupportsThem()
    {
        CreateWall();
        yield return null;
        SetKeys(Key.W, Key.D);
        yield return new WaitForSeconds(1f);
        Assert.That(player.position.z, Is.InRange(1.8f, 2.08f));
        Assert.That(Mathf.Abs(player.position.x), Is.LessThan(0.02f));
        Assert.That(player.position.y, Is.InRange(0.95f, 1.1f));
    }

    [UnityTest]
    public IEnumerator WalkingDiagonallyIntoAHouseSlidesAlongItsWall()
    {
        CreateWall();
        controller.enabled = false;
        player.position = new Vector3(0f, 1f, 1.8f);
        controller.enabled = true;
        yield return null;
        SetKeys(Key.W);
        yield return new WaitForSeconds(1f);
        Assert.That(player.position.z, Is.LessThanOrEqualTo(2.08f));
        Assert.That(player.position.x, Is.LessThan(-1.5f));
    }

    private void SetKeys(params Key[] keys)
    {
        // Queue one keyboard state: separate bit-packed key events can overwrite each other.
        InputSystem.QueueStateEvent(keyboard, new KeyboardState(keys));
    }

    private void CreateWall()
    {
        GameObject wall = GameObject.CreatePrimitive(PrimitiveType.Cube);
        wall.transform.SetParent(root.transform);
        wall.transform.position = new Vector3(0f, 1.5f, 3f);
        wall.transform.localScale = new Vector3(8f, 3f, 1f);
        Physics.SyncTransforms();
    }
}
#endif

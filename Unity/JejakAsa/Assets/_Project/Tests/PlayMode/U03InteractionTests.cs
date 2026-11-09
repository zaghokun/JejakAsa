#if UNITY_EDITOR
using System.Collections;
using JejakAsa;
using NUnit.Framework;
using TMPro;
using UnityEditor;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.LowLevel;
using UnityEngine.TestTools;
using UnityEngine.UI;

public sealed class U03InteractionTests : InputTestFixture
{
    private GameObject root;
    private Transform player;
    private PlayerController movement;
    private PlayerInteraction interaction;
    private GameObject prompt;
    private GameObject dialogue;
    private Keyboard keyboard;
    private float previousTimeScale;

    public override void Setup()
    {
        base.Setup();
        previousTimeScale = Time.timeScale;
        Time.timeScale = 1f;
        keyboard = InputSystem.AddDevice<Keyboard>();
        root = new GameObject("U03 test world");
        GameObject floor = GameObject.CreatePrimitive(PrimitiveType.Cube);
        floor.transform.SetParent(root.transform);
        floor.transform.position = new Vector3(0f, -0.5f, 0f);
        floor.transform.localScale = new Vector3(30f, 1f, 30f);
        GameObject view = new GameObject("Test view");
        view.transform.SetParent(root.transform);

        GameObject actor = new GameObject("Test player");
        actor.SetActive(false);
        actor.transform.SetParent(root.transform);
        actor.transform.position = new Vector3(0f, 1f, 0f);
        player = actor.transform;
        CharacterController controller = actor.AddComponent<CharacterController>();
        controller.height = 2f;
        controller.radius = 0.5f;
        movement = actor.AddComponent<PlayerController>();
        SetReference(movement, "movementCamera", view.transform);

        GameObject canvas = UiObject("Canvas", root.transform);
        canvas.AddComponent<Canvas>().renderMode = RenderMode.ScreenSpaceOverlay;
        prompt = UiObject("Prompt", canvas.transform);
        TMP_Text promptText = UiObject("Prompt text", prompt.transform).AddComponent<TextMeshProUGUI>();
        dialogue = UiObject("Dialogue", canvas.transform);
        TMP_Text title = UiObject("Title", dialogue.transform).AddComponent<TextMeshProUGUI>();
        TMP_Text body = UiObject("Body", dialogue.transform).AddComponent<TextMeshProUGUI>();
        Button close = UiObject("Close", dialogue.transform).AddComponent<Button>();
        interaction = actor.AddComponent<PlayerInteraction>();
        SetReference(interaction, "promptPanel", prompt);
        SetReference(interaction, "promptText", promptText);
        SetReference(interaction, "dialoguePanel", dialogue);
        SetReference(interaction, "dialogueTitle", title);
        SetReference(interaction, "dialogueBody", body);
        SetReference(interaction, "closeButton", close);
        actor.SetActive(true);
        Physics.SyncTransforms();
    }

    public override void TearDown()
    {
        Object.DestroyImmediate(root);
        Time.timeScale = previousTimeScale;
        base.TearDown();
    }

    [UnityTest]
    public IEnumerator OnePressSelectsOnlyTheNearestAndHoldingEDoesNotReopen()
    {
        InteractionTarget near = Target("Near", 1f);
        InteractionTarget other = Target("Other", 1.4f);
        yield return null;
        Assert.That(interaction.CurrentTarget, Is.SameAs(near));
        Assert.That(prompt.activeSelf, Is.True);
        Keys(Key.E);
        yield return null;
        Assert.That(interaction.IsDialogueOpen, Is.True);
        Assert.That(interaction.DialogueTarget, Is.SameAs(near));
        other.transform.position = player.position + Vector3.right * 0.2f;
        yield return null;
        Assert.That(interaction.DialogueTarget, Is.SameAs(near), "An open conversation must not switch to another nearby NPC.");
        Keys(Key.E, Key.Escape);
        yield return null;
        Assert.That(interaction.IsDialogueOpen, Is.False);
        Keys(Key.E);
        yield return null;
        yield return null;
        Assert.That(interaction.IsDialogueOpen, Is.False, "E is still held, not newly pressed.");
        Keys();
        yield return null;
        Keys(Key.E);
        yield return null;
        Assert.That(interaction.DialogueTarget, Is.SameAs(other));
    }

    [UnityTest]
    public IEnumerator RangeBoundaryAndInactiveTargetsAreRespected()
    {
        InteractionTarget target = Target("Boundary", 1.8f);
        yield return null;
        Assert.That(interaction.CurrentTarget, Is.SameAs(target));
        target.transform.position = player.position + Vector3.right * 1.81f;
        Keys(Key.E);
        yield return null;
        Assert.That(interaction.IsDialogueOpen, Is.False);
        Assert.That(prompt.activeSelf, Is.False);

        InteractionTarget disabled = Target("Disabled", 0.2f);
        disabled.enabled = false;
        InteractionTarget valid = Target("Valid", 1.5f);
        Keys();
        yield return null;
        Assert.That(interaction.CurrentTarget, Is.SameAs(valid));
        valid.gameObject.SetActive(false);
        yield return null;
        Assert.That(prompt.activeSelf, Is.False);
    }

    [UnityTest]
    public IEnumerator DialogueStopsMovementAndEscapeRestoresIt()
    {
        Target("Resident", 1f);
        yield return null;
        Vector3 start = player.position;
        Keys(Key.E, Key.W);
        yield return new WaitForSeconds(0.2f);
        Assert.That(dialogue.activeSelf, Is.True);
        Assert.That(movement.enabled, Is.False);
        Assert.That(Vector3.ProjectOnPlane(player.position - start, Vector3.up).magnitude, Is.LessThan(0.01f));
        Keys(Key.Escape, Key.W);
        yield return new WaitForSeconds(0.2f);
        Assert.That(dialogue.activeSelf, Is.False);
        Assert.That(movement.enabled, Is.True);
        Assert.That(Vector3.ProjectOnPlane(player.position - start, Vector3.up).magnitude, Is.GreaterThan(0.3f));
    }

    [UnityTest]
    public IEnumerator RemovingTargetOrDisablingInteractionCannotLeaveMovementLocked()
    {
        InteractionTarget target = Target("Resident", 1f);
        yield return null;
        Keys(Key.E);
        yield return null;
        Assert.That(interaction.IsDialogueOpen, Is.True);
        Object.DestroyImmediate(target.gameObject);
        yield return null;
        Assert.That(dialogue.activeSelf, Is.False);
        Assert.That(movement.enabled, Is.True);

        Target("New resident", 1f);
        Keys();
        yield return null;
        Keys(Key.E);
        yield return null;
        Assert.That(interaction.IsDialogueOpen, Is.True);
        interaction.enabled = false;
        Assert.That(dialogue.activeSelf, Is.False);
        Assert.That(prompt.activeSelf, Is.False);
        Assert.That(movement.enabled, Is.True);
    }

    [UnityTest]
    public IEnumerator EqualDistanceTargetsKeepASingleStableSelection()
    {
        InteractionTarget first = Target("Left", -1f);
        InteractionTarget second = Target("Right", 1f);
        yield return null;
        InteractionTarget selected = interaction.CurrentTarget;
        Assert.That(selected == first || selected == second, Is.True);
        for (int i = 0; i < 4; i++)
        {
            yield return null;
            Assert.That(interaction.CurrentTarget, Is.SameAs(selected));
        }
        Keys(Key.E);
        yield return null;
        Assert.That(interaction.DialogueTarget, Is.SameAs(selected));
    }

    private InteractionTarget Target(string name, float offset)
    {
        GameObject target = new GameObject(name);
        target.transform.SetParent(root.transform);
        target.transform.position = player.position + Vector3.right * offset;
        return target.AddComponent<InteractionTarget>();
    }

    private void Keys(params Key[] keys)
    {
        InputSystem.QueueStateEvent(keyboard, new KeyboardState(keys));
    }

    private static GameObject UiObject(string name, Transform parent)
    {
        GameObject result = new GameObject(name, typeof(RectTransform));
        result.transform.SetParent(parent, false);
        return result;
    }

    private static void SetReference(Object component, string field, Object value)
    {
        SerializedObject fields = new SerializedObject(component);
        fields.FindProperty(field).objectReferenceValue = value;
        fields.ApplyModifiedPropertiesWithoutUndo();
    }
}
#endif

#if UNITY_EDITOR
using System.Collections;
using System.Reflection;
using JejakAsa;
using NUnit.Framework;
using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.LowLevel;
using UnityEngine.TestTools;
using UnityEngine.UI;

public sealed class U06TimerTests : InputTestFixture
{
    private GameObject root;
    private Transform player;
    private PlayerInteraction interaction;
    private InteractionTarget resident;
    private Keyboard keyboard;

    public override void Setup()
    {
        base.Setup();
        keyboard = InputSystem.AddDevice<Keyboard>();
        root = new GameObject("U06 test world");

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

        PlayerController movement = actor.AddComponent<PlayerController>();
        SetReference(movement, "movementCamera", view.transform);

        GameObject canvas = new GameObject("Canvas", typeof(RectTransform));
        canvas.transform.SetParent(root.transform, false);
        canvas.AddComponent<Canvas>().renderMode = RenderMode.ScreenSpaceOverlay;

        GameObject prompt = UiObject("Prompt", canvas.transform);
        TMP_Text promptText = UiObject("Prompt text", prompt.transform).AddComponent<TextMeshProUGUI>();

        GameObject dialogue = UiObject("Dialogue", canvas.transform);
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

        GameObject npc = GameObject.CreatePrimitive(PrimitiveType.Capsule);
        npc.name = "Warga 01";
        npc.transform.SetParent(root.transform);
        npc.transform.position = new Vector3(0f, 1f, 1.2f);
        resident = npc.AddComponent<InteractionTarget>();

        Physics.SyncTransforms();
    }

    public override void TearDown()
    {
        Object.DestroyImmediate(root);
        base.TearDown();
    }

    [UnityTest]
    public IEnumerator TimerDecreasesDuringExplorationAndFreezesDuringDialogue()
    {
        float initialTime = interaction.TimeRemaining;
        Assert.That(initialTime, Is.GreaterThan(179f).And.LessThanOrEqualTo(180f), "Timer harus mulai dari sekitar 180 detik.");
        yield return null;

        // Tunggu 0.2 detik game time saat eksplorasi
        yield return new WaitForSeconds(0.2f);
        Assert.That(interaction.TimeRemaining, Is.LessThan(initialTime), "Timer harus berkurang saat eksplorasi aktif.");

        // Buka dialog dengan warga
        InputSystem.QueueStateEvent(keyboard, new KeyboardState(Key.E));
        yield return null;
        InputSystem.QueueStateEvent(keyboard, new KeyboardState());
        yield return null;
        Assert.That(interaction.IsDialogueOpen, Is.True);

        float timeWhenOpened = interaction.TimeRemaining;
        // Tunggu 0.2 detik saat dialog terbuka
        yield return new WaitForSeconds(0.2f);
        Assert.That(interaction.TimeRemaining, Is.EqualTo(timeWhenOpened), "Timer harus membeku / berhenti saat dialog terbuka.");

        // Tutup dialog
        InputSystem.QueueStateEvent(keyboard, new KeyboardState(Key.Escape));
        yield return null;
        InputSystem.QueueStateEvent(keyboard, new KeyboardState());
        yield return null;
        Assert.That(interaction.IsDialogueOpen, Is.False);

        // Tunggu lagi setelah dialog ditutup
        yield return new WaitForSeconds(0.2f);
        Assert.That(interaction.TimeRemaining, Is.LessThan(timeWhenOpened), "Timer harus lanjut berkurang setelah dialog ditutup.");
    }

    [UnityTest]
    public IEnumerator DeliveryRejectedWhenTimeExpired()
    {
        yield return null;
        // Simulasikan waktu habis (0 detik)
        interaction.SetTimeRemaining(0f);
        Assert.That(interaction.IsTimeExpired, Is.True);

        // Buka dialog di depan warga saat waktu habis
        InputSystem.QueueStateEvent(keyboard, new KeyboardState(Key.E));
        yield return null;
        InputSystem.QueueStateEvent(keyboard, new KeyboardState());
        yield return null;

        // Penyerahan harus ditolak karena waktu habis (U06 & U07)
        Assert.That(interaction.PackagesRemaining, Is.EqualTo(3), "Paket tidak boleh berkurang saat waktu habis.");
        Assert.That(interaction.Trust, Is.EqualTo(0), "Trust tidak boleh bertambah saat waktu habis.");
        Assert.That(resident.IsDelivered, Is.False);
    }

    private static GameObject UiObject(string name, Transform parent)
    {
        GameObject result = new GameObject(name, typeof(RectTransform));
        result.transform.SetParent(parent, false);
        return result;
    }

    private static void SetReference(object target, string fieldName, object value)
    {
        FieldInfo field = target.GetType().GetField(fieldName, BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public);
        Assert.That(field, Is.Not.Null, "Field " + fieldName + " not found on " + target);
        field.SetValue(target, value);
    }
}
#endif


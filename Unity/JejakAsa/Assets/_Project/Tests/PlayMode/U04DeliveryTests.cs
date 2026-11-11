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

public sealed class U04DeliveryTests : InputTestFixture
{
    private GameObject root;
    private Transform player;
    private PlayerInteraction interaction;
    private InteractionTarget resident;
    private TMP_Text bodyText;
    private Button closeBtn;
    private Keyboard keyboard;

    public override void Setup()
    {
        base.Setup();
        keyboard = InputSystem.AddDevice<Keyboard>();
        root = new GameObject("U04 test world");

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

        GameObject canvas = new GameObject("Canvas");
        canvas.transform.SetParent(root.transform);
        canvas.AddComponent<Canvas>().renderMode = RenderMode.ScreenSpaceOverlay;

        GameObject prompt = new GameObject("Prompt");
        prompt.transform.SetParent(canvas.transform);
        TMP_Text promptText = prompt.AddComponent<TextMeshProUGUI>();

        GameObject dialogue = new GameObject("Dialogue");
        dialogue.transform.SetParent(canvas.transform);
        TMP_Text title = dialogue.AddComponent<TextMeshProUGUI>();
        bodyText = dialogue.AddComponent<TextMeshProUGUI>();
        closeBtn = dialogue.AddComponent<Button>();

        interaction = actor.AddComponent<PlayerInteraction>();
        SetReference(interaction, "promptPanel", prompt);
        SetReference(interaction, "promptText", promptText);
        SetReference(interaction, "dialoguePanel", dialogue);
        SetReference(interaction, "dialogueTitle", title);
        SetReference(interaction, "dialogueBody", bodyText);
        SetReference(interaction, "closeButton", closeBtn);

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
    public IEnumerator PressingEDeliversBansosInstantlyAndIncrementsTrustExactlyOnce()
    {
        yield return null;
        Assert.That(interaction.CurrentTarget, Is.SameAs(resident));
        Assert.That(interaction.PackagesRemaining, Is.EqualTo(3));
        Assert.That(interaction.Trust, Is.EqualTo(0));
        Assert.That(interaction.DeliveredCount, Is.EqualTo(0));

        // Tekan tombol E di depan warga
        InputSystem.QueueStateEvent(keyboard, new KeyboardState(Key.E));
        yield return null;
        InputSystem.QueueStateEvent(keyboard, new KeyboardState());
        yield return null;

        // Dialog langsung terbuka DAN paket otomatis langsung terkirim (U04)
        Assert.That(interaction.IsDialogueOpen, Is.True);
        Assert.That(interaction.PackagesRemaining, Is.EqualTo(2), "Paket harus berkurang 1 menjadi 2.");
        Assert.That(interaction.DeliveredCount, Is.EqualTo(1), "Jumlah warga terkirim harus bertambah menjadi 1.");
        Assert.That(interaction.Trust, Is.EqualTo(1), "Kepercayaan warga harus naik menjadi 1.");
        Assert.That(resident.IsDelivered, Is.True, "Status warga harus ditandai sudah menerima bansos.");

        // Tutup dialog
        InputSystem.QueueStateEvent(keyboard, new KeyboardState(Key.Escape));
        yield return null;
        InputSystem.QueueStateEvent(keyboard, new KeyboardState());
        yield return null;
        Assert.That(interaction.IsDialogueOpen, Is.False);

        // Dekati dan tekan E lagi ke warga yang sama
        InputSystem.QueueStateEvent(keyboard, new KeyboardState(Key.E));
        yield return null;
        InputSystem.QueueStateEvent(keyboard, new KeyboardState());
        yield return null;

        // Dialog terbuka, tapi paket dan trust TIDAK BOLEH berkurang/bertambah ganda
        Assert.That(interaction.IsDialogueOpen, Is.True);
        Assert.That(interaction.PackagesRemaining, Is.EqualTo(2), "Paket tidak boleh berkurang lagi.");
        Assert.That(interaction.DeliveredCount, Is.EqualTo(1), "Jumlah warga terkirim tidak boleh bertambah lagi.");
        Assert.That(interaction.Trust, Is.EqualTo(1), "Kepercayaan tidak boleh bertambah lagi.");
    }

    private static void SetReference(object target, string fieldName, object value)
    {
        FieldInfo field = target.GetType().GetField(fieldName, BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public);
        Assert.That(field, Is.Not.Null, "Field " + fieldName + " not found on " + target);
        field.SetValue(target, value);
    }
}
#endif

#if UNITY_EDITOR
using System.Collections;
using System.Reflection;
using JejakAsa;
using NUnit.Framework;
using TMPro;
using UnityEngine;
using UnityEngine.TestTools;
using UnityEngine.UI;

public sealed class PremanControllerTests
{
    private GameObject root;
    private PlayerInteraction player;
    private PremanController preman;
    private float previousTimeScale;

    [SetUp]
    public void SetUp()
    {
        previousTimeScale = Time.timeScale;
        Time.timeScale = 1f;
        root = new GameObject("Preman test world");
        GameObject floor = GameObject.CreatePrimitive(PrimitiveType.Cube);
        floor.transform.SetParent(root.transform);
        floor.transform.position = new Vector3(0f, -0.5f, 0f);
        floor.transform.localScale = new Vector3(40f, 1f, 40f);
        GameObject view = new GameObject("View");
        view.transform.SetParent(root.transform);
        GameObject actor = new GameObject("Player");
        actor.SetActive(false);
        actor.transform.SetParent(root.transform);
        actor.transform.position = Vector3.up;
        actor.AddComponent<CharacterController>();
        PlayerController movement = actor.AddComponent<PlayerController>();
        SetField(movement, "movementCamera", view.transform);
        GameObject canvas = new GameObject("Canvas", typeof(RectTransform), typeof(Canvas));
        canvas.transform.SetParent(root.transform);
        GameObject prompt = Ui("Prompt", canvas.transform);
        TMP_Text promptText = Ui("Prompt text", prompt.transform).AddComponent<TextMeshProUGUI>();
        GameObject dialogue = Ui("Dialogue", canvas.transform);
        TMP_Text title = Ui("Title", dialogue.transform).AddComponent<TextMeshProUGUI>();
        TMP_Text body = Ui("Body", dialogue.transform).AddComponent<TextMeshProUGUI>();
        Button close = Ui("Close", dialogue.transform).AddComponent<Button>();
        player = actor.AddComponent<PlayerInteraction>();
        SetField(player, "promptPanel", prompt);
        SetField(player, "promptText", promptText);
        SetField(player, "dialoguePanel", dialogue);
        SetField(player, "dialogueTitle", title);
        SetField(player, "dialogueBody", body);
        SetField(player, "closeButton", close);
        actor.SetActive(true);
        movement.enabled = false;
        SetField(player, "isMissionActive", true);
        GameObject enemy = new GameObject("Preman");
        enemy.SetActive(false);
        enemy.transform.SetParent(root.transform);
        enemy.transform.position = new Vector3(0.9f, 1f, 0f);
        enemy.AddComponent<CharacterController>();
        preman = enemy.AddComponent<PremanController>();
        SetField(preman, "player", player);
        enemy.SetActive(true);
        Physics.SyncTransforms();
    }

    [TearDown]
    public void TearDown()
    {
        Time.timeScale = previousTimeScale;
        Object.DestroyImmediate(root);
    }

    [UnityTest]
    public IEnumerator ContactPenalizesOnceThenAgainAfterCooldown()
    {
        SetField(preman, "contactCooldown", 0.2f);
        yield return WaitForContact();
        float afterHit = player.TimeRemaining;
        Assert.That(afterHit, Is.LessThan(171f).And.GreaterThan(169f));
        yield return new WaitForSeconds(0.08f);
        Assert.That(player.TimeRemaining, Is.GreaterThan(afterHit - 1f));
        yield return new WaitForSeconds(0.2f);
        Assert.That(player.TimeRemaining, Is.LessThan(afterHit - 10f));
    }

    [UnityTest]
    public IEnumerator PremanDisappearsAfterThreeContactsAndReturnsOnRetry()
    {
        SetField(preman, "contactCooldown", 0.15f);
        yield return WaitForContact();
        Assert.That(preman.gameObject.activeSelf, Is.True);
        yield return WaitForContact();
        Assert.That(preman.gameObject.activeSelf, Is.True);
        yield return WaitForContact();
        Assert.That(preman.gameObject.activeSelf, Is.False);

        float afterLastContact = player.TimeRemaining;
        yield return new WaitForSeconds(0.2f);
        Assert.That(player.TimeRemaining, Is.GreaterThan(afterLastContact - 1f));

        player.ResetMissionSession();
        Assert.That(preman.gameObject.activeSelf, Is.True);
        Assert.That(preman.transform.position.x, Is.EqualTo(0.9f).Within(0.01f));
        yield return WaitForContact();
        Assert.That(preman.gameObject.activeSelf, Is.True);
    }

    [UnityTest]
    public IEnumerator PlayerPassesThroughPremanAndContactStillCostsTime()
    {
        SetField(preman, "moveSpeed", 0f);
        CharacterController playerCollider = player.GetComponent<CharacterController>();
        CharacterController enemyCollider = preman.GetComponent<CharacterController>();
        Assert.That(Physics.GetIgnoreCollision(playerCollider, enemyCollider), Is.True);
        SetField(preman, "contactCooldown", 10f);
        float beforeCrossing = player.TimeRemaining;
        for (int i = 0; i < 20; i++)
        {
            playerCollider.Move(Vector3.right * 0.15f);
            yield return null;
        }
        Assert.That(player.transform.position.x, Is.GreaterThan(2.5f));
        Assert.That(player.TimeRemaining, Is.LessThan(beforeCrossing - 9f), "Kontak saat melintas harus mengurangi waktu.");
        Object.DestroyImmediate(preman);
        Assert.That(Physics.GetIgnoreCollision(playerCollider, enemyCollider), Is.False);
    }

    [UnityTest]
    public IEnumerator RadiusStopsChaseAndWallBlocksMovement()
    {
        preman.transform.position = new Vector3(9f, 1f, 0f);
        Physics.SyncTransforms();
        yield return new WaitForSeconds(0.1f);
        Assert.That(preman.transform.position.x, Is.EqualTo(9f).Within(0.01f));
        preman.transform.position = new Vector3(4f, 1f, 0f);
        GameObject wall = GameObject.CreatePrimitive(PrimitiveType.Cube);
        wall.transform.SetParent(root.transform);
        wall.transform.position = new Vector3(2f, 1f, 0f);
        wall.transform.localScale = new Vector3(0.5f, 3f, 5f);
        Physics.SyncTransforms();
        yield return new WaitForSeconds(1f);
        Assert.That(preman.transform.position.x, Is.GreaterThan(2.6f));
        Assert.That(preman.transform.position.x, Is.LessThan(4f));
        Assert.That(player.TimeRemaining, Is.GreaterThan(175f));
        preman.transform.position = new Vector3(9f, 1f, 0f);
        Physics.SyncTransforms();
        yield return new WaitForSeconds(0.1f);
        Assert.That(preman.transform.position.x, Is.EqualTo(9f).Within(0.01f));
    }

    [UnityTest]
    public IEnumerator ChasesAroundWallAndReachesPlayerWithoutCrossingIt()
    {
        preman.transform.position = new Vector3(4f, 1f, 0f);
        GameObject wall = GameObject.CreatePrimitive(PrimitiveType.Cube);
        wall.transform.SetParent(root.transform);
        wall.transform.position = new Vector3(2f, 1f, 0f);
        wall.transform.localScale = new Vector3(0.5f, 3f, 5f);
        Physics.SyncTransforms();
        float deadline = Time.time + 8f;
        while (Time.time < deadline && player.TimeRemaining > 169f)
        {
            yield return null;
            Vector3 position = preman.transform.position;
            Assert.That(position.x > 2.25f || position.x < 1.75f || Mathf.Abs(position.z) > 2.5f,
                Is.True, "Preman tidak boleh melewati collider pembatas.");
        }
        Assert.That(player.TimeRemaining, Is.LessThan(169f), "Preman harus mengitari ujung pembatas dan menyentuh pemain.");
    }

    [UnityTest]
    public IEnumerator DialogueAndPauseFreezeChaseAndCooldown()
    {
        SetField(preman, "contactCooldown", 0.2f);
        yield return WaitForContact();
        float afterHit = player.TimeRemaining;
        player.OpenSystemDialogue("Test", "Test");
        Vector3 position = preman.transform.position;
        yield return new WaitForSeconds(0.3f);
        Assert.That(preman.transform.position, Is.EqualTo(position));
        Assert.That(player.TimeRemaining, Is.EqualTo(afterHit));
        ((Button)typeof(PlayerInteraction).GetField("closeButton", BindingFlags.Instance | BindingFlags.NonPublic)
            .GetValue(player)).onClick.Invoke();
        yield return new WaitForSeconds(0.05f);
        Assert.That(player.TimeRemaining, Is.GreaterThan(afterHit - 1f));
        yield return new WaitForSeconds(0.2f);
        Assert.That(player.TimeRemaining, Is.LessThan(afterHit - 10f));
        player.ResetMissionSession();
        preman.transform.position = new Vector3(4f, 1f, 0f);
        Time.timeScale = 0f;
        position = preman.transform.position;
        yield return new WaitForSecondsRealtime(0.1f);
        Assert.That(preman.transform.position, Is.EqualTo(position));
        Time.timeScale = 1f;
        yield return new WaitForSeconds(0.1f);
        Assert.That(preman.transform.position.x, Is.LessThan(position.x));
    }

    [UnityTest]
    public IEnumerator ContactTimeoutOpensOutcomeAndRetryResetsDisabledPreman()
    {
        player.SetTimeRemaining(5f);
        yield return WaitForContact();
        Assert.That(player.TimeRemaining, Is.Zero);
        Assert.That(player.Outcome, Is.EqualTo(MissionOutcome.Failed));
        Assert.That(player.IsOutcomeOpen, Is.True);
        Vector3 stopped = preman.transform.position;
        yield return new WaitForSeconds(0.1f);
        Assert.That(preman.transform.position, Is.EqualTo(stopped));
        preman.enabled = false;
        preman.transform.position = new Vector3(4f, 1f, 0f);
        player.ResetMissionSession();
        Assert.That(preman.transform.position.x, Is.EqualTo(0.9f).Within(0.01f));
        preman.enabled = true;
        Physics.SyncTransforms();
        yield return WaitForContact();
        Assert.That(player.TimeRemaining, Is.LessThan(171f));
    }

    [Test]
    public void TimeoutKeepsDeliveryPriority()
    {
        SetField(player, "deliveredCount", 3);
        player.SetTimeRemaining(0f);
        Assert.That(player.Outcome, Is.EqualTo(MissionOutcome.Success));
        player.ResetMissionSession();
        SetField(player, "deliveredCount", 1);
        player.SetTimeRemaining(0f);
        Assert.That(player.Outcome, Is.EqualTo(MissionOutcome.PartialSuccess));
    }

    private IEnumerator WaitForContact()
    {
        float threshold = Mathf.Max(0f, player.TimeRemaining - 5f);
        float deadline = Time.time + 1f;
        while (player.TimeRemaining > threshold && Time.time < deadline)
            yield return null;
        Assert.That(player.TimeRemaining, Is.LessThanOrEqualTo(threshold), "Preman harus menyentuh pemain.");
    }

    private static GameObject Ui(string name, Transform parent)
    {
        GameObject item = new GameObject(name, typeof(RectTransform));
        item.transform.SetParent(parent, false);
        return item;
    }

    private static void SetField(object target, string name, object value)
    {
        FieldInfo field = target.GetType().GetField(name, BindingFlags.Instance | BindingFlags.NonPublic);
        Assert.That(field, Is.Not.Null, name);
        field.SetValue(target, value);
    }
}
#endif

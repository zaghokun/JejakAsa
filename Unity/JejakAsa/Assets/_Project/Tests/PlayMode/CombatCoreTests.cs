#if UNITY_EDITOR
using System.Collections;
using System.Reflection;
using JejakAsa;
using NUnit.Framework;
using TMPro;
using UnityEngine;
using UnityEngine.TestTools;
using UnityEngine.UI;

public sealed class CombatCoreTests
{
    private GameObject root;
    private BattleController battle;
    private GameProgressData progress;
    private string originalProgress;
    private string originalSpawnPoint;

    [SetUp]
    public void SetUp()
    {
        progress = GameFlowManager.Instance.Progress;
        originalProgress = JsonUtility.ToJson(progress);
        originalSpawnPoint = GameFlowManager.Instance.LastSpawnPoint;
        progress.inventory.Clear();
        progress.equippedWeapon = "";
        root = new GameObject("CombatCoreTests");
        battle = root.AddComponent<BattleController>();
        battle.StartBattle();
    }

    [TearDown]
    public void TearDown()
    {
        JsonUtility.FromJsonOverwrite(originalProgress, progress);
        GameFlowManager.Instance.LastSpawnPoint = originalSpawnPoint;
        Object.DestroyImmediate(root);
    }

    [Test]
    public void HealingConsumesOneItemAndTakesATurnWhileWeaponPersistsAndIncreasesDamage()
    {
        progress.inventory.Add(BattleController.HealingItemId);
        progress.inventory.Add(BattleController.BasicWeaponId);
        Assert.IsFalse(battle.PlayerUseHealing());
        Assert.IsTrue(battle.EquipBasicWeapon());
        Assert.AreEqual(BattleController.BasicWeaponId, JsonUtility.FromJson<GameProgressData>(JsonUtility.ToJson(progress)).equippedWeapon);
        Assert.IsTrue(battle.PlayerAttack());
        Assert.AreEqual(35, battle.EnemyHP);
        Assert.AreEqual(24, battle.PlayerHP);
        Assert.IsTrue(battle.PlayerUseHealing());
        Assert.AreEqual(24, battle.PlayerHP);
        Assert.IsFalse(progress.inventory.Contains(BattleController.HealingItemId));
        Assert.IsFalse(battle.PlayerUseHealing());
        Assert.AreEqual(2, battle.PatternIndex);
    }

    [Test]
    public void BossMechanicsDifferAndDeadBossDoesNotAttackOrReportTwice()
    {
        battle.StartBattle(3);
        Assert.IsTrue(battle.PlayerAttack());
        Assert.AreEqual(43, battle.EnemyHP);
        Assert.AreEqual(BattleController.EnemyAction.Persiapan, battle.NextEnemyAction);
        battle.StartBattle(5);
        Assert.AreEqual(BattleController.EnemyAction.Persiapan, battle.NextEnemyAction);
        Assert.IsTrue(battle.PlayerDefend());
        Assert.IsTrue(battle.PlayerDefend());
        Assert.AreEqual(10, battle.LastEnemyDamage);
        battle.enemyMaxHP = 10;
        battle.StartBattle(5);
        int results = 0;
        battle.OnBattleEnd += won => { if (won) results++; };
        Assert.IsTrue(battle.PlayerAttack());
        Assert.AreEqual(0, battle.PatternIndex);
        Assert.AreEqual(battle.playerMaxHP, battle.PlayerHP);
        Assert.IsFalse(battle.PlayerAttack());
        Assert.AreEqual(1, results);
    }

    [Test]
    public void BossCannotBeClearedByCitizenHelpAndResetsForRetry()
    {
        var encounter = root.AddComponent<EncounterController>();
        encounter.isBoss = true;
        encounter.OpenEncounter();
        Assert.IsFalse(encounter.TryUseCitizenHelp());
        Assert.IsFalse(encounter.IsResolved);
        encounter.ResolveFromBattleVictory();
        Assert.IsTrue(encounter.IsResolved);
        encounter.ResetEncounter();
        Assert.IsFalse(encounter.IsResolved);
    }

    [UnityTest]
    public IEnumerator Level3BossFirst() => ObjectiveOrder(3, true);

    [UnityTest]
    public IEnumerator Level3PackagesFirst() => ObjectiveOrder(3, false);

    [UnityTest]
    public IEnumerator Level5BossFirst() => ObjectiveOrder(5, true);

    [UnityTest]
    public IEnumerator Level5PackagesFirst() => ObjectiveOrder(5, false);

    private IEnumerator ObjectiveOrder(int level, bool bossFirst)
    {
        progress.currentLevel = level;
        progress.currentPhase = DayPhase.DiLevel;
        progress.hasPickedUpBansos = true;
        GameFlowManager.Instance.LastSpawnPoint = "";

        GameObject actor = new GameObject("Mission player");
        actor.SetActive(false);
        actor.transform.SetParent(root.transform);
        var movement = actor.AddComponent<PlayerController>();
        SetField(movement, "movementCamera", root.transform);
        var interaction = actor.AddComponent<PlayerInteraction>();
        var prompt = new GameObject("Prompt");
        prompt.transform.SetParent(root.transform);
        var dialogue = new GameObject("Dialogue");
        dialogue.transform.SetParent(root.transform);
        SetField(interaction, "promptPanel", prompt);
        var promptText = new GameObject("Prompt text", typeof(RectTransform));
        promptText.transform.SetParent(root.transform);
        SetField(interaction, "promptText", promptText.AddComponent<TextMeshProUGUI>());
        SetField(interaction, "dialoguePanel", dialogue);
        var title = new GameObject("Title", typeof(RectTransform));
        title.transform.SetParent(root.transform);
        SetField(interaction, "dialogueTitle", title.AddComponent<TextMeshProUGUI>());
        var body = new GameObject("Body", typeof(RectTransform));
        body.transform.SetParent(root.transform);
        SetField(interaction, "dialogueBody", body.AddComponent<TextMeshProUGUI>());
        var buttonObject = new GameObject("Close", typeof(RectTransform));
        buttonObject.transform.SetParent(root.transform);
        SetField(interaction, "closeButton", buttonObject.AddComponent<Button>());
        actor.SetActive(true);

        var bossObject = new GameObject("Boss");
        bossObject.transform.SetParent(root.transform);
        bossObject.transform.position = Vector3.right * 100f;
        var boss = bossObject.AddComponent<EncounterController>();
        boss.isBoss = true;
        boss.ConfigureForTests(interaction);
        if (bossFirst)
            boss.ResolveFromBattleVictory();

        var open = typeof(PlayerInteraction).GetMethod("OpenDialogueWith", BindingFlags.Instance | BindingFlags.NonPublic);
        var close = typeof(PlayerInteraction).GetMethod("CloseDialogue", BindingFlags.Instance | BindingFlags.NonPublic);
        for (int i = 0; i < 3; i++)
        {
            var residentObject = new GameObject("Resident " + i);
            residentObject.transform.SetParent(root.transform);
            var resident = residentObject.AddComponent<InteractionTarget>();
            open.Invoke(interaction, new object[] { resident });
            Assert.AreEqual(i + 1, interaction.DeliveredCount);
            if (i < 2 || !bossFirst)
                Assert.AreEqual(MissionOutcome.InProgress, interaction.Outcome);
            close.Invoke(interaction, null);
        }

        if (!bossFirst)
        {
            boss.OpenEncounter();
            Assert.IsFalse(boss.TryUseCitizenHelp(), "Trust must not bypass the boss.");
            boss.AvoidEncounter();
            Assert.IsFalse(boss.IsResolved);
            yield return null;
            Assert.AreEqual(MissionOutcome.InProgress, interaction.Outcome);
            boss.ResolveFromBattleVictory();
            yield return null;
        }
        Assert.AreEqual(MissionOutcome.Success, interaction.Outcome);
    }

    [Test]
    public void ExplicitDeliveryCannotBypassBossObjective()
    {
        progress.currentLevel = 3;
        progress.currentPhase = DayPhase.DiLevel;
        progress.hasPickedUpBansos = true;
        GameFlowManager.Instance.LastSpawnPoint = "";
        GameObject actor = new GameObject("Mission player");
        actor.SetActive(false);
        actor.transform.SetParent(root.transform);
        var movement = actor.AddComponent<PlayerController>();
        SetField(movement, "movementCamera", root.transform);
        var interaction = actor.AddComponent<PlayerInteraction>();
        var panel = new GameObject("Panel");
        panel.transform.SetParent(root.transform);
        var textObject = new GameObject("Text", typeof(RectTransform));
        textObject.transform.SetParent(root.transform);
        var text = textObject.AddComponent<TextMeshProUGUI>();
        var button = new GameObject("Button", typeof(RectTransform));
        button.transform.SetParent(root.transform);
        SetField(interaction, "promptPanel", panel);
        SetField(interaction, "dialoguePanel", panel);
        SetField(interaction, "promptText", text);
        SetField(interaction, "dialogueTitle", text);
        SetField(interaction, "dialogueBody", text);
        SetField(interaction, "closeButton", button.AddComponent<Button>());
        actor.SetActive(true);
        var boss = new GameObject("Boss");
        boss.transform.SetParent(root.transform);
        boss.AddComponent<EncounterController>().isBoss = true;
        var targetProperty = typeof(PlayerInteraction).GetProperty("DialogueTarget");
        for (int i = 0; i < 3; i++)
        {
            var resident = new GameObject("Resident " + i);
            resident.transform.SetParent(root.transform);
            targetProperty.SetValue(interaction, resident.AddComponent<InteractionTarget>());
            Assert.IsTrue(interaction.TryDeliverBansos());
        }
        Assert.AreEqual(MissionOutcome.InProgress, interaction.Outcome);
    }

    private static void SetField(object target, string name, object value)
    {
        target.GetType().GetField(name, BindingFlags.Instance | BindingFlags.NonPublic).SetValue(target, value);
    }
}
#endif

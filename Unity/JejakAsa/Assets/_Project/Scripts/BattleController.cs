using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

namespace JejakAsa
{
    /// <summary>
    /// Logika turn-based battle deterministik (U11–U14) beserta animasi 3D sederhana &amp; HUD arena bawah.
    /// HP, damage, dan pola lawan semuanya tetap — tidak ada RNG, critical, atau item.
    /// Urutan: aksi pemain → cek HP musuh → aksi lawan (bila hidup) → cek HP pemain.
    /// </summary>
    [DisallowMultipleComponent]
    public sealed class BattleController : MonoBehaviour
    {
        private static readonly int BaseColorPropId = Shader.PropertyToID("_BaseColor");

        // ─── Baseline parameters (section 3.6) ───────────────────────────
        [Header("Baseline")]
        public int playerMaxHP = 32;
        public int enemyMaxHP  = 50;
        public int attackDamage = 10;
        public int biasaDamage  = 8;
        public int biasaGuarded = 2;
        public int kuatDamage   = 18;
        public int kuatGuarded  = 4;

        // ─── Runtime state ───────────────────────────────────────────────
        public int  PlayerHP          { get; private set; }
        public int  EnemyHP           { get; private set; }
        public bool IsGuarding        { get; private set; }
        public bool IsPlayerTurn      { get; private set; }
        public bool IsBattleOver      { get; private set; }
        public bool PlayerWon         { get; private set; }
        public int  TurnNumber        { get; private set; }
        public int  PatternIndex      { get; private set; }
        public bool ActionLocked      { get; private set; }
        public bool IsDismissRequested { get; private set; }

        // ─── Enemy pattern ───────────────────────────────────────────────
        public enum EnemyAction { Biasa, Persiapan, Kuat }

        public static readonly EnemyAction[] Pattern =
            { EnemyAction.Biasa, EnemyAction.Biasa, EnemyAction.Persiapan, EnemyAction.Kuat };

        public EnemyAction LastEnemyAction { get; private set; }
        public EnemyAction NextEnemyAction => Pattern[PatternIndex % Pattern.Length];
        public int LastEnemyDamage { get; private set; }

        /// <summary>
        /// Peringatan serangan kuat berikutnya — aktif setelah lawan Persiapan.
        /// </summary>
        public bool ShowKuatWarning =>
            PatternIndex > 0 &&
            Pattern[(PatternIndex - 1) % Pattern.Length] == EnemyAction.Persiapan;

        // ─── Events & singleton ──────────────────────────────────────────
        /// <summary>Hasil battle: true = menang, false = kalah. Dikirim sekali (U14).</summary>
        public event Action<bool> OnBattleEnd;
        private bool resultSent;

        public static BattleController Active { get; private set; }

        // ─── Battle log ──────────────────────────────────────────────────
        public struct LogEntry
        {
            public int turn;
            public string playerAction;
            public string enemyAction;
            public int playerHPAfter;
            public int enemyHPAfter;
        }
        public List<LogEntry> Log { get; private set; } = new List<LogEntry>();

        private struct FloatingText
        {
            public Vector3 worldOrigin;
            public string text;
            public Color color;
            public float elapsed;
            public float duration;
        }
        private readonly List<FloatingText> popups = new List<FloatingText>();

        // ─── Visuals & animation state ───────────────────────────────────
        private Transform playerVisual;
        private Transform enemyVisual;
        private Renderer playerRenderer;
        private Renderer enemyRenderer;
        private Color playerBaseColor = new Color(0.10f, 0.58f, 0.54f, 1f);
        private Color enemyBaseColor  = new Color(0.82f, 0.24f, 0.22f, 1f);
        private Camera battleCamera;

        private Vector3 playerBasePos;
        private Vector3 enemyBasePos;
        private Vector3 cameraBasePos;
        private bool visualsCaptured;
        private bool isAnimatingTurn;
        private float cameraShakeTimer;
        private float cameraShakeMagnitude;
        private float playerFlashTimer;
        private float enemyFlashTimer;
        private Color playerFlashColor;
        private Color enemyFlashColor;
        private MaterialPropertyBlock propBlock;

        private string lastPlayerActionText = "";
        private string lastEnemyActionText  = "";
        private bool hasStarted;

        // ─── Cached IMGUI resources ──────────────────────────────────────
        private static Texture2D hpBarPlayerTex;
        private static Texture2D hpBarEnemyTex;
        private static GUIStyle hpTextStyle;
        private static GUIStyle dotStyle;
        private static GUIStyle turnBadgeStyle;
        private static GUIStyle guardBadgeStyle;
        private static GUIStyle popupStyle;
        private static GUIStyle popupShadowStyle;

        // ─── Lifecycle ───────────────────────────────────────────────────
        private void Awake()
        {
            Active = this;
            propBlock = new MaterialPropertyBlock();
            ResolveArenaVisuals();
        }

        private void Start()
        {
            if (!hasStarted)
                StartBattle();
        }

        private void OnDestroy()
        {
            if (Active == this)
                Active = null;
        }

        private void ResolveArenaVisuals()
        {
            if (visualsCaptured)
                return;

            GameObject[] roots = gameObject.scene.GetRootGameObjects();
            for (int i = 0; i < roots.Length; i++)
            {
                if (roots[i].name == "Pemain_Battle")
                    playerVisual = roots[i].transform;
                else if (roots[i].name == "Preman_Battle")
                    enemyVisual = roots[i].transform;
                else if (battleCamera == null)
                {
                    Camera cam = roots[i].GetComponentInChildren<Camera>(true);
                    if (cam != null)
                        battleCamera = cam;
                }
            }

            if (playerVisual != null)
            {
                playerBasePos = playerVisual.position;
                playerRenderer = playerVisual.GetComponent<Renderer>();
                if (playerRenderer != null && playerRenderer.sharedMaterial != null && playerRenderer.sharedMaterial.HasProperty(BaseColorPropId))
                    playerBaseColor = playerRenderer.sharedMaterial.GetColor(BaseColorPropId);
            }

            if (enemyVisual != null)
            {
                enemyBasePos = enemyVisual.position;
                enemyRenderer = enemyVisual.GetComponent<Renderer>();
                if (enemyRenderer != null && enemyRenderer.sharedMaterial != null && enemyRenderer.sharedMaterial.HasProperty(BaseColorPropId))
                    enemyBaseColor = enemyRenderer.sharedMaterial.GetColor(BaseColorPropId);
            }

            if (battleCamera != null)
                cameraBasePos = battleCamera.transform.position;

            visualsCaptured = true;
        }

        /// <summary>Memulai battle baru dari HP penuh.</summary>
        public void StartBattle()
        {
            StopAllCoroutines();
            ResolveArenaVisuals();

            hasStarted         = true;
            PlayerHP           = playerMaxHP;
            EnemyHP            = enemyMaxHP;
            IsGuarding         = false;
            IsPlayerTurn       = true;
            IsBattleOver       = false;
            PlayerWon          = false;
            TurnNumber         = 1;
            PatternIndex       = 0;
            ActionLocked       = false;
            IsDismissRequested = false;
            resultSent         = false;
            isAnimatingTurn    = false;
            LastEnemyAction    = EnemyAction.Biasa;
            LastEnemyDamage    = 0;
            lastPlayerActionText = "";
            lastEnemyActionText  = "";
            playerFlashTimer   = 0f;
            enemyFlashTimer    = 0f;
            cameraShakeTimer   = 0f;
            popups.Clear();
            Log.Clear();

            ResetVisualTransforms();
        }

        private void ResetVisualTransforms()
        {
            if (playerVisual != null)
            {
                playerVisual.position = playerBasePos;
                playerVisual.rotation = Quaternion.identity;
                playerVisual.localScale = Vector3.one;
            }
            if (enemyVisual != null)
            {
                enemyVisual.position = enemyBasePos;
                enemyVisual.rotation = Quaternion.identity;
                enemyVisual.localScale = Vector3.one;
            }
            if (battleCamera != null)
                battleCamera.transform.position = cameraBasePos;

            ApplyTint(playerRenderer, playerBaseColor);
            ApplyTint(enemyRenderer, enemyBaseColor);
        }

        // ─── Synchronous test/core API (U11–U14 deterministic) ───────────

        /// <summary>Pemain menyerang musuh (10 damage) secara sinkron. Mengembalikan false jika aksi ditolak (U14).</summary>
        public bool PlayerAttack()
        {
            if (SessionMenu.InputBlocked || !IsPlayerTurn || IsBattleOver || ActionLocked)
                return false;

            ActionLocked = true;
            IsPlayerTurn = false;
            IsGuarding = false;

            ApplyPlayerAttackDamage();

            if (EnemyHP <= 0)
            {
                LogTurn("Serang", "—");
                EndBattle(true);
                return true;
            }

            ResolveEnemyTurnMath("Serang");
            return true;
        }

        /// <summary>Pemain bertahan secara sinkron — damage lawan berikutnya dikurangi.</summary>
        public bool PlayerDefend()
        {
            if (SessionMenu.InputBlocked || !IsPlayerTurn || IsBattleOver || ActionLocked)
                return false;

            ActionLocked = true;
            IsPlayerTurn = false;
            IsGuarding = true;
            lastPlayerActionText = "Bertahan (perlindungan aktif)";

            ResolveEnemyTurnMath("Bertahan");
            return true;
        }

        // ─── Animated player action API (for HUD buttons & keyboard) ──────

        /// <summary>Memicu aksi Serang dengan animasi maju-mundur dan jeda giliran ~0,4 detik.</summary>
        public bool TriggerPlayerAttackAnimated()
        {
            if (SessionMenu.InputBlocked || !IsPlayerTurn || IsBattleOver || ActionLocked)
                return false;

            ActionLocked = true;
            IsPlayerTurn = false;
            IsGuarding = false;
            StartCoroutine(PlayerAttackRoutine());
            return true;
        }

        /// <summary>Memicu aksi Bertahan dengan animasi perisai dan jeda giliran ~0,4 detik.</summary>
        public bool TriggerPlayerDefendAnimated()
        {
            if (SessionMenu.InputBlocked || !IsPlayerTurn || IsBattleOver || ActionLocked)
                return false;

            ActionLocked = true;
            IsPlayerTurn = false;
            IsGuarding = true;
            lastPlayerActionText = "Bertahan (perlindungan aktif)";
            StartCoroutine(PlayerDefendRoutine());
            return true;
        }

        private void ApplyPlayerAttackDamage()
        {
            int prev = EnemyHP;
            EnemyHP = Mathf.Max(0, EnemyHP - attackDamage);
            lastPlayerActionText = $"Serang → HP Musuh {prev} → {EnemyHP}";
            SpawnPopup(enemyVisual, $"-{attackDamage}", new Color(1.0f, 0.35f, 0.25f, 1f));
        }

        private void ResolveEnemyTurnMath(string playerActionLabel)
        {
            EnemyAction action = Pattern[PatternIndex % Pattern.Length];
            LastEnemyAction = action;
            PatternIndex++;

            bool guardedHit = IsGuarding;
            int damage = 0;
            string desc;

            switch (action)
            {
                case EnemyAction.Biasa:
                    damage = guardedHit ? biasaGuarded : biasaDamage;
                    desc = guardedHit
                        ? $"Serangan Biasa ({damage} dmg, ditahan!)"
                        : $"Serangan Biasa ({damage} dmg)";
                    break;
                case EnemyAction.Persiapan:
                    damage = 0;
                    desc = "Persiapan — serangan kuat berikutnya!";
                    break;
                case EnemyAction.Kuat:
                    damage = guardedHit ? kuatGuarded : kuatDamage;
                    desc = guardedHit
                        ? $"Serangan Kuat! ({damage} dmg, ditahan!)"
                        : $"Serangan Kuat! ({damage} dmg)";
                    break;
                default:
                    desc = "";
                    break;
            }

            // U13: guard habis setelah satu aksi lawan (termasuk Persiapan)
            IsGuarding = false;

            PlayerHP = Mathf.Max(0, PlayerHP - damage);
            LastEnemyDamage = damage;
            lastEnemyActionText = desc;

            if (action == EnemyAction.Persiapan)
            {
                SpawnPopup(enemyVisual, "SIAP KUAT!", new Color(1.0f, 0.55f, 0.08f, 1f));
            }
            else if (damage > 0)
            {
                string label = guardedHit ? $"-{damage} Ditahan!" : $"-{damage}";
                Color col = guardedHit ? new Color(0.18f, 0.85f, 0.85f, 1f) : new Color(1.0f, 0.25f, 0.25f, 1f);
                SpawnPopup(playerVisual, label, col);
            }

            LogTurn(playerActionLabel, desc);

            if (PlayerHP <= 0)
            {
                EndBattle(false);
                return;
            }

            if (!isAnimatingTurn)
            {
                IsPlayerTurn = true;
                ActionLocked = false;
            }
            TurnNumber++;
        }

        private IEnumerator PlayerAttackRoutine()
        {
            isAnimatingTurn = true;

            if (playerVisual != null && enemyVisual != null)
            {
                // Wind-up kecil ke belakang
                yield return MoveTransform(playerVisual, playerBasePos, playerBasePos + Vector3.left * 0.35f, 0.07f, 0f);

                // Maju menerjang ke arah preman
                Vector3 hitPos = enemyBasePos + Vector3.left * 1.15f;
                yield return MoveTransform(playerVisual, playerVisual.position, hitPos, 0.13f, 0.40f);
                yield return new WaitWhile(() => SessionMenu.InputBlocked);

                // Impact
                ApplyPlayerAttackDamage();
                TriggerFlash(isEnemy: true, new Color(1f, 0.92f, 0.72f, 1f), 0.18f);
                TriggerCameraShake(0.14f, 0.12f);

                // Preman terdorong mundur sejenak lalu kembali
                Vector3 knockPos = enemyBasePos + Vector3.right * 0.55f;
                StartCoroutine(RecoilRoutine(enemyVisual, enemyBasePos, knockPos, 0.22f));

                // Pemain melompat mundur ke posisi awal
                yield return MoveTransform(playerVisual, hitPos, playerBasePos, 0.18f, 0.20f);
            }
            else
            {
                yield return new WaitForSeconds(0.18f);
                yield return new WaitWhile(() => SessionMenu.InputBlocked);
                ApplyPlayerAttackDamage();
            }
            yield return new WaitWhile(() => SessionMenu.InputBlocked);

            if (EnemyHP <= 0)
            {
                LogTurn("Serang", "—");
                isAnimatingTurn = false;
                EndBattle(true);
                yield break;
            }

            // Jeda singkat antar giliran pemain dan lawan
            yield return new WaitForSeconds(0.24f);
            yield return EnemyTurnRoutine("Serang");
            isAnimatingTurn = false;
        }

        private IEnumerator PlayerDefendRoutine()
        {
            isAnimatingTurn = true;
            SpawnPopup(playerVisual, "🛡 Bertahan!", new Color(0.20f, 0.85f, 0.82f, 1f));
            TriggerFlash(isEnemy: false, new Color(0.25f, 0.90f, 0.90f, 1f), 0.28f);

            if (playerVisual != null)
            {
                playerVisual.localScale = new Vector3(1.16f, 0.86f, 1.16f);
                yield return new WaitForSeconds(0.28f);
                yield return new WaitWhile(() => SessionMenu.InputBlocked);
                playerVisual.localScale = Vector3.one;
            }
            else
            {
                yield return new WaitForSeconds(0.20f);
                yield return new WaitWhile(() => SessionMenu.InputBlocked);
            }

            yield return new WaitForSeconds(0.15f);
            yield return new WaitWhile(() => SessionMenu.InputBlocked);
            yield return EnemyTurnRoutine("Bertahan");
            isAnimatingTurn = false;
        }

        private IEnumerator EnemyTurnRoutine(string playerActionLabel)
        {
            EnemyAction upcoming = NextEnemyAction;
            bool wasGuarded = IsGuarding;

            if (enemyVisual != null && playerVisual != null)
            {
                if (upcoming == EnemyAction.Persiapan)
                {
                    // Pose Persiapan: membesar dan bergetar oranye merah
                    float t = 0f;
                    while (t < 0.36f)
                    {
                        t += Time.deltaTime;
                        float pulse = 1.24f + Mathf.Sin(t * 32f) * 0.06f;
                        enemyVisual.localScale = new Vector3(pulse, pulse, pulse);
                        enemyVisual.position = enemyBasePos + new Vector3(Mathf.Sin(t * 50f) * 0.08f, 0f, 0f);
                        yield return null;
                        while (SessionMenu.InputBlocked)
                            yield return null;
                    }
                    enemyVisual.position = enemyBasePos;
                    ResolveEnemyTurnMath(playerActionLabel);
                    TriggerFlash(isEnemy: true, new Color(1.0f, 0.40f, 0.10f, 1f), 0.35f);
                }
                else if (upcoming == EnemyAction.Kuat)
                {
                    // Serangan Kuat: ancang-ancang lebih dalam & lompat tinggi
                    yield return MoveTransform(enemyVisual, enemyBasePos, enemyBasePos + Vector3.right * 0.65f, 0.14f, 0f);
                    Vector3 strikePos = playerBasePos + Vector3.right * 1.15f;
                    yield return MoveTransform(enemyVisual, enemyVisual.position, strikePos, 0.16f, 0.85f);
                    yield return new WaitWhile(() => SessionMenu.InputBlocked);

                    ResolveEnemyTurnMath(playerActionLabel);
                    TriggerFlash(isEnemy: false, wasGuarded ? new Color(0.25f, 0.90f, 0.95f, 1f) : new Color(1f, 0.20f, 0.20f, 1f), 0.25f);
                    TriggerCameraShake(wasGuarded ? 0.14f : 0.26f, wasGuarded ? 0.14f : 0.24f);

                    Coroutine recoil = PlayerHP > 0
                        ? StartCoroutine(RecoilRoutine(playerVisual, playerBasePos, playerBasePos + Vector3.left * (wasGuarded ? 0.25f : 0.65f), 0.24f))
                        : null;

                    yield return MoveTransform(enemyVisual, strikePos, enemyBasePos, 0.20f, 0.25f);
                    if (recoil != null)
                        yield return recoil;
                }
                else
                {
                    // Serangan Biasa: lunge kiri cepat
                    yield return MoveTransform(enemyVisual, enemyBasePos, enemyBasePos + Vector3.right * 0.30f, 0.07f, 0f);
                    Vector3 strikePos = playerBasePos + Vector3.right * 1.15f;
                    yield return MoveTransform(enemyVisual, enemyVisual.position, strikePos, 0.14f, 0.32f);
                    yield return new WaitWhile(() => SessionMenu.InputBlocked);

                    ResolveEnemyTurnMath(playerActionLabel);
                    TriggerFlash(isEnemy: false, wasGuarded ? new Color(0.25f, 0.90f, 0.95f, 1f) : new Color(1f, 0.30f, 0.25f, 1f), 0.20f);
                    TriggerCameraShake(0.12f, 0.12f);

                    Coroutine recoil = PlayerHP > 0
                        ? StartCoroutine(RecoilRoutine(playerVisual, playerBasePos, playerBasePos + Vector3.left * (wasGuarded ? 0.20f : 0.45f), 0.20f))
                        : null;

                    yield return MoveTransform(enemyVisual, strikePos, enemyBasePos, 0.18f, 0.18f);
                    if (recoil != null)
                        yield return recoil;
                }
            }
            else
            {
                yield return new WaitForSeconds(0.20f);
                yield return new WaitWhile(() => SessionMenu.InputBlocked);
                ResolveEnemyTurnMath(playerActionLabel);
            }
            yield return new WaitWhile(() => SessionMenu.InputBlocked);

            if (!IsBattleOver)
            {
                IsPlayerTurn = true;
                ActionLocked = false;
            }
        }

        private static IEnumerator MoveTransform(Transform target, Vector3 from, Vector3 to, float duration, float arcHeight)
        {
            float elapsed = 0f;
            while (elapsed < duration || SessionMenu.InputBlocked)
            {
                if (SessionMenu.InputBlocked)
                {
                    yield return null;
                    continue;
                }
                elapsed += Time.deltaTime;
                float u = Mathf.Clamp01(elapsed / Mathf.Max(0.001f, duration));
                Vector3 pos = Vector3.Lerp(from, to, u);
                if (arcHeight > 0f)
                    pos.y += Mathf.Sin(u * Mathf.PI) * arcHeight;
                target.position = pos;
                yield return null;
            }
            target.position = to;
        }

        private IEnumerator RecoilRoutine(Transform target, Vector3 homePos, Vector3 pushedPos, float duration)
        {
            float half = duration * 0.4f;
            yield return MoveTransform(target, homePos, pushedPos, half, 0f);
            yield return MoveTransform(target, pushedPos, homePos, duration - half, 0f);
        }

        private void TriggerFlash(bool isEnemy, Color flashColor, float duration)
        {
            if (isEnemy)
            {
                enemyFlashColor = flashColor;
                enemyFlashTimer = duration;
            }
            else
            {
                playerFlashColor = flashColor;
                playerFlashTimer = duration;
            }
        }

        private void TriggerCameraShake(float magnitude, float duration)
        {
            cameraShakeMagnitude = magnitude;
            cameraShakeTimer = duration;
        }

        private void SpawnPopup(Transform target, string text, Color color)
        {
            Vector3 origin = target != null ? target.position + Vector3.up * 1.45f : Vector3.zero;
            popups.Add(new FloatingText
            {
                worldOrigin = origin,
                text = text,
                color = color,
                elapsed = 0f,
                duration = 0.85f
            });
        }

        private void ApplyTint(Renderer rend, Color color)
        {
            if (rend == null)
                return;

            rend.GetPropertyBlock(propBlock);
            propBlock.SetColor(BaseColorPropId, color);
            rend.SetPropertyBlock(propBlock);
        }

        private void LogTurn(string playerAct, string enemyAct)
        {
            Log.Add(new LogEntry
            {
                turn          = TurnNumber,
                playerAction  = playerAct,
                enemyAction   = enemyAct,
                playerHPAfter = PlayerHP,
                enemyHPAfter  = EnemyHP
            });
        }

        private void EndBattle(bool won)
        {
            IsBattleOver = true;
            PlayerWon = won;
            IsPlayerTurn = false;

            if (!resultSent)
            {
                resultSent = true;
                OnBattleEnd?.Invoke(won);
            }
        }

        // ─── Frame update: idle poses, Persiapan telegraph, defeated tilt, popups, input ──

        private void Update()
        {
            if (SessionMenu.InputBlocked)
                return;

            UpdatePopups();
            UpdateVisualEffects();

            Keyboard kb = Keyboard.current;
            if (kb == null)
                return;

            if (IsBattleOver)
            {
                if (kb.enterKey.wasPressedThisFrame || kb.spaceKey.wasPressedThisFrame)
                    IsDismissRequested = true;
                return;
            }

            if (!IsPlayerTurn || ActionLocked)
                return;

            if (kb.digit1Key.wasPressedThisFrame)
                TriggerPlayerAttackAnimated();
            else if (kb.digit2Key.wasPressedThisFrame)
                TriggerPlayerDefendAnimated();
        }

        private void UpdatePopups()
        {
            for (int i = popups.Count - 1; i >= 0; i--)
            {
                FloatingText entry = popups[i];
                entry.elapsed += Time.deltaTime;
                if (entry.elapsed >= entry.duration)
                    popups.RemoveAt(i);
                else
                    popups[i] = entry;
            }
        }

        private void UpdateVisualEffects()
        {
            if (battleCamera != null)
            {
                if (cameraShakeTimer > 0f)
                {
                    cameraShakeTimer -= Time.deltaTime;
                    float damper = Mathf.Clamp01(cameraShakeTimer / 0.25f);
                    Vector2 shake = UnityEngine.Random.insideUnitCircle * (cameraShakeMagnitude * damper);
                    battleCamera.transform.position = cameraBasePos + new Vector3(shake.x, shake.y, 0f);
                }
                else
                {
                    battleCamera.transform.position = cameraBasePos;
                }
            }

            if (playerVisual != null && !isAnimatingTurn)
            {
                if (IsBattleOver && !PlayerWon)
                {
                    playerVisual.position = new Vector3(playerBasePos.x, 0.45f, playerBasePos.z);
                    playerVisual.rotation = Quaternion.Euler(0f, 0f, 80f);
                }
                else
                {
                    float bob = Mathf.Sin(Time.time * 2.8f) * 0.03f;
                    playerVisual.position = playerBasePos + new Vector3(0f, bob, 0f);
                    playerVisual.rotation = Quaternion.identity;
                }
            }

            if (enemyVisual != null && !isAnimatingTurn)
            {
                if (IsBattleOver && PlayerWon)
                {
                    enemyVisual.position = new Vector3(enemyBasePos.x, 0.45f, enemyBasePos.z);
                    enemyVisual.rotation = Quaternion.Euler(0f, 0f, -80f);
                    enemyVisual.localScale = Vector3.one;
                }
                else if (ShowKuatWarning)
                {
                    // Pose Persiapan: bergetar & lebih besar sebelum serangan Kuat
                    float pulse = 1.15f + Mathf.Sin(Time.time * 12f) * 0.04f;
                    float jitterX = Mathf.Sin(Time.time * 38f) * 0.04f;
                    enemyVisual.localScale = new Vector3(pulse, pulse, pulse);
                    enemyVisual.position = enemyBasePos + new Vector3(jitterX, 0.05f, 0f);
                    enemyVisual.rotation = Quaternion.identity;
                }
                else
                {
                    float bob = Mathf.Sin(Time.time * 2.8f + 1.2f) * 0.03f;
                    enemyVisual.position = enemyBasePos + new Vector3(0f, bob, 0f);
                    enemyVisual.rotation = Quaternion.identity;
                    enemyVisual.localScale = Vector3.one;
                }
            }

            if (playerRenderer != null)
            {
                if (playerFlashTimer > 0f)
                {
                    playerFlashTimer -= Time.deltaTime;
                    ApplyTint(playerRenderer, playerFlashColor);
                }
                else
                {
                    ApplyTint(playerRenderer, playerBaseColor);
                }
            }

            if (enemyRenderer != null)
            {
                if (enemyFlashTimer > 0f)
                {
                    enemyFlashTimer -= Time.deltaTime;
                    ApplyTint(enemyRenderer, enemyFlashColor);
                }
                else if (!IsBattleOver && ShowKuatWarning)
                {
                    float glow = 0.55f + 0.30f * Mathf.Sin(Time.time * 10f);
                    ApplyTint(enemyRenderer, Color.Lerp(enemyBaseColor, new Color(1.0f, 0.45f, 0.08f, 1f), glow));
                }
                else
                {
                    ApplyTint(enemyRenderer, enemyBaseColor);
                }
            }
        }

        // ─── IMGUI Battle HUD (Top VS cards + Floating Popups + Bottom Dock) ───

        private static void EnsureBattleStyles()
        {
            CozyGuiTheme.EnsureInitialized();

            if (hpTextStyle == null)
            {
                hpTextStyle = new GUIStyle(GUI.skin.label)
                {
                    alignment = TextAnchor.MiddleCenter,
                    fontSize = 12,
                    fontStyle = FontStyle.Bold
                };
                hpTextStyle.normal.textColor = Color.white;
            }

            if (dotStyle == null)
            {
                dotStyle = new GUIStyle(GUI.skin.label)
                {
                    alignment = TextAnchor.MiddleCenter,
                    fontSize = 11,
                    fontStyle = FontStyle.Bold
                };
            }

            if (turnBadgeStyle == null)
            {
                turnBadgeStyle = new GUIStyle(GUI.skin.label)
                {
                    alignment = TextAnchor.MiddleCenter,
                    fontSize = 11,
                    fontStyle = FontStyle.Bold,
                    border = new RectOffset(10, 10, 10, 10)
                };
                turnBadgeStyle.normal.textColor = Color.white;
            }

            if (guardBadgeStyle == null)
            {
                guardBadgeStyle = new GUIStyle(GUI.skin.label)
                {
                    alignment = TextAnchor.MiddleCenter,
                    fontSize = 10,
                    fontStyle = FontStyle.Bold,
                    border = new RectOffset(8, 8, 8, 8)
                };
                guardBadgeStyle.normal.background = CozyGuiTheme.BannerInfoTex;
                guardBadgeStyle.normal.textColor = new Color(0.05f, 0.45f, 0.50f);
            }

            if (popupStyle == null)
            {
                popupStyle = new GUIStyle(GUI.skin.label)
                {
                    alignment = TextAnchor.MiddleCenter,
                    fontSize = 20,
                    fontStyle = FontStyle.Bold
                };
                popupShadowStyle = new GUIStyle(popupStyle);
                popupShadowStyle.normal.textColor = new Color(0f, 0f, 0f, 0.75f);
            }
        }

        private void OnGUI()
        {
            if (!hasStarted || SessionMenu.InputBlocked)
                return;

            EnsureBattleStyles();
            GUI.depth = -100;

            float scale = CozyGuiTheme.Scale;
            Matrix4x4 previousMatrix = GUI.matrix;
            GUI.matrix = previousMatrix * Matrix4x4.Scale(new Vector3(scale, scale, 1f));
            float sw = Screen.width / scale;
            float sh = Screen.height / scale;

            DrawTopStatusHud(sw);
            DrawWorldPopups();
            DrawBottomActionDock(sw, sh);
            GUI.matrix = previousMatrix;
        }

        private void DrawTopStatusHud(float sw)
        {
            float margin = 12f;
            float topY = 16f;
            float centerW = Mathf.Clamp(sw * 0.22f, 134f, 196f);
            float cardW = Mathf.Min(290f, (sw - centerW - margin * 4f) * 0.5f);
            float cardH = 82f;

            // ── Kartu Pemain (kiri atas) ────────────────────────────────
            Rect pCard = new Rect(margin, topY, cardW, cardH);
            GUI.DrawTexture(new Rect(pCard.x + 3, pCard.y + 4, pCard.width, pCard.height), CozyGuiTheme.ShadowTex);
            GUI.Box(pCard, GUIContent.none, CozyGuiTheme.CardStyle);
            GUI.Label(new Rect(pCard.x + 10, pCard.y - 11, 74, 22), "PEMAIN", CozyGuiTheme.RibbonStyle);
            if (IsGuarding)
                GUI.Label(new Rect(pCard.x + pCard.width - 92, pCard.y - 11, 84, 22), "🛡 GUARD", guardBadgeStyle);

            DrawHPBar(new Rect(pCard.x + 12, pCard.y + 20, pCard.width - 24, 30), "HP", PlayerHP, playerMaxHP,
                new Color(0.10f, 0.72f, 0.50f, 1f), ref hpBarPlayerTex);
            GUI.Label(new Rect(pCard.x + 10, pCard.y + 54, pCard.width - 20, 20), "Serang: 10  |  Bertahan: ¼", CozyGuiTheme.HintStyle);

            // ── Pil Giliran (tengah atas) ───────────────────────────────
            float centerX = (sw - centerW) * 0.5f;
            Rect cCard = new Rect(centerX, topY, centerW, 56f);
            GUI.DrawTexture(new Rect(cCard.x + 3, cCard.y + 4, cCard.width, cCard.height), CozyGuiTheme.ShadowTex);
            GUI.Box(cCard, GUIContent.none, CozyGuiTheme.CardStyle);

            string turnTitle = IsBattleOver ? "HASIL BATTLE" : $"GILIRAN {TurnNumber}";
            GUI.Label(new Rect(cCard.x + 6, cCard.y + 4, cCard.width - 12, 20), turnTitle, CozyGuiTheme.HintStyle);

            turnBadgeStyle.normal.background = IsBattleOver
                ? (PlayerWon ? CozyGuiTheme.OutcomeSuccessTex : CozyGuiTheme.OutcomeFailTex)
                : (IsPlayerTurn && !ActionLocked ? CozyGuiTheme.RibbonTex : CozyGuiTheme.TimerWarnTex);
            string badgeLabel = IsBattleOver
                ? (PlayerWon ? "MENANG!" : "KALAH")
                : (IsPlayerTurn && !ActionLocked ? "GILIRANMU" : "LAWAN...");
            GUI.Label(new Rect(cCard.x + 8, cCard.y + 26, cCard.width - 16, 22), badgeLabel, turnBadgeStyle);

            // Peringatan Serangan Kuat di bawah pil tengah
            if (ShowKuatWarning && !IsBattleOver)
            {
                float warnW = Mathf.Min(420f, sw - 32f);
                Rect warnRect = new Rect((sw - warnW) * 0.5f, topY + 64f, warnW, 30f);
                CozyGuiTheme.BannerStyle.normal.background = CozyGuiTheme.BannerFailTex;
                CozyGuiTheme.BannerStyle.normal.textColor = new Color(0.68f, 0.14f, 0.14f);
                GUI.Label(warnRect, "⚠  Serangan kuat berikutnya — gunakan Bertahan!", CozyGuiTheme.BannerStyle);
            }

            // ── Kartu Preman & Pola (kanan atas) ────────────────────────
            Rect eCard = new Rect(sw - margin - cardW, topY, cardW, cardH);
            GUI.DrawTexture(new Rect(eCard.x + 3, eCard.y + 4, eCard.width, eCard.height), CozyGuiTheme.ShadowTex);
            GUI.Box(eCard, GUIContent.none, CozyGuiTheme.CardStyle);
            CozyGuiTheme.OutcomeHeaderStyle.normal.background = CozyGuiTheme.OutcomeFailTex;
            GUI.Label(new Rect(eCard.x + 10, eCard.y - 11, Mathf.Min(134f, eCard.width - 20f), 22), "PREMAN", CozyGuiTheme.RibbonStyle);

            DrawHPBar(new Rect(eCard.x + 12, eCard.y + 20, eCard.width - 24, 30), "HP", EnemyHP, enemyMaxHP,
                new Color(0.84f, 0.26f, 0.24f, 1f), ref hpBarEnemyTex);

            // Strip pola musuh langsung di bagian bawah kartu musuh
            float dotSize = 20f;
            float dotsW = Pattern.Length * dotSize + (Pattern.Length - 1) * 4f;
            float dotX = eCard.x + eCard.width - 10f - dotsW;
            float dotY = eCard.y + 55f;
            GUI.Label(new Rect(eCard.x + 10, dotY, Mathf.Max(40f, eCard.width - dotsW - 22f), 20), "Pola:", CozyGuiTheme.SubLabelStyle);

            for (int i = 0; i < Pattern.Length; i++)
            {
                bool isNext = !IsBattleOver && (PatternIndex % Pattern.Length) == i;
                string label = Pattern[i] switch
                {
                    EnemyAction.Biasa     => "B",
                    EnemyAction.Persiapan => "P",
                    EnemyAction.Kuat      => "K",
                    _ => "?"
                };
                dotStyle.normal.textColor = isNext ? Color.white : new Color(0.45f, 0.40f, 0.35f);
                GUI.DrawTexture(new Rect(dotX, dotY, dotSize, dotSize), isNext ? CozyGuiTheme.TimerWarnTex : CozyGuiTheme.InnerBoxTex);
                GUI.Label(new Rect(dotX, dotY, dotSize, dotSize), label, dotStyle);
                dotX += dotSize + 4f;
            }
        }

        private void DrawWorldPopups()
        {
            if (popups.Count == 0 || battleCamera == null)
                return;

            for (int i = 0; i < popups.Count; i++)
            {
                FloatingText p = popups[i];
                float ratio = Mathf.Clamp01(p.elapsed / p.duration);
                Vector3 screen = battleCamera.WorldToScreenPoint(p.worldOrigin);
                if (screen.z <= 0f)
                    continue;

                float guiX = screen.x / CozyGuiTheme.Scale - 80f;
                float guiY = (Screen.height - screen.y) / CozyGuiTheme.Scale - 24f - ratio * 42f;
                float alpha = ratio < 0.7f ? 1f : 1f - ((ratio - 0.7f) / 0.3f);

                Rect rect = new Rect(guiX, guiY, 160f, 32f);
                Color shadowCol = new Color(0f, 0f, 0f, 0.75f * alpha);
                popupShadowStyle.normal.textColor = shadowCol;
                GUI.Label(new Rect(rect.x + 2, rect.y + 2, rect.width, rect.height), p.text, popupShadowStyle);

                popupStyle.normal.textColor = new Color(p.color.r, p.color.g, p.color.b, alpha);
                GUI.Label(rect, p.text, popupStyle);
            }
        }

        private void DrawBottomActionDock(float sw, float sh)
        {
            float dockW = Mathf.Min(560f, sw - 24f);
            float dockH = 114f;
            float dockX = (sw - dockW) * 0.5f;
            float dockY = sh - dockH - 14f;

            GUI.DrawTexture(new Rect(dockX + 4, dockY + 6, dockW, dockH), CozyGuiTheme.ShadowTex);
            GUI.Box(new Rect(dockX, dockY, dockW, dockH), GUIContent.none, CozyGuiTheme.CardStyle);

            float inX = dockX + 16f;
            float inW = dockW - 32f;

            // Baris ringkasan aksi terakhir atau aksi berikutnya
            string summary;
            if (!string.IsNullOrEmpty(lastPlayerActionText) || !string.IsNullOrEmpty(lastEnemyActionText))
                summary = $"{lastPlayerActionText}   •   {lastEnemyActionText}";
            else
            {
                summary = NextEnemyAction switch
                {
                    EnemyAction.Biasa     => "Preman bersiap menyerang (Biasa). Pilih tindakanmu!",
                    EnemyAction.Persiapan => "Preman sedang ancang-ancang (Persiapan).",
                    EnemyAction.Kuat      => "Awas! Serangan Kuat datang!",
                    _ => ""
                };
            }
            GUI.Label(new Rect(inX, dockY + 10f, inW, 22f), summary, CozyGuiTheme.OutcomeDescStyle);

            float rowY = dockY + 38f;
            if (!IsBattleOver)
            {
                bool canAct = IsPlayerTurn && !ActionLocked;
                float btnW = (inW - 14f) * 0.5f;

                bool prevEnabled = GUI.enabled;
                GUI.enabled = canAct;

                if (GUI.Button(new Rect(inX, rowY, btnW, 40f), "⚔  Serang (10 Dmg)", canAct ? CozyGuiTheme.TealBtnStyle : CozyGuiTheme.LockedBtnStyle))
                    TriggerPlayerAttackAnimated();

                if (GUI.Button(new Rect(inX + btnW + 14f, rowY, btnW, 40f), "🛡  Bertahan", canAct ? CozyGuiTheme.AmberBtnStyle : CozyGuiTheme.LockedBtnStyle))
                    TriggerPlayerDefendAnimated();

                GUI.enabled = prevEnabled;

                string hint = canAct ? "[1] Serang   |   [2] Bertahan" : "Giliran sedang berlangsung...";
                GUI.Label(new Rect(inX, dockY + dockH - 24f, inW, 18f), hint, CozyGuiTheme.HintStyle);
            }
            else
            {
                Texture2D resultBg = PlayerWon ? CozyGuiTheme.BannerSuccessTex : CozyGuiTheme.BannerFailTex;
                Color resultColor = PlayerWon ? new Color(0.05f, 0.45f, 0.30f) : new Color(0.68f, 0.14f, 0.14f);
                string resultText = PlayerWon
                    ? $"Menang! Sisa HP Pemain: {PlayerHP}"
                    : "Kalah! Preman terlalu kuat.";

                bool hasStandaloneBtn = BattleTransition.Active == null;
                float bannerW = inW - 154f;

                CozyGuiTheme.BannerStyle.normal.background = resultBg;
                CozyGuiTheme.BannerStyle.normal.textColor = resultColor;
                GUI.Label(new Rect(inX, rowY, bannerW, 38f), resultText, CozyGuiTheme.BannerStyle);

                string btnLabel = hasStandaloneBtn ? "Main Lagi" : "Lanjut";
                if (GUI.Button(new Rect(inX + bannerW + 12f, rowY, 142f, 38f), btnLabel, CozyGuiTheme.TealBtnStyle))
                {
                    if (hasStandaloneBtn)
                        StartBattle();
                    else
                        IsDismissRequested = true;
                }

                string hint = hasStandaloneBtn
                    ? "Mode Battle Mandiri"
                    : "Tekan Enter / Spasi atau klik Lanjut untuk kembali";
                GUI.Label(new Rect(inX, dockY + dockH - 24f, inW, 18f), hint, CozyGuiTheme.HintStyle);
            }
        }

        private static void DrawHPBar(Rect area, string label, int hp, int max, Color fillColor, ref Texture2D barTex)
        {
            GUI.DrawTexture(area, CozyGuiTheme.InnerBoxTex);

            float ratio = Mathf.Clamp01(max > 0 ? (float)hp / max : 0f);
            if (ratio > 0f)
            {
                if (barTex == null)
                {
                    barTex = new Texture2D(1, 1);
                    barTex.SetPixel(0, 0, fillColor);
                    barTex.Apply();
                    barTex.hideFlags = HideFlags.HideAndDontSave;
                }
                Rect fill = new Rect(area.x + 2, area.y + 2, (area.width - 4) * ratio, area.height - 4);
                GUI.DrawTexture(fill, barTex);
            }

            GUI.Label(area, $"{label}: {hp} / {max}", hpTextStyle);
        }
    }
}

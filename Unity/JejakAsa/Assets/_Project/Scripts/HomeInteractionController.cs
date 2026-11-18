using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;

namespace JejakAsa
{
    /// <summary>
    /// Pengendali interaksi di dalam interior Rumah (R04 / AB-D02).
    /// Mengatur pintu keluar menuju posko dan tempat tidur untuk istirahat tidur & simpan game (Slot 1..3).
    /// </summary>
    [DisallowMultipleComponent]
    public class HomeInteractionController : MonoBehaviour
    {
        [Header("Referensi")]
        public Transform playerTransform;
        public Transform doorTransform;
        public Transform bedTransform;
        public float interactionRadius = 2.2f;

        [Header("Scene Target")]
        public string poskoScenePath = "Assets/_Project/Scenes/Posko.unity";

        private static HomeInteractionController activeMenu;
        internal static bool AnyMenuOpen => activeMenu != null && activeMenu.isBedMenuOpen;
        private void OnEnable() => activeMenu = this;
        private void OnDisable() { if (activeMenu == this) activeMenu = null; }

        private bool nearDoor;
        private bool nearBed;
        private bool isBedMenuOpen;
        private float bedOpenedAt;
        private int pendingSaveSlot;
        private string feedbackNotice = string.Empty;
        private float feedbackTimer;

        private void Start()
        {
            if (playerTransform == null)
            {
                var playerObj = GameObject.FindWithTag("Player");
                if (playerObj != null)
                    playerTransform = playerObj.transform;
            }
        }

        private void Update()
        {
            if (feedbackTimer > 0f)
            {
                feedbackTimer -= Time.deltaTime;
                if (feedbackTimer <= 0f)
                    feedbackNotice = string.Empty;
            }

            if (playerTransform == null || SessionMenu.InputBlocked)
                return;

            nearDoor = doorTransform != null && Vector3.Distance(doorTransform.position, playerTransform.position) <= interactionRadius;
            nearBed = bedTransform != null && Vector3.Distance(bedTransform.position, playerTransform.position) <= interactionRadius;

            Keyboard kb = Keyboard.current;
            if (kb == null)
                return;

            // 1. Interaksi Pintu Keluar Rumah
            if (nearDoor && !isBedMenuOpen && kb.eKey.wasPressedThisFrame)
            {
                TryExitToPosko();
            }

            // 2. Interaksi Tempat Tidur
            if (nearBed && !isBedMenuOpen && kb.eKey.wasPressedThisFrame)
            {
                isBedMenuOpen = true;
                bedOpenedAt = Time.unscaledTime;
            }
            else if (isBedMenuOpen)
            {
                if (pendingSaveSlot != 0)
                {
                    if (kb.escapeKey.wasPressedThisFrame || kb.digit1Key.wasPressedThisFrame)
                        pendingSaveSlot = 0;
                    else if (kb.digit2Key.wasPressedThisFrame)
                        ConfirmSave();
                }
                else if (kb.escapeKey.wasPressedThisFrame || kb.digit5Key.wasPressedThisFrame)
                {
                    isBedMenuOpen = false;
                }
                else if (kb.digit1Key.wasPressedThisFrame)
                    TryRestAndAdvanceDay();
                else if (kb.digit2Key.wasPressedThisFrame)
                    DoSave(1);
                else if (kb.digit3Key.wasPressedThisFrame)
                    DoSave(2);
                else if (kb.digit4Key.wasPressedThisFrame)
                    DoSave(3);
            }
        }

        public void TryExitToPosko()
        {
            if (GameFlowManager.Instance.CanGoToPoskoFromHome(out string reason))
            {
                GameFlowManager.Instance.GoToPoskoFromHome();
                GameFlowManager.Instance.LastSpawnPoint = "RumahVicky";
                SceneManager.LoadScene(poskoScenePath, LoadSceneMode.Single);
            }
            else
            {
                ShowNotice(reason);
            }
        }

        public void TryRestAndAdvanceDay()
        {
            if (GameFlowManager.Instance.CanRestAndAdvanceDay(out string reason))
            {
                GameFlowManager.Instance.RestAndAdvanceDay();
                isBedMenuOpen = false;
                ShowNotice(GameFlowManager.Instance.CurrentPhase == DayPhase.Tamat
                    ? "Kelima level selesai! Tekan Esc untuk kembali ke menu utama."
                    : $"Selamat pagi! Memasuki Hari ke-{GameFlowManager.Instance.CurrentDay}.");
            }
            else
            {
                ShowNotice(reason);
            }
        }

        public void DoSave(int slot)
        {
            if (slot < 1 || slot > GameFlowManager.MaxSaveSlots)
            {
                ShowNotice($"Gagal menyimpan ke Slot {slot}.");
                return;
            }
            if (GameFlowManager.HasSave(slot))
            {
                pendingSaveSlot = slot;
                return;
            }
            SaveSlot(slot);
        }

        public void ConfirmSave()
        {
            if (pendingSaveSlot == 0)
                return;
            int slot = pendingSaveSlot;
            pendingSaveSlot = 0;
            SaveSlot(slot);
        }

        private void SaveSlot(int slot)
        {
            if (GameFlowManager.Instance.SaveGame(slot))
            {
                ShowNotice($"Progres berhasil disimpan ke Slot {slot}!");
            }
            else
            {
                ShowNotice($"Gagal menyimpan ke Slot {slot}.");
            }
        }

        private void ShowNotice(string msg)
        {
            feedbackNotice = msg;
            feedbackTimer = 3.5f;
        }

        private void OnGUI()
        {
            if (SessionMenu.InputBlocked || StoryNpcController.AnyDialogueOpen) return;
            CozyGuiTheme.EnsureInitialized();
            if (!isBedMenuOpen)
            {
                if (nearDoor) CozyGuiTheme.Prompt("Keluar menuju posko");
                else if (nearBed) CozyGuiTheme.Prompt("Istirahat & simpan perjalanan");
            }
            Matrix4x4 old = GUI.matrix; float scale = CozyGuiTheme.Scale;
            GUI.matrix = old * Matrix4x4.Scale(new Vector3(scale, scale, 1));
            float sw = Screen.width / scale, sh = Screen.height / scale;
            if (isBedMenuOpen)
            {
                GUI.depth = -100; GUI.DrawTexture(new Rect(0,0,sw,sh),CozyGuiTheme.DimTex);
                Rect r = new Rect((sw-660)/2,(sh-644)/2,660,644);
                bool overwrite = pendingSaveSlot != 0;
                CozyGuiTheme.Header(r,"RUMAH VICKY / HARI " + GameFlowManager.Instance.CurrentDay,overwrite ? "Simpan jejak yang baru?" : "Pulang untuk melangkah.",overwrite ? "Catatan lama pada slot ini akan diganti dengan progres saat ini." : "Istirahat setelah laporan selesai, atau catat perjalananmu hari ini.");
                if (overwrite)
                {
                    CozyGuiTheme.Card(new Rect(r.x+28,r.y+198,604,100),true);
                    GUI.Label(new Rect(r.x+48,r.y+218,564,58),"Slot 0"+pendingSaveSlot+" akan ditimpa. Pilihan ini tidak dapat dibatalkan setelah disimpan.",CozyGuiTheme.Body);
                    if(CozyGuiTheme.Button(new Rect(r.x+28,r.y+338,604,54),"1. Kembali",CozyGuiTheme.AmberBtnStyle)) pendingSaveSlot=0;
                    if(CozyGuiTheme.Button(new Rect(r.x+28,r.y+406,604,54),"2. Ya, simpan di slot ini",CozyGuiTheme.CoralBtnStyle)) ConfirmSave();
                }
                else
                {
                    bool canRest=GameFlowManager.Instance.CanRestAndAdvanceDay(out string reason);
                    if(CozyGuiTheme.Button(new Rect(r.x+28,r.y+214,604,54),"1. Istirahat & sambut hari berikutnya",canRest?CozyGuiTheme.TealBtnStyle:CozyGuiTheme.LockedBtnStyle)) TryRestAndAdvanceDay();
                    GUI.Label(new Rect(r.x+32,r.y+276,596,36),canRest?"Semua urusan hari ini selesai. Kamu berhak beristirahat.":reason,CozyGuiTheme.Small);
                    for(int slot=1;slot<=3;slot++)
                    {
                        var meta=GameFlowManager.GetSlotMeta(slot);
                        Rect row=new Rect(r.x+28,r.y+322+(slot-1)*70,604,58);
                        if(CozyGuiTheme.Button(row,GUIContent.none,CozyGuiTheme.AmberBtnStyle)) DoSave(slot);
                        GUI.Label(new Rect(row.x+18,row.y+5,520,25),(slot+1)+". Simpan / Slot 0"+slot,CozyGuiTheme.ButtonLabel);
                        GUI.Label(new Rect(row.x+18,row.y+31,560,20),meta.exists?"Hari "+meta.day+"  ·  "+meta.timestamp:"Belum tersimpan",CozyGuiTheme.Small);
                    }
                    if(CozyGuiTheme.Button(new Rect(r.x+28,r.y+542,604,44),"5. Kembali ke rumah",CozyGuiTheme.AmberBtnStyle)) isBedMenuOpen=false;
                }
                CozyGuiTheme.Rule(r.x+28,r.y+597,604);
                GUI.Label(new Rect(r.x+28,r.y+609,604,20),overwrite?"1  Kembali     2  Simpan     Esc  Batal":"1–5  Pilih     Esc  Kembali",CozyGuiTheme.HintStyle);
            }
            if(!string.IsNullOrEmpty(feedbackNotice))
            {
                GUI.depth=-120;
                Rect toast=new Rect((sw-560)/2,28,560,64); CozyGuiTheme.Card(toast);
                GUI.Label(new Rect(toast.x+20,toast.y+8,520,48),feedbackNotice,CozyGuiTheme.Body);
            }
            GUI.matrix=old;
        }
    }
}

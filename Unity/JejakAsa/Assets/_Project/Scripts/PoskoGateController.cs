using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;

namespace JejakAsa
{
    /// <summary>
    /// Pengendali gerbang keluar posko bersama (R03 / AB-D02).
    /// Memberi pilihan berangkat ke Level, pulang ke Rumah, atau Batal.
    /// </summary>
    [DisallowMultipleComponent]
    public class PoskoGateController : MonoBehaviour
    {
        [Header("Konfigurasi Interaksi")]
        public Transform playerTransform;
        public float interactionRadius = 2.5f;

        [Header("Scene Target")]
        public string levelScenePath = "Assets/_Project/Scenes/Kampung.unity";
        public string homeScenePath = "Assets/_Project/Scenes/Rumah.unity";

        private static PoskoGateController activeMenu;
        internal static bool AnyMenuOpen => activeMenu != null && activeMenu.isGateMenuOpen;
        private void OnEnable() => activeMenu = this;
        private void OnDisable() { if (activeMenu == this) activeMenu = null; }

        private bool isPlayerInRange;
        private bool isGateMenuOpen;
        private float gateOpenedAt;
        private string lockedDepartReason;
        private string lockedHomeReason;
        internal bool IsMenuOpen => isGateMenuOpen;

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
            if (playerTransform == null || SessionMenu.InputBlocked)
                return;

            float dist = Vector3.Distance(transform.position, playerTransform.position);
            isPlayerInRange = dist <= interactionRadius;

            Keyboard kb = Keyboard.current;
            if (kb == null)
                return;

            if (isPlayerInRange && !isGateMenuOpen && kb.eKey.wasPressedThisFrame && PoskoDistributionTrigger.IsNearestInteraction(this, playerTransform))
            {
                OpenGateMenu();
            }
            else if (isGateMenuOpen)
            {
                if (kb.escapeKey.wasPressedThisFrame || kb.digit3Key.wasPressedThisFrame)
                {
                    CloseGateMenu();
                }
                else if (kb.digit1Key.wasPressedThisFrame)
                {
                    TryDepartToLevel();
                }
                else if (kb.digit2Key.wasPressedThisFrame)
                {
                    TryGoHome();
                }
            }
        }

        public void OpenGateMenu()
        {
            isGateMenuOpen = true;
            gateOpenedAt = Time.unscaledTime;
            StoryFlow_PengaturAlur.Instance.CanDepartToLevel(out lockedDepartReason);
            StoryFlow_PengaturAlur.Instance.CanGoHomeFromPosko(out lockedHomeReason);
        }

        public void CloseGateMenu()
        {
            isGateMenuOpen = false;
        }

        public void TryDepartToLevel()
        {
            if (StoryFlow_PengaturAlur.Instance.CanDepartToLevel(out string reason))
            {
                int level = StoryFlow_PengaturAlur.Instance.CurrentLevel;
                if (level < 1 || level > 5)
                {
                    lockedDepartReason = "Level aktif tidak valid.";
                    return;
                }
                string scenePath = level == 1 ? levelScenePath : $"Assets/_Project/Scenes/Kampung_L{level}.unity";
                if (SceneUtility.GetBuildIndexByScenePath(scenePath) < 0)
                {
                    lockedDepartReason = "Wilayah misi tidak tersedia di build.";
                    return;
                }
                StoryFlow_PengaturAlur.Instance.LastSpawnPoint = "PosBantuan";
                StoryFlow_PengaturAlur.Instance.DepartToLevel();
                isGateMenuOpen = false;
                SceneManager.LoadScene(scenePath, LoadSceneMode.Single);
            }
            else
            {
                lockedDepartReason = reason;
            }
        }

        public void TryGoHome()
        {
            if (StoryFlow_PengaturAlur.Instance.CanGoHomeFromPosko(out string reason))
            {
                StoryFlow_PengaturAlur.Instance.GoHomeFromPosko();
                isGateMenuOpen = false;
                SceneManager.LoadScene(homeScenePath, LoadSceneMode.Single);
            }
            else
            {
                lockedHomeReason = reason;
            }
        }

        private void OnGUI()
        {
            if(SessionMenu.InputBlocked || StoryDialog_InteraksiNPC.AnyDialogueOpen) return;
            CozyGuiTheme.EnsureInitialized();
            if(isPlayerInRange && !isGateMenuOpen && PoskoDistributionTrigger.IsNearestInteraction(this,playerTransform)) CozyGuiTheme.Prompt("Pilih tujuan perjalanan");
            if(!isGateMenuOpen) return;
            GUI.depth=-100;
            Matrix4x4 old=GUI.matrix; float scale=CozyGuiTheme.Scale;
            GUI.matrix=old*Matrix4x4.Scale(new Vector3(scale,scale,1));
            float sw=Screen.width/scale,sh=Screen.height/scale;
            GUI.DrawTexture(new Rect(0,0,sw,sh),CozyGuiTheme.DimTex);
            Rect r=new Rect((sw-660)/2,(sh-580)/2,660,580);
            CozyGuiTheme.Header(r,"GERBANG POSKO / HARI "+StoryFlow_PengaturAlur.Instance.CurrentDay,"Ke mana langkahmu?","Perjalanan mengikuti persiapan dan tugas yang sudah kamu selesaikan.");
            bool depart=StoryFlow_PengaturAlur.Instance.CanDepartToLevel(out string departReason);
            bool home=StoryFlow_PengaturAlur.Instance.CanGoHomeFromPosko(out string homeReason);
            if(CozyGuiTheme.Button(new Rect(r.x+28,r.y+214,604,58),"1. Antarkan bantuan / Wilayah "+StoryFlow_PengaturAlur.Instance.CurrentLevel,depart?CozyGuiTheme.TealBtnStyle:CozyGuiTheme.LockedBtnStyle)) TryDepartToLevel();
            GUI.Label(new Rect(r.x+32,r.y+278,596,44),depart?(string.IsNullOrEmpty(lockedDepartReason)?"Paket siap. Warga menantikan kedatanganmu.":lockedDepartReason):"Terkunci · "+departReason,CozyGuiTheme.Small);
            if(CozyGuiTheme.Button(new Rect(r.x+28,r.y+330,604,58),"2. Pulang ke rumah",home?CozyGuiTheme.TealBtnStyle:CozyGuiTheme.LockedBtnStyle)) TryGoHome();
            GUI.Label(new Rect(r.x+32,r.y+400,596,48),home?"Kembali ke rumah Vicky untuk beristirahat.":"Terkunci · "+homeReason,CozyGuiTheme.Small);
            if(CozyGuiTheme.Button(new Rect(r.x+28,r.y+468,604,48),"3. Tetap di posko",CozyGuiTheme.AmberBtnStyle)) CloseGateMenu();
            CozyGuiTheme.Rule(r.x+28,r.y+538,604);
            GUI.Label(new Rect(r.x+28,r.y+550,604,20),"1–3  Pilih tujuan     Esc  Kembali",CozyGuiTheme.HintStyle);
            GUI.matrix=old;
        }
    }
}

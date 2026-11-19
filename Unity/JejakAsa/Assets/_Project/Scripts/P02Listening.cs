using UnityEngine;

namespace JejakAsa
{
    [DisallowMultipleComponent]
    [RequireComponent(typeof(StoryNpcController))]
    public sealed class P02Listening : MonoBehaviour
    {
        private static readonly DialogueLine[] Lines =
        {
            new DialogueLine { speaker="Vicky", text="Bu, saya Vicky. Saya baru pulang ke sini. Apa ada yang bisa saya bantu?" },
            new DialogueLine { speaker="Bu Rina", text="Kami sedang mendata kebutuhan beberapa rumah. Ada yang sulit keluar, ada juga yang lebih butuh bahan pokok daripada barang lain." },
            new DialogueLine { speaker="Vicky", text="Boleh saya ikut mendata dan mengantar?" },
            new DialogueLine { speaker="Bu Rina", text="Boleh. Tapi mari kita cocokkan dulu dengan warga lain, supaya pembagiannya jelas dan tidak ada rumah yang terlewat." },
            new DialogueLine { speaker="Vicky", text="Setuju, Bu. Kita buat catatan yang bisa diperiksa bersama." }
        };
        [SerializeField] private StoryNpcController[] briefingNpcs;
        [SerializeField] private GameObject logisticsVisuals;
        private StoryNpcController resident;
        private bool sequenceStarted;
        public static bool IsPending(GameProgressData data) => data.requiresP02Listening && !data.isP02ListeningDone &&
            data.currentDay==1 && data.currentPhase==DayPhase.PoskoBriefing;
        private void Awake() { resident=GetComponent<StoryNpcController>(); }
        private void Start() { UpdateAvailability(); }
        private void Update() { UpdateAvailability(); }
        private void UpdateAvailability()
        {
            bool pending=IsPending(GameFlowManager.Instance.Progress);
            foreach(var npc in briefingNpcs) if(npc!=null) npc.enabled=!pending;
            if(logisticsVisuals!=null) logisticsVisuals.SetActive(!pending);
            // Resident remains present after the introduction for short repeat conversations.
        }
        public bool BeginConversation()
        {
            if(SessionMenu.InputBlocked || resident==null) return false;
            if(IsPending(GameFlowManager.Instance.Progress))
            {
                bool opened=resident.OpenSequence(Lines,Complete);
                sequenceStarted|=opened;
                return opened;
            }
            return resident.OpenSequence(new[]{new DialogueLine {speaker="Bu Rina",text="Catatannya bisa kita periksa bersama. Kalau kebutuhan berubah, warga akan menyampaikannya."}},null);
        }
        private void Complete()
        {
            sequenceStarted=false;
            if(IsPending(GameFlowManager.Instance.Progress)) GameFlowManager.Instance.Progress.isP02ListeningDone=true;
            UpdateAvailability();
        }
        private void OnDisable()
        {
            if(sequenceStarted && resident!=null) resident.CancelSequence();
            sequenceStarted=false;
            foreach(var npc in briefingNpcs) if(npc!=null)npc.enabled=true;
            if(logisticsVisuals!=null)logisticsVisuals.SetActive(true);
        }
    }
}

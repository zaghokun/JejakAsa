using System.Collections.Generic;
using UnityEngine;

namespace JejakAsa
{
    [DisallowMultipleComponent]
    public sealed class InteractionTarget : MonoBehaviour
    {
        internal static readonly List<InteractionTarget> Active = new List<InteractionTarget>();

        [SerializeField] private string displayName = "Warga";
        [SerializeField, TextArea(2, 5)] private string dialogue = "Terima kasih sudah datang membantu warga.";
        
        [Header("Bansos Delivery Settings (U04)")]
        [SerializeField] private bool isRecipient = true;
        [SerializeField] private string recipientId = "warga_01";
        [SerializeField, TextArea(2, 5)] private string deliveredDialogue = "Bantuan sembako dari kamu sangat membantu keluarga kami. Terima kasih banyak ya nak!";

        [SerializeField] private bool isDelivered = false;

        public string DisplayName => displayName;
        public string Dialogue
        {
            get
            {
                // 1. Dialog saat bantuan SUDAH / BARU SAJA diserahkan (U04)
                if (isDelivered)
                {
                    if (displayName.Contains("Pos"))
                    {
                        return "Alhamdulillah, terima kasih banyak ya mas. Lancar selalu rejekinya. Beras dan sembakonya langsung kami masak buat anak-anak hari ini.";
                    }
                    if (displayName.Contains("Tengah"))
                    {
                        return "Wah makasih banyak loh mas, bantuan ini sangat bermanfaat buat keluarga saya. Titip salam buat teman-teman relawan yang lain ya!";
                    }
                    if (displayName.Contains("Timur"))
                    {
                        return "Matur nuwun sanget nggih mas... Semoga kebaikan mas dan teman-teman dibalas berlipat ganda. Hati-hati di jalan ya mas.";
                    }

                    return "Alhamdulillah, terima kasih banyak atas bantuan sembakonya mas! Bantuan ini sangat bermanfaat untuk keluarga kami.";
                }

                // 2. Dialog sebelum bantuan diserahkan
                if (displayName.Contains("Pos"))
                {
                    return "Saya sedang menunggu bantuan kebutuhan pokok. Semoga masih ada bagian beras untuk dapur kami...";
                }
                if (displayName.Contains("Tengah"))
                {
                    return "Di gang dalam sini banyak lansia yang kesulitan keluar membeli bahan pokok...";
                }
                if (displayName.Contains("Timur"))
                {
                    return "Gang ini sering dijaga preman yang melarang bantuan masuk. Berhati-hatilah kalau lewat sana...";
                }

                return dialogue;
            }
        }
        public string InitialDialogue => dialogue;
        public string DeliveredDialogue => deliveredDialogue;
        public bool IsRecipient => isRecipient;
        public string RecipientId => recipientId;
        public bool IsDelivered => isDelivered;

        private void OnEnable()
        {
            Active.Add(this);
        }

        private void OnDisable()
        {
            Active.Remove(this);
        }

        /// <summary>
        /// Menandai bahwa warga ini telah sah menerima bansos (U04).
        /// </summary>
        public void MarkAsDelivered()
        {
            isDelivered = true;
        }

        /// <summary>
        /// Reset status pengantaran untuk sesi baru / restart.
        /// </summary>
        public void ResetDelivery()
        {
            isDelivered = false;
        }
    }
}

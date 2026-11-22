namespace JejakAsa
{
    public static class P04_PertemuanOphelia
    {
        private static readonly DialogueLine[] Lines =
        {
            new DialogueLine {speaker="Ophelia",text="Jadi benar kamu pulang. Aku kira setelah lulus kamu bakal lupa jalan ke kampung."},
            new DialogueLine {speaker="Vicky",text="Jalan ke kampung masih ingat. Jalan ke rumahmu saja yang hampir salah belok."},
            new DialogueLine {speaker="Ophelia",text="Padahal dulu kamu yang paling sering mengajak main di gang ini."},
            new DialogueLine {speaker="Vicky",text="Hai, Ophelia. Senang bertemu lagi."},
            new DialogueLine {speaker="Ophelia",text="Aku juga senang. Aku sering membantu warga menyampaikan kabar ke posko. Kalau kamu mau bekerja di sini, bersiaplah mendengar banyak cerita."},
            new DialogueLine {speaker="Vicky",text="Justru itu yang kubutuhkan. Semoga aku tidak merepotkanmu."},
            new DialogueLine {speaker="Ophelia",text="Belum tentu. Kita lihat setelah hari pertamamu."}
        };

        public static bool Open(StoryDialog_InteraksiNPC npc) =>
            npc.OpenSequence(Lines, () => StoryFlow_PengaturAlur.Instance.Progress.isOpheliaReunionDone = true);
    }
}

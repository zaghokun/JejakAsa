using UnityEngine;
using TMPro;
using UnityEngine.TextCore.LowLevel;

namespace JejakAsa
{
    // Volunteer field journal: one responsive visual system for every gameplay screen.
    internal static class CozyGuiTheme
    {
        // warna hybrid (menu kampung + aksen pixel)
        public static readonly Color Ink = Hex(0x45332A), Muted = Hex(0x7A6A55), Paper = Hex(0xF3EEE0), Linen = Hex(0xE3D9C2), Green = Hex(0x2B6E5E), Clay = Hex(0xC4553A), Gold = Hex(0xE0A040), Line = Hex(0xD8CCB0);
        public static float Scale => Mathf.Max(0.1f, Mathf.Min(Screen.width / 1280f, Screen.height / 720f));
        public static Texture2D ShadowTex, CardTex, DimTex, InnerBoxTex, BannerInfoTex, BannerSuccessTex, BannerFailTex, OutcomeSuccessTex, OutcomePartialTex, OutcomeFailTex;
        public static GUIStyle CardStyle, InnerBoxStyle, RibbonStyle, TimerStyle, LabelStyle, SubLabelStyle, BannerStyle, OutcomeHeaderStyle, OutcomeDescStyle, StatLeftStyle, StatRightStyle, HintStyle, TealBtnStyle, CoralBtnStyle, AmberBtnStyle, LockedBtnStyle, DialogueTextStyle;
        public static GUIStyle Title, Display, Eyebrow, Body, Small, White, ButtonLabel;
        private static GUIStyle shadowStyle, keyStyle;
        private static Texture2D interactionKey;
        private static Font uiFont, headingFont;
        private static GUIStyle hitStyle;
        private static TMP_FontAsset tmpFont;
        internal static TMP_FontAsset DialogueFont
        {
            get
            {
                if (tmpFont == null)
                {
                    if (uiFont == null) uiFont = Resources.Load<Font>("JejakAsaUI/Hybrid/Font/Sniglet-Regular");
                    if (uiFont == null) uiFont = Resources.Load<Font>("JejakAsaUI/Fonts/Nunito-SemiBold");
                    if (uiFont != null) tmpFont = TMP_FontAsset.CreateFontAsset(uiFont, 90, 9, GlyphRenderMode.SDFAA, 2048, 2048, AtlasPopulationMode.Dynamic, true);
                }
                return tmpFont;
            }
        }
        public static Color Hex(uint v) => new Color32((byte)(v >> 16), (byte)(v >> 8), (byte)v, 255);

        public static void EnsureInitialized()
        {
            if (CardTex != null && Body != null) return;
            uiFont = Resources.Load<Font>("JejakAsaUI/Hybrid/Font/Sniglet-Regular");
            headingFont = Resources.Load<Font>("JejakAsaUI/Hybrid/Font/LilitaOne-Regular");
            if (uiFont == null) uiFont = Resources.Load<Font>("JejakAsaUI/Fonts/Nunito-SemiBold");
            if (headingFont == null) headingFont = Resources.Load<Font>("JejakAsaUI/Fonts/BreeSerif-Regular");
            // gambar2 hybrid
            kartuTex = Resources.Load<Texture2D>("JejakAsaUI/Hybrid/kartu");
            tombolKrem = Resources.Load<Texture2D>("JejakAsaUI/Hybrid/tombol_krem");
            tombolTeal = Resources.Load<Texture2D>("JejakAsaUI/Hybrid/tombol_teal");
            tombolTekan = Resources.Load<Texture2D>("JejakAsaUI/Hybrid/tombol_tekan");
            tombolBata = Resources.Load<Texture2D>("JejakAsaUI/Hybrid/tombol_bata");
            tombolAbu = Resources.Load<Texture2D>("JejakAsaUI/Hybrid/tombol_abu");
            pitaGelap = Resources.Load<Texture2D>("JejakAsaUI/Hybrid/pita_gelap");
            tombolE = Resources.Load<Texture2D>("JejakAsaUI/Hybrid/tombol_e");
            berlian = Resources.Load<Texture2D>("JejakAsaUI/Hybrid/berlian");
            CardTex = Rounded(Paper); InnerBoxTex = Rounded(Linen);
            ShadowTex = Rounded(new Color(0.10f, 0.07f, 0.04f, 0.22f));
            DimTex = Rounded(new Color(0.10f, 0.07f, 0.04f, 0.72f));
            BannerInfoTex = Rounded(Linen); BannerSuccessTex = Rounded(Hex(0xDCEADF)); BannerFailTex = Rounded(Hex(0xF3DED4));
            OutcomeSuccessTex = Rounded(Green); OutcomePartialTex = Rounded(Hex(0x8B602A)); OutcomeFailTex = Rounded(Clay);
            CardStyle = PanelStyle(CardTex); InnerBoxStyle = PanelStyle(InnerBoxTex); shadowStyle = PanelStyle(ShadowTex);
            Body = Text(20, Ink); Small = Text(16, Muted); Eyebrow = Text(14, Green, true); Title = Text(36, Ink, true); Display = Text(72, Ink, true);
            if (headingFont != null) { Title.font = headingFont; Title.fontStyle = FontStyle.Normal; }
            White = Text(22, Paper, true); ButtonLabel = Text(22, Ink, true);
            if (headingFont != null) { White.font = ButtonLabel.font = headingFont; White.fontStyle = ButtonLabel.fontStyle = FontStyle.Normal; } keyStyle = Text(18, Paper, true, TextAnchor.MiddleCenter);
            LabelStyle = Text(18, Ink, true); SubLabelStyle = Text(14, Muted); HintStyle = Text(13, Muted, false, TextAnchor.MiddleCenter);
            RibbonStyle = Text(12, Green, true); TimerStyle = Text(30, Ink, true, TextAnchor.MiddleCenter);
            BannerStyle = Text(15, Ink, false, TextAnchor.MiddleCenter); BannerStyle.border = new RectOffset(12,12,12,12); BannerStyle.padding = new RectOffset(16,16,8,8);
            OutcomeHeaderStyle = Text(14, Paper, true, TextAnchor.MiddleCenter); OutcomeHeaderStyle.border = new RectOffset(12,12,12,12);
            OutcomeDescStyle = Text(17, Ink, false, TextAnchor.MiddleCenter); StatLeftStyle = Text(16, Muted); StatRightStyle = Text(18, Ink, true, TextAnchor.MiddleRight);
            // panel/tombol gambar lama ga dipake lagi, diganti gaya hybrid (Card, Button, Prompt di bawah)
            interactionKey = tombolE;
            DialogueTextStyle = Text(20, Ink); TealBtnStyle = ButtonStyle(Green, Paper);
            CoralBtnStyle = ButtonStyle(Clay, Paper); AmberBtnStyle = ButtonStyle(Linen, Ink);
            LockedBtnStyle = ButtonStyle(Hex(0xE7E5DC), Muted);
            centeredWhite = new GUIStyle(White) { alignment = TextAnchor.MiddleCenter };
            centeredInk = new GUIStyle(ButtonLabel) { alignment = TextAnchor.MiddleCenter };
        }
        public static GUIStyle Text(int size, Color color, bool bold = false, TextAnchor alignment = TextAnchor.MiddleLeft)
        {
            var s = new GUIStyle(GUI.skin.label) { font = uiFont != null ? uiFont : GUI.skin.label.font, fontSize = size, fontStyle = bold ? FontStyle.Bold : FontStyle.Normal, alignment = alignment, wordWrap = true, richText = true, padding = new RectOffset(0,0,0,0) };
            if (bold && headingFont != null) { s.font = headingFont; s.fontStyle = FontStyle.Normal; } // tulisan tebal pake Lilita
            s.normal.textColor = s.hover.textColor = s.active.textColor = s.focused.textColor = color;
            return s;
        }
        private static GUIStyle PanelStyle(Texture2D tex) => new GUIStyle(GUI.skin.box) { normal = { background = tex }, border = new RectOffset(12,12,12,12) };
        private static GUIStyle ButtonStyle(Color fill, Color ink)
        {
            var s = Text(17, ink, true, TextAnchor.MiddleCenter);
            s.normal.background = Rounded(fill); s.hover.background = Rounded(Color.Lerp(fill, Gold, .17f)); s.focused.background = s.hover.background; s.active.background = Rounded(Color.Lerp(fill, Ink, .15f)); s.border = new RectOffset(12,12,12,12); return s;
        }
        public static void Fill(Rect r, Color c) { Color old = GUI.color; GUI.color = c; GUI.DrawTexture(r, Texture2D.whiteTexture); GUI.color = old; }
        public static void Card(Rect r, bool inset = false)
        {
            EnsureInitialized();
            if (inset) { Rule(r.x + 8, r.y, r.width - 16); return; }
            if (kartuTex == null) { GUI.Box(r, GUIContent.none, CardStyle); return; }
            if (kartuStyle == null) kartuStyle = new GUIStyle { normal = { background = kartuTex }, border = new RectOffset(12,12,12,14) };
            if (Event.current.type == EventType.Repaint) kartuStyle.Draw(r, false, false, false, false);
        }
        // UV framing removes the empty margins from the illustration at draw time.
        public static bool Button(Rect r, string text, GUIStyle style) => Button(r, new GUIContent(text), style);
        public static bool Button(Rect r, GUIContent text, GUIStyle style)
        {
            bool primary = style == TealBtnStyle || style == CoralBtnStyle;
            bool hover = r.Contains(Event.current.mousePosition);
            bool ditekan = hover && GUIUtility.hotControl != 0;
            // pilih gambar tombol sesuai jenisnya
            Texture2D art = tombolKrem;
            if (style == TealBtnStyle) art = ditekan ? tombolTekan : tombolTeal;
            else if (style == CoralBtnStyle) art = tombolBata;
            else if (style == LockedBtnStyle) art = tombolAbu;
            Color old = GUI.color;
            if (hover && style != LockedBtnStyle) GUI.color = new Color(1f,.97f,.9f,1f);
            if (art != null)
            {
                if (tombolStyle == null) tombolStyle = new GUIStyle();
                tombolStyle.normal.background = art; tombolStyle.border = new RectOffset(8,8,6,12);
                if (Event.current.type == EventType.Repaint) tombolStyle.Draw(r, false, false, false, false);
            }
            GUI.color = old;
            if (hitStyle == null) hitStyle = new GUIStyle(GUIStyle.none);
            bool clicked = GUI.Button(r,GUIContent.none,hitStyle);
            Rect labelRect = new Rect(r.x+26,r.y-4,r.width-52,r.height);
            if (style == LockedBtnStyle) Ikon(new Rect(r.x+14,r.y+(r.height-22)/2-4,18,18),"kunci",Muted);
            if (!string.IsNullOrEmpty(text.text)) GUI.Label(labelRect,text,primary ? centeredWhite : centeredInk);
            return clicked;
        }
        private static GUIStyle centeredWhite, centeredInk;
        private static Texture2D kartuTex, tombolKrem, tombolTeal, tombolTekan, tombolBata, tombolAbu, pitaGelap, tombolE, berlian;
        private static GUIStyle kartuStyle, tombolStyle, pitaStyle;
        private static readonly System.Collections.Generic.Dictionary<string, Texture2D> ikonIkon = new System.Collections.Generic.Dictionary<string, Texture2D>();

        // bikin Sprite dari gambar hybrid (buat UI canvas kayak dialog)
        private static readonly System.Collections.Generic.Dictionary<string, Sprite> spriteHybrid = new System.Collections.Generic.Dictionary<string, Sprite>();
        internal static Sprite HybridSprite(string nama, Vector4 border)
        {
            if (spriteHybrid.TryGetValue(nama, out Sprite sp) && sp != null) return sp;
            var t = Resources.Load<Texture2D>("JejakAsaUI/Hybrid/" + nama);
            if (t == null) return null;
            sp = Sprite.Create(t, new Rect(0, 0, t.width, t.height), new Vector2(.5f, .5f), 100, 0, SpriteMeshType.FullRect, border);
            sp.hideFlags = HideFlags.HideAndDontSave;
            spriteHybrid[nama] = sp;
            return sp;
        }

        // gambar ikon pixel kecil (paket, timer, kunci, dll). warnanya bisa diganti
        public static void Ikon(Rect r, string nama, Color warna)
        {
            if (!ikonIkon.TryGetValue(nama, out Texture2D t) || t == null)
            {
                t = Resources.Load<Texture2D>("JejakAsaUI/Hybrid/ikon_" + nama);
                ikonIkon[nama] = t;
            }
            if (t == null) return;
            Color old = GUI.color; GUI.color = warna;
            GUI.DrawTexture(r, t, ScaleMode.ScaleToFit, true);
            GUI.color = old;
        }
        public static void Rule(float x, float y, float width) => Fill(new Rect(x,y,width,1), Line);
        public static void Header(Rect r, string category, string title, string description, Texture2D artwork = null)
        {
            Card(r);
            // artwork (gambar lama) ga digambar lagi, ganti berlian kecil
            if (berlian != null) GUI.DrawTexture(new Rect(r.x+56,r.y+47,18,18), berlian, ScaleMode.ScaleToFit, true);
            else Fill(new Rect(r.x+56,r.y+55,24,3), Clay);
            GUI.Label(new Rect(r.x+94,r.y+44,r.width-150,24), category, Eyebrow);
            GUI.Label(new Rect(r.x+56,r.y+80,r.width-112,48), title, Title);
            GUI.Label(new Rect(r.x+56,r.y+132,r.width-112,48), description, Small);
            Rule(r.x+56,r.y+190,r.width-112);
        }
        public static void Prompt(string text)
        {
            Matrix4x4 old = GUI.matrix; float scale = Scale; GUI.matrix = old * Matrix4x4.Scale(new Vector3(scale,scale,1));
            float w = Mathf.Clamp(Body.CalcSize(new GUIContent(text)).x+98,300,650);
            Rect r = new Rect((Screen.width/scale-w)/2,Screen.height/scale-104,w,56);
            if (pitaGelap != null)
            {
                if (pitaStyle == null) pitaStyle = new GUIStyle { normal = { background = pitaGelap }, border = new RectOffset(8,8,6,10) };
                if (Event.current.type == EventType.Repaint) pitaStyle.Draw(new Rect(r.x+40,r.y+4,r.width-40,50), false, false, false, false);
            }
            else Fill(new Rect(r.x+48,r.y+6,r.width-48,44),new Color(.04f,.13f,.09f,.88f));
            if (interactionKey != null) GUI.DrawTexture(new Rect(r.x+6,r.y+2,48,51), interactionKey, ScaleMode.ScaleToFit, true);
            else { Fill(new Rect(r.x+12,r.y+10,36,36),Green); GUI.Label(new Rect(r.x+12,r.y+10,36,36),"E",keyStyle); }
            GUI.Label(new Rect(r.x+62,r.y+6,w-76,44),text,White); GUI.matrix = old;
        }
        public static string PhaseName(DayPhase p) => p switch
        {
            DayPhase.Prolog => "Awal perjalanan", DayPhase.RumahPagi => "Pagi di rumah", DayPhase.PoskoBriefing => "Persiapan di posko", DayPhase.DiLevel => "Pengantaran bantuan", DayPhase.PoskoLaporan => "Laporan di posko", DayPhase.RumahMalam => "Malam di rumah", DayPhase.Tamat => "Perjalanan selesai", _ => "Jejak Asa"
        };
        public static void Journey(Rect r, int day, int completed = 0)
        {
            GUI.Label(new Rect(r.x,r.y,r.width,20),"LIMA HARI, SELANGKAH LEBIH BERARTI",Eyebrow);
            float step = (r.width-40)/4; Rule(r.x+20,r.y+48,r.width-40);
            for (int i=0;i<5;i++)
            {
                float x = r.x+step*i; Fill(new Rect(x,r.y+33,32,32),i<completed?Green:i+1==day?Clay:Linen);
                GUI.Label(new Rect(x,r.y+33,32,32),(i+1).ToString(),i<completed||i+1==day?keyStyle:HintStyle);
                GUI.Label(new Rect(x-2,r.y+73,65,18),"HARI "+(i+1),Small);
            }
        }
        public static Texture2D Rounded(Color fill)
        {
            // kotak rata, pojoknya dipotong kotak2 (aksen pixel)
            const int size=32; var t = new Texture2D(size,size,TextureFormat.RGBA32,false) { hideFlags=HideFlags.HideAndDontSave, filterMode=FilterMode.Point };
            var pixels = new Color[size*size];
            for(int y=0;y<size;y++) for(int x=0;x<size;x++) { int dx=Mathf.Max(4-x,x-27,0),dy=Mathf.Max(4-y,y-27,0); Color c=fill; if(dx+dy>4) c.a=0; pixels[y*size+x]=c; }
            t.SetPixels(pixels); t.Apply(); return t;
        }
    }
}

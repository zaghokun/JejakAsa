using UnityEngine;
using TMPro;
using UnityEngine.TextCore.LowLevel;

namespace JejakAsa
{
    // Volunteer field journal: one responsive visual system for every gameplay screen.
    internal static class CozyGuiTheme
    {
        public static readonly Color Ink = Hex(0x263D36), Muted = Hex(0x64736A), Paper = Hex(0xFBF5E9), Linen = Hex(0xEDE5D5), Green = Hex(0x285748), Clay = Hex(0xA94F38), Gold = Hex(0xD5AA60), Line = Hex(0xD7D4C3);
        public static float Scale => Mathf.Max(0.1f, Mathf.Min(Screen.width / 1280f, Screen.height / 720f));
        public static Texture2D ShadowTex, CardTex, DimTex, InnerBoxTex, BannerInfoTex, BannerSuccessTex, BannerFailTex, OutcomeSuccessTex, OutcomePartialTex, OutcomeFailTex;
        public static GUIStyle CardStyle, InnerBoxStyle, RibbonStyle, TimerStyle, LabelStyle, SubLabelStyle, BannerStyle, OutcomeHeaderStyle, OutcomeDescStyle, StatLeftStyle, StatRightStyle, HintStyle, TealBtnStyle, CoralBtnStyle, AmberBtnStyle, LockedBtnStyle, DialogueTextStyle;
        public static GUIStyle Title, Display, Eyebrow, Body, Small, White, ButtonLabel;
        private static GUIStyle shadowStyle, keyStyle;
        private static Texture2D interactionKey, journalPanel, notePanel;
        private static Font uiFont, headingFont;
        private static Texture2D greenButton, paperButton;
        private static GUIStyle hitStyle;
        private static TMP_FontAsset tmpFont;
        internal static TMP_FontAsset DialogueFont
        {
            get
            {
                if (tmpFont == null)
                {
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
            uiFont = Resources.Load<Font>("JejakAsaUI/Fonts/Nunito-SemiBold");
            headingFont = Resources.Load<Font>("JejakAsaUI/Fonts/BreeSerif-Regular");
            CardTex = Rounded(Paper); InnerBoxTex = Rounded(Linen);
            ShadowTex = Rounded(new Color(0.03f, 0.10f, 0.08f, 0.22f));
            DimTex = Rounded(new Color(0.04f, 0.10f, 0.08f, 0.78f));
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
            interactionKey = Resources.Load<Texture2D>("JejakAsaUI/Generated/InteractionKeyE");
            journalPanel = Resources.Load<Texture2D>("JejakAsaUI/Generated/JournalPanel");
            notePanel = Resources.Load<Texture2D>("JejakAsaUI/Generated/VolunteerDialogPanel");
            DialogueTextStyle = Text(20, Ink); TealBtnStyle = ButtonStyle(Green, Paper);
            greenButton = Resources.Load<Texture2D>("JejakAsaUI/Generated/VolunteerButton");
            var illustratedButton = greenButton;
            if (illustratedButton != null) { TealBtnStyle.normal.background = illustratedButton; TealBtnStyle.hover.background = illustratedButton; TealBtnStyle.active.background = illustratedButton; TealBtnStyle.focused.background = illustratedButton; TealBtnStyle.border = new RectOffset(0,0,0,0); } CoralBtnStyle = ButtonStyle(Clay, Paper); AmberBtnStyle = ButtonStyle(Linen, Ink);
            paperButton = Resources.Load<Texture2D>("JejakAsaUI/Generated/PaperButton");
            if (paperButton != null) { AmberBtnStyle.normal.background = paperButton; AmberBtnStyle.hover.background = paperButton; AmberBtnStyle.active.background = paperButton; AmberBtnStyle.focused.background = paperButton; AmberBtnStyle.border = new RectOffset(0,0,0,0); }
            LockedBtnStyle = ButtonStyle(Hex(0xE7E5DC), Muted);
            centeredWhite = new GUIStyle(White) { alignment = TextAnchor.MiddleCenter };
            centeredInk = new GUIStyle(ButtonLabel) { alignment = TextAnchor.MiddleCenter };
        }
        public static GUIStyle Text(int size, Color color, bool bold = false, TextAnchor alignment = TextAnchor.MiddleLeft)
        {
            var s = new GUIStyle(GUI.skin.label) { font = uiFont != null ? uiFont : GUI.skin.label.font, fontSize = size, fontStyle = bold ? FontStyle.Bold : FontStyle.Normal, alignment = alignment, wordWrap = true, richText = true, padding = new RectOffset(0,0,0,0) };
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
            if (journalPanel != null && r.height >= 300f) GUI.DrawTexture(r, journalPanel, ScaleMode.StretchToFill, true);
            else if (notePanel != null) GUI.DrawTexture(r, notePanel, ScaleMode.StretchToFill, true);
        }
        // UV framing removes the empty margins from the illustration at draw time.
        public static bool Button(Rect r, string text, GUIStyle style) => Button(r, new GUIContent(text), style);
        public static bool Button(Rect r, GUIContent text, GUIStyle style)
        {
            bool primary = style == TealBtnStyle || style == CoralBtnStyle;
            Texture2D art = primary ? greenButton : paperButton;
            Color old = GUI.color;
            bool hover = r.Contains(Event.current.mousePosition);
            if (style == LockedBtnStyle) GUI.color = new Color(.65f,.65f,.55f,.75f);
            else if (style == CoralBtnStyle) GUI.color = new Color(1f,.70f,.58f,1f);
            else if (hover) GUI.color = new Color(1f,.94f,.75f,1f);
            if (art != null) GUI.DrawTextureWithTexCoords(r,art,new Rect(0,.19f,1,.62f),true);
            GUI.color = old;
            if (hitStyle == null) hitStyle = new GUIStyle(GUIStyle.none);
            bool clicked = GUI.Button(r,GUIContent.none,hitStyle);
            if (!string.IsNullOrEmpty(text.text)) GUI.Label(new Rect(r.x+26,r.y,r.width-52,r.height),text,primary ? centeredWhite : centeredInk);
            return clicked;
        }
        private static GUIStyle centeredWhite, centeredInk;
        public static void Rule(float x, float y, float width) => Fill(new Rect(x,y,width,1), Line);
        public static void Header(Rect r, string category, string title, string description, Texture2D artwork = null)
        {
            Card(r);
            if (artwork != null) GUI.DrawTexture(r, artwork, ScaleMode.StretchToFill, true);
            Fill(new Rect(r.x+56,r.y+55,24,3), Clay);
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
            Fill(new Rect(r.x+48,r.y+6,r.width-48,44),new Color(.04f,.13f,.09f,.88f));
            if (interactionKey != null) GUI.DrawTexture(new Rect(r.x+8,r.y+4,48,48), interactionKey, ScaleMode.ScaleToFit, true);
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
            const int size=32; var t = new Texture2D(size,size,TextureFormat.RGBA32,false) { hideFlags=HideFlags.HideAndDontSave, filterMode=FilterMode.Bilinear };
            var pixels = new Color[size*size];
            for(int y=0;y<size;y++) for(int x=0;x<size;x++) { float dx=Mathf.Max(9-x,x-22,0),dy=Mathf.Max(9-y,y-22,0); Color c=fill; c.a*=Mathf.Clamp01(9.5f-Mathf.Sqrt(dx*dx+dy*dy)); pixels[y*size+x]=c; }
            t.SetPixels(pixels); t.Apply(); return t;
        }
    }
}

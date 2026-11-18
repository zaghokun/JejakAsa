from pathlib import Path
root=Path('Unity/JejakAsa/Assets/_Project/Scripts')
p=root/'CozyGuiTheme.cs';s=p.read_text(encoding='utf-8-sig');s=s.replace('private static Font uiFont;','private static Font uiFont, headingFont;\n        private static Texture2D greenButton, paperButton;\n        private static GUIStyle hitStyle;')
s=s.replace('            CardTex = Rounded(Paper);','            headingFont = Resources.Load<Font>("JejakAsaUI/Fonts/BreeSerif-Regular");\n            CardTex = Rounded(Paper);')
s=s.replace('Body = Text(18, Ink); Small = Text(14, Muted); Eyebrow = Text(12, Green, true); Title = Text(34, Ink, true);','Body = Text(20, Ink); Small = Text(16, Muted); Eyebrow = Text(14, Green, true); Title = Text(36, Ink, true);')
s=s.replace('            White = Text(18, Paper, true); ButtonLabel = Text(18, Ink, true);','            if (headingFont != null) { Title.font = headingFont; Title.fontStyle = FontStyle.Normal; }\n            White = Text(22, Paper, true); ButtonLabel = Text(22, Ink, true);\n            if (headingFont != null) { White.font = ButtonLabel.font = headingFont; White.fontStyle = ButtonLabel.fontStyle = FontStyle.Normal; }')
s=s.replace('            var illustratedButton = Resources.Load<Texture2D>("JejakAsaUI/Generated/VolunteerButton");','            greenButton = Resources.Load<Texture2D>("JejakAsaUI/Generated/VolunteerButton");\n            var illustratedButton = greenButton;')
s=s.replace('            var paperButton = Resources.Load<Texture2D>("JejakAsaUI/Generated/PaperButton");','            paperButton = Resources.Load<Texture2D>("JejakAsaUI/Generated/PaperButton");')
a=s.index('            if (!inset) GUI.Box(new Rect(r.x, r.y + 5,');b=s.index('        public static void Rule',a)
s=s[:a]+'''            if (inset) { Rule(r.x + 8, r.y, r.width - 16); return; }
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
''' +s[b:]
s=s.replace('            LockedBtnStyle = ButtonStyle(Hex(0xE7E5DC), Muted);','            LockedBtnStyle = ButtonStyle(Hex(0xE7E5DC), Muted);\n            centeredWhite = new GUIStyle(White) { alignment = TextAnchor.MiddleCenter };\n            centeredInk = new GUIStyle(ButtonLabel) { alignment = TextAnchor.MiddleCenter };')
s=s.replace('Fill(new Rect(r.x+28,r.y+29,24,3), Clay);','Fill(new Rect(r.x+56,r.y+55,24,3), Clay);').replace('new Rect(r.x+64,r.y+21,r.width-92,20)','new Rect(r.x+94,r.y+44,r.width-150,24)').replace('new Rect(r.x+28,r.y+58,r.width-56,45)','new Rect(r.x+56,r.y+80,r.width-112,48)').replace('new Rect(r.x+28,r.y+110,r.width-56,44)','new Rect(r.x+56,r.y+132,r.width-112,48)').replace('Rule(r.x+28,r.y+170,r.width-56);','Rule(r.x+56,r.y+190,r.width-112);')
p.write_text(s,encoding='utf-8')
for p in root.glob('*.cs'):
 if p.name=='CozyGuiTheme.cs':continue
 s=p.read_text(encoding='utf-8-sig').replace('GUI.Button(', 'CozyGuiTheme.Button(') if p.name not in ['SceneStartFade.cs'] else p.read_text(encoding='utf-8-sig')
 p.write_text(s,encoding='utf-8')
p=root/'SessionMenu.cs';s=p.read_text();s=s.replace('                CozyGuiTheme.Card(sheet);','                CozyGuiTheme.Fill(new Rect(0,0,490,sh), new Color(.04f,.10f,.07f,.55f));',1);s=s.replace('GUI.Label(new Rect(68, y + 244, 354, 48), "Bantuan sampai,\\nharapan tumbuh.", CozyGuiTheme.Body);','GUI.Label(new Rect(68, y + 244, 354, 48), "Bantuan sampai,\\nharapan tumbuh.", CozyGuiTheme.White);');a=s.index('                float x = (sw - 1060) / 2;');b=s.index('            }\n            GUI.matrix = old;',a)
s=s[:a]+'''                float cardHeight = Mathf.Min(656, 280 + OptionCount() * 64);
                float x = (sw - 720) / 2;
                float top = (sh - cardHeight) / 2;
                Rect sheet = new Rect(x,top,720,cardHeight);
                CozyGuiTheme.Header(sheet,isMainMenu ? "JEJAK ASA" : "PERJALANAN DIJEDA",PageTitle(),PageDescription());
                DrawRows(new Rect(x+60,top+214,600,320));
                if (!string.IsNullOrEmpty(notice)) GUI.Label(new Rect(x+60,top+cardHeight-100,600,44),notice,CozyGuiTheme.Body);
                GUI.Label(new Rect(x+60,top+cardHeight-48,600,22),"Atas / Bawah  Pilih     Enter  Lanjut     Esc  Kembali",CozyGuiTheme.HintStyle);
''' +s[b:]
s=s.replace('float rowHeight = page == MenuPage.Load ? 76 : 57;','float rowHeight = page == MenuPage.Load ? 76 : 62;')
p.write_text(s)

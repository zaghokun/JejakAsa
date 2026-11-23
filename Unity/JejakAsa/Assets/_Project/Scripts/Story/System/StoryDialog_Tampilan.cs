using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace JejakAsa
{
    internal sealed class StoryDialog_Tampilan : MonoBehaviour
    {
        private static readonly Dictionary<string, Texture2D> Portraits = new Dictionary<string, Texture2D>();
        private RawImage portrait;
        private CanvasGroup group;
        private TMP_Text body;
        private float openedAt;
        private float lineAt;

        internal static Texture2D Portrait(string speaker)
        {
            string key = speaker == null ? "" : speaker.Trim().ToLowerInvariant();
            string asset = null;

            if (key.Contains("vicky")) asset = "Vicky";
            else if (key.Contains("ibu nur") || key.Contains("warga pos")) asset = "Ibu";
            else if (key.Contains("ibu")) asset = "Ibu";
            else if (key.Contains("rina") || key.Contains("warga timur")) asset = "WargaPerempuan";
            else if (key.Contains("arif") || key.Contains("warga tengah")) asset = "WargaLaki";
            else if (key.Contains("ayah")) asset = "Ayah";
            else if (key.Contains("jonathan") || key.Contains("pak jonathan")) asset = "Jonathan";
            else if (key.Contains("ophelia")) asset = "Ophelia";
            else if (key.Contains("deddy")) asset = "Deddy";
            else if (key.Contains("gas oke")) asset = "GasOke";
            else if (key.Contains("preman")) asset = "Preman";
            else if (key.Contains("perempuan")) asset = "WargaPerempuan";
            else if (key.Contains("laki") || key.Contains("warga") || key.Contains("pak")) asset = "WargaLaki";

            if (asset == null) return null;
            if (!Portraits.TryGetValue(asset, out Texture2D texture) || texture == null)
            {
                texture = Resources.Load<Texture2D>("JejakAsaUI/Generated/Portraits/" + asset);
                if (texture == null) texture = Resources.Load<Texture2D>("JejakAsaUI/Portraits/" + asset);
                Portraits[asset] = texture;
            }
            return texture;
        }

        private static Sprite panelSprite;
        internal static void Skin(Image image, Color color)
        {
            if (image == null) return;
            if (panelSprite == null)
            {
                var tex = CozyGuiTheme.Rounded(Color.white);
                panelSprite = Sprite.Create(tex, new Rect(0, 0, 32, 32), new Vector2(.5f, .5f), 100, 0, SpriteMeshType.FullRect, new Vector4(12,12,12,12));
                panelSprite.hideFlags = HideFlags.HideAndDontSave;
            }
            image.sprite = panelSprite; image.type = Image.Type.Sliced; image.color = color;
        }
        internal static void StylePrompt(GameObject panel, TMP_Text label)
        {
            CanvasScaler scaler = panel.GetComponentInParent<CanvasScaler>();
            if (scaler != null) { scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize; scaler.referenceResolution = new Vector2(1280,720); scaler.screenMatchMode = CanvasScaler.ScreenMatchMode.Expand; }
            var promptBackground = panel.GetComponent<Image>(); if (promptBackground != null) { promptBackground.color = Color.white; promptBackground.sprite = CozyGuiTheme.HybridSprite("pita_gelap", new Vector4(8,10,8,6)); promptBackground.type = Image.Type.Sliced; }
            RectTransform r = panel.GetComponent<RectTransform>();
            r.anchorMin = r.anchorMax = new Vector2(.5f,0); r.pivot = new Vector2(.5f,0);
            r.anchoredPosition = new Vector2(0,48); r.sizeDelta = new Vector2(600,56);
            if (CozyGuiTheme.DialogueFont != null) label.font = CozyGuiTheme.DialogueFont;
            label.fontSize = 18; label.enableAutoSizing = true; label.fontSizeMin = 16; label.fontSizeMax = 18;
            label.color = CozyGuiTheme.Paper; label.alignment = TextAlignmentOptions.Center;
            label.rectTransform.anchorMin = Vector2.zero; label.rectTransform.anchorMax = Vector2.one;
            var keyTexture = Resources.Load<Texture2D>("JejakAsaUI/Hybrid/tombol_e");
            if (keyTexture != null)
            {
                var key = new GameObject("InteractionKey", typeof(RectTransform), typeof(RawImage));
                key.transform.SetParent(panel.transform, false);
                var icon = key.GetComponent<RawImage>(); icon.texture = keyTexture; icon.raycastTarget = false;
                var keyRect = icon.rectTransform; keyRect.anchorMin = keyRect.anchorMax = new Vector2(0,.5f);
                keyRect.pivot = new Vector2(0,.5f); keyRect.anchoredPosition = new Vector2(8,0); keyRect.sizeDelta = new Vector2(48,48);
            }
            label.rectTransform.offsetMin = new Vector2(64,8); label.rectTransform.offsetMax = new Vector2(-18,-8);
        }
        private static void Place(RectTransform r, float x, float y, float w, float h)
        {
            r.anchorMin = r.anchorMax = r.pivot = new Vector2(0,1);
            r.anchoredPosition = new Vector2(x,-y); r.sizeDelta = new Vector2(w,h);
        }
        private Image Panel(string name, Color color, float x, float y, float w, float h)
        {
            var go = new GameObject(name, typeof(RectTransform), typeof(Image)); go.transform.SetParent(transform,false);
            var image = go.GetComponent<Image>(); image.raycastTarget = false; Skin(image,color); Place(image.rectTransform,x,y,w,h); return image;
        }
        internal void Setup(TMP_Text title, TMP_Text dialogueBody, Button button)
        {
            body = dialogueBody;
            if (CozyGuiTheme.DialogueFont != null) { title.font = CozyGuiTheme.DialogueFont; dialogueBody.font = CozyGuiTheme.DialogueFont; }
            group = GetComponent<CanvasGroup>(); if (group == null) group = gameObject.AddComponent<CanvasGroup>();
            RectTransform root = GetComponent<RectTransform>();
            root.anchorMin = root.anchorMax = new Vector2(.5f,0); root.pivot = new Vector2(.5f,0);
            root.anchoredPosition = new Vector2(0,32); root.sizeDelta = new Vector2(1120,236);
            var background = GetComponent<Image>(); if (background != null) background.color = Color.clear;
            // panel dialog gaya hybrid (kartu krem), gantiin gambar panel yang lama
            var kartu = new GameObject("KartuDialog", typeof(RectTransform), typeof(Image)); kartu.transform.SetParent(transform, false);
            var kartuImg = kartu.GetComponent<Image>(); kartuImg.raycastTarget = false;
            kartuImg.sprite = CozyGuiTheme.HybridSprite("kartu", new Vector4(12,14,12,12)); kartuImg.type = Image.Type.Sliced;
            var rect = kartuImg.rectTransform; rect.anchorMin = Vector2.zero; rect.anchorMax = Vector2.one; rect.offsetMin = rect.offsetMax = Vector2.zero; kartu.transform.SetAsFirstSibling();

            var portraitObject = new GameObject("SpeakerPortrait",typeof(RectTransform),typeof(RawImage)); portraitObject.transform.SetParent(transform,false);
            portrait = portraitObject.GetComponent<RawImage>(); portrait.raycastTarget = false; Place(portrait.rectTransform,16,-22,168,242);
            Place(title.rectTransform,206,36,850,32); title.fontSize = 24; title.enableAutoSizing = true; title.fontSizeMin=22; title.fontSizeMax=24; title.fontStyle=FontStyles.Bold; title.color=CozyGuiTheme.Green; title.alignment=TextAlignmentOptions.Left;
            Place(body.rectTransform,206,80,850,86); body.fontSize=23; body.enableAutoSizing=true; body.fontSizeMin=21; body.fontSizeMax=23; body.color=CozyGuiTheme.Ink; body.alignment=TextAlignmentOptions.TopLeft;
            Panel("Divider",CozyGuiTheme.Line,206,174,850,1);
            TMP_Text hint = transform.Find("DialogueHint")?.GetComponent<TMP_Text>();
            if(hint != null) { if (CozyGuiTheme.DialogueFont != null) hint.font = CozyGuiTheme.DialogueFont; Place(hint.rectTransform,206,190,670,24); hint.color=CozyGuiTheme.Muted; hint.fontSize=14; hint.text="Enter / Spasi / Esc  Lanjut"; hint.alignment=TextAlignmentOptions.Left; }
            Place(button.GetComponent<RectTransform>(),946,183,146,40); var tombolImg = button.GetComponent<Image>(); tombolImg.sprite = CozyGuiTheme.HybridSprite("tombol_teal", new Vector4(8,12,8,6)); tombolImg.type = Image.Type.Sliced; tombolImg.color = Color.white;
            var colors=button.colors; colors.normalColor=Color.white; colors.highlightedColor=new Color(.85f,.95f,.87f); colors.pressedColor=new Color(.7f,.8f,.72f); button.colors=colors;
            TMP_Text buttonLabel=button.GetComponentInChildren<TMP_Text>(true); if(buttonLabel!=null) { if (CozyGuiTheme.DialogueFont != null) buttonLabel.font = CozyGuiTheme.DialogueFont; buttonLabel.color=CozyGuiTheme.Paper; buttonLabel.fontSize=17; }
            title.transform.SetAsLastSibling(); body.transform.SetAsLastSibling(); button.transform.SetAsLastSibling();
        }
        internal void Open(string speaker) { openedAt=Time.unscaledTime; Line(speaker); }
        internal void Line(string speaker)
        {
            lineAt=Time.unscaledTime; var texture=Portrait(speaker); portrait.texture=texture; portrait.enabled=texture!=null;
            if (texture != null)
            {
                float aspect=(float)texture.width/texture.height;
                float width=Mathf.Min(168,242*aspect),height=Mathf.Min(242,168/aspect);
                Place(portrait.rectTransform,16+(168-width)/2,220-height,width,height);
            }
        }
        private void Update()
        {
            if(group!=null) group.alpha=Mathf.Lerp(.7f,1,Mathf.Clamp01((Time.unscaledTime-openedAt)/.2f));
            if(body!=null) body.alpha=Mathf.Lerp(.7f,1,Mathf.Clamp01((Time.unscaledTime-lineAt)/.16f));
        }
    }
}

using UnityEngine;

namespace JejakAsa
{
    /// <summary>
    /// Gaya visual IMGUI bersama (hangat, ramah anak, rounded card &amp; stempel vektor)
    /// agar HUD misi, layar hasil (U08), dan panel intimidasi (U09/U10) konsisten tanpa duplikasi.
    /// </summary>
    internal static class CozyGuiTheme
    {
        public static Texture2D ShadowTex { get; private set; }
        public static Texture2D CardTex { get; private set; }
        public static Texture2D RibbonTex { get; private set; }
        public static Texture2D TimerNormalTex { get; private set; }
        public static Texture2D TimerWarnTex { get; private set; }
        public static Texture2D TimerDangerTex { get; private set; }
        public static Texture2D StampDoneTex { get; private set; }
        public static Texture2D StampEmptyTex { get; private set; }
        public static Texture2D BannerInfoTex { get; private set; }
        public static Texture2D BannerSuccessTex { get; private set; }
        public static Texture2D BannerFailTex { get; private set; }

        public static Texture2D DimTex { get; private set; }
        public static Texture2D InnerBoxTex { get; private set; }
        public static Texture2D OutcomeSuccessTex { get; private set; }
        public static Texture2D OutcomePartialTex { get; private set; }
        public static Texture2D OutcomeFailTex { get; private set; }

        public static GUIStyle CardStyle { get; private set; }
        public static GUIStyle RibbonStyle { get; private set; }
        public static GUIStyle TimerStyle { get; private set; }
        public static GUIStyle LabelStyle { get; private set; }
        public static GUIStyle SubLabelStyle { get; private set; }
        public static GUIStyle BannerStyle { get; private set; }
        public static GUIStyle OutcomeHeaderStyle { get; private set; }
        public static GUIStyle OutcomeDescStyle { get; private set; }
        public static GUIStyle InnerBoxStyle { get; private set; }
        public static GUIStyle StatLeftStyle { get; private set; }
        public static GUIStyle StatRightStyle { get; private set; }
        public static GUIStyle HintStyle { get; private set; }

        // Varian tombol untuk Hasil Misi (U08) dan Pilihan Intimidasi (U09/U10)
        public static GUIStyle TealBtnStyle { get; private set; }
        public static GUIStyle CoralBtnStyle { get; private set; }
        public static GUIStyle AmberBtnStyle { get; private set; }
        public static GUIStyle LockedBtnStyle { get; private set; }
        public static GUIStyle LockReasonStyle { get; private set; }
        public static GUIStyle DialogueTextStyle { get; private set; }

        public static float Scale => Mathf.Max(1f, Screen.height / 720f);

        private static bool initialized;

        public static void EnsureInitialized()
        {
            if (initialized && ShadowTex != null && CardTex != null)
                return;

            ShadowTex = MakeRoundedRectTex(32, 32, 14, new Color(0.10f, 0.08f, 0.06f, 0.25f), Color.clear, 0);
            CardTex = MakeRoundedRectTex(32, 32, 14, new Color(1.0f, 0.98f, 0.94f, 0.97f), new Color(0.08f, 0.56f, 0.52f, 1.0f), 2);
            RibbonTex = MakeRoundedRectTex(24, 24, 10, new Color(0.08f, 0.56f, 0.52f, 1.0f), new Color(1.0f, 0.98f, 0.94f, 1.0f), 2);
            TimerNormalTex = MakeRoundedRectTex(24, 24, 10, new Color(0.96f, 0.62f, 0.04f, 1.0f), new Color(1.0f, 0.98f, 0.94f, 1.0f), 2);
            TimerWarnTex = MakeRoundedRectTex(24, 24, 10, new Color(0.98f, 0.45f, 0.09f, 1.0f), new Color(1.0f, 0.98f, 0.94f, 1.0f), 2);
            TimerDangerTex = MakeRoundedRectTex(24, 24, 10, new Color(0.90f, 0.25f, 0.25f, 1.0f), new Color(1.0f, 0.98f, 0.94f, 1.0f), 2);

            StampDoneTex = MakeCircleTex(32, new Color(0.10f, 0.72f, 0.50f, 1.0f), new Color(0.05f, 0.50f, 0.35f, 1.0f), 2, drawCheckmark: true);
            StampEmptyTex = MakeCircleTex(32, new Color(0.92f, 0.89f, 0.84f, 1.0f), new Color(0.80f, 0.76f, 0.70f, 1.0f), 2, drawCheckmark: false);

            BannerInfoTex = MakeRoundedRectTex(24, 24, 8, new Color(0.99f, 0.94f, 0.78f, 1.0f), new Color(0.96f, 0.78f, 0.35f, 0.6f), 1);
            BannerSuccessTex = MakeRoundedRectTex(24, 24, 8, new Color(0.82f, 0.98f, 0.90f, 1.0f), new Color(0.10f, 0.72f, 0.50f, 0.6f), 1);
            BannerFailTex = MakeRoundedRectTex(24, 24, 8, new Color(0.99f, 0.86f, 0.86f, 1.0f), new Color(0.90f, 0.25f, 0.25f, 0.6f), 1);

            DimTex = MakeRoundedRectTex(4, 4, 0, new Color(0.05f, 0.08f, 0.12f, 0.65f), Color.clear, 0);
            InnerBoxTex = MakeRoundedRectTex(24, 24, 10, new Color(0.96f, 0.93f, 0.86f, 1.0f), new Color(0.85f, 0.78f, 0.68f, 1.0f), 1);

            OutcomeSuccessTex = MakeRoundedRectTex(24, 24, 12, new Color(0.06f, 0.65f, 0.42f, 1.0f), new Color(1.0f, 0.98f, 0.94f, 1.0f), 2);
            OutcomePartialTex = MakeRoundedRectTex(24, 24, 12, new Color(0.90f, 0.55f, 0.04f, 1.0f), new Color(1.0f, 0.98f, 0.94f, 1.0f), 2);
            OutcomeFailTex = MakeRoundedRectTex(24, 24, 12, new Color(0.85f, 0.22f, 0.22f, 1.0f), new Color(1.0f, 0.98f, 0.94f, 1.0f), 2);

            CardStyle = new GUIStyle(GUI.skin.box)
            {
                normal = { background = CardTex },
                border = new RectOffset(14, 14, 14, 14)
            };

            RibbonStyle = new GUIStyle(GUI.skin.label)
            {
                normal = { background = RibbonTex, textColor = Color.white },
                border = new RectOffset(10, 10, 10, 10),
                fontSize = 11,
                fontStyle = FontStyle.Bold,
                alignment = TextAnchor.MiddleCenter
            };

            TimerStyle = new GUIStyle(GUI.skin.label)
            {
                normal = { background = TimerNormalTex, textColor = Color.white },
                border = new RectOffset(10, 10, 10, 10),
                fontSize = 12,
                fontStyle = FontStyle.Bold,
                alignment = TextAnchor.MiddleCenter
            };

            LabelStyle = new GUIStyle(GUI.skin.label)
            {
                fontSize = 13,
                fontStyle = FontStyle.Bold,
                alignment = TextAnchor.MiddleLeft,
                richText = true
            };
            LabelStyle.normal.textColor = new Color(0.24f, 0.19f, 0.16f);

            SubLabelStyle = new GUIStyle(GUI.skin.label)
            {
                fontSize = 11,
                fontStyle = FontStyle.Normal,
                alignment = TextAnchor.MiddleLeft,
                richText = true
            };
            SubLabelStyle.normal.textColor = new Color(0.42f, 0.36f, 0.32f);

            BannerStyle = new GUIStyle(GUI.skin.label)
            {
                border = new RectOffset(8, 8, 8, 8),
                padding = new RectOffset(8, 8, 4, 4),
                fontSize = 11,
                fontStyle = FontStyle.Bold,
                wordWrap = true,
                alignment = TextAnchor.MiddleCenter
            };

            OutcomeHeaderStyle = new GUIStyle(GUI.skin.label)
            {
                normal = { textColor = Color.white },
                border = new RectOffset(12, 12, 12, 12),
                fontSize = 14,
                fontStyle = FontStyle.Bold,
                alignment = TextAnchor.MiddleCenter
            };

            OutcomeDescStyle = new GUIStyle(GUI.skin.label)
            {
                fontSize = 12,
                wordWrap = true,
                alignment = TextAnchor.MiddleCenter,
                richText = true
            };
            OutcomeDescStyle.normal.textColor = new Color(0.32f, 0.25f, 0.20f);

            InnerBoxStyle = new GUIStyle(GUI.skin.box)
            {
                normal = { background = InnerBoxTex },
                border = new RectOffset(10, 10, 10, 10)
            };

            StatLeftStyle = new GUIStyle(GUI.skin.label)
            {
                fontSize = 12,
                alignment = TextAnchor.MiddleLeft
            };
            StatLeftStyle.normal.textColor = new Color(0.40f, 0.33f, 0.28f);

            StatRightStyle = new GUIStyle(GUI.skin.label)
            {
                fontSize = 12,
                fontStyle = FontStyle.Bold,
                alignment = TextAnchor.MiddleRight,
                richText = true
            };
            StatRightStyle.normal.textColor = new Color(0.18f, 0.14f, 0.10f);

            HintStyle = new GUIStyle(GUI.skin.label)
            {
                fontSize = 10,
                alignment = TextAnchor.MiddleCenter
            };
            HintStyle.normal.textColor = new Color(0.55f, 0.48f, 0.42f);

            TealBtnStyle = CreateButtonStyle(
                new Color(0.08f, 0.56f, 0.52f, 1.0f), new Color(0.05f, 0.40f, 0.38f, 1.0f),
                new Color(0.10f, 0.68f, 0.62f, 1.0f), new Color(0.05f, 0.45f, 0.42f, 1.0f),
                Color.white);

            CoralBtnStyle = CreateButtonStyle(
                new Color(0.84f, 0.26f, 0.24f, 1.0f), new Color(0.65f, 0.16f, 0.15f, 1.0f),
                new Color(0.92f, 0.32f, 0.30f, 1.0f), new Color(0.70f, 0.18f, 0.17f, 1.0f),
                Color.white);

            AmberBtnStyle = CreateButtonStyle(
                new Color(0.92f, 0.56f, 0.06f, 1.0f), new Color(0.72f, 0.42f, 0.04f, 1.0f),
                new Color(0.98f, 0.64f, 0.12f, 1.0f), new Color(0.78f, 0.46f, 0.05f, 1.0f),
                Color.white);

            LockedBtnStyle = CreateButtonStyle(
                new Color(0.84f, 0.81f, 0.77f, 1.0f), new Color(0.68f, 0.64f, 0.60f, 1.0f),
                new Color(0.84f, 0.81f, 0.77f, 1.0f), new Color(0.68f, 0.64f, 0.60f, 1.0f),
                new Color(0.45f, 0.41f, 0.38f, 1.0f));

            LockReasonStyle = new GUIStyle(GUI.skin.label)
            {
                normal = { background = BannerFailTex, textColor = new Color(0.68f, 0.14f, 0.14f) },
                border = new RectOffset(8, 8, 8, 8),
                fontSize = 11,
                fontStyle = FontStyle.Bold,
                wordWrap = true,
                alignment = TextAnchor.MiddleCenter
            };

            DialogueTextStyle = new GUIStyle(GUI.skin.label)
            {
                fontSize = 13,
                wordWrap = true,
                alignment = TextAnchor.UpperLeft,
                richText = true
            };
            DialogueTextStyle.normal.textColor = new Color(0.20f, 0.16f, 0.12f);

            initialized = true;
        }

        private static GUIStyle CreateButtonStyle(Color normalFill, Color normalBorder, Color hoverFill, Color hoverBorder, Color textColor)
        {
            Texture2D normalTex = MakeRoundedRectTex(24, 24, 12, normalFill, normalBorder, 2);
            Texture2D hoverTex = MakeRoundedRectTex(24, 24, 12, hoverFill, hoverBorder, 2);
            return new GUIStyle(GUI.skin.button)
            {
                normal = { background = normalTex, textColor = textColor },
                hover = { background = hoverTex, textColor = textColor },
                active = { background = hoverTex, textColor = textColor },
                border = new RectOffset(12, 12, 12, 12),
                fontSize = 13,
                fontStyle = FontStyle.Bold,
                alignment = TextAnchor.MiddleCenter
            };
        }

        private static Texture2D MakeRoundedRectTex(int width, int height, int radius, Color fillColor, Color borderColor, int borderWidth)
        {
            Texture2D tex = new Texture2D(width, height, TextureFormat.RGBA32, false)
            {
                filterMode = FilterMode.Bilinear,
                hideFlags = HideFlags.HideAndDontSave
            };
            Color[] colors = new Color[width * height];

            for (int y = 0; y < height; y++)
            {
                for (int x = 0; x < width; x++)
                {
                    float cx = (x < radius) ? radius : (x >= width - radius) ? (width - 1 - radius) : x;
                    float cy = (y < radius) ? radius : (y >= height - radius) ? (height - 1 - radius) : y;
                    float dist = Vector2.Distance(new Vector2(x, y), new Vector2(cx, cy));

                    int index = y * width + x;
                    if (dist > radius)
                    {
                        float alpha = Mathf.Clamp01(1f - (dist - radius));
                        colors[index] = borderWidth > 0
                            ? new Color(borderColor.r, borderColor.g, borderColor.b, borderColor.a * alpha)
                            : new Color(fillColor.r, fillColor.g, fillColor.b, fillColor.a * alpha);
                    }
                    else if (borderWidth > 0 && dist > radius - borderWidth)
                    {
                        colors[index] = borderColor;
                    }
                    else
                    {
                        colors[index] = fillColor;
                    }
                }
            }
            tex.SetPixels(colors);
            tex.Apply();
            return tex;
        }

        private static float DistToSegment(Vector2 p, Vector2 a, Vector2 b)
        {
            Vector2 ab = b - a;
            float t = Mathf.Clamp01(Vector2.Dot(p - a, ab) / Mathf.Max(0.0001f, ab.sqrMagnitude));
            return Vector2.Distance(p, a + t * ab);
        }

        private static Texture2D MakeCircleTex(int size, Color fillColor, Color borderColor, int borderWidth, bool drawCheckmark)
        {
            Texture2D tex = new Texture2D(size, size, TextureFormat.RGBA32, false)
            {
                filterMode = FilterMode.Bilinear,
                hideFlags = HideFlags.HideAndDontSave
            };
            Color[] colors = new Color[size * size];
            float center = (size - 1) * 0.5f;
            float radius = size * 0.5f - 1f;

            Vector2 p1 = new Vector2(size * 0.28f, size * 0.48f);
            Vector2 p2 = new Vector2(size * 0.44f, size * 0.33f);
            Vector2 p3 = new Vector2(size * 0.72f, size * 0.65f);

            for (int y = 0; y < size; y++)
            {
                for (int x = 0; x < size; x++)
                {
                    float dist = Vector2.Distance(new Vector2(x, y), new Vector2(center, center));
                    int idx = y * size + x;
                    Color baseCol;

                    if (dist > radius)
                    {
                        float a = Mathf.Clamp01(1f - (dist - radius));
                        baseCol = new Color(borderColor.r, borderColor.g, borderColor.b, borderColor.a * a);
                    }
                    else if (dist > radius - borderWidth)
                    {
                        baseCol = borderColor;
                    }
                    else
                    {
                        baseCol = fillColor;
                        if (drawCheckmark)
                        {
                            Vector2 pt = new Vector2(x, y);
                            float d1 = DistToSegment(pt, p1, p2);
                            float d2 = DistToSegment(pt, p2, p3);
                            float checkDist = Mathf.Min(d1, d2);
                            float thickness = size * 0.065f;
                            if (checkDist < thickness + 1f)
                            {
                                float checkAlpha = Mathf.Clamp01(1f - (checkDist - thickness));
                                baseCol = Color.Lerp(fillColor, Color.white, checkAlpha);
                            }
                        }
                    }
                    colors[idx] = baseCol;
                }
            }
            tex.SetPixels(colors);
            tex.Apply();
            return tex;
        }
    }
}

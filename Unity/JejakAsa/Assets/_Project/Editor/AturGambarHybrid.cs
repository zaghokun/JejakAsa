#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;

// setting import buat gambar2 UI hybrid (biar pixelnya tajem + bisa di-slice)
public class AturGambarHybrid : AssetPostprocessor
{
    void OnPreprocessTexture()
    {
        if (!assetPath.StartsWith("Assets/_Project/Resources/JejakAsaUI/Hybrid/")) return;
        TextureImporter ti = (TextureImporter)assetImporter;
        ti.textureType = TextureImporterType.Sprite;
        ti.spriteImportMode = SpriteImportMode.Single;
        ti.filterMode = FilterMode.Point;
        ti.mipmapEnabled = false;
        ti.npotScale = TextureImporterNPOTScale.None;
        ti.alphaIsTransparency = true;
        ti.textureCompression = TextureImporterCompression.Uncompressed;
        ti.wrapMode = TextureWrapMode.Clamp;
        ti.spritePixelsPerUnit = 100;

        // pinggiran buat 9-slice (kiri, bawah, kanan, atas)
        string nama = System.IO.Path.GetFileNameWithoutExtension(assetPath);
        if (nama.StartsWith("tombol_") && nama != "tombol_e") ti.spriteBorder = new Vector4(8, 12, 8, 6);
        if (nama == "kartu") ti.spriteBorder = new Vector4(12, 14, 12, 12);
        if (nama == "lencana") ti.spriteBorder = new Vector4(6, 8, 6, 4);
        if (nama == "pita_gelap") ti.spriteBorder = new Vector4(8, 10, 8, 6);
    }
}
#endif

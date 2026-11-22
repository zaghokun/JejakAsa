#if UNITY_EDITOR
using UnityEditor;

// UI art is viewed close to the screen; keep edges and textural detail crisp.
internal sealed class JejakAsaUiTextureImport : AssetPostprocessor
{
    private void OnPreprocessTexture()
    {
        if (!assetPath.StartsWith("Assets/_Project/Resources/JejakAsaUI/Generated/")) return;
        var importer = (TextureImporter)assetImporter;
        importer.mipmapEnabled = false;
        importer.npotScale = TextureImporterNPOTScale.None;
        importer.alphaIsTransparency = true;
        importer.textureCompression = TextureImporterCompression.Uncompressed;
        importer.maxTextureSize = 2048;
        importer.filterMode = UnityEngine.FilterMode.Bilinear;
        importer.wrapMode = UnityEngine.TextureWrapMode.Clamp;
    }
}
#endif

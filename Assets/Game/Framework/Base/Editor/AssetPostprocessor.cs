using UnityEditor;
using UnityEngine;

public class AssetPostprocessor : UnityEditor.AssetPostprocessor
{
    public void OnPostprocessTexture(Texture2D texture)
    {
#if MANAGE_IMPORT
        if (!assetPath.Contains("Assets")) return;

        var importer = (TextureImporter)assetImporter;
        if (!importer) return;

        var iosSettings = importer.GetPlatformTextureSettings("IOS");
        if (iosSettings.format != TextureImporterFormat.ASTC_6x6)
        {
            iosSettings.format = TextureImporterFormat.ASTC_6x6;
            if (!iosSettings.overridden) iosSettings.overridden = true;
        }
        importer.SetPlatformTextureSettings(iosSettings);
#endif
    }
}
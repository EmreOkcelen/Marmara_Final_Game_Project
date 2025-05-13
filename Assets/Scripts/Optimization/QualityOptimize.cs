using UnityEngine;
using UnityEditor;

// This Editor script enables GPU instancing on all materials,
// sets mesh compression to Low for all imported models,
// and sets texture compression to Low for all textures in the project.
public class BatchImportSettings : EditorWindow
{
    [MenuItem("Tools/Batch Import Settings")]
    public static void ApplyBatchSettings()
    {
        // Materials: Enable GPU Instancing
        string[] materialGUIDs = AssetDatabase.FindAssets("t:Material");
        foreach (string guid in materialGUIDs)
        {
            string path = AssetDatabase.GUIDToAssetPath(guid);
            Material mat = AssetDatabase.LoadAssetAtPath<Material>(path);
            if (mat != null)
            {
                mat.enableInstancing = true;
                EditorUtility.SetDirty(mat);
            }
        }

        // Meshes: Set Mesh Compression to Low
        string[] modelGUIDs = AssetDatabase.FindAssets("t:Model");
        foreach (string guid in modelGUIDs)
        {
            string path = AssetDatabase.GUIDToAssetPath(guid);
            ModelImporter importer = AssetImporter.GetAtPath(path) as ModelImporter;
            if (importer != null)
            {
                importer.meshCompression = ModelImporterMeshCompression.Low;
                importer.SaveAndReimport();
            }
        }

        // Textures: Set Compression to Low
        string[] textureGUIDs = AssetDatabase.FindAssets("t:Texture");
        foreach (string guid in textureGUIDs)
        {
            string path = AssetDatabase.GUIDToAssetPath(guid);
            TextureImporter tImporter = AssetImporter.GetAtPath(path) as TextureImporter;
            if (tImporter != null)
            {
                // Use CompressedLQ for lowest-quality compression (requires Unity 2021.2+)
                #if UNITY_2021_2_OR_NEWER
                tImporter.textureCompression = TextureImporterCompression.CompressedLQ;
                #else
                tImporter.textureCompression = TextureImporterCompression.Compressed;
                tImporter.compressionQuality = 0; // lowest quality
                #endif
                tImporter.SaveAndReimport();
            }
        }

        // Finalize
        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();
        Debug.Log("Batch import settings applied: GPU Instancing enabled on materials, mesh compression set to Low, texture compression set to Low.");
    }
}

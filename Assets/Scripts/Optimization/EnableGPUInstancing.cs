using UnityEngine;
using UnityEditor;
using System.IO;

public class EnableGPUInstancing : MonoBehaviour
{
    [MenuItem("Tools/Enable GPU Instancing on All Materials")]
    static void EnableInstancingOnAllMaterials()
    {
        string[] materialGuids = AssetDatabase.FindAssets("t:Material");
        int count = 0;

        foreach (string guid in materialGuids)
        {
            string path = AssetDatabase.GUIDToAssetPath(guid);
            Material mat = AssetDatabase.LoadAssetAtPath<Material>(path);

            if (mat != null && !mat.enableInstancing)
            {
                mat.enableInstancing = true;
                EditorUtility.SetDirty(mat);
                count++;
            }
        }

        AssetDatabase.SaveAssets();
        Debug.Log($"GPU Instancing enabled on {count} materials.");
    }
}
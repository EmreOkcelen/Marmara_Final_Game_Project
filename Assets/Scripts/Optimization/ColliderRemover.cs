#if UNITY_EDITOR
using UnityEngine;
using UnityEditor;

public static class ColliderRemover
{
    [MenuItem("Tools/Remove Colliders from Selected %#r")]
    static void RemoveCollidersFromSelected()
    {
        int removed = 0;
        foreach (var go in Selection.gameObjects)
        {
            var cols = go.GetComponentsInChildren<Collider>(true);
            foreach (var col in cols)
            {
                Object.DestroyImmediate(col);
                removed++;
            }
        }
        Debug.Log($"Removed {removed} colliders.");
    }
}
#endif
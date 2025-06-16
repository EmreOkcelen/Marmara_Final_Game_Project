#if UNITY_EDITOR
using UnityEngine;
using UnityEditor;

public static class ColliderRemover
{
    [MenuItem("Tools/Remove All Colliders from Selected %#r")]
    static void RemoveCollidersFromSelected()
    {
        int removedCount = 0;

        foreach (var root in Selection.gameObjects)
        {
            // 3D collider'ları al (inaktive objeleri de dahil)
            var cols3D = root.GetComponentsInChildren<Collider>(true);
            foreach (var c in cols3D)
            {
                // Undo desteği ile sil
                Undo.DestroyObjectImmediate(c);
                removedCount++;
            }

            // 2D collider'ları da al
            var cols2D = root.GetComponentsInChildren<Collider2D>(true);
            foreach (var c2 in cols2D)
            {
                Undo.DestroyObjectImmediate(c2);
                removedCount++;
            }
        }

        Debug.Log($"Removed {removedCount} collider components (3D + 2D).");
    }
}
#endif
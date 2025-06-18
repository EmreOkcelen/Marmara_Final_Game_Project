using UnityEngine;

[RequireComponent(typeof(Canvas))]
public class CanvasFollowHead : MonoBehaviour
{
    [Tooltip("VR rig içindeki Camera (Head) transform'ı")]
    public Transform headTransform;
    [Tooltip("Kameradan ne kadar uzaklıkta görünmesi istiyorsun")]
    public float distance = 2f;
    [Tooltip("Y ekseninde ne kadar yukarı/aşağı kaydırılsın")]
    public float heightOffset = 0f;

    private void LateUpdate()
    {
        // 1) Pozisyonu güncelle
        Vector3 forward = headTransform.forward;
        Vector3 targetPos = headTransform.position + forward * distance + Vector3.up * heightOffset;
        transform.position = targetPos;

        // 2) Rotasyonu güncelle: Canvas’ın ön yüzü kameraya dönük olsun
        transform.rotation = Quaternion.LookRotation(forward, Vector3.up);
    }
}

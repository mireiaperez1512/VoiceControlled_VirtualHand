using UnityEngine;

public class BH_VirtualGrabber : MonoBehaviour
// VARIOS CUBOS
{
    public Transform handTransform;

    [Header("Búsqueda de objetos por Tag")]
    public string grabbableTag = "Grabbable";
    public float grabRadius = 0.35f;           // Radio para buscar objetos "grabbable"

    public Vector3 grabOffset = Vector3.zero;

    /* ----- runtime ----- */
    private GameObject targetObject;
    private Rigidbody targetRb;
    private bool isHolding = false;
    private bool originalUseGravity;
    private bool originalIsKinematic;


    /* ---------- GRAB ---------- */
    public void GrabObject()
    {
         if (isHolding) ReleaseObject();   // suéltalo si ya llevaba algo

        // Encuentra el objeto más cercano con tag "grabbable" dentro del radio
        GameObject nearest = FindNearestGrabbable();
        if (nearest == null)
        {
            Debug.Log("[VirtualGrabber] No hay objetos 'grabbable' cercanos.");
            return;
        }

        //si otra mano lo tenía, que lo suelte
        foreach (var g in FindObjectsByType<BH_VirtualGrabber>(FindObjectsSortMode.None))
            if (g != this && g.isHolding && g.targetObject == nearest)
                g.ForceExternalRelease(); 
        
        // Prepara referencias
        targetObject = nearest;
        targetRb = targetObject.GetComponent<Rigidbody>();
        originalUseGravity = targetRb.useGravity;
        originalIsKinematic = targetRb.isKinematic;

        // Preparación para seguir la mano
        targetRb.isKinematic = true;
        targetRb.useGravity = false;

        // Parentamos a la mano
        targetObject.transform.SetParent(handTransform, worldPositionStays: false);
        targetObject.transform.localPosition = grabOffset;
        targetObject.transform.localRotation = Quaternion.identity;

        isHolding = true;
        Debug.Log("Grabbed." + targetObject.name);
    }

    //Forzar que la mano que sujetaba el objeto lo suelte
    public void ForceExternalRelease()
    {
        if (!isHolding || targetRb == null) return;

        //Desparentar manteniendo posición global
        targetObject.transform.SetParent(null, true);

        //Restaurar las flags originales de física
        targetRb.isKinematic = originalIsKinematic;
        targetRb.useGravity = originalUseGravity;

        //Limpieza de estado interno
        isHolding = false;
        targetObject = null;
        targetRb = null;
    
    }

    /* ---------- RELEASE ---------- */
    public void ReleaseObject()
    {
        if (!isHolding || targetRb == null) return;

        // Desparentar manteniendo posición global */
        targetObject.transform.SetParent(null, true);

        // Restaurar flags originales */
        targetRb.isKinematic = originalIsKinematic;
        targetRb.useGravity = originalUseGravity;

        // Limpiar referencias
        isHolding = false;
        targetObject = null;
        targetRb = null;

        Debug.Log("Released.");
    }
    
    /* ------------- AUXILIAR ------------- */
    private GameObject FindNearestGrabbable()
    {
        Collider[] hits = Physics.OverlapSphere(
            handTransform.position,
            grabRadius,
            ~0                        
            , QueryTriggerInteraction.Collide
            );

        float minSqr = float.MaxValue;
        GameObject nearest = null;

        foreach (var c in hits)
        {
            if (!c.gameObject.CompareTag(grabbableTag))
                continue;

            float sqr = (c.transform.position - handTransform.position).sqrMagnitude;
            if (sqr < minSqr)
            {
                minSqr  = sqr;
                nearest = c.gameObject;
            }
        }

        return nearest;
    }

    /* -------- Gizmos opcionales -------- */
    void OnDrawGizmosSelected()
    {
        if (handTransform == null) return;
        Gizmos.color = Color.cyan;
        Gizmos.DrawWireSphere(handTransform.position, grabRadius);
    }
}


using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using Oculus.Voice;

// public class VirtualGrabber : MonoBehaviour
// // ÚNICO CUBO
// {
//     public Transform handTransform;
//     public GameObject targetObject;
//     public Vector3 grabOffset = Vector3.zero;

//     private Rigidbody targetRb;
//     private bool isHolding = false;

//     private bool originalUseGravity;
//     private bool originalIsKinematic;

//     void Awake()
//     {
//         if (targetObject != null)
//         {
//             targetRb = targetObject.GetComponent<Rigidbody>();
//             if (targetRb == null)
//                 Debug.LogError("[VirtualGrabber] El objeto necesita un Rigidbody.");
//         }
//     }

//     /* ---------- AGARRAR ---------- */
//     public void GrabObject()
//     {
//         if (isHolding || targetRb == null) return;
//
//         originalUseGravity   = targetRb.useGravity;
//         originalIsKinematic  = targetRb.isKinematic;
//
//         targetRb.linearVelocity        = Vector3.zero;
//         targetRb.angularVelocity = Vector3.zero;
//         targetRb.isKinematic     = true;
//         targetRb.useGravity      = false;
//
//         targetObject.transform.SetParent(handTransform, worldPositionStays: false);
//         targetObject.transform.localPosition = grabOffset;
//         targetObject.transform.localRotation = Quaternion.identity;

//         isHolding = true;
//         Debug.Log("Grabbed.");
//     }

//     /* ---------- SOLTAR ---------- */
//     public void ReleaseObject()
//     {
//         if (!isHolding || targetRb == null) return;
//
//         targetObject.transform.SetParent(null, true);
//
//         targetRb.isKinematic = originalIsKinematic;
//         targetRb.useGravity  = originalUseGravity;
//
//         targetRb.linearVelocity        = Vector3.zero;
//         targetRb.angularVelocity = Vector3.zero;

//         isHolding = false;
//         Debug.Log("Released.");
//     }
// }


public class VirtualGrabber : MonoBehaviour
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
        if (isHolding)
            return;

        // Encuentra el objeto más cercano con tag "grabbable" dentro del radio
        GameObject nearest = FindNearestGrabbable();
        if (nearest == null)
        {
            Debug.Log("[VirtualGrabber] No hay objetos 'grabbable' cercanos.");
            return;
        }

        // Prepara referencias
        targetObject = nearest;
        targetRb = targetObject.GetComponent<Rigidbody>();
        originalUseGravity = targetRb.useGravity;
        originalIsKinematic = targetRb.isKinematic;

        // Prepara el objeto para seguir la mano
        targetRb.isKinematic = true;
        targetRb.useGravity = false;

        //Parentamos a la mano
        targetObject.transform.SetParent(handTransform, worldPositionStays: false);
        targetObject.transform.localPosition = grabOffset;
        targetObject.transform.localRotation = Quaternion.identity;

        isHolding = true;
        Debug.Log("Grabbed." + targetObject.name);
    }

    /* ---------- RELEASE ---------- */
    public void ReleaseObject()
    {
        if (!isHolding || targetRb == null) return;

        // Desparentar manteniendo posición global
        targetObject.transform.SetParent(null, true);

        // Restaurar flags
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
                minSqr = sqr;
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





//DOS APP VOICE EXPERIENCE
// {
//     public Transform handTransform;
//     public GameObject targetObject;
//     public Vector3 grabOffset = Vector3.zero;
//     public ActivationWord controller;          // arrástralo en el Inspector

//     private Rigidbody targetRb;
//     private bool isHolding = false;

//     // ← guardamos flags originales
//     private bool originalUseGravity;
//     private bool originalIsKinematic;

//     void Awake()
//     {
//         // if (controller == null)
//         //     controller = FindObjectOfType<ActivationWord>();

//         if (targetObject != null)
//         {
//             targetRb = targetObject.GetComponent<Rigidbody>();
//             if (targetRb == null)
//                 Debug.LogError("[VirtualGrabber] El objeto necesita un Rigidbody.");
//         }
//     }

//     /* ---------- AGARRAR ---------- */
//     public void GrabObject()
//     {
//         if (isHolding || targetRb == null) return;

//         /* 1) Guarda el estado original (sin gravedad + kinematic) */
//         originalUseGravity   = targetRb.useGravity;
//         originalIsKinematic  = targetRb.isKinematic;

//         /* 2) Prepara el objeto para seguir la mano */
//         targetRb.linearVelocity        = Vector3.zero;
//         targetRb.angularVelocity = Vector3.zero;
//         targetRb.isKinematic     = true;
//         targetRb.useGravity      = false;

//         /* 3) Lo parentamos a la mano */
//         targetObject.transform.SetParent(handTransform, worldPositionStays: false);
//         targetObject.transform.localPosition = grabOffset;
//         targetObject.transform.localRotation = Quaternion.identity;

//         isHolding = true;
//         Debug.Log("Grabbed.");
//         controller.OnActionExecuted();
//     }

//     /* ---------- SOLTAR ---------- */
//     public void ReleaseObject()
//     {
//         if (!isHolding || targetRb == null) return;

//         /* 1) Desparentar manteniendo posición global */
//         targetObject.transform.SetParent(null, true);

//         /* 2) Restaurar flags originales */
//         targetRb.isKinematic = originalIsKinematic;
//         targetRb.useGravity  = originalUseGravity;

//         /* 3) Opcional: reiniciar velocidades para que no “salga volando” */
//         targetRb.linearVelocity        = Vector3.zero;
//         targetRb.angularVelocity = Vector3.zero;

//         isHolding = false;
//         Debug.Log("Released.");
//         controller.OnActionExecuted();
//     }
// }
using System.Collections;
using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;
using UnityEngine.XR.Interaction.Toolkit.Interactables;
using UnityEngine.XR.Interaction.Toolkit.Interactors;

public class KunaiThrow : MonoBehaviour
{
    [Header("Lancer")]
    [SerializeField] public float throwVelocity = 20f;       // Vitesse du lancer
    [SerializeField] public float rotationVelocity = 10f;    // Vitesse de rotation (axe Y)
    [SerializeField] public float gravityScale = 0f;         // Gravité (0 = droit)
    [SerializeField] public float throwThreshold = 1.5f;     // Seuil de vélocité pour déclencher le lancer

    [Header("Retour au Socket")]
    [SerializeField] public float returnDelay = 3f;          // Secondes avant que le kunai revienne
    [SerializeField] public float returnSpeed = 5f;          // Vitesse de déplacement vers le socket
    [SerializeField] public XRSocketInteractor homeSocket;   // Le socket qui possède le kunai

    private XRGrabInteractable grabInteractable;
    private Rigidbody rb;
    private Vector3 lastHandPosition;
    private Vector3 lastHandVelocity;
    private Transform handTransform;
    private bool hasBeenThrown = false;
    private Coroutine returnCoroutine;
    private bool isReturning = false;

    void Start()
    {
        grabInteractable = GetComponent<XRGrabInteractable>();
        rb = GetComponent<Rigidbody>();

        if (grabInteractable != null)
        {
            grabInteractable.selectEntered.AddListener(OnGrab);
            grabInteractable.selectExited.AddListener(OnRelease);
        }
    }

    void OnGrab(SelectEnterEventArgs args)
    {
        // Annuler le retour si le joueur attrape le kunai
        if (returnCoroutine != null)
        {
            StopCoroutine(returnCoroutine);
            returnCoroutine = null;
        }
        isReturning = false;

        handTransform = args.interactorObject.transform;
        lastHandPosition = handTransform.position;
        lastHandVelocity = Vector3.zero;
        hasBeenThrown = false;
        grabInteractable.throwOnDetach = true;
    }

    void Update()
    {
        if (handTransform != null && grabInteractable != null && grabInteractable.isSelected)
        {
            Vector3 currentPosition = handTransform.position;
            lastHandVelocity = (currentPosition - lastHandPosition) / Time.deltaTime;
            lastHandPosition = currentPosition;
        }

        // Déplacement du kunai vers le socket
        if (isReturning && homeSocket != null)
        {
            Transform target = homeSocket.attachTransform != null ? homeSocket.attachTransform : homeSocket.transform;

            transform.position = Vector3.MoveTowards(transform.position, target.position, returnSpeed * Time.deltaTime);
            transform.rotation = Quaternion.RotateTowards(transform.rotation, target.rotation, returnSpeed * 180f * Time.deltaTime);

            // Snap au socket une fois assez proche
            if (Vector3.Distance(transform.position, target.position) < 0.05f)
            {
                isReturning = false;
                rb.linearVelocity = Vector3.zero;
                rb.angularVelocity = Vector3.zero;
                rb.isKinematic = true;

                // Forcer le socket à prendre le kunai
                homeSocket.StartManualInteraction(grabInteractable as IXRSelectInteractable);
                rb.isKinematic = false;
            }
        }
    }

    void OnRelease(SelectExitEventArgs args)
    {
        if (rb == null || hasBeenThrown) return;

        bool isThrow = lastHandVelocity.magnitude >= throwThreshold;

        if (isThrow)
        {
            hasBeenThrown = true;
            grabInteractable.throwOnDetach = false;

            Vector3 throwDirection = lastHandVelocity.normalized;
            transform.rotation = Quaternion.FromToRotation(transform.right, throwDirection) * transform.rotation;

            rb.useGravity = gravityScale > 0f;
            rb.linearVelocity = throwDirection * throwVelocity;
            rb.angularVelocity = Vector3.zero;
        }

        // Dans les deux cas (lancer ou simple relâchement), démarrer le timer de retour
        if (homeSocket != null)
        {
            if (returnCoroutine != null) StopCoroutine(returnCoroutine);
            returnCoroutine = StartCoroutine(ReturnAfterDelay());
        }
    }

    private IEnumerator ReturnAfterDelay()
    {
        yield return new WaitForSeconds(returnDelay);

        // Ne pas revenir si le kunai est à nouveau tenu
        if (grabInteractable != null && grabInteractable.isSelected)
        {
            returnCoroutine = null;
            yield break;
        }

        // Couper la physique pour un déplacement contrôlé
        rb.linearVelocity = Vector3.zero;
        rb.angularVelocity = Vector3.zero;
        rb.useGravity = false;
        isReturning = true;
        returnCoroutine = null;
    }

    void OnDestroy()
    {
        if (grabInteractable != null)
        {
            grabInteractable.selectEntered.RemoveListener(OnGrab);
            grabInteractable.selectExited.RemoveListener(OnRelease);
        }
    }
}
using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;

public class KunaiThrow : MonoBehaviour
{
    [SerializeField] public float throwVelocity = 20f; // Vitesse du lancer
    [SerializeField] public float rotationVelocity = 10f; // Vitesse de rotation (axe Y)
    [SerializeField] public float gravityScale = 0f; // Gravité (0 pour lancer droit)
    [SerializeField] public float throwThreshold = 1.5f; // Seuil de vélocité pour déclencher le lancer

    private UnityEngine.XR.Interaction.Toolkit.Interactables.XRGrabInteractable grabInteractable;
    private Rigidbody rb;
    private Vector3 lastHandPosition;
    private Vector3 lastHandVelocity;
    private Transform handTransform;
    private bool hasBeenThrown = false;

    void Start()
    {
        grabInteractable = GetComponent<UnityEngine.XR.Interaction.Toolkit.Interactables.XRGrabInteractable>();
        rb = GetComponent<Rigidbody>();

        if (grabInteractable != null)
        {
            grabInteractable.selectEntered.AddListener(OnGrab);
            grabInteractable.selectExited.AddListener(OnRelease);
        }
    }

    void OnGrab(SelectEnterEventArgs args)
    {
        handTransform = args.interactorObject.transform;
        lastHandPosition = handTransform.position;
        lastHandVelocity = Vector3.zero;
        hasBeenThrown = false;
        // Comportement normal du XRGrabInteractable pendant la saisie
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
    }

    void OnRelease(SelectExitEventArgs args)
    {
        if (rb == null || hasBeenThrown) return;

        // Sous le seuil : on ne fait RIEN, le Rigidbody et XRGrabInteractable gèrent seuls
        if (lastHandVelocity.magnitude < throwThreshold)
            return;

        hasBeenThrown = true;

        // Désactiver le throw automatique du XRGrabInteractable pour que notre code prenne le contrôle
        grabInteractable.throwOnDetach = false;

        // Direction du lancer = direction réelle du mouvement de la main
        Vector3 throwDirection = lastHandVelocity.normalized;

        // Orienter la pointe (axe X local) vers la direction du lancer
        transform.rotation = Quaternion.FromToRotation(transform.right, throwDirection) * transform.rotation;

        rb.useGravity = gravityScale > 0f;
        rb.linearVelocity = throwDirection * throwVelocity;
        rb.angularVelocity = Vector3.zero;
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

<<<<<<< HEAD
=======
<<<<<<< HEAD
using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;

public class KunaiThrow : MonoBehaviour
{
    [SerializeField] public float throwVelocity = 20f; // Vitesse du lancer
    [SerializeField] public float rotationVelocity = 10f; // Vitesse de rotation (axe Y)
    [SerializeField] public float gravityScale = 0f; // Gravité (0 pour lancer droit)
    [SerializeField] public float throwThreshold = 1.5f; // Seuil de vélocité pour déclencher le lancer

    private UnityEngine.XR.Interaction.Toolkit.Interactables.XRGrabInteractable grabInteractable;
=======
>>>>>>> bd44468b5d08d2c646636f0f6d16830dad86e68d
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
<<<<<<< HEAD
=======
>>>>>>> 14df85a7309ad62b7d51107dbe314698f9d19109
>>>>>>> bd44468b5d08d2c646636f0f6d16830dad86e68d
    private Rigidbody rb;
    private Vector3 lastHandPosition;
    private Vector3 lastHandVelocity;
    private Transform handTransform;
    private bool hasBeenThrown = false;
<<<<<<< HEAD
=======
<<<<<<< HEAD

    void Start()
    {
        grabInteractable = GetComponent<UnityEngine.XR.Interaction.Toolkit.Interactables.XRGrabInteractable>();
=======
>>>>>>> bd44468b5d08d2c646636f0f6d16830dad86e68d
    private Coroutine returnCoroutine;
    private bool isReturning = false;

    void Start()
    {
        grabInteractable = GetComponent<XRGrabInteractable>();
<<<<<<< HEAD
=======
>>>>>>> 14df85a7309ad62b7d51107dbe314698f9d19109
>>>>>>> bd44468b5d08d2c646636f0f6d16830dad86e68d
        rb = GetComponent<Rigidbody>();

        if (grabInteractable != null)
        {
            grabInteractable.selectEntered.AddListener(OnGrab);
            grabInteractable.selectExited.AddListener(OnRelease);
        }
    }

    void OnGrab(SelectEnterEventArgs args)
    {
<<<<<<< HEAD
=======
<<<<<<< HEAD
=======
>>>>>>> bd44468b5d08d2c646636f0f6d16830dad86e68d
        // Annuler le retour si le joueur attrape le kunai
        if (returnCoroutine != null)
        {
            StopCoroutine(returnCoroutine);
            returnCoroutine = null;
        }
        isReturning = false;

<<<<<<< HEAD
=======
>>>>>>> 14df85a7309ad62b7d51107dbe314698f9d19109
>>>>>>> bd44468b5d08d2c646636f0f6d16830dad86e68d
        handTransform = args.interactorObject.transform;
        lastHandPosition = handTransform.position;
        lastHandVelocity = Vector3.zero;
        hasBeenThrown = false;
<<<<<<< HEAD
=======
<<<<<<< HEAD
        // Comportement normal du XRGrabInteractable pendant la saisie
=======
>>>>>>> 14df85a7309ad62b7d51107dbe314698f9d19109
>>>>>>> bd44468b5d08d2c646636f0f6d16830dad86e68d
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
<<<<<<< HEAD
=======
<<<<<<< HEAD
=======
>>>>>>> bd44468b5d08d2c646636f0f6d16830dad86e68d

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
<<<<<<< HEAD
=======
>>>>>>> 14df85a7309ad62b7d51107dbe314698f9d19109
>>>>>>> bd44468b5d08d2c646636f0f6d16830dad86e68d
    }

    void OnRelease(SelectExitEventArgs args)
    {
        if (rb == null || hasBeenThrown) return;

<<<<<<< HEAD
=======
<<<<<<< HEAD
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
=======
>>>>>>> bd44468b5d08d2c646636f0f6d16830dad86e68d
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
<<<<<<< HEAD
=======
>>>>>>> 14df85a7309ad62b7d51107dbe314698f9d19109
>>>>>>> bd44468b5d08d2c646636f0f6d16830dad86e68d
    }

    void OnDestroy()
    {
        if (grabInteractable != null)
        {
            grabInteractable.selectEntered.RemoveListener(OnGrab);
            grabInteractable.selectExited.RemoveListener(OnRelease);
        }
    }
<<<<<<< HEAD
}
=======
}
<<<<<<< HEAD
>>>>>>> 5eafe34 (fix kunai throw script)
=======
>>>>>>> 14df85a7309ad62b7d51107dbe314698f9d19109
>>>>>>> bd44468b5d08d2c646636f0f6d16830dad86e68d

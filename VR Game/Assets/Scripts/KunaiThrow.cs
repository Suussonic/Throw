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
    [SerializeField] public XRSocketInteractor homeSocket;   // Le socket qui possède le kunai (À ASSIGNER DANS L'INSPECTOR!)

    private XRGrabInteractable grabInteractable;
    private Rigidbody rb;
    private Vector3 lastHandPosition;
    private Vector3 lastHandVelocity;
    private Transform handTransform;
    private bool hasBeenThrown = false;
    private Coroutine returnCoroutine;
    private bool isReturning = false;
    private XRInteractionManager interactionManager;

    void Start()
    {
        grabInteractable = GetComponent<XRGrabInteractable>();
        rb = GetComponent<Rigidbody>();

        if (grabInteractable != null)
        {
            grabInteractable.selectEntered.AddListener(OnGrab);
            grabInteractable.selectExited.AddListener(OnRelease);
            
            // Récupérer l'Interaction Manager depuis le grabInteractable
            interactionManager = grabInteractable.interactionManager;
        }
        
        // Vérification de configuration
        if (homeSocket == null)
        {
            Debug.LogWarning($"[KunaiThrow] '{gameObject.name}': homeSocket n'est pas assigné! Le kunai ne reviendra pas automatiquement.", this);
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
            if (Vector3.Distance(transform.position, target.position) < 0.1f)
            {
                isReturning = false;
                
                // Arrêter toute physique
                rb.linearVelocity = Vector3.zero;
                rb.angularVelocity = Vector3.zero;
                rb.useGravity = false;
                
                // Positionner exactement au socket
                transform.position = target.position;
                transform.rotation = target.rotation;

                // Réattacher au socket via l'Interaction Manager
                if (interactionManager != null && homeSocket.interactablesSelected.Count == 0)
                {
                    interactionManager.SelectEnter((IXRSelectInteractor)homeSocket, (IXRSelectInteractable)grabInteractable);
                    Debug.Log($"[KunaiThrow] {gameObject.name} est revenu au socket!");
                }
                else if (homeSocket.interactablesSelected.Count > 0)
                {
                    Debug.LogWarning($"[KunaiThrow] Le socket est déjà occupé par {homeSocket.interactablesSelected[0]}");
                    // Reste en position mais ne s'attache pas
                }
            }
        }
    }

    void OnRelease(SelectExitEventArgs args)
    {
        if (rb == null) return;

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
            
            Debug.Log($"[KunaiThrow] {gameObject.name} lancé avec vélocité {lastHandVelocity.magnitude:F2} m/s");
        }
        else
        {
            // Simple relâchement
            hasBeenThrown = false;
            Debug.Log($"[KunaiThrow] {gameObject.name} relâché sans lancer (vélocité {lastHandVelocity.magnitude:F2} < {throwThreshold})");
        }

        // Dans les deux cas (lancer ou simple relâchement), démarrer le timer de retour
        if (homeSocket != null)
        {
            if (returnCoroutine != null) StopCoroutine(returnCoroutine);
            returnCoroutine = StartCoroutine(ReturnAfterDelay());
        }
        else
        {
            Debug.LogWarning($"[KunaiThrow] {gameObject.name}: Pas de homeSocket assigné, le kunai ne reviendra pas!");
        }
        
        handTransform = null;
    }

    private IEnumerator ReturnAfterDelay()
    {
        Debug.Log($"[KunaiThrow] {gameObject.name} reviendra au socket dans {returnDelay} secondes...");
        yield return new WaitForSeconds(returnDelay);

        // Ne pas revenir si le kunai est à nouveau tenu
        if (grabInteractable != null && grabInteractable.isSelected)
        {
            Debug.Log($"[KunaiThrow] {gameObject.name} est tenu par le joueur, annulation du retour.");
            returnCoroutine = null;
            yield break;
        }

        // Vérifier que le socket existe toujours
        if (homeSocket == null)
        {
            Debug.LogWarning($"[KunaiThrow] {gameObject.name}: homeSocket est null, impossible de revenir!");
            returnCoroutine = null;
            yield break;
        }

        Debug.Log($"[KunaiThrow] {gameObject.name} commence son retour au socket...");
        
        // Couper la physique pour un déplacement contrôlé
        rb.linearVelocity = Vector3.zero;
        rb.angularVelocity = Vector3.zero;
        rb.useGravity = false;
        isReturning = true;
        hasBeenThrown = false;
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
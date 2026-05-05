using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;
using UnityEngine.XR.Interaction.Toolkit.Interactables;
using UnityEngine.XR.Interaction.Toolkit.Interactors;

public class ShurikenThrow : MonoBehaviour
{
    [Header("Lancer")]
    [SerializeField] public float throwVelocity = 18f;              // Vitesse du shuriken principal
    [SerializeField] public float gravityScale = 0f;                // 0 = vol droit, >0 active la gravité
    [SerializeField] public float throwThreshold = 1.2f;            // Vélocité main minimum pour déclencher (m/s)

    [Header("Détection Arc de Cercle")]
    [Tooltip("Angle total balayé par la main requis pour valider l'arc (degrés)")]
    [SerializeField] public float arcAngleThreshold = 55f;
    [Tooltip("Fenêtre de temps analysée pour détecter l'arc (secondes)")]
    [SerializeField] public float arcTimeWindow = 0.45f;
    [Tooltip("Nombre maximum d'échantillons de vélocité conservés")]
    [SerializeField] public int arcSampleCount = 20;
    [Tooltip("Vérifie que l'arc est continu (évite les aller-retours)")]
    [SerializeField] public bool requireConsistentArc = true;
    [Tooltip("Tolérance d'inversion de direction pour requireConsistentArc (degrés)")]
    [SerializeField] public float arcConsistencyTolerance = 40f;

    [Header("Division en 3")]
    [Tooltip("Angle de dispersion des copies gauche/droite (degrés)")]
    [SerializeField] public float spreadAngle = 45f;
    [Tooltip("Multiplicateur de vitesse appliqué aux copies latérales (0–1)")]
    [Range(0.1f, 1.5f)]
    [SerializeField] public float sideVelocityMultiplier = 0.85f;
    [Tooltip("Axe de dispersion : true = plan horizontal (Y), false = plan de la caméra")]
    [SerializeField] public bool spreadOnHorizontalPlane = true;
    [Tooltip("Durée de vie des copies avant auto-destruction (secondes)")]
    [SerializeField] public float copyLifetime = 6f;
    [Tooltip("Prefab dédié pour les copies (si null, duplique ce GameObject)")]
    [SerializeField] public GameObject shurikenCopyPrefab;

    [Header("Rotation en Vol")]
    [Tooltip("Vitesse de rotation du shuriken en vol (degrés/seconde)")]
    [SerializeField] public float spinSpeed = 720f;
    [Tooltip("Axe local de rotation (en général l'axe Z ou Y selon l'orientation du modèle)")]
    [SerializeField] public Vector3 spinAxis = Vector3.forward;

    [Header("Retour au Socket")]
    [SerializeField] public float returnDelay = 3f;
    [SerializeField] public float returnSpeed = 6f;
    [Tooltip("Distance de snap au socket (mètres)")]
    [SerializeField] public float snapDistance = 0.08f;
    [SerializeField] public XRSocketInteractor homeSocket;

    private XRGrabInteractable grabInteractable;
    private Rigidbody rb;
    private Transform handTransform;
    private Vector3 lastHandPosition;
    private bool hasBeenThrown = false;
    private bool isReturning = false;
    private Coroutine returnCoroutine;
    private XRInteractionManager interactionManager;

    // Arc detection
    private readonly Queue<ArcSample> velocitySamples = new Queue<ArcSample>();

    private struct ArcSample
    {
        public Vector3 velocity;
        public float time;
    }

    void Start()
    {
        grabInteractable = GetComponent<XRGrabInteractable>();
        rb = GetComponent<Rigidbody>();

        if (grabInteractable != null)
        {
            grabInteractable.selectEntered.AddListener(OnGrab);
            grabInteractable.selectExited.AddListener(OnRelease);
            interactionManager = grabInteractable.interactionManager;
        }

        if (homeSocket == null)
            Debug.LogWarning($"[ShurikenThrow] '{gameObject.name}': homeSocket non assigné — le shuriken ne reviendra pas.", this);
    }

    void OnGrab(SelectEnterEventArgs args)
    {
        if (returnCoroutine != null) { StopCoroutine(returnCoroutine); returnCoroutine = null; }
        isReturning = false;

        handTransform = args.interactorObject.transform;
        lastHandPosition = handTransform.position;
        velocitySamples.Clear();
        hasBeenThrown = false;

        // On gère le lancer manuellement
        grabInteractable.throwOnDetach = false;
    }

    void Update()
    {
        // Échantillonnage de la vélocité de la main
        if (handTransform != null && grabInteractable != null && grabInteractable.isSelected)
        {
            Vector3 currentPos = handTransform.position;
            Vector3 vel = (currentPos - lastHandPosition) / Time.deltaTime;
            lastHandPosition = currentPos;

            velocitySamples.Enqueue(new ArcSample { velocity = vel, time = Time.time });

            // Supprimer les échantillons hors fenêtre de temps
            float cutoff = Time.time - arcTimeWindow;
            while (velocitySamples.Count > arcSampleCount ||
                   (velocitySamples.Count > 0 && velocitySamples.Peek().time < cutoff))
                velocitySamples.Dequeue();
        }

        // Retour au socket
        if (isReturning && homeSocket != null)
        {
            Transform target = homeSocket.attachTransform != null
                ? homeSocket.attachTransform
                : homeSocket.transform;

            transform.position = Vector3.MoveTowards(transform.position, target.position, returnSpeed * Time.deltaTime);
            transform.rotation = Quaternion.RotateTowards(transform.rotation, target.rotation, returnSpeed * 180f * Time.deltaTime);

            if (Vector3.Distance(transform.position, target.position) < snapDistance)
            {
                isReturning = false;
                rb.linearVelocity = Vector3.zero;
                rb.angularVelocity = Vector3.zero;
                rb.useGravity = false;
                transform.SetPositionAndRotation(target.position, target.rotation);

                if (interactionManager != null && homeSocket.interactablesSelected.Count == 0)
                {
                    interactionManager.SelectEnter((IXRSelectInteractor)homeSocket, (IXRSelectInteractable)grabInteractable);
                    Debug.Log($"[ShurikenThrow] {gameObject.name} est revenu au socket.");
                }
            }
        }

        // Rotation en vol
        if (hasBeenThrown && !isReturning)
            transform.Rotate(spinAxis, spinSpeed * Time.deltaTime, Space.Self);
    }

    void OnRelease(SelectExitEventArgs args)
    {
        if (rb == null) return;

        bool arcDetected = DetectArc(out Vector3 throwDirection);

        if (arcDetected)
        {
            hasBeenThrown = true;
            grabInteractable.throwOnDetach = false;

            // Orienter le shuriken
            if (throwDirection != Vector3.zero)
                transform.rotation = Quaternion.LookRotation(throwDirection);

            rb.useGravity = gravityScale > 0f;
            rb.linearVelocity = throwDirection * throwVelocity;
            rb.angularVelocity = Vector3.zero;

            SpawnSideCopies(throwDirection);

            Debug.Log($"[ShurikenThrow] {gameObject.name} lancé! Direction: {throwDirection} | Vitesse: {throwVelocity} m/s");
        }
        else
        {
            hasBeenThrown = false;
            Debug.Log($"[ShurikenThrow] {gameObject.name} relâché — arc non détecté.");
        }

        // Démarrer le timer de retour dans les deux cas
        if (homeSocket != null)
        {
            if (returnCoroutine != null) StopCoroutine(returnCoroutine);
            returnCoroutine = StartCoroutine(ReturnAfterDelay());
        }

        handTransform = null;
    }

    private bool DetectArc(out Vector3 throwDirection)
    {
        throwDirection = Vector3.forward;
        var samples = new List<ArcSample>(velocitySamples);

        // Filtrer les échantillons dans la fenêtre et avec assez de vitesse
        var valid = samples.FindAll(s =>
            Time.time - s.time <= arcTimeWindow && s.velocity.magnitude > throwThreshold * 0.3f);

        if (valid.Count < 3)
            return false;

        // Angle total balayé par le vecteur vélocité
        float totalAngle = 0f;
        float maxSpeed = 0f;
        bool consistencyOk = true;
        float previousSign = 0f;

        for (int i = 1; i < valid.Count; i++)
        {
            Vector3 v1 = valid[i - 1].velocity;
            Vector3 v2 = valid[i].velocity;

            if (v1.magnitude < 0.05f || v2.magnitude < 0.05f) continue;

            float angle = Vector3.Angle(v1, v2);
            totalAngle += angle;
            maxSpeed = Mathf.Max(maxSpeed, v2.magnitude);

            // Vérifier la cohérence de l'arc (pas d'aller-retour)
            if (requireConsistentArc && angle > 2f)
            {
                Vector3 cross = Vector3.Cross(v1.normalized, v2.normalized);
                float sign = Mathf.Sign(cross.y + cross.x + cross.z);
                if (previousSign != 0f && sign != previousSign && angle > arcConsistencyTolerance)
                {
                    consistencyOk = false;
                    break;
                }
                if (angle > 2f) previousSign = sign;
            }
        }

        // La direction de lancer = dernière vélocité valide
        throwDirection = valid[valid.Count - 1].velocity.normalized;

        bool isArc = totalAngle >= arcAngleThreshold && maxSpeed >= throwThreshold && consistencyOk;

        if (isArc)
            Debug.Log($"[ShurikenThrow] Arc validé — angle total: {totalAngle:F1}° | vitesse max: {maxSpeed:F2} m/s");
        else
            Debug.Log($"[ShurikenThrow] Pas d'arc — angle: {totalAngle:F1}° (requis {arcAngleThreshold}°) | vitesse: {maxSpeed:F2} m/s");

        return isArc;
    }

    private void SpawnSideCopies(Vector3 throwDirection)
    {
        // Axe de rotation pour la dispersion
        Vector3 upAxis = spreadOnHorizontalPlane ? Vector3.up : Vector3.Cross(throwDirection, Vector3.right).normalized;
        if (upAxis == Vector3.zero) upAxis = Vector3.up;

        Vector3 leftDir  = Quaternion.AngleAxis(-spreadAngle, upAxis) * throwDirection;
        Vector3 rightDir = Quaternion.AngleAxis( spreadAngle, upAxis) * throwDirection;

        SpawnCopy(leftDir);
        SpawnCopy(rightDir);
    }

    private void SpawnCopy(Vector3 direction)
    {
        GameObject copy;

        if (shurikenCopyPrefab != null)
        {
            // Utiliser le prefab dédié (recommandé)
            copy = Instantiate(shurikenCopyPrefab, transform.position, Quaternion.LookRotation(direction));
        }
        else
        {
            // Dupliquer ce GameObject et nettoyer les composants XR/logique
            copy = Instantiate(gameObject, transform.position, Quaternion.LookRotation(direction));

            if (copy.TryGetComponent<ShurikenThrow>(out var st))       Destroy(st);
            if (copy.TryGetComponent<XRGrabInteractable>(out var grab)) Destroy(grab);
        }

        // Physique de la copie
        if (!copy.TryGetComponent<Rigidbody>(out var copyRb))
            copyRb = copy.AddComponent<Rigidbody>();

        copyRb.useGravity  = gravityScale > 0f;
        copyRb.linearVelocity  = direction * throwVelocity * sideVelocityMultiplier;
        copyRb.angularVelocity = Vector3.zero;

        // Ajouter la logique de rotation + destruction automatique
        var proj = copy.AddComponent<ShurikenProjectile>();
        proj.spinAxis    = spinAxis;
        proj.spinSpeed   = spinSpeed;
        proj.lifetime    = copyLifetime;

        Debug.Log($"[ShurikenThrow] Copie lancée → {direction} ({throwVelocity * sideVelocityMultiplier:F1} m/s)");
    }

    private IEnumerator ReturnAfterDelay()
    {
        Debug.Log($"[ShurikenThrow] Retour au socket dans {returnDelay}s...");
        yield return new WaitForSeconds(returnDelay);

        if (grabInteractable != null && grabInteractable.isSelected)
        {
            returnCoroutine = null;
            yield break;
        }

        if (homeSocket == null)
        {
            returnCoroutine = null;
            yield break;
        }

        rb.linearVelocity  = Vector3.zero;
        rb.angularVelocity = Vector3.zero;
        rb.useGravity      = false;
        isReturning        = true;
        hasBeenThrown      = false;
        returnCoroutine    = null;
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

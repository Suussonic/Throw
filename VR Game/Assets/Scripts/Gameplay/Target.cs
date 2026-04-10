using UnityEngine;
using TMPro;

/// <summary>
/// Script pour les cibles qui sont détruites quand touchées par un projectile
/// et qui donnent des points au joueur
/// </summary>
public class Target : MonoBehaviour
{
    [Header("Points")]
    [Tooltip("Nombre de points gagnés quand la cible est touchée")]
    [SerializeField] private int pointsValue = 10;

    [Header("Destruction")]
    [Tooltip("Délai avant destruction de la cible (pour effets visuels/audio)")]
    [SerializeField] private float destroyDelay = 0.1f;
    
    [Tooltip("Désactiver au lieu de détruire (pour réutilisation)")]
    [SerializeField] private bool disableInsteadOfDestroy = false;

    [Header("Effets Visuels (Optionnel)")]
    [Tooltip("Particules à spawn quand la cible est touchée")]
    [SerializeField] private GameObject hitParticlesPrefab;
    
    [Tooltip("Son joué quand la cible est touchée")]
    [SerializeField] private AudioClip hitSound;
    
    [Header("Debug")]
    [SerializeField] private bool showDebugLogs = false;

    private bool isAlreadyHit = false; // Pour éviter les hits multiples
    private AudioSource audioSource;

    private void Start()
        {
            // Get ou créer un AudioSource si un son est défini
            if (hitSound != null)
            {
                audioSource = GetComponent<AudioSource>();
                if (audioSource == null)
                {
                    audioSource = gameObject.AddComponent<AudioSource>();
                    audioSource.playOnAwake = false;
                    audioSource.spatialBlend = 1.0f; // Son 3D
                }
            }
        }

        private void OnCollisionEnter(Collision collision)
        {
            // Éviter les hits multiples
            if (isAlreadyHit) return;

            // Vérifier si l'objet qui touche est un projectile
            if (IsProjectile(collision.gameObject))
            {
                OnTargetHit(collision);
            }
        }

        private void OnTriggerEnter(Collider other)
        {
            // Éviter les hits multiples
            if (isAlreadyHit) return;

            // Vérifier si l'objet qui touche est un projectile
            if (IsProjectile(other.gameObject))
            {
                OnTargetHit(other);
            }
        }

        /// <summary>
        /// Vérifie si l'objet est un projectile (kunai, shuriken, etc.)
        /// </summary>
        private bool IsProjectile(GameObject obj)
        {
            // Vérifier par tag
            if (obj.CompareTag("Projectile") || obj.CompareTag("Weapon"))
                return true;

            // Vérifier par nom (kunai, shuriken, axe, etc.)
            string objName = obj.name.ToLower();
            if (objName.Contains("kunai") || 
            objName.Contains("shuriken") || 
            objName.Contains("axe") ||
            objName.Contains("weapon"))
            return true;

        // Vérifier par Rigidbody et vitesse (objet lancé avec force)
        Rigidbody rb = obj.GetComponent<Rigidbody>();
        if (rb != null && rb.linearVelocity.magnitude > 2f) // Vitesse > 2 m/s
            return true;

        return false;
    }

    /// <summary>
    /// Appelé quand la cible est touchée par un projectile
    /// </summary>
    private void OnTargetHit(Collision collision)
    {
        OnTargetHit(collision.collider);
    }

    private void OnTargetHit(Collider collider)
    {
        isAlreadyHit = true;

        if (showDebugLogs)
            Debug.Log($"[Target] {gameObject.name} touché par {collider.gameObject.name} → +{pointsValue} points!");

        // Ajouter les points au score
        AddScore();

        // Spawner les particules
        SpawnHitEffect(collider.ClosestPoint(transform.position));

        // Jouer le son
        PlayHitSound();

        // Désactiver ou détruire la cible
        DestroyTarget();
    }

    /// <summary>
    /// Ajoute les points au ScoreManager
    /// </summary>
    private void AddScore()
    {
        // Chercher le ScoreManager dans la scène
        ScoreUI scoreUI = FindObjectOfType<ScoreUI>();
        if (scoreUI != null)
        {
            scoreUI.AddScore(pointsValue);
        }
        else
        {
            Debug.LogWarning("[Target] Aucun ScoreUI trouvé dans la scène! Le score n'est pas ajouté.");
        }
    }

    /// <summary>
    /// Spawn les effets visuels au point d'impact
    /// </summary>
    private void SpawnHitEffect(Vector3 hitPosition)
    {
        if (hitParticlesPrefab != null)
        {
            GameObject particles = Instantiate(hitParticlesPrefab, hitPosition, Quaternion.identity);
            
            // Auto-détruire les particules après 2 secondes
            Destroy(particles, 2f);
        }
    }

    /// <summary>
    /// Joue le son de hit
    /// </summary>
    private void PlayHitSound()
    {
        if (hitSound != null && audioSource != null)
        {
            audioSource.PlayOneShot(hitSound);
        }
    }

    /// <summary>
    /// Désactive ou détruit la cible
    /// </summary>
    private void DestroyTarget()
    {
        if (disableInsteadOfDestroy)
        {
            // Désactiver après le délai (pour laisser le son jouer)
            Invoke(nameof(DisableTarget), destroyDelay);
        }
        else
        {
            // Détruire après le délai
            Destroy(gameObject, destroyDelay);
        }
    }

    private void DisableTarget()
    {
        gameObject.SetActive(false);
    }

    /// <summary>
    /// Réactive la cible (si désactivée au lieu de détruite)
    /// </summary>
    public void ResetTarget()
    {
        isAlreadyHit = false;
        gameObject.SetActive(true);
    }
}

using UnityEngine;
using TMPro;

/// <summary>
/// Gère l'affichage du score dans le Canvas
/// </summary>
public class ScoreUI : MonoBehaviour
{
    [Header("Références UI")]
    [Tooltip("TextMeshPro pour afficher le score")]
    [SerializeField] private TextMeshProUGUI scoreText;

    [Header("Configuration")]
    [Tooltip("Texte à afficher avant le score (ex: 'Score: ')")]
    [SerializeField] private string scorePrefix = "Score: ";
    
    [Tooltip("Animation du texte quand le score augmente")]
    [SerializeField] private bool animateOnScoreChange = true;
    
    [Tooltip("Durée de l'animation (secondes)")]
    [SerializeField] private float animationDuration = 0.3f;

    [Header("Couleurs (Optionnel)")]
    [SerializeField] private bool useColorChange = false;
    [SerializeField] private Color normalColor = Color.white;
    [SerializeField] private Color scoreGainColor = Color.green;

    private int currentScore = 0;
    private Vector3 originalScale;
    private float animationTimer = 0f;

    private void Start()
    {
        // Auto-trouver le TextMeshProUGUI si pas assigné
        if (scoreText == null)
        {
            scoreText = GetComponentInChildren<TextMeshProUGUI>();
            
            if (scoreText == null)
            {
                Debug.LogError("[ScoreUI] Aucun TextMeshProUGUI trouvé! Assignez-le dans l'Inspector.", this);
                return;
            }
        }

        originalScale = scoreText.transform.localScale;
        UpdateScoreDisplay();
    }

    private void Update()
    {
        // Animation du texte
        if (animateOnScoreChange && animationTimer > 0f)
        {
            animationTimer -= Time.deltaTime;
            float progress = 1f - (animationTimer / animationDuration);

            // Scale animation (grossit puis revient)
            float scale = 1f + (0.2f * (1f - progress));
            scoreText.transform.localScale = originalScale * scale;

            // Color animation
            if (useColorChange)
            {
                scoreText.color = Color.Lerp(scoreGainColor, normalColor, progress);
            }

            if (animationTimer <= 0f)
            {
                scoreText.transform.localScale = originalScale;
                if (useColorChange)
                    scoreText.color = normalColor;
            }
        }
    }

    /// <summary>
    /// Ajoute des points au score
    /// </summary>
    public void AddScore(int points)
    {
        currentScore += points;
        UpdateScoreDisplay();
        
        if (animateOnScoreChange)
        {
            animationTimer = animationDuration;
        }

        Debug.Log($"[ScoreUI] Score: {currentScore} (+{points})");
    }

    /// <summary>
    /// Retire des points au score
    /// </summary>
    public void SubtractScore(int points)
    {
        currentScore -= points;
        if (currentScore < 0) currentScore = 0;
        UpdateScoreDisplay();
    }

    /// <summary>
    /// Réinitialise le score à 0
    /// </summary>
    public void ResetScore()
    {
        currentScore = 0;
        UpdateScoreDisplay();
    }

    /// <summary>
    /// Définit le score à une valeur spécifique
    /// </summary>
    public void SetScore(int score)
    {
        currentScore = score;
        UpdateScoreDisplay();
    }

    /// <summary>
    /// Retourne le score actuel
    /// </summary>
    public int GetScore()
    {
        return currentScore;
    }

    /// <summary>
    /// Met à jour l'affichage du texte
    /// </summary>
    private void UpdateScoreDisplay()
    {
        if (scoreText != null)
        {
            scoreText.text = scorePrefix + currentScore.ToString();
        }
    }
}

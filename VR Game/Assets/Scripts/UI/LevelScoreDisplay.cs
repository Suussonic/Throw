using UnityEngine;
using TMPro;
using Core;

/// <summary>
/// Affiche le meilleur score d'un niveau spécifique au format "Score: X/Y"
/// Ajouter ce component à chaque TextMeshPro qui affiche un score dans MainVR
/// </summary>
public class LevelScoreDisplay : MonoBehaviour
{
    [Header("Configuration du Niveau")]
    [Tooltip("Le niveau dont on affiche le score")]
    [SerializeField] private LevelType levelType = LevelType.PassivLevel;

    [Tooltip("Score maximum possible pour ce niveau (Y dans 'Score: X/Y')")]
    [SerializeField] private int maxScore = 200;

    [Header("Références")]
    [Tooltip("TextMeshPro à mettre à jour (auto-détecté si vide)")]
    [SerializeField] private TextMeshProUGUI scoreText;

    [Header("Format d'Affichage")]
    [Tooltip("Format du texte (utilisez {0} pour le score actuel, {1} pour le max)")]
    [SerializeField] private string displayFormat = "Score: {0}/{1}";

    [Tooltip("Texte à afficher quand aucun score n'est enregistré")]
    [SerializeField] private string noScoreText = "Score: --/{1}";

    [Header("Mise à Jour")]
    [Tooltip("Mettre à jour le score automatiquement à chaque frame")]
    [SerializeField] private bool autoUpdate = true;

    [Tooltip("Intervalle de mise à jour (secondes) si autoUpdate est true")]
    [SerializeField] private float updateInterval = 1f;

    private float updateTimer = 0f;
    private int lastDisplayedScore = -1;

    private void Start()
    {
        // Auto-trouver le TextMeshProUGUI si pas assigné
        if (scoreText == null)
        {
            scoreText = GetComponent<TextMeshProUGUI>();
            
            if (scoreText == null)
            {
                scoreText = GetComponentInChildren<TextMeshProUGUI>();
            }

            if (scoreText == null)
            {
                Debug.LogError($"[LevelScoreDisplay] Aucun TextMeshProUGUI trouvé pour {levelType}!", this);
                return;
            }
        }

        // Affichage initial
        UpdateDisplay();
    }

    private void Update()
    {
        if (!autoUpdate) return;

        updateTimer += Time.deltaTime;
        if (updateTimer >= updateInterval)
        {
            updateTimer = 0f;
            UpdateDisplay();
        }
    }

    /// <summary>
    /// Met à jour l'affichage du score
    /// </summary>
    public void UpdateDisplay()
    {
        if (scoreText == null)
            return;

        int bestScore = (LevelScoreManager.Instance != null)
            ? LevelScoreManager.Instance.GetBestScore(levelType)
            : LevelScoreManager.GetBestScoreFromPrefs(levelType);

        // Optimisation: ne mettre à jour que si le score a changé
        if (bestScore == lastDisplayedScore && lastDisplayedScore != -1)
            return;

        lastDisplayedScore = bestScore;

        // Choisir le format approprié
        string format = (bestScore > 0) ? displayFormat : noScoreText;
        
        // Mettre à jour le texte
        scoreText.text = string.Format(format, bestScore, maxScore);
    }

    /// <summary>
    /// Change le score maximum et met à jour l'affichage
    /// </summary>
    public void SetMaxScore(int newMaxScore)
    {
        maxScore = newMaxScore;
        lastDisplayedScore = -1; // Force update
        UpdateDisplay();
    }

    /// <summary>
    /// Change le niveau affiché
    /// </summary>
    public void SetLevel(LevelType newLevelType)
    {
        levelType = newLevelType;
        lastDisplayedScore = -1; // Force update
        UpdateDisplay();
    }

    // Debug: Forcer la mise à jour via l'Inspector
    [ContextMenu("Force Update Display")]
    private void ForceUpdateDisplay()
    {
        lastDisplayedScore = -1;
        UpdateDisplay();
    }

    [ContextMenu("Reset This Level Score")]
    private void ResetScore()
    {
        if (LevelScoreManager.Instance != null)
        {
            LevelScoreManager.Instance.ResetLevelScore(levelType);
            lastDisplayedScore = -1;
            UpdateDisplay();
        }
    }
}

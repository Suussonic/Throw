using UnityEngine;
using Core;

/// <summary>
/// Système singleton pour gérer la sauvegarde et le chargement des scores de chaque niveau
/// Utilise PlayerPrefs pour la persistance
/// </summary>
public class LevelScoreManager : MonoBehaviour
{
    public static LevelScoreManager Instance { get; private set; }

    private const string SCORE_KEY_PREFIX = "BestScore_";

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject);
    }

    /// <summary>
    /// Sauvegarde le score pour un niveau (seulement si c'est un nouveau meilleur score)
    /// </summary>
    public void SaveScore(LevelType levelType, int score)
    {
        int currentBest = GetBestScore(levelType);
        
        if (score > currentBest)
        {
            string key = SCORE_KEY_PREFIX + levelType.ToString();
            PlayerPrefs.SetInt(key, score);
            PlayerPrefs.Save();
            
            Debug.Log($"<color=lime>✓ Nouveau meilleur score pour {levelType}: {score} (ancien: {currentBest})</color>");
        }
        else
        {
            Debug.Log($"<color=yellow>Score {score} pour {levelType} (meilleur: {currentBest})</color>");
        }
    }

    /// <summary>
    /// Récupère le meilleur score pour un niveau
    /// </summary>
    public int GetBestScore(LevelType levelType)
    {
        string key = SCORE_KEY_PREFIX + levelType.ToString();
        return PlayerPrefs.GetInt(key, 0); // 0 par défaut si pas de score sauvegardé
    }

    /// <summary>
    /// Réinitialise le score d'un niveau
    /// </summary>
    public void ResetLevelScore(LevelType levelType)
    {
        string key = SCORE_KEY_PREFIX + levelType.ToString();
        PlayerPrefs.DeleteKey(key);
        PlayerPrefs.Save();
        Debug.Log($"Score réinitialisé pour {levelType}");
    }

    /// <summary>
    /// Réinitialise tous les scores
    /// </summary>
    public void ResetAllScores()
    {
        foreach (LevelType levelType in System.Enum.GetValues(typeof(LevelType)))
        {
            ResetLevelScore(levelType);
        }
        Debug.Log("Tous les scores réinitialisés!");
    }

    /// <summary>
    /// Affiche tous les scores dans la console (debug)
    /// </summary>
    public void PrintAllScores()
    {
        Debug.Log("=== MEILLEURS SCORES ===");
        foreach (LevelType levelType in System.Enum.GetValues(typeof(LevelType)))
        {
            if (levelType == LevelType.Main) continue; // Ignorer Main menu
            
            int score = GetBestScore(levelType);
            Debug.Log($"{levelType}: {score}");
        }
    }
}

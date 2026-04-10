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

    private static string GetScoreKey(LevelType levelType)
    {
        return SCORE_KEY_PREFIX + levelType;
    }

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
    /// Sauvegarde le score courant pour un niveau
    /// </summary>
    public void SaveScore(LevelType levelType, int score)
    {
        SaveScoreToPrefs(levelType, score);
    }

    /// <summary>
    /// Récupère le score enregistré pour un niveau
    /// </summary>
    public int GetBestScore(LevelType levelType)
    {
        return GetBestScoreFromPrefs(levelType);
    }

    /// <summary>
    /// Réinitialise le score d'un niveau
    /// </summary>
    public void ResetLevelScore(LevelType levelType)
    {
        string key = GetScoreKey(levelType);
        PlayerPrefs.DeleteKey(key);
        PlayerPrefs.Save();
        Debug.Log($"Score réinitialisé pour {levelType}");
    }

    public static void SaveScoreToPrefs(LevelType levelType, int score)
    {
        string key = GetScoreKey(levelType);
        int previousScore = PlayerPrefs.GetInt(key, 0);

        PlayerPrefs.SetInt(key, score);
        PlayerPrefs.Save();

        Debug.Log($"<color=lime>✓ Score sauvegardé pour {levelType}: {score} (ancien: {previousScore})</color>");
    }

    public static int GetBestScoreFromPrefs(LevelType levelType)
    {
        string key = GetScoreKey(levelType);
        return PlayerPrefs.GetInt(key, 0); // 0 par défaut si pas de score sauvegardé
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

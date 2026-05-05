using UnityEngine;
using Unity.Entities;
using Core;
using ECS.Components;
using ECS.Components.Balloon;
using System.Collections;

/// <summary>
/// Vérifie si tous les ballons ont été détruits, sauvegarde le score final
/// et retourne automatiquement à MainVR
/// </summary>
public class LevelCompletionChecker : MonoBehaviour
{
    [Header("Configuration")]
    [Tooltip("Temps d'attente après la destruction de tous les ballons avant de retourner au menu")]
    [SerializeField] private float delayBeforeReturn = 3f;
    
    [Tooltip("Intervalle de vérification (en secondes)")]
    [SerializeField] private float checkInterval = 0.5f;

    [Tooltip("Attendre que les ballons commencent à spawn avant de vérifier")]
    [SerializeField] private float initialDelay = 5f;

    [Header("Debug")]
    [SerializeField] private bool showDebugLogs = true;
    [SerializeField] private bool autoReturnEnabled = true;

    private EntityManager entityManager;
    private EntityQuery balloonRiseQuery;
    private EntityQuery balloonWalkQuery;
    private bool hasStartedChecking = false;
    private bool isReturning = false;
    private int totalBalloonsSpawned = 0;

    void Start()
    {
        StartCoroutine(StartCheckingAfterDelay());
    }

    IEnumerator StartCheckingAfterDelay()
    {
        if (showDebugLogs)
        {
            Debug.Log($"<color=yellow>LevelCompletionChecker: Attente de {initialDelay}s avant de commencer...</color>");
        }

        yield return new WaitForSeconds(initialDelay);

        var world = World.DefaultGameObjectInjectionWorld;
        if (world == null || !world.IsCreated)
        {
            Debug.LogError("LevelCompletionChecker: ECS World n'existe pas!");
            yield break;
        }

        entityManager = world.EntityManager;
        balloonRiseQuery = entityManager.CreateEntityQuery(typeof(BalloonRiseRate));
        balloonWalkQuery = entityManager.CreateEntityQuery(typeof(BalloonWalkProperties));

        hasStartedChecking = true;

        if (showDebugLogs)
        {
            Debug.Log("<color=green>✓ LevelCompletionChecker actif!</color>");
        }

        StartCoroutine(CheckBalloonCountRoutine());
    }

    IEnumerator CheckBalloonCountRoutine()
    {
        while (hasStartedChecking)
        {
            yield return new WaitForSeconds(checkInterval);

            if (isReturning || GameLoopManager.Instance == null)
                continue;

            int balloonCount = GetTotalBalloonCount();

            if (balloonCount > totalBalloonsSpawned)
                totalBalloonsSpawned = balloonCount;

            // Tous les ballons détruits ET au moins 1 ballon a été spawné
            if (balloonCount == 0 && totalBalloonsSpawned > 0)
            {
                if (showDebugLogs)
                {
                    Debug.Log($"<color=lime>✓ Niveau terminé! Retour dans {delayBeforeReturn}s...</color>");
                }

                if (autoReturnEnabled)
                {
                    StartCoroutine(ReturnToMainVRAfterDelay());
                }
                yield break;
            }
        }
    }

    int GetTotalBalloonCount()
    {
        if (balloonRiseQuery == null || balloonWalkQuery == null)
            return 0;

        return balloonRiseQuery.CalculateEntityCount() + balloonWalkQuery.CalculateEntityCount();
    }

    IEnumerator ReturnToMainVRAfterDelay()
    {
        isReturning = true;

        // IMPORTANT: Sauvegarder le score AVANT de quitter le niveau
        SaveFinalScore();

        yield return new WaitForSeconds(delayBeforeReturn);

        if (showDebugLogs)
        {
            Debug.Log("<color=yellow>→ Retour à MainVR...</color>");
        }

        if (GameLoopManager.Instance != null)
        {
            GameLoopManager.Instance.ReturnToMenu();
        }
        else
        {
            Debug.LogError("GameLoopManager.Instance est null!");
            UnityEngine.SceneManagement.SceneManager.LoadScene(0);
        }
    }

    /// <summary>
    /// Récupère et sauvegarde le score final du niveau
    /// </summary>
    private void SaveFinalScore()
    {
        // Trouver le ScoreUI dans la scène
        ScoreUI scoreUI = FindObjectOfType<ScoreUI>();
        
        if (scoreUI == null)
        {
            Debug.LogWarning("LevelCompletionChecker: Aucun ScoreUI trouvé dans la scène. Score non sauvegardé.");
            return;
        }

        int finalScore = scoreUI.GetScore();

        // Récupérer le niveau actuel depuis GameLoopManager
        if (GameLoopManager.Instance == null)
        {
            Debug.LogError("GameLoopManager.Instance est null! Impossible de sauvegarder le score.");
            return;
        }

        // Utiliser la source officielle du niveau courant
        LevelType currentLevel = GameLoopManager.Instance.GetCurrentLevel();

        if (currentLevel == LevelType.Main)
        {
            currentLevel = GuessLevelFromSceneName();
        }

        // Sauvegarder via LevelScoreManager
        if (LevelScoreManager.Instance != null)
        {
            LevelScoreManager.Instance.SaveScore(currentLevel, finalScore);
            
            if (showDebugLogs)
            {
                Debug.Log($"<color=cyan>Score final sauvegardé: {finalScore} pour {currentLevel}</color>");
            }
        }
        else
        {
            LevelScoreManager.SaveScoreToPrefs(currentLevel, finalScore);

            if (showDebugLogs)
            {
                Debug.Log($"<color=cyan>Score final sauvegardé (fallback): {finalScore} pour {currentLevel}</color>");
            }
        }
    }

    /// <summary>
    /// Fallback: détermine le niveau depuis le nom de la scène active
    /// </summary>
    private LevelType GuessLevelFromSceneName()
    {
        string sceneName = UnityEngine.SceneManagement.SceneManager.GetActiveScene().name;
        
        if (sceneName.Contains("Agressive"))
            return LevelType.AgressiveLevel;
        else if (sceneName.Contains("Static") || sceneName.Contains("Passiv"))
            return LevelType.PassivLevel;
        else if (sceneName.Contains("Test"))
            return LevelType.LevelTest;
        
        Debug.LogWarning($"Impossible de déterminer le niveau actuel depuis la scène '{sceneName}'");
        return LevelType.LevelTest; // Valeur par défaut
    }

    void OnDestroy()
    {
        hasStartedChecking = false;

        if (balloonRiseQuery != null && entityManager.IsQueryValid(balloonRiseQuery))
            balloonRiseQuery.Dispose();
        
        if (balloonWalkQuery != null && entityManager.IsQueryValid(balloonWalkQuery))
            balloonWalkQuery.Dispose();
    }

    void OnGUI()
    {
        if (!showDebugLogs || !hasStartedChecking)
            return;

        int balloonCount = GetTotalBalloonCount();
        
        GUIStyle style = new GUIStyle();
        style.fontSize = 24;
        style.normal.textColor = Color.white;
        style.alignment = TextAnchor.UpperLeft;

        GUI.Label(new Rect(10, 10, 400, 30), $"Ballons: {balloonCount}", style);
        GUI.Label(new Rect(10, 40, 400, 30), $"Total: {totalBalloonsSpawned}", style);
    }

    public void ForceReturnToMainVR()
    {
        if (!isReturning)
        {
            Debug.Log("ForceReturnToMainVR appelé!");
            StartCoroutine(ReturnToMainVRAfterDelay());
        }
    }
}

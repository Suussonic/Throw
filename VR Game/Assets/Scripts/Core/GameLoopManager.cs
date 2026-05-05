using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.InputSystem;
using System.Collections; // Nécessaire pour les Coroutines

namespace Core
{
    public class GameLoopManager : MonoBehaviour
    {
        public static GameLoopManager Instance { get; private set; }
        [SerializeField] private GameStateSO gameState;
        [Header("Level Settings")]
        [SerializeField] private float passivLevelDuration = 60f;
        private Coroutine _passivLevelTimerCoroutine;
        public float PassivLevelRemainingTime { get; private set; }
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
        
        private void Start()
        {
            gameState.ChangeState(GameState.MainMenu);
        }
        
        private void ResetECSWorld()
        {
            // Force le cleanup des entités ECS
            var world = Unity.Entities.World.DefaultGameObjectInjectionWorld;
            if (world != null && world.IsCreated)
            {
                world.Dispose();
                // On recrée un monde par défaut propre
                Unity.Entities.DefaultWorldInitialization.Initialize("Default World");
            }
        }

        public void StartMain()
        {
                StartCoroutine(LoadLevelRoutine(0, LevelType.Main));
            }
        public void StartPassivLevel()
        {
            //TODO ajouter un timer et quand timer fini retour au menu
            StartCoroutine(LoadLevelRoutine(1, LevelType.PassivLevel));

            if (_passivLevelTimerCoroutine != null)
            {
                StopCoroutine(_passivLevelTimerCoroutine);
            }
            
            _passivLevelTimerCoroutine = StartCoroutine(PassivLevelTimerRoutine());
        }

        private IEnumerator PassivLevelTimerRoutine()
        {

            yield return new WaitUntil(() => gameState != null && gameState.CurrentState == GameState.Playing);

            PassivLevelRemainingTime = passivLevelDuration;

            while (PassivLevelRemainingTime > 0)
            {
                PassivLevelRemainingTime -= Time.deltaTime;
                yield return null;
            }

            if (GetCurrentLevel() == LevelType.PassivLevel)
            {
                Debug.Log("Timer terminé ! Retour au menu principal.");
                ReturnToMenu();
            }
        }
        
        public void StartAgressiveLevel()
        {
            StartCoroutine(LoadLevelRoutine(2, LevelType.AgressiveLevel));
        }

        public void StartLevelTest()
        {
            StartCoroutine(LoadLevelRoutine(3, LevelType.LevelTest));
        }

        // Coroutine générique pour charger un niveau
        private IEnumerator LoadLevelRoutine(int sceneIndex, LevelType levelType)
        {
            // Attend une frame pour laisser les systèmes ECS finir leur mise à jour
            yield return null;

            SaveCurrentSceneScoreIfAny();

            gameState.ChangeLevel(levelType);
            gameState.ChangeState(GameState.Playing);
            
            ResetECSWorld();
            
            SceneManager.LoadScene(sceneIndex);
        }

        private void Update()
        {
            HandlePauseInput();
            HandleDebugInput();
        }

        private void HandleDebugInput()
        {
            if (Keyboard.current.pKey.wasPressedThisFrame)
            {
                StartPassivLevel();
            }
            if (Keyboard.current.aKey.wasPressedThisFrame)
            {
                StartAgressiveLevel();
            }
            if (Keyboard.current.mKey.wasPressedThisFrame)
            {
                ReturnToMenu();
            }
            if (Keyboard.current.qKey.wasPressedThisFrame)
            {
                QuitGame();
            }
        }

        private void HandlePauseInput()
        {
            if (Keyboard.current.escapeKey.wasPressedThisFrame)
            {
                if (gameState.CurrentState == GameState.Playing)
                {
                    gameState.ChangeState(GameState.Paused);
                    Time.timeScale = 0f;
                }
                else if (gameState.CurrentState == GameState.Paused)
                {
                    gameState.ChangeState(GameState.Playing);
                    Time.timeScale = 1f;
                }
            }
        }

        public void ReturnToMenu()
        {
            // Pareil ici, on lance la coroutine au lieu de le faire directement
            StartCoroutine(ReturnToMenuRoutine());
        }

        private IEnumerator ReturnToMenuRoutine()
        {
            if (_passivLevelTimerCoroutine != null)
            {
                StopCoroutine(_passivLevelTimerCoroutine);
                _passivLevelTimerCoroutine = null;
            }
            
            yield return null;

            SaveCurrentSceneScoreIfAny();

            Time.timeScale = 1f;
            gameState.ChangeState(GameState.MainMenu);
            
            ResetECSWorld();
            
            SceneManager.LoadScene(0);
        }

        public void QuitGame()
        {
            Application.Quit();
        }

        public LevelType GetCurrentLevel()
        {
            if (gameState != null)
            {
                return gameState.CurrentLevel;
            }

            Debug.LogWarning("GameLoopManager: GameStateSO non assigné, niveau par défaut utilisé.");
            return LevelType.Main;
        }

        private void SaveCurrentSceneScoreIfAny()
        {
            ScoreUI scoreUI = FindObjectOfType<ScoreUI>();
            if (scoreUI == null)
                return;

            LevelType currentLevel = GetCurrentLevel();
            if (currentLevel == LevelType.Main)
            {
                currentLevel = GuessLevelFromSceneName();
            }

            if (currentLevel == LevelType.Main)
                return;

            int score = scoreUI.GetScore();

            if (LevelScoreManager.Instance != null)
            {
                LevelScoreManager.Instance.SaveScore(currentLevel, score);
            }
            else
            {
                LevelScoreManager.SaveScoreToPrefs(currentLevel, score);
            }
        }

        private LevelType GuessLevelFromSceneName()
        {
            string sceneName = SceneManager.GetActiveScene().name;

            if (sceneName.Contains("Agressive"))
                return LevelType.AgressiveLevel;
            if (sceneName.Contains("Static") || sceneName.Contains("Passiv"))
                return LevelType.PassivLevel;
            if (sceneName.Contains("Test"))
                return LevelType.LevelTest;

            return LevelType.Main;
        }
    }
}

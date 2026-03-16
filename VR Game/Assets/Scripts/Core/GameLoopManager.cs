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

        public void StartPassivLevel()
        {
            StartCoroutine(LoadLevelRoutine(1, LevelType.PassivLevel));
        }

        public void StartAgressiveLevel()
        {
            StartCoroutine(LoadLevelRoutine(2, LevelType.AgressiveLevel));
        }

        // Coroutine générique pour charger un niveau
        private IEnumerator LoadLevelRoutine(int sceneIndex, LevelType levelType)
        {
            // Attend une frame pour laisser les systèmes ECS finir leur mise à jour
            yield return null;

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
            // IMPORTANT : On attend que la frame ECS soit finie
            yield return null;

            Time.timeScale = 1f;
            gameState.ChangeState(GameState.MainMenu);
            
            ResetECSWorld();
            
            SceneManager.LoadScene(0);
        }

        public void QuitGame()
        {
            Application.Quit();
        }
    }
}

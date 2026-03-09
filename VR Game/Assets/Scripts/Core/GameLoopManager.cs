using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.InputSystem;

namespace Core
{
    public class GameLoopManager : MonoBehaviour
    {
        [SerializeField] private GameStateSO gameState;

        private void Start()
        {
            gameState.ChangeState(GameState.MainMenu);
        }

        public void StartPassivLevel()
        {
            gameState.ChangeLevel(LevelType.PassivLevel);
            gameState.ChangeState(GameState.Playing);
            SceneManager.LoadScene(1);
        }

        public void StartAgressiveLevel()
        {
            gameState.ChangeLevel(LevelType.AgressiveLevel);
            gameState.ChangeState(GameState.Playing);
            SceneManager.LoadScene(2);
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
            Time.timeScale = 1f;
            gameState.ChangeState(GameState.MainMenu);
            SceneManager.LoadScene(0);
        }

        public void QuitGame()
        {
            Application.Quit();
        }
    }
}

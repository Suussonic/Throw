﻿using Player;
using UnityEngine;

namespace Core
{
    
    public class GameManager : MonoBehaviour
    {
        [SerializeField] private GameStateSO gameState;
        
        
        private void Start()
        {
            gameState.ChangeState(GameState.MainMenu);
            
            var playerHealth = PlayerHealthRef.Instance;
            if (playerHealth != null)
                playerHealth.OnDeath += GameOver;
        }

        private void OnDisable()
        {
            var playerHealth = PlayerHealthRef.Instance;
            if (playerHealth != null)
                playerHealth.OnDeath -= GameOver;
        }

        public void PauseGame()
        {
            if (gameState.CurrentState is GameState.AgressiveLevel or GameState.PassivLevel)
            {
                gameState.ChangeState(GameState.Paused);
                Time.timeScale = 0f;
            }
        }

        public void ResumeGame()
        {
            if (gameState.CurrentState is GameState.Paused)
            {
                gameState.ChangeState(GameState.Playing);
            }
        }
        
        public void GameOver()
        {
            gameState.ChangeState(GameState.GameOver);
            Time.timeScale = 0f;
            Debug.Log("[GameManager] Game Over !");
        }
        
    }
}
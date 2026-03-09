using UnityEngine;
using System;

namespace Core
{
    public enum GameState
    {
        MainMenu,
        AgressiveLevel,
        PassivLevel,
        Paused,
        Playing,
        GameOver
    }

    [CreateAssetMenu(menuName = "Game/GameState")]
    public class GameStateSO : ScriptableObject
    {
        [SerializeField] private GameState _currentState;
        public GameState CurrentState => _currentState;

        public Action<GameState> OnStateChanged;

        public void ChangeState(GameState newState)
        {
            if (_currentState == newState) return;
            _currentState = newState;
            OnStateChanged?.Invoke(newState);
        }
    }
}
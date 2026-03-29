using UnityEngine;
using System;

namespace Core
{
    public enum GameState
    {
        Paused,
        Playing,
        GameOver,
        MainMenu
    }

    public enum LevelType
    {
        AgressiveLevel,
        PassivLevel
    }

    [CreateAssetMenu(menuName = "Game/GameState")]
    public class GameStateSO : ScriptableObject
    {
        [SerializeField] private GameState _currentState;
        [SerializeField] public LevelType _CurrentLevelType;
        public GameState CurrentState => _currentState; 
        public LevelType CurrentLevel => _CurrentLevelType;

        public Action<GameState> OnStateChanged;
        public Action<LevelType> OnLevelChanged;

        public void ChangeState(GameState newState)
        {
            if (_currentState == newState) return;
            _currentState = newState;
            OnStateChanged?.Invoke(newState);
        }

        public void ChangeLevel(LevelType newLevelType)
        {
            if (_CurrentLevelType == newLevelType) return;
            _CurrentLevelType = newLevelType;
            OnLevelChanged?.Invoke(newLevelType);
        }
    }
}
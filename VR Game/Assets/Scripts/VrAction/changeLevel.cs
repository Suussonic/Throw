using UnityEngine;
using Core;

namespace VrAction
{
    public class changeLevel : MonoBehaviour
    {
        [SerializeField] private LevelType levelToLoad; 
        public void changeLevelOnInteraction()
        {
            if (levelToLoad == LevelType.PassivLevel)
            {
                GameLoopManager.Instance.StartPassivLevel();
            }
            else if(levelToLoad == LevelType.AgressiveLevel)
            {
                GameLoopManager.Instance.StartAgressiveLevel();
            }
        }
    }
}
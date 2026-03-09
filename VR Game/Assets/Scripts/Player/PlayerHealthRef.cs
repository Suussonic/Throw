using UnityEngine;

namespace Player
{
    public class PlayerHealthRef :  MonoBehaviour
    {
        public static  PlayerHealth Instance { get; private set; }

        private void Awake()
        {
            Instance = GetComponent<PlayerHealth>();
        }

        private void OnDestroy()
        {
            if (Instance == GetComponent<PlayerHealth>())
            {
                Instance = null;
            }
        }
    }
}
using Core;
using TMPro;
using UnityEngine;

namespace UI
{
    public class TimerUI : MonoBehaviour
    {
        [Header("Références UI")]
        [Tooltip("TextMeshPro pour afficher le timer")]
        [SerializeField] private TextMeshProUGUI timerText;

        [Header("Configuration")]
        [Tooltip("Texte à afficher avant le temps (ex: 'Temps: ')")]
        [SerializeField] private string timerPrefix = "Time: ";

        [Header("Couleurs de fin (Optionnel)")]
        [SerializeField] private bool useColorChange = true;
        [SerializeField] private Color normalColor = Color.white;
        [Tooltip("Couleur quand le temps est presque écoulé")]
        [SerializeField] private Color lowTimeColor = Color.red;
        [Tooltip("Temps en secondes avant de passer à la couleur de fin")]
        [SerializeField] private float lowTimeThreshold = 10f;

        private void Start()
        {
            // Auto-trouver le TextMeshProUGUI si pas assigné (Même logique que ScoreUI)
            if (timerText == null)
            {
                timerText = GetComponentInChildren<TextMeshProUGUI>();
            
                if (timerText == null)
                {
                    Debug.LogError("[TimerUI] Aucun TextMeshProUGUI trouvé! Assignez-le dans l'Inspector.", this);
                    return;
                }
            }
        
            if (useColorChange)
                timerText.color = normalColor;
        }

        private void Update()
        {
            // On s'assure que le GameLoopManager est là et qu'on est au bon niveau
            if (GameLoopManager.Instance == null || GameLoopManager.Instance.GetCurrentLevel() != LevelType.PassivLevel)
            {
                if (timerText != null && timerText.enabled)
                    timerText.enabled = false;
                return;
            }

            // Réactiver le texte s'il était caché
            if (timerText != null && !timerText.enabled)
                timerText.enabled = true;

            UpdateTimerDisplay();
        }

        /// <summary>
        /// Met à jour l'affichage du texte (calqué sur UpdateScoreDisplay)
        /// </summary>
        private void UpdateTimerDisplay()
        {
            if (timerText != null)
            {
                float timeRemaining = GameLoopManager.Instance.PassivLevelRemainingTime;
                if (timeRemaining < 0) timeRemaining = 0;

                // Formater en minutes:secondes
                int minutes = Mathf.FloorToInt(timeRemaining / 60F);
                int seconds = Mathf.FloorToInt(timeRemaining - minutes * 60);

                timerText.text = timerPrefix + string.Format("{0:00}:{1:00}", minutes, seconds);

                // Gestion des couleurs
                if (useColorChange)
                {
                    if (timeRemaining <= lowTimeThreshold && timeRemaining > 0)
                    {
                        timerText.color = lowTimeColor;
                    }
                    else
                    {
                        timerText.color = normalColor;
                    }
                }
            }
        }
    }
}
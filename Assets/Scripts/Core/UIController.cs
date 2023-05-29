using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Core
{
    public class UIController : MonoBehaviour
    {
        [Tooltip("The nectar progress bar for the player")]
        public Slider playerNectarBar;

        [Tooltip("The nectar progress bar for the opponent")]
        public Slider opponentNectarBar;

        [Tooltip("The timer text")]
        public TextMeshProUGUI timerText;

        [Tooltip("The banner text")]
        public TextMeshProUGUI bannerText;

        [Tooltip("The button")]
        public Button button;

        [Tooltip("The button text")]
        public TextMeshProUGUI buttonText;
    
        public delegate void ButtonClick();
        public ButtonClick OnButtonClicked;
    
        public void ButtonClicked()
        {
            if (OnButtonClicked != null) 
                OnButtonClicked();
        }

        public void ShowButton(string text)
        {
            buttonText.text = text;
            button.gameObject.SetActive(true);
        }


        public void HideButton()
        {
            button.gameObject.SetActive(false);
        }
    
        public void ShowBanner(string text)
        {
            bannerText.text = text;
            bannerText.gameObject.SetActive(true);
        }
    
        public void HideBanner()
        {
            bannerText.gameObject.SetActive(false);
        }
    
        public void SetTimer(float timeRemaining)
        {
            if (timeRemaining > 0f)
                timerText.text = timeRemaining.ToString("00");
            else
                timerText.text = "";
        }
    
        public void SetPlayerNectar(float nectarAmount)
        {
            playerNectarBar.value = nectarAmount;
        }
    
        public void SetOpponentNectar(float nectarAmount)
        {
            opponentNectarBar.value = nectarAmount;
        }
    }
}

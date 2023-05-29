using System.Collections;
using Gameplay;
using UnityEngine;

namespace Core
{
    public class GameManager : MonoBehaviour
    {
        [Tooltip("Game ends when an agent collects this much nectar")]
        public float maxNectar = 8f;

        [Tooltip("Game ends after this many seconds have elapsed")]
        public float timerAmount = 60f;

        [Tooltip("The UI Controller")]
        public UIController uiController;

        [Tooltip("The player hummingbird")]
        public HummingbirdAgent player;

        [Tooltip("The ML-Agent opponent hummingbird")]
        public HummingbirdAgent opponent;

        [Tooltip("The flower area")]
        public FlowerArea flowerArea;

        [Tooltip("The main camera for the scene")]
        public Camera mainCamera;
    
        private float gameTimerStartTime;
    
        public enum GameState
        {
            Default = 0,
            MainMenu = 1,
            Preparing = 2,
            Playing = 3,
            Gameover = 4
        }
    
        public GameState State { get; private set; } = GameState.Default;

        public float TimeRemaining
        {
            get
            {
                if (State == GameState.Playing)
                {
                    float timeRemaining = timerAmount - (Time.time - gameTimerStartTime);
                    return Mathf.Max(0f, timeRemaining);
                }
                else
                {
                    return 0f;
                }
            }
        }


        public void ButtonClicked()
        {
            if (State == GameState.Gameover)
            {
                MainMenu();
            }
            else if (State == GameState.MainMenu)
            {
                StartCoroutine(StartGame());
            }
            else
            {
                Debug.LogWarning("Button clicked in unexpected state: " + State);
            }
        }
    
        private void Start()
        {
            uiController.OnButtonClicked += ButtonClicked;
            MainMenu();
        }

        private void OnDestroy()
        {
            uiController.OnButtonClicked -= ButtonClicked;
        }

        private void MainMenu()
        {
            State = GameState.MainMenu;
        
            uiController.ShowBanner("");
            uiController.ShowButton("Start");
        
            mainCamera.gameObject.SetActive(true);
            player.agentCamera.gameObject.SetActive(false);
            opponent.agentCamera.gameObject.SetActive(false);
        
            flowerArea.ResetFlowers();

            player.OnEpisodeBegin();
            opponent.OnEpisodeBegin();

            player.FreezeAgent();
            opponent.FreezeAgent();
        }
    
        private IEnumerator StartGame()
        {
            State = GameState.Preparing;

            uiController.ShowBanner("");
            uiController.HideButton();

            mainCamera.gameObject.SetActive(false);
            player.agentCamera.gameObject.SetActive(true);

            uiController.ShowBanner("3");
            yield return new WaitForSeconds(1f);
            uiController.ShowBanner("2");
            yield return new WaitForSeconds(1f);
            uiController.ShowBanner("1");
            yield return new WaitForSeconds(1f);
            uiController.ShowBanner("Go!");
            yield return new WaitForSeconds(1f);
            uiController.ShowBanner("");

            State = GameState.Playing;

            gameTimerStartTime = Time.time;

            player.UnfreezeAgent();
            opponent.UnfreezeAgent();
        }
    
        private void EndGame()
        {
            State = GameState.Gameover;

            player.FreezeAgent();
            opponent.FreezeAgent();

            if (player.NectarObtained >= opponent.NectarObtained )
            {
                uiController.ShowBanner("You win!");
            }
            else
            {
                uiController.ShowBanner("ML-Agent wins!");
            }

            uiController.ShowButton("Main Menu");
        }
    
        private void Update()
        {
            if (State == GameState.Playing)
            {
                if (TimeRemaining <= 0f ||
                    player.NectarObtained >= maxNectar ||
                    opponent.NectarObtained >= maxNectar)
                {
                    EndGame();
                }

                uiController.SetTimer(TimeRemaining);
                uiController.SetPlayerNectar(player.NectarObtained / maxNectar);
                uiController.SetOpponentNectar(opponent.NectarObtained / maxNectar);
            }
            else if (State == GameState.Preparing || State == GameState.Gameover)
            {
                uiController.SetTimer(TimeRemaining);
            }
            else
            {
                uiController.SetTimer(-1f);
                uiController.SetPlayerNectar(0f);
                uiController.SetOpponentNectar(0f);
            }
        }
    }
}

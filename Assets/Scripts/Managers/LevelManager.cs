using UnityEngine;
using UnityEngine.UI;

namespace FlowFree
{
    public class LevelManager : MonoBehaviour
    {
        [Tooltip("Camera in the scene.")]
        [SerializeField] private Camera _sceneCamera;           // Camera in the scene.

        [Tooltip("Button the player will press to undo a movement.")]
        [SerializeField] private Button _undoMovementButton;    // Button the player will press to undo a movement.
        
        [Tooltip("Button the player will press to go to the previous level.")]
        [SerializeField] private Button _previousLevelButton;   // Button the player will press to go to the previous level.
        
        [Tooltip("Button the player will press to go to the next level.")]
        [SerializeField] private Button _nextLevelButton;       // Button the player will press to go to the next level.
        
        [Tooltip("Button the player will press to get a hint on the board.")]
        [SerializeField] private Button _hintsButton;           // Button the player will press to get a hint on the board.
        
        [Tooltip("Text that shows the amount of hints the player can use.")]
        [SerializeField] private Text _hintsText;               // Text that shows the amount of hints the player can use.

        [Tooltip("Text that shows the number of the level the player is playing.")]
        [SerializeField] private Text _levelText;               // Text that shows the number of the level the player is playing.
        
        [Tooltip("Text that shows the size of the level the player is playing.")]
        [SerializeField] private Text _sizeText;                // Text that shows the size of the level the player is playing.
        
        [Tooltip("Text that shows the amount of flows the level has.")]
        [SerializeField] private Text _flowsText;               // Text that shows the amount of flows the level has.
        
        [Tooltip("Text that shows the amount of steps the player has given already.")]
        [SerializeField] private Text _stepsText;               // Text that shows the amount of steps the player has given already.
        
        [Tooltip("Text that shows the percentage of the board that is covered by flows.")]
        [SerializeField] private Text _coverageText;            // Text used to show the percentage of the flows that are covered.
        

        private int _playerMovements;                           // The number of movements the player has used to solve the level. It changes in two situations:
                                                                // When the player touches a flow (or a circle), different from the last one they touched (playerMovements++).
                                                                // When the player undoes the last movement (playerMovements--).
        
        private BoardManager _boardManager;                     // Instance of the boardManager, so that it is not necessary to call the Game Manager in every Update.
        private Map _currentMap;                                // Map that is currently being played.
        private LevelData _currentLevel;                        // Information about the level that is being currently played.

        public void CreateLevel(LevelData data)
        {
            _currentMap = new Map();

            if (_currentMap.loadMap(data.Data))
            {
                _playerMovements = 0;
                _boardManager = GameManager.Instance().GetBoardManager();
                _boardManager.CreateBoard(_currentMap);
                _levelText.text = "Level " + (data.LevelNumber + 1);
                _levelText.color = data.Color;

                _sizeText.text = _currentMap.getWidth() + "x" + _currentMap.getHeight();
                UpdateFlowsText(0);
                ResetMovements();
                UpdatePipePercentage(0);
                UpdateHintsButton();
            }
            else Debug.LogError("Nivel incorrecto");
        }
        private void Update()
        {
            // If there is no input, there is nothing to do.
            if (Input.touchCount < 1) return;

            // Gets the position of the touch, in world units.
            Touch touch = Input.GetTouch(0);
            Vector3 touchPosition = _sceneCamera.ScreenToWorldPoint(touch.position);

            // If the touch is not inside the grid boundaries, it is the same as if there was no touch.
            if (!_boardManager.InsideBoundaries(touchPosition)) return;
            
            if(touch.phase == TouchPhase.Began) _boardManager.OnTouchStarted(touchPosition);
            else if(touch.phase == TouchPhase.Moved) _boardManager.OnTouchMoved(touchPosition);
            else if (touch.phase == TouchPhase.Canceled || touch.phase == TouchPhase.Ended)
            {
                _boardManager.OnTouchFinished();
                if(_boardManager.GetStateChanged()) _undoMovementButton.interactable = true;
                
                if(_boardManager.LevelFinished()) FinishLevel();
            }
        }
        
        public void UndoMovement()
        {
            _boardManager.UndoMovement();
            _undoMovementButton.interactable = false;
        }

        public void RestartLevel()
        {
            _boardManager.CreateBoard(_currentMap);
        }
        public void NextLevel()
        {
            
        }

        public void ResetMovements()
        {
            UpdateMovements(-_playerMovements);
        }
        public void UpdateMovements(int addition)
        {
            _playerMovements += addition;
            _stepsText.text = "Steps: " + _playerMovements;
        }

        public void UpdatePipePercentage(int newPercentage)
        {
            _coverageText.text = "Pipe: " + newPercentage + "%";
        }

        public void UpdateFlowsText(int flowsCompleted)
        {
            _flowsText.text = "Flows: " + flowsCompleted + " / " + _currentMap.getFlowsNumber();
        }
        
        public void GiveHint()
        {
            // Uses the hint if it is possible.
            int hints = GameManager.Instance().GetHints();
            if (hints >= 0 && _boardManager.UseHint()) GameManager.Instance().UseHint();

            // Updates the button and the text
            UpdateHintsButton();
            
            if(_boardManager.LevelFinished()) FinishLevel();
        }

        private void FinishLevel()
        {
            GameManager.Instance().PlayIntersticialAd();
            GameManager.Instance().FinishLevel(_playerMovements);
        }

        public void GetHintsByWatchingAds()
        {
            if (GameManager.Instance().PlayRewardedAd())
            {
                Debug.Log("PISTA");
            }
            else Debug.Log("NO PISTA");
        }

        public void GoBack()
        {
            GameManager.Instance().ToLevelSelectionScene();
        }
        private void UpdateHintsButton()
        {
            int hints = GameManager.Instance().GetHints();
            _hintsText.text = hints + " x ";
            if (hints <= 0) _hintsButton.interactable = false;
        }
    }
}
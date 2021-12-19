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

        [Tooltip("Panel that appears when the player finishes a level.")]
        [SerializeField] private GameObject _victoryPanel;      // Panel that appears when the player finishes a level.

        [Tooltip("Text that will appear in the top part of the victory panel.")]
        [SerializeField] private Text _panelTitle;              // Text that will appear in the top part of the victory panel.

        [Tooltip("Text that will appear in the middle part of the victory panel.")]
        [SerializeField] private Text _panelMovementsText;      // Text that will appear in the middle part of the victory panel.

        [Tooltip("Button in the victory panel that lets the player move to the next level.")]
        [SerializeField] private Button _panelNextLevelButton;  // Button in the victory panel that lets the player move to the next level.

        [Tooltip("Button in the victory panel that lets the player move to another pack.")]
        [SerializeField] private Button _panelNextPackButton;   // Button in the victory panel that lets the player move to another pack.

        [Tooltip("Icon that will appear in the upper part of the screen if the player has solved perfectly the level in the past.")]
        [SerializeField] private GameObject _perfectSolveIcon;  // Icon that will appear in the upper part of the screen if the player has solved perfectly the level in the past.
        
        [Tooltip("Icon that will appear in the upper part of the screen if the player has solved perfectly the level in the past.")]
        [SerializeField] private GameObject _solveIcon;         // Icon that will appear in the upper part of the screen if the player has solved the level in the past.
        
        private bool _hasNextLevel;                             // Whether the player can move to the next level or not.
        private bool _hasPreviousLevel;                         // Whether there is a level before this one or not.
        
        private bool _levelFinished;                            // Whether the level has already finished or not. If it has, the player can no longer touch the board.
        private string _bestMovements;                          // The number of movements the player has used in their best solve ( "-" if the player has not solved this level before). 
        private int _playerMovements;                           // The number of movements the player has used to solve the level. It changes in two situations:
                                                                // When the player touches a flow (or a circle), different from the last one they touched (playerMovements++).
                                                                // When the player undoes the last movement (playerMovements--).
        
        private BoardManager _boardManager;                     // Instance of the boardManager, so that it is not necessary to call the Game Manager in every Update.
        private Map _currentMap;                                // Map that is currently being played.
        private LevelData _currentLevelData;                    // LevelData with information about the level that is being played.
        
        
        /// <summary>
        /// Reads the input and sends it to the board manager, if necessary.
        /// </summary>
        private void Update()
        { 
            // If there is no input (or if it is supposed to be ignored), there is nothing to do.
            if (Input.touchCount < 1 || _levelFinished) return;

            // Gets the position of the touch, in world units.
            Touch touch = Input.GetTouch(0);
            Vector3 touchPosition = _sceneCamera.ScreenToWorldPoint(touch.position);

            // If the touch is not inside the grid boundaries, it is the same as if there was no touch.
            if (!_boardManager.InsideBoundaries(touchPosition)) return;
            
            // Sends the touch to the board manager.
            if(touch.phase == TouchPhase.Began) _boardManager.OnTouchStarted(touchPosition);
            else if(touch.phase == TouchPhase.Moved) _boardManager.OnTouchMoved(touchPosition);
            else if (touch.phase == TouchPhase.Canceled || touch.phase == TouchPhase.Ended)
            {
                _boardManager.OnTouchFinished();
                
                // Checks whether there is any update needed in the level.
                if(_boardManager.GetStateChanged()) _undoMovementButton.interactable = true;
                if(_boardManager.LevelFinished()) FinishLevel();
            }
        }
        
        /// <summary>
        /// Creates the level. That includes not only the board, but every element in the UI.
        /// </summary>
        /// <param name="data"></param>
        public void CreateLevel(LevelData data)
        {
            _currentLevelData = data;
            _currentMap = new Map();

            // If the level can be loaded, it is played.
            if (_currentMap.loadMap(data.Data))
            {
                _boardManager = GameManager.Instance().GetBoardManager();
                RestartLevel();

                // Sets the UI elements information.
                _levelText.text = "Level " + (data.LevelNumber + 1);
                _levelText.color = data.Color;

                _sizeText.text = _currentMap.GetWidth() + "x" + _currentMap.GetHeight();
                
                // If there was a previous solve, activates the icons and changes the movements text.
                if (_currentLevelData.BestSolve != -1)
                {
                    _bestMovements = _currentLevelData.BestSolve.ToString();
                    if (_currentLevelData.BestSolve == _currentMap.GetFlowsNumber()) _perfectSolveIcon.SetActive(true);
                    else _solveIcon.SetActive(true);
                }
                else _bestMovements = "-";

                UpdateHintsButton();

                // Checks which buttons shall remain interactable, and which shall not.
                _hasNextLevel = GameManager.Instance().IsThereANextLevel();
                _hasPreviousLevel = GameManager.Instance().IsThereAPreviousLevel();
                _previousLevelButton.interactable = _hasPreviousLevel;
                _nextLevelButton.interactable = _hasNextLevel;
            }
            else
                Debug.LogError($"Incorrect Level: Category {data.CategoryNumber}, Pack {data.PackNumber}, Level {data.LevelNumber}. ");
        }
        
        /// <summary>
        /// Resets the information used for this level.
        /// </summary>
        public void RestartLevel()
        {
            // Updates the information.
            ResetMovements();
            UpdateFlowsText(0);
            UpdatePipePercentage(0);
            UpdateHintsButton();
            SetPanelActive(false);
            
            // Creates the board again.
            _boardManager.CreateBoard(_currentMap);
            
            _levelFinished = false;
            _perfectSolveIcon.SetActive(false);
            _solveIcon.SetActive(false);
        }
        
        /// <summary>
        /// Undoes the last movement.
        /// </summary>
        public void UndoMovement()
        {
            _boardManager.UndoMovement();
            _undoMovementButton.interactable = false;
        }

        /// <summary>
        /// Goes to the previous level.
        /// </summary>
        public void PreviousLevel()
        {
            GameManager.Instance().PreviousLevel();
        }
        
        /// <summary>
        /// Goes to the next level.
        /// </summary>
        public void NextLevel()
        {
            GameManager.Instance().NextLevel();
        }

        /// <summary>
        /// Resets the steps text.
        /// </summary>
        public void ResetMovements()
        {
            UpdateMovements(-_playerMovements);
        }
        
        /// <summary>
        /// Updates the number of movements, and the UI text.
        /// </summary>
        /// <param name="addition">The amount of steps to be added to the current ones.</param>
        public void UpdateMovements(int addition)
        {
            _playerMovements += addition;
            _stepsText.text = "Steps: " + _playerMovements + " | Best: " + _bestMovements;
        }

        /// <summary>
        /// Updates the percentage of tiles that are occupied.
        /// </summary>
        /// <param name="newPercentage"></param>
        public void UpdatePipePercentage(int newPercentage)
        {
            _coverageText.text = "Pipe: " + newPercentage + "%";
        }

        /// <summary>
        /// Updates the number of flows that are complete.
        /// </summary>
        /// <param name="flowsCompleted"> Amount of flows that are complete.</param>
        public void UpdateFlowsText(int flowsCompleted)
        {
            _flowsText.text = "Flows: " + flowsCompleted + " / " + _currentMap.GetFlowsNumber();
        }
        
        /// <summary>
        /// Gives the player a hint, if possible.
        /// </summary>
        public void GiveHint()
        {
            // Uses the hint if it is possible.
            int hints = GameManager.Instance().GetHints();
            if (hints >= 0 && _boardManager.UseHint()) GameManager.Instance().UseHint();

            // Updates the button and the text
            UpdateHintsButton();
            
            if(_boardManager.LevelFinished()) FinishLevel();
        }
        
        // TODO: make this work.
        /// <summary>
        /// Lets the player watch an ad, and gives them a hint as a result.
        /// </summary>
        public void GetHintsByWatchingAds()
        {
            if(_levelFinished) return;
            GameManager.Instance().PlayRewardedAd();
        }

        /// <summary>
        /// Updates the hints button.
        /// </summary>
        public void UpdateHintsButton()
        {
            int hints = GameManager.Instance().GetHints();
            _hintsText.text = hints + " x ";
            _hintsButton.interactable = hints > 0;
        }
        
        /// <summary>
        /// Goes to the menu scene.
        /// </summary>
        public void GoBack()
        {
            GameManager.Instance().ToMenuScene();
        }
        
        // ----- PRIVATE METHODS ----- //
        
        /// <summary>
        /// Makes the victory appear (or disappear), and updates its information.
        /// </summary>
        /// <param name="active">Whether the panel is going to be active from this moment on or not.</param>
        public void SetPanelActive(bool active)
        {
            _victoryPanel.SetActive(active);
            if (!active) return;
            
            // The text depends on whether the solve was a perfect one.
            string title;
            if (_playerMovements == _currentMap.GetFlowsNumber()) title = "Perfect!";
            else title = "Congratulations";
            _panelTitle.text = title;

            // Changes the text and activates a button, depending on the possible actions.
            _panelMovementsText.text = "You have completed the level in " + _playerMovements + " movements.";
            _panelNextLevelButton.gameObject.SetActive(_hasNextLevel);
            _panelNextPackButton.gameObject.SetActive(!_hasNextLevel);
        }
        
        /// <summary>
        /// Finishes the level.
        /// </summary>
        private void FinishLevel()
        {
            // Changes these values so that the player cannot change the board.
            _levelFinished = true;
            _undoMovementButton.interactable = false;
            _hintsButton.interactable = false;
            
            // Shows an interstitial ad and saves the level.
            GameManager.Instance().PlayIntersticialAd();
            GameManager.Instance().FinishLevel(_playerMovements, _currentMap.GetFlowsNumber());
            
            // Activates the panel.
            SetPanelActive(true);
        }
    }
}
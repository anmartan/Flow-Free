using UnityEngine;
using UnityEngine.UI;

namespace FlowFree
{
    public class LevelManager : MonoBehaviour
    {
        [SerializeField] private Camera _sceneCamera;
        [SerializeField] private BoardManager _boardManager;
        
        [SerializeField] private Button _backToMenuButton;
        
        [SerializeField] private Button _undoMovementButton;
        [SerializeField] private Button _previousLevelButton;
        [SerializeField] private Button _nextLevelButton;
        [SerializeField] private Button _hintsButton;
        [SerializeField] private Text _hintsText;

        [SerializeField] private Text _levelText;
        [SerializeField] private Text _sizeText;
        [SerializeField] private Text _flowsText;
        [SerializeField] private Text _stepsText;
        [SerializeField] private Text _coverageText;
        
        private int _playerMovements;                       // The number of movements the player has used to solve the level. It changes in two situations:
                                                            // When the player touches a flow (or a circle), different from the last one they touched (playerMovements++).
                                                            // When the player undoes the last movement (playerMovements--).

        private bool _hasNextLevel;
        private bool _hasPreviousLevel;
        
        private Map _currentMap;

        public void CreateLevel(LevelData data)
        {
            _currentMap = new Map();

            if (_currentMap.loadMap(data.level))
            {
                _playerMovements = 0;
                
                _boardManager.CreateBoard(_currentMap);
                _levelText.text = "Level " + (data.levelNumber + 1);
                _levelText.color = data.color;

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
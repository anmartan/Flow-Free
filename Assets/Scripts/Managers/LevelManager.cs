using UnityEngine;
using UnityEngine.UI;

namespace FlowFree
{
    public class LevelManager : MonoBehaviour
    {
        [SerializeField] private Camera _sceneCamera;
        [SerializeField] private BoardManager _boardManager;
        
        [SerializeField] private Button _SettingsButton;
        [SerializeField] private Button _BackToMenuButton;
        [SerializeField] private Button _AdsButton;
        
        [SerializeField] private Button _undoMovementButton;
        [SerializeField] private Button _previousLevelButton;
        [SerializeField] private Button _nextLevelButton;
        [SerializeField] private Button _hintsButton;

        [SerializeField] private Text _levelText;
        [SerializeField] private Text _sizeText;
        [SerializeField] private Text _flowsText;
        [SerializeField] private Text _stepsText;
        [SerializeField] private Text _coverageText;
        

        private bool _hasNextLevel;
        private bool _hasPreviousLevel;
        
        private Map _currentMap;

        public void CreateLevel(LevelData data)
        {
            _currentMap = new Map();

            if (_currentMap.loadMap(data.level))
            {
                _boardManager.CreateBoard(_currentMap);
                _levelText.text = "Level " + (data.levelNumber + 1);
                _levelText.color = data.color;

                _sizeText.text = _currentMap.getWidth() + "x" + _currentMap.getHeight();
                _flowsText.text = "Flows: 0/" + _currentMap.getFlowsNumber();
                _stepsText.text = "Steps: " + 0;
                _coverageText.text = "Pipe: 0%";
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
                
                if(_boardManager.LevelFinished()) Debug.Log("Aparcao");
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

        public void GiveHint()
        {
            if (GameManager.Instance().GetHints() >= 0 && _boardManager.UseHint()) GameManager.Instance().UseHint();

            if (GameManager.Instance().GetHints() <= 0) _hintsButton.interactable = false;
            
            if(_boardManager.LevelFinished()) Debug.Log("Aparcao");
        }
    }
}
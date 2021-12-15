using System;
using System.Collections;
using System.Collections.Generic;
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
        [SerializeField] private Button _retryLevelButton;
        [SerializeField] private Button _nextLevelButton;
        [SerializeField] private Button _hintsButton;


        private bool _hasNextLevel;
        private bool _hasPreviousLevel;

        public void CreateLevel()
        {
            
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
                if (_boardManager.OnTouchFinished()) _undoMovementButton.interactable = true;
                
                if(_boardManager.LevelFinished()) Debug.Log("Aparcao");
            }
        }
        
        public void UndoMovement()
        {
            _boardManager.UndoMovement();
            _undoMovementButton.interactable = false;
        }

        public void NextLevel()
        {
            
        }
        
    }
}
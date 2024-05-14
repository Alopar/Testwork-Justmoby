using System;
using UnityEngine;
using UnityEngine.UI;

namespace DefaultNamespace
{
    public class RootController : MonoBehaviour
    {
        [SerializeField] 
        private Button _createButton;
        [SerializeField] 
        private Button _interactButton;
        [SerializeField] 
        private Button _destroyButton;

        [SerializeField] 
        private MyPanelController _panelPrefab;
        
        [SerializeField] 
        private Transform _panelContainer;

        private MyPanelController _currentPanel;
        
        private void Awake()
        {
            _createButton.onClick.AddListener(OnClickCreate);
            _interactButton.onClick.AddListener(OnClickInteract);
            _destroyButton.onClick.AddListener(OnClickDestroy);
        }

        private void OnDestroy()
        {
            _createButton.onClick.RemoveListener(OnClickCreate);
            _interactButton.onClick.RemoveListener(OnClickInteract);
            _destroyButton.onClick.RemoveListener(OnClickDestroy);
        }

        private void OnClickCreate()
        {
            if (_currentPanel != null)
                Destroy(_currentPanel.gameObject);
            
            _currentPanel = Instantiate(_panelPrefab, _panelContainer);
            _currentPanel.TimelineAnimations.ChangeState("Idle");
        }

        private void OnClickInteract()
        {
            if (_currentPanel == null)
                return;
            
            _currentPanel.TimelineAnimations.ApplyEffect("Zoom");
        }
        
        
        private async void OnClickDestroy()
        {
            if (_currentPanel == null)
                return;
            
            await _currentPanel.TimelineAnimations.ChangeState("Finish");
            Destroy(_currentPanel.gameObject);
        }
    }
}

// public static class TweenExtensions
// {
//     public static TaskAwaiter<Tween> GetAwaiter(this Tween self)
//     {
//         var source = new TaskCompletionSource<Tween>();
//         self.onComplete += Complete;
//         return source.Task.GetAwaiter();
//
//         void Complete()
//         {
//             self.onComplete -= Complete;
//             source.SetResult(self);
//         }
//     }
//         
//     public static Task AsTask(this Tween tween, CancellationToken cancellationToken)
//     {
//         var taskCompletionSource = new TaskCompletionSource<bool>();
//
//         cancellationToken.Register(obj => tween.Kill(), taskCompletionSource);
//         tween.OnComplete(() => { taskCompletionSource.TrySetResult(true); });
//         tween.OnKill(() => { taskCompletionSource.TrySetCanceled(); });
//            
//         return taskCompletionSource.Task;
//     }
// }
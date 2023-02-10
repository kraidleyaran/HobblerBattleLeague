using Assets.Resources.Ancible_Tools.Scripts.System.Abilities;
using Assets.Resources.Ancible_Tools.Scripts.System.TickTimers;
using UnityEngine;
using UnityEngine.UI;

namespace Assets.Resources.Ancible_Tools.Scripts.System.UI.MInigame
{
    public class UiMinigameCastingBarController : MonoBehaviour
    {
        [SerializeField] private Image _abilityIcon = null;
        [SerializeField] private UiFillBarController _castingFillBarController;
        [SerializeField] private Color _fillColor = Color.blue;

        private TickTimer _timer = null;
        private string _actionName = string.Empty;

        void Awake()
        {
            gameObject.SetActive(false);
        }

        public void Setup(TickTimer timer, string actionName, Sprite icon)
        {
            gameObject.SetActive(true);
            if (_timer != null)
            {
                _timer.OnTickUpdate -= OnTickUpdate;
                _timer.OnTickFinished -= OnTickFinish;
            }
            _abilityIcon.sprite = icon;
            _abilityIcon.gameObject.SetActive(_abilityIcon.sprite);
            _actionName = actionName;
            timer.OnTickUpdate += OnTickUpdate;
            timer.OnTickFinished += OnTickFinish;
            timer.OnDestroyed += OnTickFinish;
            _castingFillBarController.Setup(0f, actionName, _fillColor);
        }

        public void Interrupt()
        {
            OnTickFinish();
        }

        private void OnTickUpdate(int current, int max)
        {
            var percent = (float) current / max;
            _castingFillBarController.Setup(percent, $"{_actionName}", _fillColor);
        }

        private void OnTickFinish()
        {
            _timer = null;
            _actionName = string.Empty;
            _abilityIcon.sprite = null;
            gameObject.SetActive(false);
        }

        void OnDestroy()
        {
            if (_timer != null)
            {
                _timer.OnTickFinished -= OnTickFinish;
                _timer.OnTickUpdate -= OnTickUpdate;
            }
        }
        
    }
}
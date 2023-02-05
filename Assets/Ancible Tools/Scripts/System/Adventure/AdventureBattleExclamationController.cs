using System;
using Assets.Resources.Ancible_Tools.Scripts.System.TickTimers;
using UnityEngine;
using UnityEngine.UI;

namespace Assets.Resources.Ancible_Tools.Scripts.System.Adventure
{
    public class AdventureBattleExclamationController : MonoBehaviour
    {
        [SerializeField] private SpriteRenderer _exclamationIcon;

        private Action _doAfter = null;
        private TickTimer _aliveTimer = null;

        public void Setup(int aliveTimer, Action doAfter, Color color)
        {
            _doAfter = doAfter;
            _exclamationIcon.color = color;
            _aliveTimer = new TickTimer(aliveTimer, 0, _doAfter, null, false);
        }

        void OnDestroy()
        {
            _aliveTimer?.Destroy();
            _aliveTimer = null;
        }
    }
}
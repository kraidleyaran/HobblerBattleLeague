using System.Collections.Generic;
using System.Linq;
using Assets.Resources.Ancible_Tools.Scripts.System;
using MessageBusLib;
using UnityEngine;

namespace Assets.Ancible_Tools.Scripts.System.UI.StatusBar
{
    public class UiHobblerStatusBarManager : MonoBehaviour
    {
        public static UiHobblerStatusIconController IconTemplate => _instance._statusIconTemplate;
        public static StatusIcon Happiness => _instance._happinessIcon;

        private static UiHobblerStatusBarManager _instance = null;

        [SerializeField] private Sprite _hungerIcon;
        [SerializeField] private StatusIcon[] _statusIcons = new StatusIcon[0];
        [SerializeField] private StatusIcon _happinessIcon = null;
        [SerializeField] private UiHobblerStatusBarController _statusBarTemplate = null;
        [SerializeField] private UiHobblerStatusIconController _statusIconTemplate = null;
        
        private Dictionary<GameObject, UiHobblerStatusBarController> _statusBars = new Dictionary<GameObject, UiHobblerStatusBarController>();

        void Awake()
        {
            if (_instance)
            {
                Destroy(gameObject);
                return;
            }

            _instance = this;
            SubscribeToMessages();
        }

        public static void RegisterHobbler(GameObject obj, Vector2 offset)
        {
            if (!_instance._statusBars.ContainsKey(obj))
            {
                var controller = Instantiate(_instance._statusBarTemplate, _instance.transform);
                controller.Setup(obj, offset);
                _instance._statusBars.Add(obj, controller);
            }
        }

        public static void UnregisterHobbler(GameObject obj)
        {
            if ( _instance && _instance._statusBars.TryGetValue(obj, out var controller))
            {
                _instance._statusBars.Remove(obj);
                if (controller)
                {
                    controller.Destroy();
                    Destroy(controller.gameObject);
                }
            }
        }

        public static StatusIcon GetStatusIcon(WellbeingStatType stat)
        {
            return _instance._statusIcons.FirstOrDefault(i => i.Type == stat);
        }

        public static void SetStatusBarActive(GameObject owner, bool active)
        {
            if (_instance._statusBars.TryGetValue(owner, out var bar))
            {
                bar.gameObject.SetActive(active);
            }
        }

        private void SubscribeToMessages()
        {
            gameObject.Subscribe<UpdateWorldStateMessage>(UpdatWorldState);
        }

        private void UpdatWorldState(UpdateWorldStateMessage msg)
        {
            gameObject.SetActive(msg.State == WorldState.World);
        }

        void OnDestroy()
        {
            gameObject.UnsubscribeFromAllMessages();
        }
    }
}
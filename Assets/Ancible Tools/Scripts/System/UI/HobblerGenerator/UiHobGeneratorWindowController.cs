using System.Collections.Generic;
using System.Linq;
using Assets.Resources.Ancible_Tools.Scripts.System.Items;
using Assets.Resources.Ancible_Tools.Scripts.System.TickTimers;
using Assets.Resources.Ancible_Tools.Scripts.System.Windows;
using MessageBusLib;
using UnityEngine;
using UnityEngine.UI;

namespace Assets.Resources.Ancible_Tools.Scripts.System.UI.HobblerGenerator
{
    public class UiHobGeneratorWindowController : UiBaseWindow
    {
        public const string FILTER = "UI_HOB_GENERATOR_WINDOW";

        public override bool Movable => true;
        public override bool Static => false;

        [SerializeField] private UiHobblerCardController _cardTemplate = null;
        [SerializeField] private HorizontalLayoutGroup _grid;
        [SerializeField] private Text _rerollCostText = null;
        [SerializeField] private Button _rerollButton = null;
        [SerializeField] private UiHobblerRerollTimerController _rerollTimer = null;
        
        private UiHobblerCardController[] _controllers = new UiHobblerCardController[0];
        private GameObject _owner = null;
        private int _rerollCost = 0;

        public void Setup(GameObject hobGenerator)
        {
            if (!_owner && _owner != hobGenerator)
            {
                _owner = hobGenerator;
                _owner.SubscribeWithFilter<RefreshUnitMessage>(RefreshUnit, FILTER);
                SubscribeToMessages();
                RefreshInfo();
            }
        }

        public void Reroll()
        {
            gameObject.SendMessageTo(RerollHobblersMessage.INSTANCE, _owner);
        }

        private void RefreshInfo()
        {
            var queryHobGeneratorMsg = MessageFactory.GenerateQueryHobGeneratorMsg();
            queryHobGeneratorMsg.DoAfter = UpdateHobblers;
            gameObject.SendMessageTo(queryHobGeneratorMsg, _owner);
            MessageFactory.CacheMessage(queryHobGeneratorMsg);
        }

        private void UpdateHobblers(KeyValuePair<int, HobblerTemplate>[] hobblers, TickTimer timer, int rerollCost)
        {
            for (var i = 0; i < _controllers.Length; i++)
            {
                _controllers[i].Destroy();
                Destroy(_controllers[i].gameObject);
            }
            _controllers = new UiHobblerCardController[0];

            var available = hobblers.Where(kv => kv.Value != null).ToArray();
            var controllers = new List<UiHobblerCardController>();
            for (var i = 0; i < available.Length; i++)
            {
                var controller = Instantiate(_cardTemplate, _grid.transform);
                controller.Setup(available[i].Value, available[i].Key, _owner);
                controllers.Add(controller);
            }

            _controllers = controllers.ToArray();
            _rerollCostText.text = $"{rerollCost}";
            _rerollCost = rerollCost;
            _rerollButton.interactable = WorldStashController.Gold >= _rerollCost;
            if (_rerollTimer.Timer == null)
            {
                _rerollTimer.Setup(timer);
            }
        }

        private void SubscribeToMessages()
        {
            gameObject.Subscribe<GoldUpdatedMessage>(GoldUpdated);
        }

        private void RefreshUnit(RefreshUnitMessage msg)
        {
            RefreshInfo();
        }

        private void GoldUpdated(GoldUpdatedMessage msg)
        {
            for (var i = 0; i < _controllers.Length; i++)
            {
                _controllers[i].RefreshBuyable();
            }
            _rerollButton.interactable = WorldStashController.Gold >= _rerollCost;
        }

        public override void Close()
        {
            _owner.UnsubscribeFromAllMessagesWithFilter(FILTER);
            _rerollTimer.Destroy();
            base.Close();
        }
    }
}
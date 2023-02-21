using System.Collections.Generic;
using System.Timers;
using Assets.Resources.Ancible_Tools.Scripts.System;
using Assets.Resources.Ancible_Tools.Scripts.System.Building;
using Assets.Resources.Ancible_Tools.Scripts.System.TickTimers;
using Assets.Resources.Ancible_Tools.Scripts.System.UI;
using MessageBusLib;
using UnityEngine;
using UnityEngine.UI;

namespace Assets.Ancible_Tools.Scripts.System.UI.UnitInfo.Buildings
{
    public class UiHobGateUnitInfoController : UiBuildingUnitInfoController
    {
        [SerializeField] private UiHobblerIconController _hobblerIconTemplate;
        [SerializeField] private HorizontalLayoutGroup _hobblerGrid;
        [SerializeField] private UiFillBarController _fillBarController;
        [SerializeField] private Color _timerFillColor = Color.yellow;

        private TickTimer _generationTimer = null;

        private List<UiHobblerIconController> _icons = new List<UiHobblerIconController>();

        public override void Setup(GameObject owner, WorldBuilding building)
        {
            base.Setup(owner, building);
            var queryHobGeneratorMsg = MessageFactory.GenerateQueryHobGeneratorMsg();
            queryHobGeneratorMsg.DoAfter = (hobblers, timer, goldCost) => { _generationTimer = timer; };
            gameObject.SendMessageTo(queryHobGeneratorMsg, _owner);
            MessageFactory.CacheMessage(queryHobGeneratorMsg);
            OnTimerUpdate(_generationTimer.TickCount, _generationTimer.TicksPerCycle);
            _generationTimer.OnTickUpdate += OnTimerUpdate;
        }

        protected internal override void RefreshOwner()
        {
            base.RefreshOwner();
            var queryHobGeneratorMsg = MessageFactory.GenerateQueryHobGeneratorMsg();
            queryHobGeneratorMsg.DoAfter = RefreshHobs;
            gameObject.SendMessageTo(queryHobGeneratorMsg, _owner);
            MessageFactory.CacheMessage(queryHobGeneratorMsg);
        }



        private void OnTimerUpdate(int current, int max)
        {
            var remainingTicks = max - current;
            var percent = (float) remainingTicks / max;
            _fillBarController.Setup(percent, $"{(int)(remainingTicks * TickController.TickRate)}", _timerFillColor);
        }

        private void RefreshHobs(KeyValuePair<int, HobblerTemplate>[] hobblers, TickTimer timer, int rerollCost)
        {
            foreach (var icon in _icons)
            {
                Destroy(icon.gameObject);
            }

            _icons.Clear();

            foreach (var template in hobblers)
            {
                if (template.Value)
                {
                    var icon = Instantiate(_hobblerIconTemplate, _hobblerGrid.transform);
                    icon.Setup(template.Value);
                    icon.Button.onClick.AddListener(ShowHobGeneratorWindow);
                    _icons.Add(icon);
                }
            }
        }

        private void ShowHobGeneratorWindow()
        {
            var showHobGeneratorWindowMsg = MessageFactory.GenerateShowHobGeneratorWindowMsg();
            showHobGeneratorWindowMsg.Owner = _owner;
            gameObject.SendMessage(showHobGeneratorWindowMsg);
            MessageFactory.CacheMessage(showHobGeneratorWindowMsg);
        }

        public override void Destroy()
        {
            base.Destroy();
            _generationTimer.OnTickUpdate -= OnTimerUpdate;
        }
    }
}
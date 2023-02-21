using Assets.Ancible_Tools.Scripts.System.Wellbeing;
using Assets.Resources.Ancible_Tools.Scripts.System;
using MessageBusLib;
using UnityEngine;
using UnityEngine.UI;

namespace Assets.Ancible_Tools.Scripts.System.UI.UnitInfo
{
    public class UiHobblerInfoController : MonoBehaviour
    {
        private const string UI_HOBBLER_INFO = "UI_HOBBLER_INFO";

        [SerializeField] private Text _nameText;
        [SerializeField] private UiHappinessStatController _happinessController;
        [SerializeField] private UiWellbeingStatController _hungerController;
        [SerializeField] private UiWellbeingStatController _fatigueController;
        [SerializeField] private UiWellbeingStatController _boredomController;
        [SerializeField] private UiWellbeingStatController _ignoranceController;
        [SerializeField] private UiExperienceBarController _experienceController;

        private QueryHappinessMessage _queryHappinessMsg = new QueryHappinessMessage();
        private QueryWellbeingStatsMessage _queryWellbeingStatsMsg = new QueryWellbeingStatsMessage();

        private GameObject _unit = null;

        public void Setup(GameObject unit)
        {
            _queryHappinessMsg.DoAfter = RefreshHappiness;
            _queryWellbeingStatsMsg.DoAfter = RefreshWellbeing;
            
            var change = false;
            if (!_unit || _unit != unit)
            {
                change = true;
                if (_unit)
                {
                    _unit.UnsubscribeFromAllMessagesWithFilter(UI_HOBBLER_INFO);
                }
                _unit = unit;
            }
            if (_unit && change)
            {
                _experienceController.Setup(unit);
                _unit.gameObject.SubscribeWithFilter<RefreshUnitMessage>(RefreshUnit,UI_HOBBLER_INFO);
                RefreshInfo();
            }
            else if (!_unit)
            {
                _experienceController.Clear();
                Clear();
            }
            
        }

        private void RefreshInfo()
        {
            Clear();
            gameObject.SendMessageTo(_queryHappinessMsg, _unit);
            gameObject.SendMessageTo(_queryWellbeingStatsMsg, _unit);

            var queryUnitNameMsg = MessageFactory.GenerateQueryUnitNameMsg();
            queryUnitNameMsg.DoAfter = RefreshName;
            gameObject.SendMessageTo(queryUnitNameMsg, _unit);
            MessageFactory.CacheMessage(queryUnitNameMsg);
        }

        private void RefreshName(string unitName)
        {
            _nameText.text = unitName;
        }

        private void RefreshHappiness(HappinessState state)
        {
            _happinessController.Setup(state);
        }

        private void RefreshWellbeing(WellbeingStats stats, WellbeingStats max)
        {
            _hungerController.Setup(stats.Hunger, max.Hunger);
            _fatigueController.Setup(stats.Fatigue, max.Fatigue);
            _boredomController.Setup(stats.Boredom, max.Boredom);
            _ignoranceController.Setup(stats.Ignorance, max.Ignorance);
        }

        private void RefreshUnit(RefreshUnitMessage msg)
        {
            RefreshInfo();
        }

        private void Clear()
        {
            _nameText.text = string.Empty;
            _happinessController.Clear();
            _hungerController.Clear();
            _fatigueController.Clear();
            _boredomController.Clear();
            _ignoranceController.Clear();
        }

        void OnDestroy()
        {
            if (_unit)
            {
                _unit.UnsubscribeFromAllMessagesWithFilter(UI_HOBBLER_INFO);
                Clear();
            }
            _experienceController.Destroy();
            gameObject.UnsubscribeFromAllMessages();
        }
    }
}
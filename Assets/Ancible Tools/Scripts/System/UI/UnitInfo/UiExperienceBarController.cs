using Assets.Resources.Ancible_Tools.Scripts.System;
using Assets.Resources.Ancible_Tools.Scripts.System.UI;
using MessageBusLib;
using UnityEngine;
using UnityEngine.UI;

namespace Assets.Ancible_Tools.Scripts.System.UI.UnitInfo
{
    public class UiExperienceBarController : MonoBehaviour
    {
        private const string UI_EXPERIENCE_BAR_FILTER = "UI_EXPERIENCE_BAR";

        [SerializeField] private Text _levelText;
        [SerializeField] private UiFillBarController _fillbarController;
        [SerializeField] private Color _fillColor = Color.red;
        [SerializeField] private string _preAppend = "  ";

        private GameObject _unit = null;

        public void Setup(GameObject unit)
        {
            if (_unit)
            {
                _unit.UnsubscribeFromAllMessagesWithFilter(UI_EXPERIENCE_BAR_FILTER);
                
            }
            _unit = unit;
            _unit.SubscribeWithFilter<RefreshUnitMessage>(RefreshUnit, UI_EXPERIENCE_BAR_FILTER);
            RefreshInfo();
        }

        private void RefreshExperience(int experience, int level, int requiredExperience)
        {
            var percent = (float)experience / requiredExperience;
            if (percent > 1)
            {
                percent = 1f;
            }
            _fillbarController.Setup(percent, $"{_preAppend}{experience} / {requiredExperience} XP", _fillColor);
            _levelText.text = $"Level {level + 1}";
        }

        private void RefreshInfo()
        {
            var queryUnitExperienceMsg = MessageFactory.GenerateQueryExperienceMsg();
            queryUnitExperienceMsg.DoAfter = RefreshExperience;
            gameObject.SendMessageTo(queryUnitExperienceMsg, _unit);
            MessageFactory.CacheMessage(queryUnitExperienceMsg);
        }

        private void RefreshUnit(RefreshUnitMessage msg)
        {
            if (_unit)
            {
                RefreshInfo();
            }
        }

        public void Clear()
        {
            _levelText.text = string.Empty;
            _fillbarController.Clear();
        }

        public void Destroy()
        {
            if (_unit)
            {
                _unit.UnsubscribeFromAllMessagesWithFilter(UI_EXPERIENCE_BAR_FILTER);
            }
        }

        void OnDestroy()
        {
            Destroy();
            gameObject.UnsubscribeFromAllMessages();
        }

        
    }
}
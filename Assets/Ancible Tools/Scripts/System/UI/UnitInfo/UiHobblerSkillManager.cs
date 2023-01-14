using System.Collections.Generic;
using System.Linq;
using Assets.Resources.Ancible_Tools.Scripts.System;
using Assets.Resources.Ancible_Tools.Scripts.System.Skills;
using MessageBusLib;
using UnityEngine;
using UnityEngine.UI;

namespace Assets.Ancible_Tools.Scripts.System.UI.UnitInfo
{
    public class UiHobblerSkillManager : MonoBehaviour
    {
        private const string FILTER = "UI_HOBBLER_SKILL_MANAGER";

        [SerializeField] private GameObject _skillControllerTemplate = null;
        [SerializeField] private VerticalLayoutGroup _verticalLayout = null;
        [SerializeField] private RectTransform _content = null;

        private GameObject _owner = null;        
        private UiSkillController _origin = null;
        private Dictionary<int, UiSkillController> _controllers = new Dictionary<int, UiSkillController>();

        public void Setup(GameObject owner)
        {
            _owner = owner;
            _origin = _skillControllerTemplate.GetComponentInChildren<UiSkillController>();
            SubscribeOwner(_owner);
            RefreshInfo();
        }

        public void RefreshInfo()
        {
            var querySkillsByPriorityMsg = MessageFactory.GenerateQuerySkillsByPriorityMsg();
            querySkillsByPriorityMsg.DoAfter = UpdateSkills;
            gameObject.SendMessageTo(querySkillsByPriorityMsg, _owner);
            MessageFactory.CacheMessage(querySkillsByPriorityMsg);
        }

        private void UpdateSkills(KeyValuePair<int,SkillInstance>[] skills)
        {
            var orderedSkills = skills.OrderBy(kv => kv.Key).ToArray();
            for (var i = 0; i < orderedSkills.Length; i++)
            {
                if (!_controllers.TryGetValue(orderedSkills[i].Key, out var controller))
                {
                    var obj = Instantiate(_skillControllerTemplate, _verticalLayout.transform);
                    controller = obj.GetComponentInChildren<UiSkillController>();
                    _controllers.Add(orderedSkills[i].Key, controller);
                }
                controller.Setup(orderedSkills[i].Value, _owner, skills.Length - 1);
                controller.transform.SetSiblingIndex(orderedSkills[i].Key);
            }

            var height = _controllers.Count * (_verticalLayout.spacing + _origin.ParentTransform.rect.height) + _verticalLayout.padding.top + _verticalLayout.padding.bottom;
            _content.SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, height);
        }

        private void SubscribeOwner(GameObject owner)
        {
            owner.SubscribeWithFilter<RefreshUnitMessage>(RefreshUnit, FILTER);
        }

        private void RefreshUnit(RefreshUnitMessage msg)
        {
            RefreshInfo();
        }

        public void Destroy()
        {
            _owner.UnsubscribeFromAllMessagesWithFilter(FILTER);
            var controllers = _controllers.Values.ToArray();
            for (var i = 0; i < controllers.Length; i++)
            {
                controllers[i].Destroy();
            }
            _controllers.Clear();
            _owner = null;
        }
    }
}
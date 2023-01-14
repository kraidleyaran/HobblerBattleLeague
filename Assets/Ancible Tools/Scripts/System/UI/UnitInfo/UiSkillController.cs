using Assets.Resources.Ancible_Tools.Scripts.System;
using Assets.Resources.Ancible_Tools.Scripts.System.Skills;
using Assets.Resources.Ancible_Tools.Scripts.System.UI;
using MessageBusLib;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace Assets.Ancible_Tools.Scripts.System.UI.UnitInfo
{
    public class UiSkillController : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
    {
        public RectTransform ParentTransform;
        [SerializeField] private Image _iconImage = null;
        [SerializeField] private Image _frameImage = null;
        [SerializeField] private Text _nameText = null;
        [SerializeField] private UiFillBarController _experienceFillBar = null;
        [SerializeField] private Button _upRankButton;
        [SerializeField] private Button _downRankButton;
        
        public SkillInstance Skill { get; private set; }

        private bool _hovered = false;
        private GameObject _owner = null;
        private int _maxSiblings = 0;

        public void Setup(SkillInstance instance, GameObject owner, int maxSiblings)
        {
            Skill = instance;
            _owner = owner;
            _iconImage.sprite = Skill.Instance.Icon;
            _maxSiblings = maxSiblings;
            _nameText.text = Skill.Instance.DisplayName;
            var percent = (float)Skill.Experience / Skill.Instance.CalculateExperienceForLevel(Skill.Level + 1);
            _experienceFillBar.Setup(percent, $"Level {Skill.Level + 1}", ColorFactoryController.Experience);
            var rank = transform.parent.GetSiblingIndex();
            _upRankButton.interactable = rank > 0;
            _downRankButton.interactable = rank < maxSiblings - 1;
        }

        public void UpRank()
        {
            var rank = transform.parent.GetSiblingIndex();
            if (rank > 0)
            {
                var changeSkillPriorityMsg = MessageFactory.GenerateChangeSkillPriorityMsg();
                changeSkillPriorityMsg.Origin = rank;
                changeSkillPriorityMsg.Priority = rank - 1;
                changeSkillPriorityMsg.Skill = Skill;
                gameObject.SendMessageTo(changeSkillPriorityMsg, _owner);
                MessageFactory.CacheMessage(changeSkillPriorityMsg);
            }
        }

        public void DownRank()
        {
            var rank = transform.parent.GetSiblingIndex();

            if (rank < _maxSiblings - 1)
            {
                var changeSkillPriorityMsg = MessageFactory.GenerateChangeSkillPriorityMsg();
                changeSkillPriorityMsg.Origin = rank;
                changeSkillPriorityMsg.Priority = rank + 1;
                changeSkillPriorityMsg.Skill = Skill;
                gameObject.SendMessageTo(changeSkillPriorityMsg, _owner);
                MessageFactory.CacheMessage(changeSkillPriorityMsg);
            }
        }

        public void OnPointerEnter(PointerEventData eventData)
        {
            if (!_hovered)
            {
                _frameImage.color = ColorFactoryController.HoveredItem;
                //TODO: Show additional hover info - experience and level
                _hovered = true;
                var showHoverInfoMsg = MessageFactory.GenerateShowHoverInfoMsg();
                showHoverInfoMsg.Title = Skill.Instance.DisplayName;
                showHoverInfoMsg.Description = Skill.Instance.Description;
                showHoverInfoMsg.World = false;
                showHoverInfoMsg.Icon = Skill.Instance.Icon;
                showHoverInfoMsg.Position = transform.position.ToVector2();
                showHoverInfoMsg.Owner = gameObject;
                gameObject.SendMessage(showHoverInfoMsg);
                MessageFactory.CacheMessage(showHoverInfoMsg);
            }
        }

        public void OnPointerExit(PointerEventData eventData)
        {
            if (_hovered)
            {
                _frameImage.color = Color.white;
                _hovered = false;
                var removeHoverInfoMsg = MessageFactory.GenerateRemoveHoverInfoMsg();
                removeHoverInfoMsg.Owner = gameObject;
                gameObject.SendMessage(removeHoverInfoMsg);
                MessageFactory.CacheMessage(removeHoverInfoMsg);
            }
        }

        public void Destroy()
        {
            Skill = null;
            _owner = null;
            if (_hovered)
            {
                var removeHoverInfoMsg = MessageFactory.GenerateRemoveHoverInfoMsg();
                removeHoverInfoMsg.Owner = gameObject;
                gameObject.SendMessage(removeHoverInfoMsg);
                MessageFactory.CacheMessage(removeHoverInfoMsg);
            }
        }
    }
}
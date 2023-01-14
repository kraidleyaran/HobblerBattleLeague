using Assets.Resources.Ancible_Tools.Scripts.System;
using Assets.Resources.Ancible_Tools.Scripts.System.Abilities;
using MessageBusLib;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace Assets.Ancible_Tools.Scripts.System.UI.UnitInfo
{
    public class UiAbilitySlotController : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
    {
        [SerializeField] private Image _frameImage = null;
        [SerializeField] private Image _abilityIcon = null;
        [SerializeField] private Text _abilityName = null;
        [SerializeField] private Text _manaCost = null;
        [SerializeField] private GameObject _rankButtonGroup;
        [SerializeField] private Button _upRankButton;
        [SerializeField] private Button _downRankButton;
        [SerializeField] private Button _forgetButton;

        public WorldAbility Ability { get; private set; }

        private bool _hovered = false;
        private GameObject _owner = null;
        private GameObject _parent = null;

        public void WakeUp(GameObject parent)
        {
            _parent = parent;
            _upRankButton.onClick.AddListener(UpRank);
            _downRankButton.onClick.AddListener(DownRank);
        }

        public void Setup(WorldAbility ability, GameObject owner)
        {
            Ability = ability;
            _owner = owner;
            if (Ability)
            {
                _abilityName.text = Ability.DisplayName;
                _abilityIcon.sprite = Ability.Icon;
                _abilityIcon.gameObject.SetActive(true);
                _manaCost.text = $"{Ability.ManaCost}";
                _rankButtonGroup.gameObject.SetActive(true);
                _forgetButton.gameObject.SetActive(true);
                var index = transform.parent.GetSiblingIndex();
                _upRankButton.interactable = index > 0;
                _downRankButton.interactable = index < DataController.MaxHobblerAbilities - 1;
            }
            else
            {
                _abilityName.text = "Empty";
                _abilityIcon.gameObject.SetActive(false);
                _manaCost.text = string.Empty;
                _rankButtonGroup.gameObject.SetActive(false);
                _forgetButton.gameObject.SetActive(false);
            }

            if (_hovered)
            {
                var showHoverInfoMsg = MessageFactory.GenerateShowHoverInfoMsg();
                showHoverInfoMsg.World = false;
                showHoverInfoMsg.Owner = gameObject;
                showHoverInfoMsg.Position = _abilityIcon.transform.position.ToVector2();
                if (Ability)
                {
                    showHoverInfoMsg.Icon = Ability.Icon;
                    showHoverInfoMsg.Title = Ability.DisplayName;
                    showHoverInfoMsg.Description = Ability.GetDescription();
                    showHoverInfoMsg.ColorMask = Color.white;
                }
                else
                {
                    showHoverInfoMsg.Title = "Empty";
                    showHoverInfoMsg.Description = "Drag an ability to this slot to equip it.";
                    showHoverInfoMsg.ColorMask = Color.white;
                    showHoverInfoMsg.Icon = null;
                }
                gameObject.SendMessage(showHoverInfoMsg);
                MessageFactory.CacheMessage(showHoverInfoMsg);
            }
        }

        public void OnPointerEnter(PointerEventData eventData)
        {
            _hovered = true;
            var setHoveredAbilitySlotControllerMsg = MessageFactory.GenerateSetHoveredAbilitySlotControllerMsg();
            setHoveredAbilitySlotControllerMsg.Controller = this;
            gameObject.SendMessageTo(setHoveredAbilitySlotControllerMsg, _parent);
            MessageFactory.CacheMessage(setHoveredAbilitySlotControllerMsg);

            var showHoverInfoMsg = MessageFactory.GenerateShowHoverInfoMsg();
            showHoverInfoMsg.World = false;
            showHoverInfoMsg.Owner = gameObject;
            showHoverInfoMsg.Position = _abilityIcon.transform.position.ToVector2();
            if (Ability)
            {
                showHoverInfoMsg.Icon = Ability.Icon;
                showHoverInfoMsg.Title = Ability.DisplayName;
                showHoverInfoMsg.Description = Ability.GetDescription();
                showHoverInfoMsg.ColorMask = Color.white;
            }
            else
            {
                showHoverInfoMsg.Title = "Empty";
                showHoverInfoMsg.Description = "Drag an ability to this slot to equip it.";
                showHoverInfoMsg.ColorMask = Color.white;
                showHoverInfoMsg.Icon = null;
            }
            gameObject.SendMessage(showHoverInfoMsg);
            MessageFactory.CacheMessage(showHoverInfoMsg);
            _frameImage.color = ColorFactoryController.HoveredItem;
        }

        public void OnPointerExit(PointerEventData eventData)
        {
            if (_hovered)
            {
                _hovered = false;
                var removeHoverInfoMsg = MessageFactory.GenerateRemoveHoverInfoMsg();
                removeHoverInfoMsg.Owner = gameObject;
                gameObject.SendMessage(removeHoverInfoMsg);
                MessageFactory.CacheMessage(removeHoverInfoMsg);


                var removeHoveredAbilitySlotControllerMsg = MessageFactory.GenerateRemoveHoveredAbilitySlotControllerMsg();
                removeHoveredAbilitySlotControllerMsg.Controller = this;
                gameObject.SendMessage(removeHoveredAbilitySlotControllerMsg);
                MessageFactory.CacheMessage(removeHoveredAbilitySlotControllerMsg);

                _frameImage.color = Color.white;
            }
        }

        public void UpRank()
        {
            var rank = transform.parent.GetSiblingIndex();
            if (rank > 0)
            {
                var toRank = rank - 1;
                var changeAbilitySlotMsg = MessageFactory.GenerateChangeAbilitySlotMsg();
                changeAbilitySlotMsg.CurrentSlot = rank;
                changeAbilitySlotMsg.NewSlot = toRank;
                gameObject.SendMessageTo(changeAbilitySlotMsg, _owner);
                MessageFactory.CacheMessage(changeAbilitySlotMsg);
            }
            
        }

        public void DownRank()
        {
            var rank = transform.parent.GetSiblingIndex();
            if (rank < DataController.MaxHobblerAbilities - 1)
            {
                var toRank = rank + 1;
                var changeAbilitySlotMsg = MessageFactory.GenerateChangeAbilitySlotMsg();
                changeAbilitySlotMsg.CurrentSlot = rank;
                changeAbilitySlotMsg.NewSlot = toRank;
                gameObject.SendMessageTo(changeAbilitySlotMsg, _owner);
                MessageFactory.CacheMessage(changeAbilitySlotMsg);
            }
        }

        public void Forget()
        {
            //TODO: Show confirmation box;
            var forgetAbilityAtSlotMsg = MessageFactory.GenerateForgetAbilityAtSlotMsg();
            forgetAbilityAtSlotMsg.Slot = transform.parent.GetSiblingIndex();
            gameObject.SendMessageTo(forgetAbilityAtSlotMsg, _owner);
            MessageFactory.CacheMessage(forgetAbilityAtSlotMsg);

        }
    }
}
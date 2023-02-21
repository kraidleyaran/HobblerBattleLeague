using Assets.Resources.Ancible_Tools.Scripts.System.Abilities;
using MessageBusLib;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace Assets.Resources.Ancible_Tools.Scripts.System.UI
{
    public class UiAbilityController : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
    {
        [SerializeField] private Image _abilityIconImage = null;

        public WorldAbility Ability { get; private set;  }

        private bool _hovered = false;

        public void Setup(WorldAbility ability)
        {
            Ability = ability;
            _abilityIconImage.sprite = Ability.Icon;
        }

        public void OnPointerEnter(PointerEventData eventData)
        {
            if (!_hovered)
            {
                _hovered = true;
                var showHoveredInfoMsg = MessageFactory.GenerateShowHoverInfoMsg();
                showHoveredInfoMsg.Title = Ability.DisplayName;
                showHoveredInfoMsg.Description = Ability.GetDescription();
                showHoveredInfoMsg.World = false;
                showHoveredInfoMsg.Owner = gameObject;
                showHoveredInfoMsg.Icon = Ability.Icon;
                gameObject.SendMessage(showHoveredInfoMsg);
                MessageFactory.CacheMessage(showHoveredInfoMsg);
            }
            
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
            }
        }

        void OnDisable()
        {
            if (_hovered)
            {
                _hovered = false;
                var removeHoverInfoMsg = MessageFactory.GenerateRemoveHoverInfoMsg();
                removeHoverInfoMsg.Owner = gameObject;
                gameObject.SendMessage(removeHoverInfoMsg);
                MessageFactory.CacheMessage(removeHoverInfoMsg);
            }
        }

        void OnDestroy()
        {
            if (_hovered)
            {
                _hovered = false;
                var removeHoverInfoMsg = MessageFactory.GenerateRemoveHoverInfoMsg();
                removeHoverInfoMsg.Owner = gameObject;
                gameObject.SendMessage(removeHoverInfoMsg);
                MessageFactory.CacheMessage(removeHoverInfoMsg);
            }
        }
    }
}
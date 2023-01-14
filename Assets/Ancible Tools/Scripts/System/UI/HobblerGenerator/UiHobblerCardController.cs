using System.Linq;
using Assets.Resources.Ancible_Tools.Scripts.System.Items;
using Assets.Resources.Ancible_Tools.Scripts.System.UI.Stash;
using MessageBusLib;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace Assets.Resources.Ancible_Tools.Scripts.System.UI.HobblerGenerator
{
    public class UiHobblerCardController : MonoBehaviour, IPointerEnterHandler,IPointerExitHandler
    {
        [SerializeField] private Image _hobblerIcon;
        [SerializeField] private UiHobblerWeaponController _weaponController = null;
        [SerializeField] private UiAbilityController[] _abilityControllers = new UiAbilityController[0];
        [SerializeField] private Text _costText = null;
        [SerializeField] private Button _buyButton = null;

        public GameObject Owner { get; private set; }

        private bool _hovered = false;
        private HobblerTemplate _template = null;
        private int _slot = 0;

        public void Setup(HobblerTemplate template, int slot, GameObject owner)
        {
            Owner = owner;
            _template = template;
            _slot = slot;
            _hobblerIcon.sprite = _template.Sprite.Sprite;
            if (_template.StartingWeapon)
            {
                _weaponController.Setup(template.StartingWeapon);
            }

            for (var i = 0; i < _template.StartingAbilities.Length && i < _abilityControllers.Length; i++)
            {
                var ability = _template.StartingAbilities[i];
                _abilityControllers[i].Setup(ability);
            }

            var emptyAbilities = _abilityControllers.Where(a => !a.Ability).ToArray();
            for (var i = 0; i < emptyAbilities.Length; i++)
            {
                emptyAbilities[i].gameObject.SetActive(false);
            }

            _costText.text = $"{_template.Cost}";
            RefreshBuyable();
        }

        public void Buy()
        {
            var purchaseHobblerAtSlotMsg = MessageFactory.GeneratePurchaseHobblerAtSlotMsg();
            purchaseHobblerAtSlotMsg.Slot = _slot;
            gameObject.SendMessageTo(purchaseHobblerAtSlotMsg, Owner);
            MessageFactory.CacheMessage(purchaseHobblerAtSlotMsg);
        }

        public void RefreshBuyable()
        {
            _buyButton.interactable = WorldStashController.Gold >= _template.Cost;
        }

        public void OnPointerEnter(PointerEventData eventData)
        {
            if (!_hovered)
            {
                _hovered = true;
                var showHoverInfoMsg = MessageFactory.GenerateShowHoverInfoMsg();
                showHoverInfoMsg.Title = _template.DisplayName;
                showHoverInfoMsg.Icon = _template.Sprite.Sprite;
                showHoverInfoMsg.Description = _template.GetDescription();
                showHoverInfoMsg.Position = transform.position.ToVector2();
                showHoverInfoMsg.World = false;
                showHoverInfoMsg.Owner = gameObject;
                gameObject.SendMessage(showHoverInfoMsg);
                MessageFactory.CacheMessage(showHoverInfoMsg);
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

        public void Destroy()
        {
            if (_hovered)
            {
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
                var removeHoverInfoMsg = MessageFactory.GenerateRemoveHoverInfoMsg();
                removeHoverInfoMsg.Owner = gameObject;
                gameObject.SendMessage(removeHoverInfoMsg);
                MessageFactory.CacheMessage(removeHoverInfoMsg);
            }
        }
    }
}
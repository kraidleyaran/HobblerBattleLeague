using System.Collections.Generic;
using System.Linq;
using Assets.Ancible_Tools.Scripts.System.Wellbeing;
using Assets.Ancible_Tools.Scripts.Traits;
using Assets.Resources.Ancible_Tools.Scripts.System.Abilities;
using Assets.Resources.Ancible_Tools.Scripts.System.Combat;
using Assets.Resources.Ancible_Tools.Scripts.System.Items;
using Assets.Resources.Ancible_Tools.Scripts.System.UI.DetailedInfo;
using MessageBusLib;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace Assets.Resources.Ancible_Tools.Scripts.System.UI.Roster
{
    public class UiRosterSlotController : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
    {
        private const string FILTER = "UI_ROSTER_SLOT_CONTROLLER";

        public HappinessState State => _state;
        public string Name => _name;

        public RectTransform RectTransform;
        [SerializeField] private Image _hobblerIconImage;
        [SerializeField] private UiEquippedItemController _equippedItemTemplate;
        [SerializeField] private RectTransform _equipmentGrid;
        [SerializeField] private UiAbilityController _abilityTemplate;
        [SerializeField] private RectTransform _abilityGrid;
        [SerializeField] private Sprite _benchButtonSprite;
        [SerializeField] private Color _benchButtonColor = Color.white;
        [SerializeField] private Sprite _rosterButtonSprite;
        [SerializeField] private Color _rosterButtonColor = Color.white;
        [SerializeField] private Image _buttonImage;
        [SerializeField] private Button _rosterButton = null;
        [SerializeField] private Image _happinessIconImage = null;
        [SerializeField] private Text _happinessText;
        
        private CombatStats _baseStats = CombatStats.Zero;
        private CombatStats _bonusStats = CombatStats.Zero;
        private SpriteTrait _sprite = null;
        private string _name = string.Empty;
        private RosterType _type = RosterType.Bench;
        private GameObject _hobbler = null;
        private HappinessState _state = HappinessState.Moderate;

        private UiEquippedItemController[] _equippedItems = new UiEquippedItemController[0];
        private UiAbilityController[] _abilities = new UiAbilityController[0];

        private bool _hovered = false;

        public void Setup(GameObject hobbler, RosterType type)
        {
            _type = type;
            _buttonImage.sprite = type == RosterType.Bench ? _benchButtonSprite : _rosterButtonSprite;
            _buttonImage.color = type == RosterType.Bench ? _benchButtonColor : _rosterButtonColor;
            _hobbler = hobbler;
            RefreshInfo();

            var queryEquipmentMsg = MessageFactory.GenerateQueryHobblerEquipmentMsg();
            queryEquipmentMsg.DoAfter = RefreshEquiment;
            gameObject.SendMessageTo(queryEquipmentMsg, _hobbler);
            MessageFactory.CacheMessage(queryEquipmentMsg);

            var queryAbilitiesMsg = MessageFactory.GenerateQueryAbilitiesMsg();
            queryAbilitiesMsg.DoAfter = RefreshAbilities;
            gameObject.SendMessageTo(queryAbilitiesMsg, _hobbler);
            MessageFactory.CacheMessage(queryAbilitiesMsg);

            _rosterButton.interactable = _type != RosterType.Bench || WorldHobblerManager.Roster.Count < WorldHobblerManager.RosterLimit;
            SubscribeToMessages();
        }

        public void ClickButton()
        {
            switch (_type)
            {
                case RosterType.Bench:
                    if (WorldHobblerManager.Roster.Count < WorldHobblerManager.RosterLimit)
                    {
                        WorldHobblerManager.AddHobblerToRoster(_hobbler);
                    }
                    break;
                case RosterType.Roster:
                    WorldHobblerManager.RemoveHobblerFromRoster(_hobbler);
                    break;
            }
        }

        public void OnPointerEnter(PointerEventData eventData)
        {
            if (!_hovered)
            {
                _hovered = true;
                var showHoveredInfoMsg = MessageFactory.GenerateShowHoverInfoMsg();
                showHoveredInfoMsg.Title = _name;
                showHoveredInfoMsg.Description = _baseStats.GetRosterDescriptions(_bonusStats);
                showHoveredInfoMsg.Owner = gameObject;
                showHoveredInfoMsg.Icon = _sprite.Sprite;
                showHoveredInfoMsg.World = false;
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

        private void RefreshInfo()
        {
            var queryCombatStatsMsg = MessageFactory.GenerateQueryCombatStatsMsg();
            queryCombatStatsMsg.DoAfter = RefreshCombatStats;
            gameObject.SendMessageTo(queryCombatStatsMsg, _hobbler);
            MessageFactory.CacheMessage(queryCombatStatsMsg);

            var queryNameMsg = MessageFactory.GenerateQueryUnitNameMsg();
            queryNameMsg.DoAfter = RefreshName;
            gameObject.SendMessageTo(queryNameMsg, _hobbler);
            MessageFactory.CacheMessage(queryNameMsg);

            var querySpriteMsg = MessageFactory.GenerateQuerySpriteMsg();
            querySpriteMsg.DoAfter = RefreshSprite;
            gameObject.SendMessageTo(querySpriteMsg, _hobbler);
            MessageFactory.CacheMessage(querySpriteMsg);

            var queryHappinessMsg = MessageFactory.GenerateQueryHappinessMsg();
            queryHappinessMsg.DoAfter = RefreshHappiness;
            gameObject.SendMessageTo(queryHappinessMsg, _hobbler);
            MessageFactory.CacheMessage(queryHappinessMsg);

            if (_hovered)
            {
                var showHoveredInfoMsg = MessageFactory.GenerateShowHoverInfoMsg();
                showHoveredInfoMsg.Title = _name;
                showHoveredInfoMsg.Description = _baseStats.GetRosterDescriptions(_bonusStats);
                showHoveredInfoMsg.Owner = gameObject;
                showHoveredInfoMsg.Icon = _sprite.Sprite;
                showHoveredInfoMsg.World = false;
                gameObject.SendMessage(showHoveredInfoMsg);
                MessageFactory.CacheMessage(showHoveredInfoMsg);
            }
        }

        private void RefreshEquiment(EquippableInstance[] armor, EquippableInstance[] trinkets, EquippableInstance weapon)
        {
            for (var i = 0; i < _equippedItems.Length; i++)
            {
                Destroy(_equippedItems[i].gameObject);
            }

            var allItems = armor.Where(i => i != null).ToList();
            allItems.AddRange(trinkets.Where(i => i != null));
            if (weapon != null)
            {
                allItems.Add(weapon);
            }

            var controllers = new List<UiEquippedItemController>();

            for (var i = 0; i < allItems.Count; i++)
            {
                var controller = Instantiate(_equippedItemTemplate, _equipmentGrid);
                controller.Setup(allItems[i].Instance, gameObject);
                controllers.Add(controller);
            }

            _equippedItems = controllers.ToArray();
        }

        private void RefreshAbilities(KeyValuePair<int, WorldAbility>[] abilities)
        {
            for (var i = 0; i < _abilities.Length; i++)
            {
                Destroy(_abilities[i].gameObject);
            }
            var controllers = new List<UiAbilityController>();
            var availableAbilities = abilities.Where(kv => kv.Value).Select(kv => kv.Value).ToArray();
            for (var i = 0; i < availableAbilities.Length; i++)
            {
                var controller = Instantiate(_abilityTemplate, _abilityGrid);
                controller.Setup(availableAbilities[i]);
                controllers.Add(controller);
            }

            _abilities = controllers.ToArray();
        }

        private void RefreshCombatStats(CombatStats baseStats, CombatStats bonusStats, GeneticCombatStats genetics)
        {
            _baseStats = baseStats + genetics;
            _bonusStats = bonusStats;
        }

        private void RefreshName(string unitName)
        {
            _name = unitName;
        }

        private void RefreshSprite(SpriteTrait trait)
        {
            _sprite = trait;
            _hobblerIconImage.sprite = _sprite.Sprite;
        }

        private void RefreshHappiness(HappinessState state)
        {
            _state = state;
            _happinessIconImage.color = _state.ToColor();
            _happinessText.text = _state.ToStateString(true);
        }

        private void SubscribeToMessages()
        {
            _hobbler.SubscribeWithFilter<RefreshUnitMessage>(RefreshUnit, FILTER);
            _hobbler.SubscribeWithFilter<EquipmentUpdatedMessage>(EquipmentUpdated, FILTER);
            _hobbler.SubscribeWithFilter<AbilitiesUpdatedMessage>(AbilitiesUpdated, FILTER);
        }

        private void RefreshUnit(RefreshUnitMessage msg)
        {
            RefreshInfo();
        }

        private void EquipmentUpdated(EquipmentUpdatedMessage msg)
        {
            var queryHobblerEquipmentMsg = MessageFactory.GenerateQueryHobblerEquipmentMsg();
            queryHobblerEquipmentMsg.DoAfter = RefreshEquiment;
            gameObject.SendMessageTo(queryHobblerEquipmentMsg, _hobbler);
            MessageFactory.CacheMessage(queryHobblerEquipmentMsg);
        }

        private void AbilitiesUpdated(AbilitiesUpdatedMessage msg)
        {
            var queryAbilitiesMsg = MessageFactory.GenerateQueryAbilitiesMsg();
            queryAbilitiesMsg.DoAfter = RefreshAbilities;
            gameObject.SendMessageTo(queryAbilitiesMsg, _hobbler);
            MessageFactory.CacheMessage(queryAbilitiesMsg);
        }

        public void Destroy()
        {
            _hobbler.UnsubscribeFromAllMessagesWithFilter(FILTER);
            _hobbler = null;
            _name = null;
            _baseStats = CombatStats.Zero;
            _bonusStats = CombatStats.Zero;
            if (_hovered)
            {
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
                var removeHoverInfoMsg = MessageFactory.GenerateRemoveHoverInfoMsg();
                removeHoverInfoMsg.Owner = gameObject;
                gameObject.SendMessage(removeHoverInfoMsg);
                MessageFactory.CacheMessage(removeHoverInfoMsg);
            }
        }

        void OnDestroy()
        {
            if (_hobbler)
            {
                Destroy();
            }

        }
    }
}
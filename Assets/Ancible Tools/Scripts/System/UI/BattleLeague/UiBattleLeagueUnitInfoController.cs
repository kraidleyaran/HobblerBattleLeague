using System.Collections.Generic;
using System.Linq;
using Assets.Ancible_Tools.Scripts.Traits;
using Assets.Resources.Ancible_Tools.Scripts.System.Abilities;
using Assets.Resources.Ancible_Tools.Scripts.System.BattleLeague;
using Assets.Resources.Ancible_Tools.Scripts.System.Combat;
using Assets.Resources.Ancible_Tools.Scripts.System.Items;
using Assets.Resources.Ancible_Tools.Scripts.System.UI.DetailedInfo;
using Assets.Resources.Ancible_Tools.Scripts.System.Windows;
using MessageBusLib;
using UnityEngine;
using UnityEngine.UI;

namespace Assets.Resources.Ancible_Tools.Scripts.System.UI.BattleLeague
{
    public class UiBattleLeagueUnitInfoController : UiBaseWindow
    {
        public override bool Movable => false;
        public override bool Static => true;

        [SerializeField] private Image _unitIconImage = null;
        [SerializeField] private Text _unitNameText = null;
        [SerializeField] private int _maxAbilityCount = 8;
        [SerializeField] private RectTransform _abilityTransform;
        [SerializeField] private GameObject _abilitiesAndAttackInfoGroup;
        [SerializeField] private GameObject _equippedItemsGroup = null;
        [SerializeField] private UiBattleBasicAttackInfoController _basicAttackInfo = null;

        [Header("Stats")]
        [SerializeField] private UiCombatStatController _healthController = null;
        [SerializeField] private UiCombatStatController _manaController = null;
        [SerializeField] private UiCombatStatController _strengthController = null;
        [SerializeField] private UiCombatStatController _agilityController = null;
        [SerializeField] private UiCombatStatController _defenseController = null;
        [SerializeField] private UiCombatStatController _magicController = null;
        [SerializeField] private UiCombatStatController _faithController = null;
        [SerializeField] private UiCombatStatController _spiritController = null;

        [Header("Equipped Items")]
        [SerializeField] private UiEquippedItemController[] _armorControllers = new UiEquippedItemController[0];
        [SerializeField] private UiEquippedItemController[] _trinketControllers = new UiEquippedItemController[0];
        [SerializeField] private UiEquippedItemController _weaponController = null;

        private UiAbilityController[] _abilityControllers = new UiAbilityController[0];

        private GameObject _unit = null;

        private QueryBattleUnitDataMessage _queryBattleUnitDataMsg = new QueryBattleUnitDataMessage();

        public override void Awake()
        {
            base.Awake();
            Setup(null);
            SubscribeToMessages();
        }

        private void Setup(GameObject unit)
        {
            _unit = unit;
            ClearEquipment();
            ClearAbilities();
            if (_unit)
            {
                RefreshInfo();
            }
            else
            {
                gameObject.SetActive(false);
            }
            
        }

        private void RefreshInfo()
        {
            BattleUnitData unitData = null;
            _queryBattleUnitDataMsg.DoAfter = data => { unitData = data;};
            gameObject.SendMessageTo(_queryBattleUnitDataMsg, _unit);
            
            _unitNameText.text = $"{unitData.Name}";
            UpdateSpriteIcon(unitData.Sprite);
            var queryCombatStatsMsg = MessageFactory.GenerateQueryCombatStatsMsg();
            queryCombatStatsMsg.DoAfter = UpdateCombatStats;
            gameObject.SendMessageTo(queryCombatStatsMsg, _unit);
            MessageFactory.CacheMessage(queryCombatStatsMsg);
            //UpdateCombatStats(unitData.Stats);
            UpdateEquipment(unitData.EquippedItems);
            if (_weaponController.Item)
            {
                _basicAttackInfo.Clear();
            }
            else
            {
                _basicAttackInfo.Setup(unitData.BasicAttack);
            }
            UpdateAbilities(unitData.Abilities);
            _abilitiesAndAttackInfoGroup.gameObject.SetActive(_abilityTransform.gameObject.activeSelf || _basicAttackInfo.gameObject.activeSelf);
            gameObject.SetActive(true);
        }

        private void ClearEquipment()
        {
            for (var i = 0; i < _armorControllers.Length; i++)
            {
                _armorControllers[i].Setup(null, gameObject);
            }

            for (var i = 0; i < _trinketControllers.Length; i++)
            {
                _trinketControllers[i].Setup(null, gameObject);
            }

            _weaponController.Setup(null, gameObject);
        }

        private void ClearAbilities()
        {
            for (var i = 0; i < _abilityControllers.Length; i++)
            {
                Destroy(_abilityControllers[i].gameObject);
            }
            _abilityControllers = new UiAbilityController[0];
        }

        private void UpdateSpriteIcon(SpriteTrait sprite)
        {
            _unitIconImage.sprite = sprite.Sprite;
        }

        private void UpdateCombatStats(CombatStats stats, CombatStats bonus, GeneticCombatStats genetics)
        {
            _healthController.Setup(stats.Health + bonus.Health);
            _manaController.Setup(stats.Mana + bonus.Health);
            _strengthController.Setup(stats.Strength + bonus.Strength);
            _agilityController.Setup(stats.Agility + bonus.Agility);
            _defenseController.Setup(stats.Defense + bonus.Defense);
            _magicController.Setup(stats.Magic + bonus.Magic);
            _faithController.Setup(stats.Faith + bonus.Faith);
            _spiritController.Setup(stats.Spirit + bonus.Faith);
        }

        private void UpdateEquipment(EquippableItem[] items)
        {
            var armor = items.Where(i => i.Slot == EquipSlot.Armor).ToArray();
            for (var i = 0; i < _armorControllers.Length && i < armor.Length; i++)
            {
                _armorControllers[i].Setup(armor[i], _unit);
            }

            var trinkets = items.Where(i => i.Slot == EquipSlot.Trinket).ToArray();

            for (var i = 0; i < _trinketControllers.Length && i < trinkets.Length; i++)
            {
                _trinketControllers[i].Setup(trinkets[i], _unit);
            }

            var weapon = items.FirstOrDefault(i => i.Slot == EquipSlot.Weapon);
            _weaponController.Setup(weapon, _unit);

            var unequipped = _armorControllers.Where(c => c.Item == null).ToList();
            unequipped.AddRange(_trinketControllers.Where(t => t.Item == null));
            for (var i = 0; i < unequipped.Count; i++)
            {
                unequipped[i].Setup(null, _unit);
            }
            _equippedItemsGroup.gameObject.SetActive(items.Length > 0);
        }

        private void UpdateAbilities(WorldAbility[] abilities)
        {
            var abilityControllers = new List<UiAbilityController>();
            for (var i = 0; i < abilities.Length && i < _maxAbilityCount; i++)
            {
                var controller = Instantiate(FactoryController.ABILITY_CONTROLLER, _abilityTransform);
                controller.Setup(abilities[i]);
                abilityControllers.Add(controller);
                
            }

            _abilityControllers = abilityControllers.ToArray();
            _abilityTransform.gameObject.SetActive(abilities.Length > 0);
        }

        private void SubscribeToMessages()
        {
            gameObject.Subscribe<UpdateSelectedBattleUnitMessage>(UpdateSelectedBattleUnit);
        }

        private void UpdateSelectedBattleUnit(UpdateSelectedBattleUnitMessage msg)
        {
            Setup(msg.Unit);
        }

        void OnDestroy()
        {
            gameObject.UnsubscribeFromAllMessages();
        }
        
    }
}
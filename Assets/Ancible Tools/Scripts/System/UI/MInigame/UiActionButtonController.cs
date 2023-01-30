using Assets.Resources.Ancible_Tools.Scripts.System.Abilities;
using Assets.Resources.Ancible_Tools.Scripts.System.BattleLeague;
using Assets.Resources.Ancible_Tools.Scripts.System.Combat;
using Assets.Resources.Ancible_Tools.Scripts.System.Items;
using Assets.Resources.Ancible_Tools.Scripts.System.Minigame;
using Assets.Resources.Ancible_Tools.Scripts.System.TickTimers;
using Assets.Resources.Ancible_Tools.Scripts.System.UI.Timer;
using MessageBusLib;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace Assets.Resources.Ancible_Tools.Scripts.System.UI.MInigame
{
    public class UiActionButtonController : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
    {
        private const string FILTER = "ActionButton-";

        public AbilityInstance Ability { get; private set; }
        public BasicAttackSetup AttackSetup { get; private set; }

        [SerializeField] private UiTimerController _cooldownController = null;
        [SerializeField] private UiTimerController _globalController = null;
        [SerializeField] private Image _icon = null;

        private GameObject _player = null;
        private bool _hovered = false;
        private string _filter = string.Empty;

        public void SetupBasicAttack(GameObject player)
        {
            _player = player;
            EquippableInstance playerWeapon = null;
            var queryEquipmentMsg = MessageFactory.GenerateQueryHobblerEquipmentMsg();
            queryEquipmentMsg.DoAfter = (armor, trinkets, weapon) => { playerWeapon = weapon; };
            gameObject.SendMessageTo(queryEquipmentMsg, _player);
            MessageFactory.CacheMessage(queryEquipmentMsg);

            var queryBasicAttackMsg = MessageFactory.GenerateQueryBasicAttackSetupMsg();
            queryBasicAttackMsg.DoAfter = attackSetup => AttackSetup = attackSetup;
            gameObject.SendMessageTo(queryBasicAttackMsg, player);
            MessageFactory.CacheMessage(queryBasicAttackMsg);

            TickTimer globalCooldown = null;
            var queryGlobalCooldownMsg = MessageFactory.GenerateQueryGlobalCooldownMsg();
            queryGlobalCooldownMsg.DoAfter = cooldown => globalCooldown = cooldown;
            gameObject.SendMessageTo(queryGlobalCooldownMsg, _player);
            MessageFactory.CacheMessage(queryGlobalCooldownMsg);

            if (globalCooldown != null)
            {
                _globalController.Setup(globalCooldown, TimerType.Cooldown, null);
            }

            _icon.sprite = playerWeapon != null ? playerWeapon.Instance.Icon : IconFactoryController.DefaultBasicAttack;
            SubscribeToMessages();
        }

        public void SetupAbility(AbilityInstance ability, GameObject player)
        {
            _player = player;
            Ability = ability;
            if (Ability != null)
            {
                _icon.sprite = Ability.Instance.Icon;
                _cooldownController.Setup(Ability.CooldownTimer, TimerType.Cooldown, null);

                TickTimer globalCooldown = null;
                var queryGlobalCooldownMsg = MessageFactory.GenerateQueryGlobalCooldownMsg();
                queryGlobalCooldownMsg.DoAfter = cooldown => globalCooldown = cooldown;
                gameObject.SendMessageTo(queryGlobalCooldownMsg, _player);
                MessageFactory.CacheMessage(queryGlobalCooldownMsg);

                if (globalCooldown != null)
                {
                    _globalController.Setup(globalCooldown, TimerType.Cooldown, null);
                }
                SubscribeToMessages();
            }
            else
            {
                _icon.gameObject.SetActive(false);
            }
            
        }

        public void SetEmpty(GameObject player)
        {
            _player = player;
            Ability = null;
            AttackSetup = null;
            _icon.gameObject.SetActive(false);
            _cooldownController.gameObject.SetActive(false);
        }

        public void Click()
        {
            var useActionBarButtonMsg = MessageFactory.GenerateActionBarButtonMessage();
            useActionBarButtonMsg.Controller = this;
            gameObject.SendMessage(useActionBarButtonMsg);
            MessageFactory.CacheMessage(useActionBarButtonMsg);
        }

        public void DoAction(GameObject target)
        {
            if (AttackSetup != null)
            {
                var canAttack = true;
                var attackCheckMsg = MessageFactory.GenerateCanAttackCheckMsg();
                attackCheckMsg.DoAfter = () => canAttack = true;
                gameObject.SendMessageTo(attackCheckMsg, _player);
                MessageFactory.CacheMessage(attackCheckMsg);
                
                if (canAttack)
                {
                    var doBasicAttackMsg = MessageFactory.GenerateDoBasicAttackMsg();
                    doBasicAttackMsg.Direction = (target.transform.position.ToVector2() - _player.transform.position.ToVector2()).normalized.ToVector2Int(true);
                    doBasicAttackMsg.Target = target;
                    gameObject.SendMessageTo(doBasicAttackMsg, _player);
                    MessageFactory.CacheMessage(doBasicAttackMsg);
                }
            }
        }

        public void OnPointerEnter(PointerEventData eventData)
        {
            _hovered = true;
            var showHoverInfoMsg = MessageFactory.GenerateShowHoverInfoMsg();
            showHoverInfoMsg.Owner = gameObject;
            showHoverInfoMsg.World = false;
            showHoverInfoMsg.Position = transform.position.ToVector2();
            if (AttackSetup != null)
            {
                showHoverInfoMsg.Title = "Basic Attack";
                showHoverInfoMsg.Description = AttackSetup.GetDescription();
                showHoverInfoMsg.Icon = _icon.sprite;
            }
            else if (Ability != null)
            {
                showHoverInfoMsg.Title = Ability.Instance.DisplayName;
                showHoverInfoMsg.Description = Ability.Instance.GetDescription();
                showHoverInfoMsg.Icon = _icon.sprite;
            }
            else
            {
                showHoverInfoMsg.Title = "Empty";
                showHoverInfoMsg.Description = string.Empty;
            }

            gameObject.SendMessage(showHoverInfoMsg);
            MessageFactory.CacheMessage(showHoverInfoMsg);
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

        private void CheckCooldowns()
        {
            if (Ability != null)
            {
                if (_cooldownController.Timer.State != TimerState.Stopped)
                {
                    var activeTimer = _globalController.Timer.State != TimerState.Stopped && _globalController.Timer.RemainingTicks > _cooldownController.Timer.RemainingTicks ? _globalController : _cooldownController;
                    var inactiveTimer = activeTimer == _globalController ? _cooldownController : _globalController;
                    activeTimer.gameObject.SetActive(true);
                    inactiveTimer.gameObject.SetActive(false);
                }
                else if (!_globalController.gameObject.activeSelf)
                {
                    _globalController.gameObject.SetActive(true);
                }
            }
        }

        private void SubscribeToMessages()
        {
            _filter = $"{FILTER}{GetInstanceID()}";
            _player.SubscribeWithFilter<UpdateMinigameUnitStateMessage>(UpdateUnitState, _filter);
            _player.SubscribeWithFilter<ActivateGlobalCooldownMessage>(ActivateGlobalCooldown, _filter);
        }

        private void UpdateUnitState(UpdateMinigameUnitStateMessage msg)
        {
            if (msg.State == MinigameUnitState.Casting)
            {
                CheckCooldowns();
            }

        }

        private void ActivateGlobalCooldown(ActivateGlobalCooldownMessage msg)
        {
            StartCoroutine(StaticMethods.WaitForFrames(1, CheckCooldowns));
        }

        void OnDestroy()
        {
            if (_player)
            {
                _player.UnsubscribeFromAllMessagesWithFilter(_filter);
                _player = null;
                Destroy(_cooldownController.gameObject);
                Destroy(_globalController.gameObject);
            }
        }
    }
}
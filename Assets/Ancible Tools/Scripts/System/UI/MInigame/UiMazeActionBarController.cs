using System.Collections.Generic;
using System.Linq;
using Assets.Resources.Ancible_Tools.Scripts.System.Abilities;
using Assets.Resources.Ancible_Tools.Scripts.System.Combat;
using Assets.Resources.Ancible_Tools.Scripts.System.Minigame;
using Assets.Resources.Ancible_Tools.Scripts.System.Pathing;
using Assets.Resources.Ancible_Tools.Scripts.System.UI.Timer;
using Assets.Resources.Ancible_Tools.Scripts.System.Windows;
using MessageBusLib;
using UnityEngine;
using UnityEngine.UI;

namespace Assets.Resources.Ancible_Tools.Scripts.System.UI.MInigame
{
    public class UiMazeActionBarController : UiBaseWindow
    {
        private const string FILTER = "UI_MAZE_ACTION_BAR";

        private static UiMazeActionBarController _instance = null;

        [SerializeField] private UiFillBarController _healthController = null;
        [SerializeField] private UiFillBarController _manaController = null;
        [SerializeField] private UiActionButtonController _basicAttackController = null;
        [SerializeField] private UiActionButtonController[] _abilityControllers = new UiActionButtonController[0];
        [SerializeField] private UiMinigameCastingBarController _castingBarController = null;
        
        private GameObject _selectedUnit = null;

        private GameObject _player = null;
        private MinigameUnitState _playerState = MinigameUnitState.Idle;
        private MapTile _playerMapTile = null;

        public override void Awake()
        {
            base.Awake();
            if (_instance)
            {
                Destroy(_instance.gameObject);
            }

            _instance = this;
            SubscribeToMessages();
        }

        public static void SetPlayer(GameObject player)
        {
            _instance._player = player;
            _instance._basicAttackController.SetupBasicAttack(player);

            var queryMinigameAbilitiesMsg = MessageFactory.GenerateQueryMinigameAbilitiesMsg();
            queryMinigameAbilitiesMsg.DoAfter = _instance.UpdateAbilities;
            _instance.gameObject.SendMessageTo(queryMinigameAbilitiesMsg, player);
            MessageFactory.CacheMessage(queryMinigameAbilitiesMsg);

            var queryMapTileMsg = MessageFactory.GenerateQueryMapTileMsg();
            queryMapTileMsg.DoAfter = tile => _instance._playerMapTile = tile;
            _instance.gameObject.SendMessageTo(queryMapTileMsg, player);
            MessageFactory.CacheMessage(queryMapTileMsg);

            _instance.RefreshHealthMana();
            _instance.SubscribePlayer(player);
        }

        private void UpdateAbilities(KeyValuePair<int, AbilityInstance>[] abilities)
        {
            for (var i = 0; i < abilities.Length && i < _abilityControllers.Length; i++)
            {
                _abilityControllers[i].SetupAbility(abilities[i].Value, _player);
            }

            var emptyControllers = _abilityControllers.Where(a => a.Ability == null).ToArray();
            for (var i = 0; i < emptyControllers.Length; i++)
            {
                emptyControllers[i].SetEmpty(_player);
            }
        }

        private void UpdateHealth(int current, int max)
        {
            var percent = (float) current / max;
            _healthController.Setup(percent, $"{current} / {max}", ColorFactoryController.HealthBar);
        }

        private void UpdateMana(int current, int max)
        {
            var percent = (float)current / max;
            _manaController.Setup(percent, $"{current} / {max}", ColorFactoryController.ManaBar);
        }

        private void RefreshHealthMana()
        {
            var queryHealthMsg = MessageFactory.GenerateQueryHealthMsg();
            queryHealthMsg.DoAfter = _instance.UpdateHealth;
            queryHealthMsg.DirectValues = true;
            _instance.gameObject.SendMessageTo(queryHealthMsg, _player);
            MessageFactory.CacheMessage(queryHealthMsg);

            var queryManaMsg = MessageFactory.GenerateQueryManaMsg();
            queryManaMsg.DoAfter = _instance.UpdateMana;
            _instance.gameObject.SendMessageTo(queryManaMsg, _player);
            MessageFactory.CacheMessage(queryManaMsg);
        }

        private bool CanCast()
        {
            var canCast = true;
            var castCheckMsg = MessageFactory.GenerateCanCastCheckMsg();
            castCheckMsg.DoAfter = () => canCast = false;
            gameObject.SendMessageTo(castCheckMsg, _player);
            MessageFactory.CacheMessage(castCheckMsg);
            return canCast;
        }

        private void SubscribePlayer(GameObject player)
        {
            player.SubscribeWithFilter<RefreshUnitMessage>(RefreshUnit, FILTER);
            player.SubscribeWithFilter<UpdateMinigameUnitStateMessage>(UpdatePlayerMinigameUnitState, FILTER);
            player.SubscribeWithFilter<UpdateMapTileMessage>(UpdatePlayerMapTile, FILTER);
            //player.SubscribeWithFilter<UpdateSelectedMinigameUnitMessage>(UpdateSelectedMinigameUnit, FILTER);
            player.SubscribeWithFilter<UpdateUnitCastTimerMessage>(UpdatePlayerUnitCastTimer, FILTER);
            player.SubscribeWithFilter<CastInterruptedMessage>(CastInterrupted, FILTER);
        }


        private void SubscribeToMessages()
        {
            gameObject.Subscribe<UpdateInputStateMessage>(UpdateInputState);
            gameObject.Subscribe<UseActionBarButtonMessage>(UseActionBarButton);
            gameObject.Subscribe<UpdateSelectedMinigameUnitMessage>(UpdateSelectedMinigameUnit);
        }

        private void UpdateInputState(UpdateInputStateMessage msg)
        {
            if (_playerState == MinigameUnitState.Idle)
            {
                if (!msg.Previous.Attack && msg.Current.Attack)
                {
                    _basicAttackController.Click();
                }
                else if (!msg.Previous.AbilitySlot0 && msg.Current.AbilitySlot0)
                {
                    _abilityControllers[0].Click();
                }
                else if (!msg.Previous.AbilitySlot1 && msg.Current.AbilitySlot1)
                {
                    _abilityControllers[1].Click();
                }
                else if (!msg.Previous.AbilitySlot2 && msg.Current.AbilitySlot2)
                {
                    _abilityControllers[2].Click();
                }
                else if (!msg.Previous.AbilitySlot3 && msg.Current.AbilitySlot3)
                {
                    _abilityControllers[3].Click();
                }
            }

        }

        private void UseActionBarButton(UseActionBarButtonMessage msg)
        {
            if (_player && _playerState == MinigameUnitState.Idle)
            {
                var button = msg.Controller;
                if (_selectedUnit)
                {
                    if (button.AttackSetup != null)
                    {
                        CombatAlignment alignment = CombatAlignment.Neutral;
                        var queryAlignmentMsg = MessageFactory.GenerateQueryCombatAlignmentMsg();
                        queryAlignmentMsg.DoAfter = targetAlignment => alignment = targetAlignment;
                        gameObject.SendMessageTo(queryAlignmentMsg, _selectedUnit);
                        MessageFactory.CacheMessage(queryAlignmentMsg);

                        if (alignment != CombatAlignment.Player)
                        {

                            button.DoAction(_selectedUnit);
                            
                        }
                    }
                    else if (button.Ability != null)
                    {
                        if (button.Ability.OnCooldown)
                        {

                        }
                        else if (CanCast())
                        {
                            MapTile targetTile = null;
                            var queryMapTileMsg = MessageFactory.GenerateQueryMapTileMsg();
                            queryMapTileMsg.DoAfter = tile => targetTile = tile;
                            gameObject.SendMessageTo(queryMapTileMsg, _selectedUnit);
                            MessageFactory.CacheMessage(queryMapTileMsg);

                            if (targetTile != null)
                            {
                                var distance = _playerMapTile.Position.DistanceTo(targetTile.Position);
                                if (button.Ability.Instance.CanApplyToTarget(_player, _selectedUnit, distance, false))
                                {
                                    var castAbilityMsg = MessageFactory.GenerateCastAbilityMsg();
                                    castAbilityMsg.Ability = button.Ability;
                                    castAbilityMsg.Target = _selectedUnit;
                                    gameObject.SendMessageTo(castAbilityMsg, _player);
                                    MessageFactory.CacheMessage(castAbilityMsg);
                                }
                            }
                        }
                    }
                }
                else if (CanCast() && button.Ability != null && (button.Ability.Instance.Type == AbilityType.Self || button.Ability.Instance.Type == AbilityType.Both))
                {
                    if (button.Ability.OnCooldown)
                    {

                    }
                    else
                    {
                        var castAbilityMsg = MessageFactory.GenerateCastAbilityMsg();
                        castAbilityMsg.Ability = button.Ability;
                        castAbilityMsg.Target = _player;
                        gameObject.SendMessageTo(castAbilityMsg, _player);
                        MessageFactory.CacheMessage(castAbilityMsg);
                    }
                }
            }


        }



        private void RefreshUnit(RefreshUnitMessage msg)
        {
            RefreshHealthMana();
        }

        private void UpdatePlayerMinigameUnitState(UpdateMinigameUnitStateMessage msg)
        {
            _playerState = msg.State;
        }

        private void UpdatePlayerMapTile(UpdateMapTileMessage msg)
        {
            _playerMapTile = msg.Tile;
        }

        private void UpdatePlayerUnitCastTimer(UpdateUnitCastTimerMessage msg)
        {
            _castingBarController.Setup(msg.CastTimer, msg.Name, msg.Icon);
        }

        private void UpdateSelectedMinigameUnit(UpdateSelectedMinigameUnitMessage msg)
        {
            _selectedUnit = msg.Unit;
        }

        private void CastInterrupted(CastInterruptedMessage msg)
        {
            _castingBarController.Interrupt();
        }

        public override void Destroy()
        {
            if (_instance && _instance == this)
            {
                _instance = null;
                if (_player)
                {
                    _player.UnsubscribeFromAllMessagesWithFilter(FILTER);
                }
            }
            base.Destroy();
        }
    }
}
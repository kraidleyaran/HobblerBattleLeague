using System.Collections.Generic;
using System.Linq;
using Assets.Resources.Ancible_Tools.Scripts.System;
using Assets.Resources.Ancible_Tools.Scripts.System.Abilities;
using Assets.Resources.Ancible_Tools.Scripts.System.BattleLeague;
using Assets.Resources.Ancible_Tools.Scripts.System.Pathing;
using Assets.Resources.Ancible_Tools.Scripts.System.TickTimers;
using MessageBusLib;
using UnityEngine;

namespace Assets.Ancible_Tools.Scripts.Traits
{
    [CreateAssetMenu(fileName = "Battle Ability Manager Trait", menuName = "Ancible Tools/Traits/Battle/Battle Ability Manager")]
    public class BattleAbilityManagerTrait : Trait
    {
        [SerializeField] private WorldAbility[] _startingAbilities = new WorldAbility[0];
        [SerializeField] private int _maxAbilities = 4;
        [SerializeField] private float _bumpDistance = 16f * .3125f;
        [SerializeField] private int _defaultBumpSpeed = 50;

        private UnitBattleState _battleState = UnitBattleState.Active;

        private Dictionary<int, AbilityInstance> _abilities = new Dictionary<int, AbilityInstance>();

        private TickTimer _castingTimer = null;

        public override void SetupController(TraitController controller)
        {
            base.SetupController(controller);
            for (var i = 0; i < _maxAbilities; i++)
            {
                _abilities.Add(i, null);
            }

            for (var i = 0; i < _startingAbilities.Length; i++)
            {
                if (_abilities.TryGetValue(i, out var ability))
                {
                    _abilities[i] = _startingAbilities[i].GenerateInstance();
                }
                else if (_maxAbilities < 0)
                {
                    _abilities.Add(i, _startingAbilities[i].GenerateInstance());
                }
            }
            SubscribeToMessages();
        }

        private void StartCastingAbility(AbilityInstance ability, GameObject target)
        {
            _castingTimer?.Destroy();
            _castingTimer = new TickTimer(ability.Instance.CastTime, 0, () => {FinishCasting(ability, target);}, null);
            var updateUnitCastingTimerMsg = MessageFactory.GenerateUpdateUnitCastTimerMsg();
            updateUnitCastingTimerMsg.CastTimer = _castingTimer;
            updateUnitCastingTimerMsg.Icon = ability.Instance.Icon;
            updateUnitCastingTimerMsg.Name = string.Empty;
            _controller.gameObject.SendMessageTo(updateUnitCastingTimerMsg, _controller.transform.parent.gameObject);
            MessageFactory.CacheMessage(updateUnitCastingTimerMsg);
        }

        private void FinishCasting(AbilityInstance ability, GameObject target)
        {
            _castingTimer?.Destroy();
            _castingTimer = null;
            if (_battleState != UnitBattleState.Dead)
            {
                var targetBattleState = UnitBattleState.Active;
                var queryUnitBattleStateMsg = MessageFactory.GenerateQueryUnitBattleStateMsg();
                queryUnitBattleStateMsg.DoAfter = state => targetBattleState = state;
                _controller.gameObject.SendMessageTo(queryUnitBattleStateMsg, target);
                MessageFactory.CacheMessage(queryUnitBattleStateMsg);
                if (targetBattleState != UnitBattleState.Dead)
                {
                    
                    var diff = target.transform.position.ToVector2() - _controller.transform.parent.position.ToVector2();
                    var doBumpMsg = MessageFactory.GenerateDoBumpOverPixelsPerSecondMsg();
                    doBumpMsg.Direction = diff.normalized;
                    doBumpMsg.Distance = _bumpDistance;
                    doBumpMsg.PixelsPerSecond = _defaultBumpSpeed;
                    doBumpMsg.OnBump = () =>{ UseAbilityOnTarget(ability, target);};
                    doBumpMsg.DoAfter = CleanUpCasting;
                    //var doBumpMsg = MessageFactory.GenerateDoBumpMsg();
                    //doBumpMsg.Direction = (target.transform.position.ToVector2() - _controller.transform.parent.position.ToVector2()).normalized;
                    //doBumpMsg.OnBump = () => { UseAbilityOnTarget(ability, target); };
                    //doBumpMsg.DoAfter = CleanUpCasting;
                    _controller.gameObject.SendMessageTo(doBumpMsg, _controller.transform.parent.gameObject);
                    MessageFactory.CacheMessage(doBumpMsg);

                    var applyManaMsg = MessageFactory.GenerateApplyManaMsg();
                    applyManaMsg.Amount = ability.Instance.ManaCost * -1;
                    _controller.gameObject.SendMessageTo(applyManaMsg, _controller.transform.parent.gameObject);
                    MessageFactory.CacheMessage(applyManaMsg);
                }
                else
                {
                    CleanUpCasting();
                }

            }
        }

        private void CleanUpCasting()
        {
            if (_battleState == UnitBattleState.Cast)
            {
                _controller.gameObject.SendMessageTo(ActivateGlobalCooldownMessage.INSTANCE, _controller.transform.parent.gameObject);
            }

        }

        private KeyValuePair<int, AbilityInstance>[] GetAvailableAllyAbilities(MapTile currentTile, GameObject[] allies, int mana)
        {
            var returnAbilities = new List<KeyValuePair<int, AbilityInstance>>();
            var abilities = _abilities.Where(kv => kv.Value != null && kv.Value.Instance.ManaCost <= mana && !kv.Value.OnCooldown && (kv.Value.Instance.Type == AbilityType.Both || kv.Value.Instance.Type == AbilityType.Other) && (kv.Value.Instance.Alignment == AbilityTargetAlignment.Ally || kv.Value.Instance.Alignment == AbilityTargetAlignment.Both)).ToList();
            
            if (abilities.Count > 0)
            {
                var queryMapTileMsg = MessageFactory.GenerateQueryMapTileMsg();
                var allyRanges = allies.ToDictionary(a => a, a => GetRangeOfTarget(currentTile, a, queryMapTileMsg));
                for (var i = 0; i < abilities.Count; i++)
                {
                    var ability = abilities[i];
                    var applicable = allyRanges.Count(kv => kv.Value <= ability.Value.Instance.Range && ability.Value.Instance.CanApplyToTarget(_controller.transform.parent.gameObject, kv.Key, kv.Value));
                    if (applicable > 0)
                    {
                        returnAbilities.Add(ability);
                    }
                }
                MessageFactory.CacheMessage(queryMapTileMsg);
            }

            return returnAbilities.ToArray();
        }

        private GameObject GetAllyTarget(AbilityInstance ability, GameObject[] allies, MapTile currentTile)
        {
            var queryMapTileMsg = MessageFactory.GenerateQueryMapTileMsg();
            var availableAllies = allies.ToDictionary(a => a, a => GetRangeOfTarget(currentTile, a, queryMapTileMsg)).Where(kv => ability.Instance.CanApplyToTarget(_controller.transform.parent.gameObject, kv.Key, kv.Value)).OrderBy(kv => kv.Value).ToArray();
            MessageFactory.CacheMessage(queryMapTileMsg);
            return availableAllies[0].Key;
        }

        private int GetRangeOfTarget(MapTile currentTile, GameObject target, QueryMapTileMessage mapTileMsg)
        {
            MapTile maptile = null;
            mapTileMsg.DoAfter = tile => maptile = tile;
            _controller.gameObject.SendMessageTo(mapTileMsg, target);
            if (maptile != null)
            {
                return currentTile.Position.DistanceTo(maptile.Position);
            }

            return -1;
        }

        private void UseAbilityOnTarget(AbilityInstance ability, GameObject target)
        {
            var targetBattleState = UnitBattleState.Active;
            var queryUnitBattleStateMsg = MessageFactory.GenerateQueryUnitBattleStateMsg();
            queryUnitBattleStateMsg.DoAfter = state => targetBattleState = state;
            _controller.gameObject.SendMessageTo(queryUnitBattleStateMsg, target);
            MessageFactory.CacheMessage(queryUnitBattleStateMsg);
            if (targetBattleState != UnitBattleState.Dead)
            {
                ability.UseAbility(_controller.transform.parent.gameObject, target);
            }
        }

        private void InterruptCasting()
        {
            if (_castingTimer != null)
            {
                _castingTimer.Destroy();
                _castingTimer = null;
                _controller.gameObject.SendMessageTo(CastInterruptedMessage.INSTANCE, _controller.transform.parent.gameObject);
            }

            if (_battleState == UnitBattleState.Cast)
            {
                var setBattleUnitStateMsg = MessageFactory.GenerateSetUnitBattleStateMsg();
                setBattleUnitStateMsg.State = UnitBattleState.Active;
                _controller.gameObject.SendMessageTo(setBattleUnitStateMsg, _controller.transform.parent.gameObject);
                MessageFactory.CacheMessage(setBattleUnitStateMsg);
            }
        }

        private void SubscribeToMessages()
        {
            _controller.gameObject.Subscribe<UpdateBattleStateMessage>(UpdateBattleState);
            _controller.transform.parent.gameObject.SubscribeWithFilter<BattleAbilityCheckMessage>(BattleAbilityCheck, _instanceId);
            _controller.transform.parent.gameObject.SubscribeWithFilter<UpdateUnitBattleStateMessage>(UpdateUnitBattleState, _instanceId);
            _controller.transform.parent.gameObject.SubscribeWithFilter<SetAbilitiesMessage>(SetAbilities, _instanceId);
            _controller.transform.parent.gameObject.SubscribeWithFilter<StunMessage>(Stun, _instanceId);
            _controller.transform.parent.gameObject.SubscribeWithFilter<SilenceMessage>(Silence, _instanceId);
            _controller.transform.parent.gameObject.SubscribeWithFilter<QueryCastingMessage>(QueryCasting, _instanceId);
        }

        private void BattleAbilityCheck(BattleAbilityCheckMessage msg)
        {
            var canCast = true;
            var canCastMsg = MessageFactory.GenerateCanCastCheckMsg();
            canCastMsg.DoAfter = () => canCast = false;
            _controller.gameObject.SendMessageTo(canCastMsg, _controller.transform.parent.gameObject);
            MessageFactory.CacheMessage(canCastMsg);
            if (canCast)
            {
                var currentMana = 0;
                var queryManaMsg = MessageFactory.GenerateQueryManaMsg();
                queryManaMsg.DoAfter = (current, max) => { currentMana = current; };
                _controller.gameObject.SendMessageTo(queryManaMsg, _controller.transform.parent.gameObject);
                MessageFactory.CacheMessage(queryManaMsg);

                var allyAbilities = GetAvailableAllyAbilities(msg.Origin, msg.Allies, currentMana).ToList();
                var availableAbilities = _abilities.Where(kv => kv.Value != null && kv.Value.Instance.ManaCost <= currentMana && !kv.Value.OnCooldown && kv.Value.Instance.CanApplyToTarget(_controller.transform.parent.gameObject, msg.Target, msg.Distance)).ToList();
                availableAbilities.AddRange(allyAbilities);
                if (availableAbilities.Count > 0)
                {
                    var setUnitBattleStateMsg = MessageFactory.GenerateSetUnitBattleStateMsg();
                    setUnitBattleStateMsg.State = UnitBattleState.Cast;
                    _controller.gameObject.SendMessageTo(setUnitBattleStateMsg, _controller.transform.parent.gameObject);
                    MessageFactory.CacheMessage(setUnitBattleStateMsg);

                    var ordered = availableAbilities.OrderBy(kv => kv.Key).ToArray();
                    var targetObj = msg.Target;
                    var ability = ordered[0];
                    if (allyAbilities.Contains(ability))
                    {
                        targetObj = GetAllyTarget(ability.Value, msg.Allies, msg.Origin);
                    }

                    if (ordered[0].Value.Instance.CastTime > 0)
                    {
                        StartCastingAbility(ordered[0].Value, targetObj);
                    }
                    else
                    {
                        FinishCasting(ordered[0].Value, targetObj);
                    }

                    var setFaceDirectionMsg = MessageFactory.GenerateSetFaceDirectionMsg();
                    setFaceDirectionMsg.Direction = (targetObj.transform.position.ToVector2() - _controller.transform.parent.position.ToVector2()).ToStaticDirections();
                    _controller.gameObject.SendMessageTo(setFaceDirectionMsg, _controller.transform.parent.gameObject);
                    MessageFactory.CacheMessage(setFaceDirectionMsg);

                    msg.DoAfter.Invoke();
                }
            }
            

        }

        private void UpdateUnitBattleState(UpdateUnitBattleStateMessage msg)
        {
            _battleState = msg.State;
            if (_battleState == UnitBattleState.Dead)
            {
                _castingTimer?.Destroy();
                _castingTimer = null;
            }
        }

        private void SetAbilities(SetAbilitiesMessage msg)
        {
            var current = _abilities.ToArray();
            for (var i = 0; i < current.Length; i++)
            {
                if (current[i].Value != null)
                {
                    current[i].Value.Destroy();
                }
            }
            _abilities.Clear();
            for (var i = 0; i < msg.Abilities.Length; i++)
            {
                _abilities.Add(i, msg.Abilities[i].GenerateInstance());
            }

            _maxAbilities = _abilities.Count;
        }

        private void UpdateBattleState(UpdateBattleStateMessage msg)
        {
            if (msg.State == BattleState.Results)
            {
                if (_castingTimer != null)
                {
                    _castingTimer.Destroy();
                    _castingTimer = null;
                }
            }
        }

        private void Silence(SilenceMessage msg)
        {
            InterruptCasting();
        }

        private void Stun(StunMessage msg)
        {
            InterruptCasting();
        }

        private void QueryCasting(QueryCastingMessage msg)
        {
            if (_castingTimer != null && _castingTimer.State == TimerState.Playing)
            {
                msg.DoAfter.Invoke();
            }
        }

        public override void Destroy()
        {
            _controller.gameObject.SendMessageTo(CastInterruptedMessage.INSTANCE, _controller.transform.parent.gameObject);
            _castingTimer?.Destroy();
            _castingTimer = null;
            var abilities = _abilities.ToArray();
            for (var i = 0; i < abilities.Length; i++)
            {
                if (abilities[i].Value != null)
                {
                    abilities[i].Value.Destroy();
                }
            }
            _abilities.Clear();
            base.Destroy();
        }
    }
}
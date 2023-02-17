using System.Collections.Generic;
using System.Linq;
using Assets.Resources.Ancible_Tools.Scripts.Hitbox;
using Assets.Resources.Ancible_Tools.Scripts.System;
using Assets.Resources.Ancible_Tools.Scripts.System.BattleLeague;
using Assets.Resources.Ancible_Tools.Scripts.System.Combat;
using Assets.Resources.Ancible_Tools.Scripts.System.Pathing;
using MessageBusLib;
using UnityEngine;

namespace Assets.Ancible_Tools.Scripts.Traits
{
    [CreateAssetMenu(fileName = "Battle Game Piece Trait", menuName = "Ancible Tools/Traits/Battle/Battle Game Piece")]
    public class BattleGamePieceTrait : Trait
    {
        [SerializeField] private Hitbox _hitbox = null;
        [SerializeField] private Vector2 _selectorSize = Vector2.zero;
        [SerializeField] private Vector2 _selectorOffset = Vector2.zero;

        private HitboxController _hitboxController = null;

        private BattleUnitData _data = null;

        private MapTile _curretBattleTile = null;
        private BattleBenchSlotController _currentBenchSlot = null;
        private BattleAlignment _alignment = BattleAlignment.None;

        private List<BattleBenchSlotController> _hoveredControllers = new List<BattleBenchSlotController>();

        private Rigidbody2D _rigidBody = null;

        private UnitSelectorController _hovered = null;
        private UnitSelectorController _selected = null;

        public override void SetupController(TraitController controller)
        {
            base.SetupController(controller);
            _rigidBody = _controller.transform.parent.gameObject.GetComponent<Rigidbody2D>();
            _hitboxController = _controller.gameObject.SetupHitbox(_hitbox, CollisionLayerFactory.BattleSelect);
            var registerCollisionMsg = MessageFactory.GenerateRegisterCollisionMsg();
            registerCollisionMsg.Object = _controller.gameObject;
            _controller.gameObject.SendMessageTo(registerCollisionMsg, _hitboxController.gameObject);
            MessageFactory.CacheMessage(registerCollisionMsg);
            SubscribeToMessages();
        }

        private void SubscribeToMessages()
        {
            _controller.gameObject.SubscribeWithFilter<EnterCollisionWithObjectMessage>(EnterCollisionWithObj, _instanceId);
            _controller.gameObject.SubscribeWithFilter<ExitCollisionWithObjectMessage>(ExitCollisionWithObj, _instanceId);

            _controller.transform.parent.gameObject.SubscribeWithFilter<UpdateBattleAlignmentMessage>(UpdateBattleAlignment, _instanceId);
            _controller.transform.parent.gameObject.SubscribeWithFilter<QueryBattlePieceMessage>(QueryBattlePiece, _instanceId);
            //_controller.transform.parent.gameObject.SubscribeWithFilter<QueryClosestGamePieceBenchSlotMessage>(QueryClosestGamePieceBenchSlot, _instanceId);
            _controller.transform.parent.gameObject.SubscribeWithFilter<QueryBattleGamePiecePlacementMessage>(QueryBattleGamePiecePlacement, _instanceId);
            _controller.transform.parent.gameObject.SubscribeWithFilter<SetGamePieceBenchMessage>(SetGamePieceBench, _instanceId);
            _controller.transform.parent.gameObject.SubscribeWithFilter<SetGamePieceMapTileMessage>(SetGamePieceMapTile, _instanceId);
            _controller.transform.parent.gameObject.SubscribeWithFilter<SetGamePieceDataMessage>(SetGamePieceData, _instanceId);
            _controller.transform.parent.gameObject.SubscribeWithFilter<QueryBattleUnitDataMessage>(QueryBattleUnitData, _instanceId);
            _controller.transform.parent.gameObject.SubscribeWithFilter<SetSelectStateMesage>(SetSelectState, _instanceId);
            _controller.transform.parent.gameObject.SubscribeWithFilter<SetHoveredStateMessage>(SetHoveredState, _instanceId);
            _controller.transform.parent.gameObject.SubscribeWithFilter<EnterBattleMessage>(EnterBattle, _instanceId);
        }

        private void UpdateBattleAlignment(UpdateBattleAlignmentMessage msg)
        {
            _alignment = msg.Alignment;
        }

        private void QueryBattlePiece(QueryBattlePieceMessage msg)
        {
            msg.DoAfter.Invoke(_data, _alignment);
        }

        private void QueryBattleUnitData(QueryBattleUnitDataMessage msg)
        {
            msg.DoAfter.Invoke(_data);
        }

        private void EnterCollisionWithObj(EnterCollisionWithObjectMessage msg)
        {
            BattleBenchSlotController benchSlot = null;
            var queryBenchSlotMsg = MessageFactory.GenerateQueryBenchSlotMsg();
            queryBenchSlotMsg.DoAfter = slot => benchSlot = slot;
            queryBenchSlotMsg.Alignment = _alignment;
            _controller.gameObject.SendMessageTo(queryBenchSlotMsg, msg.Object);
            MessageFactory.CacheMessage(queryBenchSlotMsg);

            if (benchSlot && !_hoveredControllers.Contains(benchSlot))
            {
                _hoveredControllers.Add(benchSlot);
            }
        }

        private void ExitCollisionWithObj(ExitCollisionWithObjectMessage msg)
        {
            BattleBenchSlotController benchSlot = null;
            var queryBenchSlotMsg = MessageFactory.GenerateQueryBenchSlotMsg();
            queryBenchSlotMsg.DoAfter = slot => benchSlot = slot;
            queryBenchSlotMsg.Alignment = _alignment;
            _controller.gameObject.SendMessageTo(queryBenchSlotMsg, msg.Object);
            MessageFactory.CacheMessage(queryBenchSlotMsg);

            if (benchSlot)
            {
                _hoveredControllers.Remove(benchSlot);
            }
        }

        private void SetGamePieceBench(SetGamePieceBenchMessage msg)
        {
            _currentBenchSlot = msg.Bench;
            _curretBattleTile = null;
        }

        private void SetGamePieceMapTile(SetGamePieceMapTileMessage msg)
        {
            _curretBattleTile = msg.Tile;
            _rigidBody.MovePosition(_curretBattleTile.World);
            _currentBenchSlot = null;
            Debug.Log($"Game Piece set to map tile - {_curretBattleTile.Position} - {_curretBattleTile.World} - {_rigidBody.position}");
        }

        private void QueryBattleGamePiecePlacement(QueryBattleGamePiecePlacementMessage msg)
        {
            BattleBenchSlotController closestSlot = null;
            if (_hoveredControllers.Count > 0)
            {
                var selfPos = _controller.transform.position.ToVector2();
                closestSlot = _hoveredControllers.OrderBy(c => (c.transform.position.ToVector2() - selfPos).sqrMagnitude).FirstOrDefault();
            }
            msg.DoAfter.Invoke(_currentBenchSlot, closestSlot, _curretBattleTile);
        }

        private void SetGamePieceData(SetGamePieceDataMessage msg)
        {
            _data = msg.Data;
            _alignment = msg.Alignment;

            var setCombatStatsMsg = MessageFactory.GenerateSetCombatStatsMsg();
            setCombatStatsMsg.Stats = _data.Stats;
            setCombatStatsMsg.Accumulated = GeneticCombatStats.Zero;
            _controller.gameObject.SendMessageTo(setCombatStatsMsg, _controller.transform.parent.gameObject);
            MessageFactory.CacheMessage(setCombatStatsMsg);

            var setEquipmentMsg = MessageFactory.GenerateSetEquipmentMsg();
            setEquipmentMsg.Items = _data.EquippedItems;
            _controller.gameObject.SendMessageTo(setEquipmentMsg, _controller.transform.parent.gameObject);
            MessageFactory.CacheMessage(setEquipmentMsg);

            
        }

        private void SetSelectState(SetSelectStateMesage msg)
        {
            _selected = msg.Selector;
            if (_selected)
            {
                
                _selected.gameObject.SetActive(true);
                _selected.SetParent(_controller.transform.parent, _selectorOffset, _selectorSize);
            }
        }

        private void SetHoveredState(SetHoveredStateMessage msg)
        {
            _hovered = msg.Selector;
            if (_hovered)
            {
                _hovered.gameObject.SetActive(true);
                _hovered.SetParent(_controller.transform.parent, _selectorOffset, _selectorSize);
            }
        }

        private void EnterBattle(EnterBattleMessage msg)
        {
            if (_selected)
            {
                BattleSelectController.ReturnSelect(_controller.transform.parent.gameObject);
            }

            if (_hovered)
            {
                BattleSelectController.ReturnHover(_controller.transform.parent.gameObject);
            }
            _controller.transform.parent.gameObject.SetActive(false);
        }

        public override void Destroy()
        {
            if (_hitboxController)
            {
                var unregisterCollisionMsg = MessageFactory.GenerateUnregisterCollisionMsg();
                unregisterCollisionMsg.Object = _controller.gameObject;
                _controller.gameObject.SendMessageTo(unregisterCollisionMsg, _hitboxController.gameObject);
                MessageFactory.CacheMessage(unregisterCollisionMsg);
                _hitboxController = null;
            }

            base.Destroy();
        }
    }
}
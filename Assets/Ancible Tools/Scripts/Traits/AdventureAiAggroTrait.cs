using System.Collections.Generic;
using System.Linq;
using Assets.Resources.Ancible_Tools.Scripts.System;
using Assets.Resources.Ancible_Tools.Scripts.System.Adventure;
using Assets.Resources.Ancible_Tools.Scripts.System.Pathing;
using Assets.Resources.Ancible_Tools.Scripts.System.TickTimers;
using MessageBusLib;
using UnityEngine;
using UnityEngine.PlayerLoop;

namespace Assets.Ancible_Tools.Scripts.Traits
{
    [CreateAssetMenu(fileName = "Adventure Ai Aggro Trait", menuName = "Ancible Tools/Traits/Adventure/Ai/Aggro")]
    public class AdventureAiAggroTrait : Trait
    {
        [SerializeField] private int _aggroRange = 1;
        [SerializeField] private int _leashRange = 0;
        [SerializeField] private int _diagonalCost = -1;
        [SerializeField] private Color _exclamationColor = Color.red;
        [SerializeField] private Vector2Int _exclamationOffset = Vector2Int.zero;
        [SerializeField] private Trait[] _applyOnAggro = null;

        private AdventureUnitState _unitState = AdventureUnitState.Idle;
        private AdventureAiState _aiState = AdventureAiState.Wander;
        private MapTile _mapTile = null;

        private List<Vector2Int> _aggroPositions = new List<Vector2Int>();
        private List<Vector2Int> _leashPositions = new List<Vector2Int>();

        private List<MapTile> _path = new List<MapTile>();

        private AdventureBattleExclamationController _exclamationController = null;

        private TraitController[] _aggrodTraits = new TraitController[0];
        private Vector2Int _direction = Vector2Int.zero;

        public override void SetupController(TraitController controller)
        {
            base.SetupController(controller);
            _exclamationController = Instantiate(FactoryController.BATTLE_EXCLAMATION, _controller.transform);
            var offset = new Vector2(_exclamationOffset.x * DataController.Interpolation, _exclamationOffset.y * DataController.Interpolation);
            _exclamationController.transform.SetLocalPosition(offset);
            _exclamationController.Setup(0, null, _exclamationColor);
            _exclamationController.gameObject.SetActive(false);
            SubscribeToMessages();
        }

        private void FinishWaitTimer()
        {
            if (_unitState == AdventureUnitState.Reaction)
            {
                var setAdventureUnitStateMsg = MessageFactory.GenerateSetAdventureUnitStateMsg();
                setAdventureUnitStateMsg.State = AdventureUnitState.Idle;
                _controller.gameObject.SendMessageTo(setAdventureUnitStateMsg, _controller.transform.parent.gameObject);
                MessageFactory.CacheMessage(setAdventureUnitStateMsg);
            }
            
        }

        private bool InAggroRange(Vector2Int pos)
        {
            return WorldAdventureController.MapController.MonsterPathing.DoesTileExist(pos) && _aggroPositions.Contains(pos);
        }

        private bool InLeashRange(Vector2Int pos)
        {
            return WorldAdventureController.MapController.MonsterPathing.DoesTileExist(pos) && _leashPositions.Contains(pos); 
        }

        private void SubscribeToMessages()
        {
            _controller.gameObject.Subscribe<UpdateTickMessage>(UpdateTick);
            WorldAdventureController.Player.gameObject.SubscribeWithFilter<UpdateMapTileMessage>(UpdatePlayerMapTile, _instanceId);
            _controller.transform.parent.gameObject.SubscribeWithFilter<UpdateAdventureAiStateMessage>(UpdateAdventureAiState, _instanceId);
            _controller.transform.parent.gameObject.SubscribeWithFilter<UpdateMapTileMessage>(UpdateMapTile, _instanceId);
            _controller.transform.parent.gameObject.SubscribeWithFilter<UpdateAdventureUnitStateMessage>(UpdateAdventureUnitState, _instanceId);
            _controller.transform.parent.gameObject.SubscribeWithFilter<ObstacleMessage>(Obstacle, _instanceId);
            _controller.transform.parent.gameObject.SubscribeWithFilter<UpdateDirectionMessage>(UpdateDirection, _instanceId);
        }

        private void UpdateTick(UpdateTickMessage msg)
        {
            if (_aiState == AdventureAiState.Aggro && _path.Count <= 0 && _unitState != AdventureUnitState.Disabled)
            {
                if (WorldAdventureController.MapController.MonsterPathing.DoesTileExist(WorldAdventureController.PlayerTile.Position))
                {
                    var path = WorldAdventureController.MapController.MonsterPathing.GetPath(_mapTile.Position, WorldAdventureController.PlayerTile.Position, _diagonalCost);
                    if (path.Length > 0)
                    {
                        _path = path.ToList();
                        var setDirectionMsg = MessageFactory.GenerateSetDirectionMsg();
                        setDirectionMsg.Direction = (_path[0].Position - _mapTile.Position).Normalize();
                        _controller.gameObject.SendMessageTo(setDirectionMsg, _controller.transform.parent.gameObject);
                        MessageFactory.CacheMessage(setDirectionMsg);
                    }
                }
                else
                {
                    var setAdventureAiStateMsg = MessageFactory.GenerateSetAdventureAiStateMsg();
                    setAdventureAiStateMsg.State = AdventureAiState.Wander;
                    _controller.gameObject.SendMessageTo(setAdventureAiStateMsg, _controller.transform.parent.gameObject);
                    MessageFactory.CacheMessage(setAdventureAiStateMsg);
                }
            }
        }

        private void UpdateAdventureAiState(UpdateAdventureAiStateMessage msg)
        {
            var prevState = _aiState;
            _aiState = msg.State;
            if (_aiState != AdventureAiState.Aggro && prevState == AdventureAiState.Aggro)
            {
                if (_path.Count > 0)
                {
                    _path.Clear();
                }

                if (_aggrodTraits.Length > 0)
                {
                    var removeTraitFromUnitByControllerMsg = MessageFactory.GenerateRemoveTraitFromUnitByControllerMsg();
                    foreach (var controller in _aggrodTraits)
                    {
                        removeTraitFromUnitByControllerMsg.Controller = controller;
                        _controller.gameObject.SendMessageTo(removeTraitFromUnitByControllerMsg, _controller.transform.parent.gameObject);
                    }
                    MessageFactory.CacheMessage(removeTraitFromUnitByControllerMsg);

                    _aggrodTraits = new TraitController[0];
                }

            }
            else if (_aiState == AdventureAiState.Aggro && prevState != AdventureAiState.Aggro)
            {
                if (_applyOnAggro.Length > 0)
                {
                    var controllers = new List<TraitController>();

                    var addTraitToUnitMsg = MessageFactory.GenerateAddTraitToUnitMsg();
                    foreach (var trait in _applyOnAggro)
                    {
                        addTraitToUnitMsg.Trait = trait;
                        if (trait.Instant)
                        {
                            addTraitToUnitMsg.DoAfter = controller => { };
                        }
                        else
                        {
                            addTraitToUnitMsg.DoAfter = controller => { controllers.Add(controller); };
                        }
                        _controller.gameObject.SendMessageTo(addTraitToUnitMsg, _controller.transform.parent.gameObject);

                    }
                    MessageFactory.CacheMessage(addTraitToUnitMsg);

                    _aggrodTraits = controllers.ToArray();
                }

                
            }
            _exclamationController.gameObject.SetActive(_aiState == AdventureAiState.Aggro);
        }

        private void UpdateMapTile(UpdateMapTileMessage msg)
        {
            _mapTile = msg.Tile;
            _aggroPositions = WorldAdventureController.MapController.MonsterPathing.GetTilesInPov(_mapTile.Position, _aggroRange).Select(t => t.Position).ToList();
            _leashPositions = WorldAdventureController.MapController.MonsterPathing.GetMapTilesInArea(_mapTile.Position, _leashRange).Select(t => t.Position).ToList();
            if (_unitState != AdventureUnitState.Disabled)
            {
                if (_aiState == AdventureAiState.Aggro)
                {
                    if (!InLeashRange(WorldAdventureController.PlayerTile.Position))
                    {
                        _path.Clear();

                        var setDirectionMsg = MessageFactory.GenerateSetDirectionMsg();
                        setDirectionMsg.Direction = Vector2.zero;
                        _controller.gameObject.SendMessageTo(setDirectionMsg, _controller.transform.parent.gameObject);
                        MessageFactory.CacheMessage(setDirectionMsg);

                        var setAdventureAiStateMsg = MessageFactory.GenerateSetAdventureAiStateMsg();
                        setAdventureAiStateMsg.State = AdventureAiState.Wander;
                        _controller.gameObject.SendMessageTo(setAdventureAiStateMsg, _controller.transform.parent.gameObject);
                        MessageFactory.CacheMessage(setAdventureAiStateMsg);
                    }
                    else if (_path.Count > 0)
                    {
                        if (_path[0] == _mapTile)
                        {
                            _path.RemoveAt(0);
                        }

                        var setDirectionMsg = MessageFactory.GenerateSetDirectionMsg();
                        setDirectionMsg.Direction = _path.Count > 0 ? (_path[0].Position - _mapTile.Position).Normalize() : Vector2.zero;
                        _controller.gameObject.SendMessageTo(setDirectionMsg, _controller.transform.parent.gameObject);
                        MessageFactory.CacheMessage(setDirectionMsg);
                    }
                }
                else if (InAggroRange(WorldAdventureController.PlayerTile.Position))
                {
                    var setAdventureAiStateMsg = MessageFactory.GenerateSetAdventureAiStateMsg();
                    setAdventureAiStateMsg.State = AdventureAiState.Aggro;
                    _controller.gameObject.SendMessageTo(setAdventureAiStateMsg, _controller.transform.parent.gameObject);
                    MessageFactory.CacheMessage(setAdventureAiStateMsg);

                }
            }

        }

        private void UpdatePlayerMapTile(UpdateMapTileMessage msg)
        {
            if (WorldAdventureController.MapController.MonsterPathing.DoesTileExist(msg.Tile.Position))
            {
                if (_aiState != AdventureAiState.Aggro)
                {
                    if (InAggroRange(msg.Tile.Position))
                    {
                        var setAdventureAiStateMsg = MessageFactory.GenerateSetAdventureAiStateMsg();
                        setAdventureAiStateMsg.State = AdventureAiState.Aggro;
                        _controller.gameObject.SendMessageTo(setAdventureAiStateMsg, _controller.transform.parent.gameObject);
                        MessageFactory.CacheMessage(setAdventureAiStateMsg);
                    }
                }
                else if (!InLeashRange(msg.Tile.Position))
                {
                    if (_aiState == AdventureAiState.Aggro)
                    {
                        var setAdventureAiStateMsg = MessageFactory.GenerateSetAdventureAiStateMsg();
                        setAdventureAiStateMsg.State = AdventureAiState.Wander;
                        _controller.gameObject.SendMessageTo(setAdventureAiStateMsg, _controller.transform.parent.gameObject);
                        MessageFactory.CacheMessage(setAdventureAiStateMsg);
                    }
                }
                else if (_path.Count > 0)
                {
                    var diff = msg.Tile.Position - _mapTile.Position;
                    var playerDirection = _diagonalCost < 0 ? diff.ToCardinal() : diff.Normalize();
                    if (_direction != playerDirection)
                    {
                        _path.Clear();
                        var setDirectionMsg = MessageFactory.GenerateSetDirectionMsg();
                        setDirectionMsg.Direction = Vector2.zero;
                        _controller.gameObject.SendMessageTo(setDirectionMsg, _controller.transform.parent.gameObject);
                        MessageFactory.CacheMessage(setDirectionMsg);
                    }
                    else
                    {
                        var tileIndex = _path.IndexOf(msg.Tile);
                        if (tileIndex >= 0)
                        {
                            if (_path.Count > 1)
                            {
                                _path.RemoveRange(tileIndex + 1, _path.Count - tileIndex - 1);
                            }
                        }
                        else
                        {
                            _path.Clear();
                            var setDirectionMsg = MessageFactory.GenerateSetDirectionMsg();
                            setDirectionMsg.Direction = Vector2.zero;
                            _controller.gameObject.SendMessageTo(setDirectionMsg, _controller.transform.parent.gameObject);
                            MessageFactory.CacheMessage(setDirectionMsg);
                        }
                    }

                    //else if (_path.Count > _leashRange)
                    //{
                    //    _path.Clear();

                    //    var setDirectionMsg = MessageFactory.GenerateSetDirectionMsg();
                    //    setDirectionMsg.Direction = Vector2.zero;
                    //    _controller.gameObject.SendMessageTo(setDirectionMsg, _controller.transform.parent.gameObject);
                    //    MessageFactory.CacheMessage(setDirectionMsg);
                    //}
                    //else if (_mapTile != msg.Tile)
                    //{
                    //    var tilePath = WorldAdventureController.MapController.MonsterPathing.GetPath(_path[_path.Count - 1].Position, msg.Tile.Position, _diagonalCost);
                    //    _path.AddRange(tilePath);
                    //}
                }
            }
            else if (_aiState == AdventureAiState.Aggro)
            {
                var setAdventureAiStateMsg = MessageFactory.GenerateSetAdventureAiStateMsg();
                setAdventureAiStateMsg.State = AdventureAiState.Wander;
                _controller.gameObject.SendMessageTo(setAdventureAiStateMsg, _controller.transform.parent.gameObject);
                MessageFactory.CacheMessage(setAdventureAiStateMsg);
            }

        }

        private void UpdateAdventureUnitState(UpdateAdventureUnitStateMessage msg)
        {
            var prevState = _unitState;
            _unitState = msg.State;
            if (_unitState == AdventureUnitState.Disabled)
            {
                _controller.gameObject.Unsubscribe<UpdateTickMessage>();
                WorldAdventureController.Player.gameObject.UnsubscribeFromAllMessagesWithFilter(_instanceId);
            }
            else if (prevState == AdventureUnitState.Disabled)
            {
                _controller.gameObject.Subscribe<UpdateTickMessage>(UpdateTick);
            }
        }

        private void Obstacle(ObstacleMessage msg)
        {
            if (_aiState == AdventureAiState.Aggro && msg.Obstacle != WorldAdventureController.Player.gameObject)
            {
                _path.Clear();
            }
        }

        private void UpdateDirection(UpdateDirectionMessage msg)
        {
            _direction = msg.Direction.ToVector2Int();
        }

        public override void Destroy()
        {
            if (_unitState != AdventureUnitState.Disabled)
            {
                WorldAdventureController.Player?.gameObject.UnsubscribeFromAllMessagesWithFilter(_instanceId);
            }
            _path.Clear();
            _aggroPositions.Clear();
            _leashPositions.Clear();
            _mapTile = null;
            base.Destroy();
        }
    }
}
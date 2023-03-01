using System;
using System.Collections.Generic;
using System.Linq;
using Assets.Ancible_Tools.Scripts.System.SaveData.Building;
using Assets.Ancible_Tools.Scripts.System.WorldNodes;
using Assets.Resources.Ancible_Tools.Scripts.System;
using Assets.Resources.Ancible_Tools.Scripts.System.Animation;
using Assets.Resources.Ancible_Tools.Scripts.System.Building;
using Assets.Resources.Ancible_Tools.Scripts.System.Items;
using Assets.Resources.Ancible_Tools.Scripts.System.Pathing;
using Assets.Resources.Ancible_Tools.Scripts.System.SaveData;
using Assets.Resources.Ancible_Tools.Scripts.System.TickTimers;
using Assets.Resources.Ancible_Tools.Scripts.System.UI;
using Assets.Resources.Ancible_Tools.Scripts.System.UnitCommands;
using MessageBusLib;
using UnityEngine;

namespace Assets.Ancible_Tools.Scripts.Traits
{
    [CreateAssetMenu(fileName = "World Node Trait", menuName = "Ancible Tools/Traits/Node/World Node")]
    public class WorldNodeTrait : Trait
    {
        [SerializeField] protected internal WorldNodeType _nodeType = WorldNodeType.Food;
        [SerializeField] protected internal int _requiredTicks = 1;
        [SerializeField] private Trait[] _applyOnFinish = new Trait[0];
        [SerializeField] protected internal int _stack = 0;
        [SerializeField] private Vector2Int[] _relativeGatheringPositions = new Vector2Int[0];
        [SerializeField] private NodeInteractionType _interactionType = NodeInteractionType.Bump;
        [SerializeField] private SpriteTrait _nodeSprite = null;
        [SerializeField] private int _unitsPerTile = 1;
        [SerializeField] private int _refillCost = 0;
        [SerializeField] private bool _autoRefill = false;
        [SerializeField] private int _autoRefillCost = 0;
        [SerializeField] private bool _destroyOnEmpty = false;

        protected internal MapTile _mapTile = null;
        protected internal RegisteredWorldNode _registeredNode = null;
        protected internal bool _autoRefillEnabled = false;

        protected internal int _currentStack = 0;
        private int _unitLimit = 0;
        private List<MapTile> _gatheringTiles = new List<MapTile>();
        
        private NodeBuildingParamaterData _data = null;
        
        private NodeBuildingParamaterData _nodeData = null;

        private CommandInstance _refillCommandInstance = null;
        private CommandInstance _autoRefillOnInstance = null;
        private CommandInstance _autoRefillOffInstance = null;

        protected internal SpriteController _nodeSpriteController = null;

        private Dictionary<GameObject, MapTile> _interactingHobblers = new Dictionary<GameObject, MapTile>();

        private TickTimer _goldCheckTimer = null;

        public override void SetupController(TraitController controller)
        {
            base.SetupController(controller);
            _currentStack = _stack;
            if (!_destroyOnEmpty)
            {
                var refillCommand = Instantiate(WorldNodeManager.RefillCommand, _controller.transform);
                refillCommand.GoldValue = _refillCost;
                _refillCommandInstance = refillCommand.GenerateInstance();

                if (_autoRefill)
                {
                    var autoRefillOnCommand = Instantiate(WorldNodeManager.AutoRefillCommand_On, _controller.transform);
                    autoRefillOnCommand.GoldValue = _autoRefillCost;
                    _autoRefillOnInstance = autoRefillOnCommand.GenerateInstance();
                    _autoRefillOffInstance = WorldNodeManager.AutoRefillCommand_Off.GenerateInstance();
                }
            }



            _nodeSpriteController = Instantiate(FactoryController.SPRITE_CONTROLLER, _controller.transform.parent);
            RefreshNodeSprite(true);
            SubscribeToMessages();
        }

        protected internal virtual void RegisterNode(MapTile mapTile)
        {
            _registeredNode = WorldNodeManager.RegisterNode(_controller.transform.parent.gameObject, mapTile, _nodeType);
        }

        protected internal virtual void UnregisterNode()
        {
            WorldNodeManager.UnregisterNode(_controller.transform.parent.gameObject, _nodeType);
            _registeredNode = null;
        }

        protected internal virtual void RefreshNodeSprite(bool refreshTrait)
        {
            if (refreshTrait)
            {
                if (_nodeSprite)
                {
                    _nodeSpriteController.SetSprite(_nodeSprite.Sprite);
                    _nodeSpriteController.SetOffset(_nodeSprite.Offset);
                    _nodeSpriteController.SetScale(_nodeSprite.Scaling);
                    _nodeSpriteController.SetColorMask(_nodeSprite.ColorMask);
                    _nodeSpriteController.FlipX(_nodeSprite.FlipX);
                    _nodeSpriteController.FlipX(_nodeSprite.FlipY);
                }
            }
            _nodeSpriteController.gameObject.SetActive(_nodeSprite && (_stack <= 0 || _currentStack > 0));
        }

        protected internal virtual bool FinishGatheringCheck(GameObject obj)
        {
            if (_stack > 0 && _currentStack <= 0)
            {
                if (_autoRefillEnabled)
                {
                    RefillNode(true);
                }
                //We refill the node so we nee to check and make sure it's stack got refilled - may not have enough gold
                if (_currentStack <= 0)
                {
                    //The refill didn't work, so let's see if we should check for gold or not
                    if (_autoRefillEnabled && _goldCheckTimer == null)
                    {
                        _goldCheckTimer = new TickTimer(WorldNodeManager.GoldCheckTicks, -1, GoldCheck, null);
                    }
                    var stopGatheringMsg = MessageFactory.GenerateStopGatheringMsg();
                    stopGatheringMsg.Node = _controller.transform.parent.gameObject;
                    _controller.gameObject.SendMessageTo(stopGatheringMsg, obj);
                    MessageFactory.CacheMessage(stopGatheringMsg);

                    RemoveFromInteractingObjects(obj);

                    if (_registeredNode != null)
                    {
                        UnregisterNode();
                    }

                    if (_destroyOnEmpty)
                    {
                        WorldBuildingManager.RemoveBuilding(_controller.transform.parent.gameObject);
                    }
                    return true;
                }
                else
                {
                    return false;
                }

                
            }

            if (_controller && (_stack <= 0 || _currentStack > 0))
            {
                return false;
            }
            else
            {
                return true;
            }
        }

        protected internal virtual void ApplyToUnit(GameObject obj)
        {
            if (_stack <= 0 || _currentStack > 0)
            {                
                var addTraitToUnitMsg = MessageFactory.GenerateAddTraitToUnitMsg();
                for (var i = 0; i < _applyOnFinish.Length; i++)
                {
                    addTraitToUnitMsg.Trait = _applyOnFinish[i];
                    _controller.gameObject.SendMessageTo(addTraitToUnitMsg, obj);
                }
                MessageFactory.CacheMessage(addTraitToUnitMsg);

                _currentStack--;
                RefreshNodeSprite(true);
                _controller.gameObject.SendMessageTo(RefreshUnitMessage.INSTANCE, _controller.transform.parent.gameObject);
            }
        }
        
        protected internal virtual void RefillNode(bool auto)
        {
            if (_stack > 0)
            {
                var cost = auto ? _autoRefillCost : _refillCost;
                if (WorldStashController.Gold >= cost)
                {
                    _currentStack = _stack;
                    if (_registeredNode == null)
                    {
                        RegisterNode(_mapTile);
                        RefreshNodeSprite(false);
                    }
                    _goldCheckTimer?.Destroy();
                    _goldCheckTimer = null;
                    WorldStashController.RemoveGold(cost);
                    _controller.gameObject.SendMessageTo(RefreshUnitMessage.INSTANCE, _controller.transform.parent.gameObject);
                }
                else if (!auto)
                {
                    UiOverlayTextManager.ShowOverlayAlert("Not enough gold", ColorFactoryController.ErrorAlertText);
                }

            }
        }

        private void FinishGathering(GameObject obj)
        {
            if (_currentStack > 0 || _stack <= 0)
            {
                switch (_interactionType)
                {
                    case NodeInteractionType.Invisible:
                        ApplyToUnit(obj);
                        if (FinishGatheringCheck(obj))
                        {
                            var setSpriteVisibilityMsg = MessageFactory.GenerateSetSpriteVisibilityMsg();
                            setSpriteVisibilityMsg.Visible = true;
                            _controller.gameObject.SendMessageTo(setSpriteVisibilityMsg, obj);
                            MessageFactory.CacheMessage(setSpriteVisibilityMsg);

                            var setActiveSelectableStateMsg = MessageFactory.GenerateSetActiveSelectableStateMsg();
                            setActiveSelectableStateMsg.Selectable = true;
                            _controller.gameObject.SendMessageTo(setActiveSelectableStateMsg, obj);
                            MessageFactory.CacheMessage(setActiveSelectableStateMsg);

                            
                        }
                        return;
                    case NodeInteractionType.Bump:
                        var doBumpMsg = MessageFactory.GenerateDoBumpMsg();
                        doBumpMsg.OnBump = () => { ApplyToUnit(obj); };
                        doBumpMsg.DoAfter = () => { FinishGatheringCheck(obj); };
                        doBumpMsg.Direction = (_controller.transform.parent.position.ToVector2() - obj.transform.position.ToVector2()).normalized;
                        _controller.gameObject.SendMessageTo(doBumpMsg, obj);
                        MessageFactory.CacheMessage(doBumpMsg);
                        return;
                    default:
                        ApplyToUnit(obj);
                        FinishGatheringCheck(obj);
                        return;
                }
            }
            if (_currentStack <= 0 && _stack > 0)
            {
                var stopGatheringMsg = MessageFactory.GenerateStopGatheringMsg();
                stopGatheringMsg.Node = _controller.transform.parent.gameObject;
                _controller.gameObject.SendMessageTo(stopGatheringMsg, obj);
                MessageFactory.CacheMessage(stopGatheringMsg);

                if (_registeredNode != null)
                {
                    UnregisterNode();
                }
            }

        }

        protected internal void RemoveFromInteractingObjects(GameObject owner)
        {
            if (_interactingHobblers.TryGetValue(owner, out var tile))
            {
                _interactingHobblers.Remove(owner);
                _gatheringTiles.Add(tile);
                if (_registeredNode == null && _gatheringTiles.Count > 0 && (_stack <= 0 || _currentStack > 0))
                {
                    RegisterNode(_mapTile);
                }
                _controller.gameObject.SendMessageTo(RefreshUnitMessage.INSTANCE, _controller.transform.parent.gameObject);
            }

        }

        protected internal virtual int GetRequiredTicks(GameObject owner)
        {
            return _requiredTicks;
        }

        private void GoldCheck()
        {
            if (WorldStashController.Gold >= _autoRefillCost)
            {
                _goldCheckTimer?.Destroy();
                _goldCheckTimer = null;
                RefillNode(true);
            }
        }

        protected internal virtual void SubscribeToMessages()
        {
            if (_stack > 0)
            {
                _controller.transform.parent.gameObject.SubscribeWithFilter<UpdateBuildingIdMessage>(UpdateBuildingId, _instanceId);
                _controller.transform.parent.gameObject.SubscribeWithFilter<QueryBuildingParamterDataMessage>(QueryBuildingParameterData, _instanceId);
                if (!_destroyOnEmpty)
                {
                    _controller.transform.parent.gameObject.SubscribeWithFilter<RefillNodeStacksMessage>(RefillNodeStacks, _instanceId);
                    _controller.transform.parent.gameObject.SubscribeWithFilter<QueryCommandsMessage>(QueryCommands, _instanceId);
                    if (_autoRefill)
                    {
                        _controller.transform.parent.gameObject.SubscribeWithFilter<SetNodeAutoRefillStateMessage>(SetNodeAutoRefillState, _instanceId);
                    }
                }
            }

            _controller.transform.parent.gameObject.SubscribeWithFilter<QueryNodeMessage>(QueryNode, _instanceId);
            _controller.transform.parent.gameObject.SubscribeWithFilter<UpdateMapTileMessage>(UpdateMapTile, _instanceId);
            _controller.transform.parent.gameObject.SubscribeWithFilter<InteractMessage>(Interact, _instanceId);
            _controller.transform.parent.gameObject.SubscribeWithFilter<UnregisterFromGatheringNodeMessage>(UnregisterFromNode, _instanceId);

        }

        protected internal virtual void UpdateMapTile(UpdateMapTileMessage msg)
        {
            _mapTile = msg.Tile;
            _gatheringTiles = _relativeGatheringPositions.Select(p => WorldController.Pathing.GetTileByPosition(p + _mapTile.Position)).Where(t => t != null).ToList();
            _unitLimit = _gatheringTiles.Count * _unitsPerTile;
            if (_gatheringTiles.Count > 0 && (_stack <= 0 || _currentStack > 0) && _interactingHobblers.Count < _unitLimit)
            {
                if (_registeredNode == null)
                {
                    RegisterNode(_mapTile);
                    RefreshNodeSprite(false);
                }
                else
                {
                    _registeredNode.Tile = _mapTile;
                }
                _nodeSpriteController.SetSortingOrder((_mapTile.Position.y - 1) * -1 );
            }
            else if (_registeredNode != null)
            {
                UnregisterNode();
                
            }

            
            var hobblers = _interactingHobblers.ToArray();
            if (_interactionType != NodeInteractionType.Invisible)
            {
                var stopGatheringMsg = MessageFactory.GenerateStopGatheringMsg();
                stopGatheringMsg.Node = _controller.transform.parent.gameObject;
                foreach (var hobbler in hobblers)
                {
                    _controller.gameObject.SendMessageTo(stopGatheringMsg, hobbler.Key);
                }
                MessageFactory.CacheMessage(stopGatheringMsg);

                _interactingHobblers.Clear();
            }
            else
            {
                var setMapTileMsg = MessageFactory.GenerateSetMapTileMsg();
                foreach (var hobbler in hobblers)
                {
                    var tile = _gatheringTiles.GetRandom();
                    setMapTileMsg.Tile = tile;
                    _controller.gameObject.SendMessageTo(setMapTileMsg, hobbler.Key);
                    _interactingHobblers[hobbler.Key] = tile;
                }
                MessageFactory.CacheMessage(setMapTileMsg);
            }
        }

        private void Interact(InteractMessage msg)
        {
            if (_gatheringTiles.Count > 0)
            {
                var gatherMsg = MessageFactory.GenerateGatherMsg();
                gatherMsg.Node = _controller.transform.parent.gameObject;
                gatherMsg.NodeType = _nodeType;
                gatherMsg.DoAfter = FinishGathering;
                gatherMsg.Ticks = GetRequiredTicks(msg.Owner);
                var tile = _gatheringTiles.GetRandom();
                if (_unitsPerTile == 1)
                {
                    _gatheringTiles.Remove(tile);
                }
                else if (_interactingHobblers.Values.Count(t => t == tile) + 1 >= _unitsPerTile)
                {
                    _gatheringTiles.Remove(tile);
                }
                gatherMsg.GatheringTile = tile;
                gatherMsg.Invisible = _interactionType == NodeInteractionType.Invisible;
                _controller.gameObject.SendMessageTo(gatherMsg, msg.Owner);
                MessageFactory.CacheMessage(gatherMsg);

                if (_interactingHobblers.TryGetValue(msg.Owner, out var mapTile))
                {
                    _gatheringTiles.Add(mapTile);
                    _interactingHobblers[msg.Owner] = tile;
                }
                else
                {
                    _interactingHobblers.Add(msg.Owner, tile);
                }
                if (_registeredNode != null && _gatheringTiles.Count <= 0)
                {
                    UnregisterNode();
                }
            }
            else if (_registeredNode != null)
            {
                UnregisterNode();
            }

        }

        private void RefillNodeStacks(RefillNodeStacksMessage msg)
        {
            RefillNode(false);
        }

        private void UnregisterFromNode(UnregisterFromGatheringNodeMessage msg)
        {
            RemoveFromInteractingObjects(msg.Unit);
            _controller.gameObject.SendMessageTo(RefreshUnitMessage.INSTANCE, _controller.transform.parent.gameObject);
        }

        private void QueryNode(QueryNodeMessage msg)
        {
            msg.DoAfter.Invoke(_currentStack, _stack, _interactingHobblers.Keys.ToArray());
        }

        protected internal virtual void UpdateBuildingId(UpdateBuildingIdMessage msg)
        {
            var data = PlayerDataController.GetBuildingData(msg.Id);
            if (data != null && data.Parameter is NodeBuildingParamaterData nodeData)
            {
                _currentStack = nodeData.Stack;
                if (_autoRefill)
                {
                    _autoRefillEnabled = nodeData.AutoRefillEnabled;
                }

                if (_currentStack <= 0 && _autoRefillEnabled)
                {
                    RefillNode(true);
                    if (_currentStack <= 0 && _autoRefillEnabled && _goldCheckTimer == null)
                    {
                        _goldCheckTimer = new TickTimer(WorldNodeManager.GoldCheckTicks, -1, GoldCheck, null);
                    }
                }
                RefreshNodeSprite(false);
            }
        }

        protected internal virtual void QueryBuildingParameterData(QueryBuildingParamterDataMessage msg)
        {
            if (_nodeData == null)
            {
                _nodeData = new NodeBuildingParamaterData();
            }
            _nodeData.Stack = _currentStack;
            _nodeData.AutoRefillEnabled = _autoRefill && _autoRefillEnabled;
            msg.DoAfter.Invoke(_nodeData);
        }

        protected internal virtual void QueryCommands(QueryCommandsMessage msg)
        {
            var commands = new List<CommandInstance>{_refillCommandInstance};
            if (_autoRefill)
            {
                commands.Add(_autoRefillEnabled ? _autoRefillOffInstance : _autoRefillOnInstance);
            }
            msg.DoAfter.Invoke(commands.ToArray());
        }

        protected internal virtual void SetNodeAutoRefillState(SetNodeAutoRefillStateMessage msg)
        {
            _autoRefillEnabled = msg.AutoRefill;
            if (_autoRefillEnabled && _currentStack <= 0)
            {
                RefillNode(true);
            }
            _controller.gameObject.SendMessageTo(ResetCommandCardMessage.INSTANCE, _controller.transform.parent.gameObject);
        }

        public override void Destroy()
        {
            var hobblers = _interactingHobblers.ToArray();
            var stopGatheringMsg = MessageFactory.GenerateStopGatheringMsg();
            stopGatheringMsg.Node = _controller.transform.parent.gameObject;
            foreach (var hobbler in hobblers)
            {
                _controller.gameObject.SendMessageTo(stopGatheringMsg, hobbler.Key);
            }
            MessageFactory.CacheMessage(stopGatheringMsg);
            _interactingHobblers.Clear();
            _refillCommandInstance?.Destroy(true);
            _refillCommandInstance = null;

            if (_autoRefill)
            {
                _autoRefillOnInstance.Destroy(true);
                _autoRefillOnInstance = null;
                _autoRefillOffInstance.Destroy();
                _autoRefillOffInstance = null;
            }
            base.Destroy();
        }
    }
}
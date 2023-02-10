using System;
using System.Collections.Generic;
using System.Linq;
using Assets.Ancible_Tools.Scripts.System.SaveData;
using Assets.Ancible_Tools.Scripts.System.SaveData.Building;
using Assets.Ancible_Tools.Scripts.System.WorldNodes;
using Assets.Resources.Ancible_Tools.Scripts.System;
using Assets.Resources.Ancible_Tools.Scripts.System.Animation;
using Assets.Resources.Ancible_Tools.Scripts.System.Pathing;
using Assets.Resources.Ancible_Tools.Scripts.System.SaveData;
using Assets.Resources.Ancible_Tools.Scripts.System.UnitCommands;
using MessageBusLib;
using UnityEngine;
using Random = UnityEngine.Random;

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
        [SerializeField] private UnitCommand _stopSelectCommand = null;
        [SerializeField] private UnitCommand _selectableHobblerCommandTemplate = null;
        

        private int _currentStack = 0;
        protected internal RegisteredWorldNode _registeredNode = null;
        private MapTile[] _gatheringTiles = new MapTile[0];
        protected internal MapTile _mapTile = null;
        private string _buildingId = string.Empty;
        private NodeBuildingParamaterData _data = null;

        
        private NodeBuildingParamaterData _nodeData = null;

        protected internal SpriteController _nodeSpriteController = null;

        private CommandInstance _nodeCommandInstance = null;

        private Dictionary<GameObject, CommandInstance> _interactingHobblers = new Dictionary<GameObject, CommandInstance>();

        public override void SetupController(TraitController controller)
        {
            base.SetupController(controller);
            _currentStack = _stack;
            if (_interactionType == NodeInteractionType.Invisible)
            {
                _nodeCommandInstance = _stopSelectCommand.GenerateInstance();
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
            RefreshNodeSprite(false);
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
            _nodeSpriteController.gameObject.SetActive(_nodeSprite && _registeredNode != null);
        }

        protected internal virtual bool FinishGatheringCheck(GameObject obj)
        {
            if (_currentStack == 0)
            {
                var stopGatheringMsg = MessageFactory.GenerateStopGatheringMsg();
                stopGatheringMsg.Node = _controller.transform.parent.gameObject;
                _controller.gameObject.SendMessageTo(stopGatheringMsg, obj);
                MessageFactory.CacheMessage(stopGatheringMsg);

                if (_registeredNode != null)
                {
                    UnregisterNode();
                }

                return true;
            }

            return false;
        }

        protected internal virtual void ApplyToUnit(GameObject obj)
        {
            var addTraitToUnitMsg = MessageFactory.GenerateAddTraitToUnitMsg();
            for (var i = 0; i < _applyOnFinish.Length; i++)
            {
                addTraitToUnitMsg.Trait = _applyOnFinish[i];
                _controller.gameObject.SendMessageTo(addTraitToUnitMsg, obj);
            }
            MessageFactory.CacheMessage(addTraitToUnitMsg);
            if (_currentStack > 0 && _stack > 0)
            {
                _currentStack--;
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

                            RemoveFromInteractingObjects(obj);
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

        private void RemoveFromInteractingObjects(GameObject owner)
        {
            if (_interactingHobblers.ContainsKey(owner))
            {
                if (_interactionType == NodeInteractionType.Invisible)
                {
                    var commandInstance = _interactingHobblers[owner];
                    _nodeCommandInstance.Tree.SubCommands.Remove(commandInstance);
                    commandInstance.Command.Destroy();
                    Destroy(commandInstance.Command);
                }
                _interactingHobblers.Remove(owner);
                _controller.gameObject.SendMessageTo(RefreshUnitMessage.INSTANCE, _controller.transform.parent.gameObject);
            }

        }

        protected internal virtual int GetRequiredTicks(GameObject owner)
        {
            return _requiredTicks;
        }

        protected internal virtual void SubscribeToMessages()
        {
            if (_stack > 0)
            {
                _controller.gameObject.Subscribe<QueryNodeMessage>(QueryNode);
                _controller.gameObject.Subscribe<LoadWorldDataMessage>(LoadWorldData);
                _controller.transform.parent.gameObject.SubscribeWithFilter<UpdateBuildingIdMessage>(UpdateBuildingId, _instanceId);
                _controller.transform.parent.gameObject.SubscribeWithFilter<QueryBuildingParamterDataMessage>(QueryBuildingParameterData, _instanceId);
            }
            
            _controller.transform.parent.gameObject.SubscribeWithFilter<UpdateMapTileMessage>(UpdateMapTile, _instanceId);
            _controller.transform.parent.gameObject.SubscribeWithFilter<InteractMessage>(Interact, _instanceId);
            _controller.transform.parent.gameObject.SubscribeWithFilter<RefillNodeStacksMessage>(RefillNodeStacks, _instanceId);
            if (_interactionType == NodeInteractionType.Invisible || _nodeType == WorldNodeType.Crafting)
            {
                _controller.transform.parent.gameObject.SubscribeWithFilter<QueryCommandsMessage>(QueryCommands, _instanceId);
            }
            _controller.transform.parent.gameObject.SubscribeWithFilter<UnregisterFromGatheringNodeMessage>(UnregisterFromNode, _instanceId);

        }

        protected internal virtual void UpdateMapTile(UpdateMapTileMessage msg)
        {
            _mapTile = msg.Tile;
            
            _gatheringTiles = _relativeGatheringPositions.Select(p => WorldController.Pathing.GetTileByPosition(p + _mapTile.Position)).Where(t => t != null).ToArray();
            if (_gatheringTiles.Length > 0)
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

            var hobblers = _interactingHobblers.Values.ToArray();
            if (_interactionType != NodeInteractionType.Invisible)
            {
                var stopGatheringMsg = MessageFactory.GenerateStopGatheringMsg();
                stopGatheringMsg.Node = _controller.transform.parent.gameObject;
                foreach (var hobbler in hobblers)
                {
                    _controller.gameObject.SendMessageTo(stopGatheringMsg, hobbler);
                }
                MessageFactory.CacheMessage(stopGatheringMsg);

                _interactingHobblers.Clear();
            }
            else
            {
                var setMapTileMsg = MessageFactory.GenerateSetMapTileMsg();
                foreach (var hobbler in hobblers)
                {
                    setMapTileMsg.Tile = _gatheringTiles.Length > 1 ? _gatheringTiles[Random.Range(0, _gatheringTiles.Length)] : _gatheringTiles[0];
                    _controller.gameObject.SendMessageTo(setMapTileMsg, hobbler);
                }
                MessageFactory.CacheMessage(setMapTileMsg);
            }
        }

        private void Interact(InteractMessage msg)
        {
            var gatherMsg = MessageFactory.GenerateGatherMsg();
            gatherMsg.Node = _controller.transform.parent.gameObject;
            gatherMsg.NodeType = _nodeType;
            gatherMsg.DoAfter = FinishGathering;
            gatherMsg.Ticks = GetRequiredTicks(msg.Owner);
            gatherMsg.GatheringTile = _gatheringTiles.Length > 1 ? _gatheringTiles[Random.Range(0, _gatheringTiles.Length)] : _gatheringTiles[0];
            gatherMsg.Invisible = _interactionType == NodeInteractionType.Invisible;
            _controller.gameObject.SendMessageTo(gatherMsg, msg.Owner);
            MessageFactory.CacheMessage(gatherMsg);

            if (_interactionType == NodeInteractionType.Invisible)
            {
                SpriteTrait spriteTrait = null;
                var querySpriteMsg = MessageFactory.GenerateQuerySpriteMsg();
                querySpriteMsg.DoAfter = trait => spriteTrait = trait;
                _controller.gameObject.SendMessageTo(querySpriteMsg, msg.Owner);
                MessageFactory.CacheMessage(querySpriteMsg);
                if (!_interactingHobblers.ContainsKey(msg.Owner))
                {
                    var owner = msg.Owner;
                    var unitCommand = Instantiate(_selectableHobblerCommandTemplate, _controller.transform);
                    var queryNameMsg = MessageFactory.GenerateQueryUnitNameMsg();
                    queryNameMsg.DoAfter = unitName => unitCommand.Command = unitName;
                    _controller.gameObject.SendMessageTo(queryNameMsg, owner);
                    MessageFactory.CacheMessage(queryNameMsg);
                    //unitCommand.Command = "Hobbler";
                    unitCommand.Icons = new[]
                    {
                        new CommandIcon {Sprite = spriteTrait.Sprite}
                    };
                    unitCommand.DoAfter = () =>
                    {
                        UnitSelectController.SelectUnit(owner);
                    };

                    var instance = unitCommand.GenerateInstance();
                    _interactingHobblers.Add(msg.Owner, instance);
                    _nodeCommandInstance.Tree.SubCommands.Add(instance);
                    _controller.gameObject.SendMessageTo(ResetCommandCardMessage.INSTANCE, _controller.transform.parent.gameObject);
                    //_controller.gameObject.SendMessageTo(RefreshUnitMessage.INSTANCE, _controller.transform.parent.gameObject);
                }
            }
            else if (!_interactingHobblers.ContainsKey(msg.Owner))
            {
                _interactingHobblers.Add(msg.Owner, null);
            }

        }

        private void RefillNodeStacks(RefillNodeStacksMessage msg)
        {
            if (_stack > 0)
            {
                var max = Mathf.Min(msg.Max, _stack);
                _currentStack = max;
                if (_registeredNode == null)
                {
                    RegisterNode(_mapTile);
                    RefreshNodeSprite(false);
                }
            }
        }

        protected internal virtual void QueryCommands(QueryCommandsMessage msg)
        {
            msg.DoAfter.Invoke(new[] { _nodeCommandInstance });
        }

        private void UnregisterFromNode(UnregisterFromGatheringNodeMessage msg)
        {
            RemoveFromInteractingObjects(msg.Unit);
            var stopGatheringMsg = MessageFactory.GenerateStopGatheringMsg();
            stopGatheringMsg.Node = _controller.transform.parent.gameObject;
            _controller.gameObject.SendMessageTo(stopGatheringMsg, msg.Unit);
            MessageFactory.CacheMessage(stopGatheringMsg);
            //switch (_interactionType)
            //{
            //    case NodeInteractionType.Invisible when _interactingHobblers.ContainsKey(msg.Unit):
                    
            //        {
            //            var stopGatheringMsg = MessageFactory.GenerateStopGatheringMsg();
            //            stopGatheringMsg.Node = _controller.transform.parent.gameObject;
            //            _controller.gameObject.SendMessageTo(stopGatheringMsg, msg.Unit);
            //            MessageFactory.CacheMessage(stopGatheringMsg);
            //        }
            //        break;
            //    case NodeInteractionType.Invisible:
            //    {
            //        var stopGatheringMsg = MessageFactory.GenerateStopGatheringMsg();
            //        stopGatheringMsg.Node = _controller.transform.parent.gameObject;
            //        _controller.gameObject.SendMessageTo(stopGatheringMsg, msg.Unit);
            //        MessageFactory.CacheMessage(stopGatheringMsg);
            //        break;
            //    }
            //    default:
            //    {
            //        var stopGatheringMsg = MessageFactory.GenerateStopGatheringMsg();
            //        stopGatheringMsg.Node = _controller.transform.parent.gameObject;
            //        _controller.gameObject.SendMessageTo(stopGatheringMsg, msg.Unit);
            //        MessageFactory.CacheMessage(stopGatheringMsg);
            //        break;
            //    }
            //}
        }

        private void QueryNode(QueryNodeMessage msg)
        {
            msg.DoAfter.Invoke(_buildingId, _currentStack);
        }

        private void LoadWorldData(LoadWorldDataMessage msg)
        {
            //var nodeData = PlayerDataController.GetNodeDataById(_buildingId);
            //if (nodeData != null)
            //{
            //    _currentStack = nodeData.Stack;
            //    RefreshNodeSprite(false);
            //}
        }
        protected internal virtual void UpdateBuildingId(UpdateBuildingIdMessage msg)
        {
            _buildingId = msg.Id;
            var data = PlayerDataController.GetBuildingData(msg.Id);
            if (data != null && data.Parameter is NodeBuildingParamaterData nodeData)
            {
                _currentStack = nodeData.Stack;
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
            msg.DoAfter.Invoke(_nodeData);
        }

        public override void Destroy()
        {
            var keys = _interactingHobblers.Keys.ToArray();
            var stopGatheringMsg = MessageFactory.GenerateStopGatheringMsg();
            stopGatheringMsg.Node = _controller.transform.parent.gameObject;
            foreach (var key in keys)
            {
                RemoveFromInteractingObjects(key);
                _controller.gameObject.SendMessageTo(stopGatheringMsg, key);
            }
            MessageFactory.CacheMessage(stopGatheringMsg);
            _interactingHobblers.Clear();
            base.Destroy();
        }
    }
}
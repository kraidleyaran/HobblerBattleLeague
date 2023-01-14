using System.Collections.Generic;
using System.Linq;
using Assets.Ancible_Tools.Scripts.System.WorldNodes;
using Assets.Resources.Ancible_Tools.Scripts.System;
using Assets.Resources.Ancible_Tools.Scripts.System.Animation;
using Assets.Resources.Ancible_Tools.Scripts.System.Pathing;
using Assets.Resources.Ancible_Tools.Scripts.System.UnitCommands;
using MessageBusLib;
using UnityEngine;

namespace Assets.Ancible_Tools.Scripts.Traits
{
    [CreateAssetMenu(fileName = "World Node Trait", menuName = "Ancible Tools/Traits/Node/World Node")]
    public class WorldNodeTrait : Trait
    {
        [SerializeField] protected internal WorldNodeType _nodeType = WorldNodeType.Food;
        [SerializeField] private int _requiredTicks = 1;
        [SerializeField] private Trait[] _applyOnFinish = new Trait[0];
        [SerializeField] private int _stack = 0;
        [SerializeField] private int _gatherArea = 0;
        [SerializeField] private bool _includeNodeTileForGathering = false;
        [SerializeField] private NodeInteractionType _interactionType = NodeInteractionType.Bump;
        [SerializeField] private SpriteTrait _nodeSprite = null;
        [SerializeField] private UnitCommand _stopSelectCommand = null;
        [SerializeField] private UnitCommand _selectableHobblerCommandTemplate = null;

        private int _currentStack = 0;
        protected internal RegisteredWorldNode _registeredNode = null;
        private MapTile[] _gatheringTiles = new MapTile[0];
        private MapTile _mapTile = null;

        private SpriteController _nodeSpriteController = null;

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

        private void RefreshNodeSprite(bool refreshTrait)
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

        private bool FinishGatheringCheck(GameObject obj)
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
                var commandInstance = _interactingHobblers[owner];
                _interactingHobblers.Remove(owner);
                _nodeCommandInstance.Tree.SubCommands.Remove(commandInstance);
                commandInstance.Command.Destroy();
                Destroy(commandInstance.Command);
                _controller.gameObject.SendMessageTo(RefreshUnitMessage.INSTANCE, _controller.transform.parent.gameObject);
            }

        }

        private void SubscribeToMessages()
        {
            _controller.transform.parent.gameObject.SubscribeWithFilter<UpdateMapTileMessage>(UpdateMapTile, _instanceId);
            _controller.transform.parent.gameObject.SubscribeWithFilter<InteractMessage>(Interact, _instanceId);
            _controller.transform.parent.gameObject.SubscribeWithFilter<RefillNodeStacksMessage>(RefillNodeStacks, _instanceId);
            if (_interactionType == NodeInteractionType.Invisible)
            {
                _controller.transform.parent.gameObject.SubscribeWithFilter<QueryCommandsMessage>(QueryCommands, _instanceId);
            }
            _controller.transform.parent.gameObject.SubscribeWithFilter<UnregisterFromGatheringNodeMessage>(UnregisterFromNode, _instanceId);
        }

        private void UpdateMapTile(UpdateMapTileMessage msg)
        {
            _mapTile = msg.Tile;
            var gatheringTiles = WorldController.Pathing.GetMapTilesInArea(msg.Tile.Position, _gatherArea).Where(t => !t.Block).ToList();
            if (!_includeNodeTileForGathering)
            {
                gatheringTiles.Remove(msg.Tile);
            }
            _gatheringTiles = gatheringTiles.ToArray();
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
            }
            else if (_registeredNode != null)
            {
                UnregisterNode();
                
            }


        }

        private void Interact(InteractMessage msg)
        {
            var gatherMsg = MessageFactory.GenerateGatherMsg();
            gatherMsg.Node = _controller.transform.parent.gameObject;
            gatherMsg.NodeType = _nodeType;
            gatherMsg.DoAfter = FinishGathering;
            gatherMsg.Ticks = _requiredTicks;
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
                    unitCommand.Command = "Hobbler";
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

        private void QueryCommands(QueryCommandsMessage msg)
        {
            msg.DoAfter.Invoke(new[] { _nodeCommandInstance });
        }

        private void UnregisterFromNode(UnregisterFromGatheringNodeMessage msg)
        {
            switch (_interactionType)
            {
                case NodeInteractionType.Invisible when _interactingHobblers.ContainsKey(msg.Unit):
                    RemoveFromInteractingObjects(msg.Unit);
                    {
                        var stopGatheringMsg = MessageFactory.GenerateStopGatheringMsg();
                        stopGatheringMsg.Node = _controller.transform.parent.gameObject;
                        _controller.gameObject.SendMessageTo(stopGatheringMsg, msg.Unit);
                        MessageFactory.CacheMessage(stopGatheringMsg);
                    }
                    break;
                case NodeInteractionType.Invisible:
                {
                    var stopGatheringMsg = MessageFactory.GenerateStopGatheringMsg();
                    stopGatheringMsg.Node = _controller.transform.parent.gameObject;
                    _controller.gameObject.SendMessageTo(stopGatheringMsg, msg.Unit);
                    MessageFactory.CacheMessage(stopGatheringMsg);
                    break;
                }
                default:
                {
                    var stopGatheringMsg = MessageFactory.GenerateStopGatheringMsg();
                    stopGatheringMsg.Node = _controller.transform.parent.gameObject;
                    _controller.gameObject.SendMessageTo(stopGatheringMsg, msg.Unit);
                    MessageFactory.CacheMessage(stopGatheringMsg);
                    break;
                }
            }
        }
    }
}
using Assets.Ancible_Tools.Scripts.System.WorldNodes;
using Assets.Resources.Ancible_Tools.Scripts.System;
using Assets.Resources.Ancible_Tools.Scripts.System.Pathing;
using Assets.Resources.Ancible_Tools.Scripts.System.TickTimers;
using MessageBusLib;
using UnityEngine;

namespace Assets.Ancible_Tools.Scripts.Traits
{
    [CreateAssetMenu(fileName = "Gathering Trait", menuName = "Ancible Tools/Traits/Hobbler/Gathering")]
    public class GatheringTrait : Trait
    {
        private TickTimer _gatheringTimer = null;
        private WorldNodeType _currentNodeType = WorldNodeType.Food;
        private GameObject _currentNode = null;
        private MapTile _currentTile = null;
        private MapTile _gatheringTile = null;
        private MonsterState _monsterState = MonsterState.Idle;
        private bool _invisibleOnGather = false;

        public override void SetupController(TraitController controller)
        {
            base.SetupController(controller);
            SubscribeToMessags();
        }

        private void SubscribeToMessags()
        {
            _controller.transform.parent.gameObject.SubscribeWithFilter<SearchForNodeMessage>(SearchForNode, _instanceId);
            _controller.transform.parent.gameObject.SubscribeWithFilter<GatherMessage>(Gather, _instanceId);
            _controller.transform.parent.gameObject.SubscribeWithFilter<StopGatheringMessage>(StopGathering, _instanceId);
            _controller.transform.parent.gameObject.SubscribeWithFilter<UpdateMapTileMessage>(UpdateMapTile, _instanceId);
            _controller.transform.parent.gameObject.SubscribeWithFilter<UpdateMonsterStateMessage>(UpdateMonsterState, _instanceId);
            _controller.transform.parent.gameObject.SubscribeWithFilter<ObstacleMessage>(Obstacle, _instanceId);
            _controller.transform.parent.gameObject.SubscribeWithFilter<SearchForResourceNodeMessage>(SearchForResourceNode, _instanceId);
            _controller.transform.parent.gameObject.SubscribeWithFilter<SearchForCraftingNodeMessage>(SearchForCraftingNode, _instanceId);
        }

        private void SearchForNode(SearchForNodeMessage msg)
        {
            switch (msg.Type)
            {
                case WorldNodeType.Food:
                case WorldNodeType.Bed:
                case WorldNodeType.Book:
                    var node = WorldNodeManager.GetClosestNodeByType(_currentTile, msg.Type);
                    if (node != null)
                    {
                        var interactMsg = MessageFactory.GenerateInteractMsg();
                        interactMsg.Owner = _controller.transform.parent.gameObject;
                        _controller.gameObject.SendMessageTo(interactMsg, node.Unit);
                        MessageFactory.CacheMessage(interactMsg);
                        msg.DoAfter?.Invoke();
                    }
                    break;
                case WorldNodeType.Resource:
                case WorldNodeType.Activity:
                case WorldNodeType.Crafting:
                    var skillCheckMsg = MessageFactory.GenerateSkillCheckMsg();
                    skillCheckMsg.DoAfter = msg.DoAfter;
                    _controller.gameObject.SendMessageTo(skillCheckMsg, _controller.transform.parent.gameObject);
                    MessageFactory.CacheMessage(skillCheckMsg);
                    break;
            }
        }

        private void SearchForResourceNode(SearchForResourceNodeMessage msg)
        {
            var node = WorldNodeManager.GetClosestNodeByItem(_currentTile, msg.Item);
            if (node != null)
            {
                var interactMsg = MessageFactory.GenerateInteractMsg();
                interactMsg.Owner = _controller.transform.parent.gameObject;
                _controller.gameObject.SendMessageTo(interactMsg, node.Unit);
                MessageFactory.CacheMessage(interactMsg);
                msg.DoAfter?.Invoke();
            }
        }

        private void SearchForCraftingNode(SearchForCraftingNodeMessage msg)
        {
            var node = WorldNodeManager.GetCraftingNodeByPriorityAndSkill(msg.Skill);
            if (node != null)
            {
                var interactMsg = MessageFactory.GenerateInteractMsg();
                interactMsg.Owner = _controller.transform.parent.gameObject;
                _controller.gameObject.SendMessageTo(interactMsg, node.Unit);
                MessageFactory.CacheMessage(interactMsg);
                msg.DoAfter?.Invoke();
            }
        }

        private void Gather(GatherMessage msg)
        {
            if (_currentNode && msg.Node != _currentNode)
            {
                var unregisterFromNodeMsg = MessageFactory.GenerateUnregisterFromGatheringNodeMsg();
                unregisterFromNodeMsg.Unit = _controller.transform.parent.gameObject;
                _controller.gameObject.SendMessageTo(unregisterFromNodeMsg, _currentNode);
                MessageFactory.CacheMessage(unregisterFromNodeMsg);
            }
            if (_gatheringTimer != null)
            {
                _gatheringTimer.Destroy();
                if (_currentNodeType == WorldNodeType.Bed)
                {
                    var setSpriteVisibilityMsg = MessageFactory.GenerateSetSpriteVisibilityMsg();
                    setSpriteVisibilityMsg.Visible = true;
                    _controller.gameObject.SendMessageTo(setSpriteVisibilityMsg, _controller.transform.parent.gameObject);
                    MessageFactory.CacheMessage(setSpriteVisibilityMsg);

                    var setActiveSelectableStateMsg = MessageFactory.GenerateSetActiveSelectableStateMsg();
                    setActiveSelectableStateMsg.Selectable = true;
                    _controller.gameObject.SendMessageTo(setActiveSelectableStateMsg, _controller.transform.parent.gameObject);
                    MessageFactory.CacheMessage(setActiveSelectableStateMsg);
                }
                _currentNode = null;
            }

            var doAfter = msg.DoAfter;
            _currentNode = msg.Node;
            _currentNodeType = msg.NodeType;
            _invisibleOnGather = msg.Invisible;
            var setMonsterStateMsg = MessageFactory.GenerateSetMonsterStateMsg();
            switch (_currentNodeType)
            {
                case WorldNodeType.Food:
                    setMonsterStateMsg.State = MonsterState.Eating;
                    break;
                case WorldNodeType.Bed:
                    setMonsterStateMsg.State = MonsterState.Resting;
                    break;
                case WorldNodeType.Book:
                    setMonsterStateMsg.State = MonsterState.Studying;
                    break;
                case WorldNodeType.Crafting:
                case WorldNodeType.Resource:
                    setMonsterStateMsg.State = MonsterState.Gathering;
                    break;
            }

            _controller.gameObject.SendMessageTo(setMonsterStateMsg, _controller.transform.parent.gameObject);
            MessageFactory.CacheMessage(setMonsterStateMsg);
            _gatheringTimer = new TickTimer(msg.Ticks, -1, () => {doAfter.Invoke(_controller.transform.parent.gameObject);}, null, true, false);
            _gatheringTile = msg.GatheringTile;
            var path = WorldController.Pathing.GetPath(_currentTile.Position, _gatheringTile.Position, false);
            var setPathMsg = MessageFactory.GenerateSetPathMsg();
            setPathMsg.Path = path;
            _controller.gameObject.SendMessageTo(setPathMsg, _controller.transform.parent.gameObject);
            MessageFactory.CacheMessage(setPathMsg);
        }

        private void StopGathering(StopGatheringMessage msg)
        {
            if (_currentNode != null && msg.Node == _currentNode)
            {
                if (_currentNodeType == WorldNodeType.Bed)
                {
                    var setSpriteVisibilityMsg = MessageFactory.GenerateSetSpriteVisibilityMsg();
                    setSpriteVisibilityMsg.Visible = true;
                    _controller.gameObject.SendMessageTo(setSpriteVisibilityMsg, _controller.transform.parent.gameObject);
                    MessageFactory.CacheMessage(setSpriteVisibilityMsg);

                    var setActiveSelectableStateMsg = MessageFactory.GenerateSetActiveSelectableStateMsg();
                    setActiveSelectableStateMsg.Selectable = true;
                    _controller.gameObject.SendMessageTo(setActiveSelectableStateMsg, _controller.transform.parent.gameObject);
                    MessageFactory.CacheMessage(setActiveSelectableStateMsg);
                }

                _gatheringTile = null;
                _currentNode = null;
                _gatheringTimer?.Destroy();
                _gatheringTimer = null;
                _invisibleOnGather = false;
                _currentNodeType = WorldNodeType.Food;

                var setMonsterStateMsg = MessageFactory.GenerateSetMonsterStateMsg();
                setMonsterStateMsg.State = MonsterState.Idle;
                _controller.gameObject.SendMessageTo(setMonsterStateMsg, _controller.transform.parent.gameObject);
                MessageFactory.CacheMessage(setMonsterStateMsg);

                var setHobblerAiStateMsg = MessageFactory.GenerateSetHobblerAiStateMsg();
                setHobblerAiStateMsg.State = HobblerAiState.Auto;
                _controller.gameObject.SendMessageTo(setHobblerAiStateMsg, _controller.transform.parent.gameObject);
                MessageFactory.CacheMessage(setHobblerAiStateMsg);
            }
        }

        private void UpdateMapTile(UpdateMapTileMessage msg)
        {
            _currentTile = msg.Tile;
            if (_gatheringTile != null && _gatheringTile == _currentTile)
            {
                _gatheringTimer?.Play();
                if (_invisibleOnGather)
                {
                    var setSpriteVisibilityMsg = MessageFactory.GenerateSetSpriteVisibilityMsg();
                    setSpriteVisibilityMsg.Visible = false;
                    _controller.gameObject.SendMessageTo(setSpriteVisibilityMsg, _controller.transform.parent.gameObject);
                    MessageFactory.CacheMessage(setSpriteVisibilityMsg);

                    var setActiveSelectableStateMsg = MessageFactory.GenerateSetActiveSelectableStateMsg();
                    setActiveSelectableStateMsg.Selectable = false;
                    _controller.gameObject.SendMessageTo(setActiveSelectableStateMsg, _controller.transform.parent.gameObject);
                    MessageFactory.CacheMessage(setActiveSelectableStateMsg);
                }
                else
                {
                    var setDirectionMsg = MessageFactory.GenerateSetDirectionMsg();
                    setDirectionMsg.Direction = (_currentNode.transform.position.ToVector2() - _currentTile.World).normalized;
                    _controller.gameObject.SendMessageTo(setDirectionMsg, _controller.transform.parent.gameObject);
                    MessageFactory.CacheMessage(setDirectionMsg);
                }
            }
        }

        private void UpdateMonsterState(UpdateMonsterStateMessage msg)
        {
            if (msg.State == MonsterState.Idle || msg.State == MonsterState.Minigame || msg.State == MonsterState.Battle)
            {
                if (_currentNode != null)
                {
                    var unregisterFromNodeMsg = MessageFactory.GenerateUnregisterFromGatheringNodeMsg();
                    unregisterFromNodeMsg.Unit = _controller.transform.parent.gameObject;
                    _controller.gameObject.SendMessageTo(unregisterFromNodeMsg, _currentNode);
                    MessageFactory.CacheMessage(unregisterFromNodeMsg);
                }
            }

            _monsterState = msg.State;
        }

        private void Obstacle(ObstacleMessage msg)
        {
            if (_currentNode != null)
            {
                var path = WorldController.Pathing.GetPath(_currentTile.Position, _gatheringTile.Position, false);
                if (path.Length > 0)
                {
                    var setPathMsg = MessageFactory.GenerateSetPathMsg();
                    setPathMsg.Path = path;
                    _controller.gameObject.SendMessageTo(setPathMsg, _controller.transform.parent.gameObject);
                    MessageFactory.CacheMessage(setPathMsg);
                }
                else
                {
                    var setMonsterStateMsg = MessageFactory.GenerateSetMonsterStateMsg();
                    setMonsterStateMsg.State = MonsterState.Idle;
                    _controller.gameObject.SendMessageTo(setMonsterStateMsg, _controller.transform.parent.gameObject);
                    MessageFactory.CacheMessage(setMonsterStateMsg);

                    _currentNode = null;
                    _gatheringTile = null;
                    _invisibleOnGather = false;
                }
            }
        }

        public override void Destroy()
        {
            _gatheringTimer?.Destroy();
            _gatheringTimer = null;
            _currentTile = null;
            _currentNode = null;
            _gatheringTile = null;
            base.Destroy();
        }
    }
}
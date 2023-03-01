using System.Collections.Generic;
using System.Linq;
using Assets.Ancible_Tools.Scripts.System.Factories;
using Assets.Ancible_Tools.Scripts.System.Items.Crafting;
using Assets.Ancible_Tools.Scripts.System.SaveData.Building;
using Assets.Ancible_Tools.Scripts.System.WorldNodes;
using Assets.Resources.Ancible_Tools.Scripts.System;
using Assets.Resources.Ancible_Tools.Scripts.System.Pathing;
using Assets.Resources.Ancible_Tools.Scripts.System.SaveData;
using Assets.Resources.Ancible_Tools.Scripts.System.Skills;
using Assets.Resources.Ancible_Tools.Scripts.System.UnitCommands;
using MessageBusLib;
using UnityEngine;

namespace Assets.Ancible_Tools.Scripts.Traits
{
    [CreateAssetMenu(fileName = "Crafting Node Trait", menuName = "Ancible Tools/Traits/Node/Crafting Node")]
    public class CraftingNodeTrait : WorldNodeTrait
    {
        [SerializeField] private CraftingRecipe[] _startingRecipes = new CraftingRecipe[0];
        [SerializeField] private Vector2 _defaultItemScale = new Vector2(31.25f, 31.25f);
        [SerializeField] private Vector2 _itemOffset = Vector2.zero;
        [SerializeField] private WorldCraftingSkill _skill = null;
        [SerializeField] private float _skillBonusPercent = 1f;
        [SerializeField] private int _maxQueue = 5;
        [SerializeField] private bool _isAbility = false;
        

        private List<QueuedCraft> _craftingQueue = new List<QueuedCraft>();
        private List<CraftingRecipe> _recipes = new List<CraftingRecipe>();

        private CraftingParameterData _craftingData = null;

        public override void SetupController(TraitController controller)
        {
            _nodeType = WorldNodeType.Crafting;
            _recipes = _startingRecipes.ToList();
            _stack = -1;
            base.SetupController(controller);

            _nodeSpriteController.SetScale(_defaultItemScale);
            _nodeSpriteController.SetOffset(_itemOffset);
            RefreshNodeSprite(true);
        }

        protected internal override void ApplyToUnit(GameObject obj)
        {
            if (_craftingQueue.Count > 0)
            {
                var currentItem = _craftingQueue[0];
                if (currentItem.Tick())
                {
                    QueuedCraftFinished(currentItem);
                }
                base.ApplyToUnit(obj);
                _controller.gameObject.SendMessageTo(RefreshUnitMessage.INSTANCE, _controller.transform.parent.gameObject);
            }
        }

        protected internal override bool FinishGatheringCheck(GameObject obj)
        {
            var finish = _craftingQueue.Count <= 0;
            if (finish)
            {
                var stopGatheringMsg = MessageFactory.GenerateStopGatheringMsg();
                stopGatheringMsg.Node = _controller.transform.parent.gameObject;
                _controller.gameObject.SendMessageTo(stopGatheringMsg, obj);
                MessageFactory.CacheMessage(stopGatheringMsg);

                RemoveFromInteractingObjects(obj);

                if (_registeredNode != null)
                {
                    UnregisterNode();
                }
            }

            return finish;
        }

        private void QueueRecipe(CraftingRecipe recipe, int count)
        {
            if (_recipes.Contains(recipe))
            {
                _craftingQueue.Add(new QueuedCraft(recipe, count));
                recipe.RemoveCost(count);
            }

        }

        private void QueuedCraftFinished(QueuedCraft craft)
        {
            _craftingQueue.Remove(craft);
            craft.Destroy();
            RefreshNodeSprite(true);
            if (_craftingQueue.Count <= 0 && _registeredNode != null)
            {
                UnregisterNode();
            }
            _controller.gameObject.SendMessageTo(RefreshUnitMessage.INSTANCE, _controller.transform.parent.gameObject);
        }

        protected internal override void RefreshNodeSprite(bool refreshTrait)
        {
            if (_craftingQueue.Count > 0)
            {
                _nodeSpriteController.SetSprite(_craftingQueue[0].Recipe.Item.Item.Icon);
            }
            _nodeSpriteController.gameObject.SetActive(_craftingQueue.Count > 0);
        }

        protected internal override void RegisterNode(MapTile mapTile)
        {
            if (_craftingQueue.Count > 0)
            {
                _registeredNode = WorldNodeManager.RegisterCraftingNode(_controller.transform.parent.gameObject, mapTile, _skill);
            }
            
        }

        protected internal override void UnregisterNode()
        {
            WorldNodeManager.UnregisterNode(_controller.transform.parent.gameObject, _nodeType);
            _registeredNode = null;
            RefreshNodeSprite(false);
        }

        protected internal override int GetRequiredTicks(GameObject owner)
        {
            var bonus = 0f;
            var querySkillBonusMsg = MessageFactory.GenerateQuerySkillBonusMsg();
            querySkillBonusMsg.DoAfter = skillBonus => bonus = skillBonus * _skillBonusPercent;
            querySkillBonusMsg.Skill = _skill;
            _controller.gameObject.SendMessageTo(querySkillBonusMsg, owner);
            MessageFactory.CacheMessage(querySkillBonusMsg);

            var tickBonus = Mathf.RoundToInt(bonus);
            var ticks = _requiredTicks - tickBonus;
            if (ticks <= 0)
            {
                ticks = 1;
            }
            return ticks;
        }

        protected internal override void SubscribeToMessages()
        {
            base.SubscribeToMessages();
            _controller.transform.parent.gameObject.SubscribeWithFilter<UpdateBuildingIdMessage>(UpdateBuildingId, _instanceId);
            _controller.transform.parent.gameObject.SubscribeWithFilter<QueryBuildingParamterDataMessage>(QueryBuildingParameterData, _instanceId);
            _controller.transform.parent.gameObject.SubscribeWithFilter<CancelCraftingQueueAtIndexMessage>(CancelCraftingQueueAtIndex, _instanceId);
            _controller.transform.parent.gameObject.SubscribeWithFilter<QueueCraftingRecipeMessage>(QueueCraftingRecipe, _instanceId);
            _controller.transform.parent.gameObject.SubscribeWithFilter<QueryCraftingQueueMessage>(QueryCraftingQueue, _instanceId);
            _controller.transform.parent.gameObject.SubscribeWithFilter<QueryCraftingRecipesMessage>(QueryCraftingRecipes, _instanceId);
            _controller.transform.parent.gameObject.SubscribeWithFilter<SetCraftingIndexMessage>(SetCraftingIndex, _instanceId);
        }

        protected internal override void QueryBuildingParameterData(QueryBuildingParamterDataMessage msg)
        {
            if (_craftingData == null)
            {
                _craftingData = new CraftingParameterData();
            }

            _craftingData.Queue = _craftingQueue.Select(q => q.GetData()).ToArray();
            msg.DoAfter.Invoke(_craftingData);
        }

        protected internal override void UpdateBuildingId(UpdateBuildingIdMessage msg)
        {
            var data = PlayerDataController.GetBuildingData(msg.Id);
            if (data != null && data.Parameter is CraftingParameterData craftingData)
            {
                foreach (var queued in craftingData.Queue)
                {
                    var recipe = WorldItemFactory.GetRecipeByName(queued.Recipe);
                    if (recipe)
                    {
                        var queuedCraft = new QueuedCraft(recipe, queued.Count);
                        queuedCraft.SetRemainingTicks(queued.RemainingTicks);
                        _craftingQueue.Add(queuedCraft);
                    }
                }
                
            }
        }

        private void CancelCraftingQueueAtIndex(CancelCraftingQueueAtIndexMessage msg)
        {
            if (_craftingQueue.Count > msg.Index)
            {
                var queuedCraft = _craftingQueue[msg.Index];
                _craftingQueue.Remove(queuedCraft);
                queuedCraft.Recipe.Refund(queuedCraft.Count);
                queuedCraft.Destroy();
                RefreshNodeSprite(true);
                if (_craftingQueue.Count <= 0)
                {
                    UnregisterNode();
                }

                _controller.gameObject.SendMessageTo(RefreshUnitMessage.INSTANCE, _controller.transform.parent.gameObject);
            }
        }

        private void QueueCraftingRecipe(QueueCraftingRecipeMessage msg)
        {
            if (_craftingQueue.Count < _maxQueue)
            {
                QueueRecipe(msg.Recipe, msg.Stack);
                if (_registeredNode == null)
                {
                    RegisterNode(_mapTile);
                }
                _controller.gameObject.SendMessageTo(RefreshUnitMessage.INSTANCE, _controller.transform.parent.gameObject);
            }
        }

        private void QueryCraftingQueue(QueryCraftingQueueMessage msg)
        {
            msg.DoAfter.Invoke(_craftingQueue.ToArray(), _maxQueue);
        }

        private void QueryCraftingRecipes(QueryCraftingRecipesMessage msg)
        {
            msg.DoAfter.Invoke(_recipes.ToArray(), _isAbility);
        }

        private void SetCraftingIndex(SetCraftingIndexMessage msg)
        {
            if (msg.Current < _craftingQueue.Count)
            {
                var queueItem = _craftingQueue[msg.Current];
                _craftingQueue.RemoveAt(msg.Current);
                if (msg.Target < _craftingQueue.Count)
                {
                    _craftingQueue.Insert(msg.Target, queueItem);
                }
                else
                {
                    _craftingQueue.Add(queueItem);
                }

                _controller.gameObject.SendMessageTo(RefreshUnitMessage.INSTANCE, _controller.transform.parent.gameObject);
            }
        }
    }
}
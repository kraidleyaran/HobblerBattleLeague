using Assets.Resources.Ancible_Tools.Scripts.System;
using Assets.Resources.Ancible_Tools.Scripts.System.Items;
using Assets.Resources.Ancible_Tools.Scripts.System.Minigame;
using MessageBusLib;
using UnityEngine;

namespace Assets.Ancible_Tools.Scripts.Traits
{
    [CreateAssetMenu(fileName = "Minigame Chest Interaction Trait", menuName = "Ancible Tools/Traits/Minigame/Interaction/Minigame Chest Interaction")]
    public class MinigameChestInteractionTrait : MinigameInteractionTrait
    {
        [SerializeField] private SpriteTrait _openSprite = null;
        [SerializeField] private LootTable _lootTable = null;
        [SerializeField] private bool _skipSpawnCount = false;

        private MinigameUnitState _unitState = MinigameUnitState.Idle;
        private bool _open = false;

        protected internal override void SubscribeToMessages()
        {
            base.SubscribeToMessages();
            _controller.transform.parent.gameObject.SubscribeWithFilter<UpdateMinigameUnitStateMessage>(UpdateMinigameUnitState, _instanceId);
            _controller.transform.parent.gameObject.SubscribeWithFilter<SetLootTableMessage>(SetLootTable, _instanceId);
        }

        protected internal override void MinigameInteract(MinigameInteractMessage msg)
        {
            if (_unitState != MinigameUnitState.Interact && !_open)
            {
                var setMinigameUnitStateMsg = MessageFactory.GenerateSetMinigameUnitStateMsg();
                setMinigameUnitStateMsg.State = MinigameUnitState.Interact;
                _controller.gameObject.SendMessageTo(setMinigameUnitStateMsg, _controller.transform.parent.gameObject);
                MessageFactory.CacheMessage(setMinigameUnitStateMsg);

                var owner = msg.Owner;
                var doBumpMsg = MessageFactory.GenerateDoBumpMsg();
                doBumpMsg.OnBump = () => { OpenChest(owner); };
                doBumpMsg.DoAfter = () => { CleanUp(owner); };
                doBumpMsg.Direction = msg.Direction;
                _controller.gameObject.SendMessageTo(doBumpMsg, owner);
                MessageFactory.CacheMessage(doBumpMsg);

                base.MinigameInteract(msg);
            }
            
        }

        private void OpenChest(GameObject owner)
        {
            if (!_open)
            {
                _open = true;
                var setSpriteMsg = MessageFactory.GenerateSetSpriteMsg();
                setSpriteMsg.Sprite = _openSprite;
                _controller.gameObject.SendMessageTo(setSpriteMsg, _controller.transform.parent.gameObject);
                MessageFactory.CacheMessage(setSpriteMsg);

                var loot = _lootTable.GenerateLoot();
                if (loot.Length > 0)
                {
                    var addItemMsg = MessageFactory.GenerateAddItemMsg();
                    for (var i = 0; i < loot.Length; i++)
                    {
                        addItemMsg.Item = loot[i].Item;
                        addItemMsg.Stack = loot[i].Stack;
                        _controller.gameObject.SendMessageTo(addItemMsg, owner);
                    }
                    MessageFactory.CacheMessage(addItemMsg);
                }

                if (!_skipSpawnCount)
                {
                    _controller.gameObject.SendMessageTo(ClaimChestMessage.INSTANCE, owner);
                }
                

                Debug.Log($"Player given {loot.Length} items");
            }

        }

        private void CleanUp(GameObject owner)
        {
            var setUnitStateMsg = MessageFactory.GenerateSetMinigameUnitStateMsg();
            setUnitStateMsg.State = MinigameUnitState.Idle;
            _controller.gameObject.SendMessageTo(setUnitStateMsg, owner);
            MessageFactory.CacheMessage(setUnitStateMsg);
            if (_skipSpawnCount)
            {
                MinigameController.UnregisterMinigameObject(_controller.transform.parent.gameObject);
            }
        }

        private void UpdateMinigameUnitState(UpdateMinigameUnitStateMessage msg)
        {
            _unitState = msg.State;
        }

        private void SetLootTable(SetLootTableMessage msg)
        {
            _lootTable = msg.Table;
        }
    }
}
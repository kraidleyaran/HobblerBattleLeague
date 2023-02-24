using Assets.Resources.Ancible_Tools.Scripts.System;
using Assets.Resources.Ancible_Tools.Scripts.System.Adventure;
using Assets.Resources.Ancible_Tools.Scripts.System.Items;
using Assets.Resources.Ancible_Tools.Scripts.System.SaveData;
using Assets.Resources.Ancible_Tools.Scripts.System.UI.Alerts;
using MessageBusLib;
using UnityEngine;

namespace Assets.Ancible_Tools.Scripts.Traits
{
    [CreateAssetMenu(fileName = "Adventure Chest Trait", menuName = "Ancible Tools/Traits/Adventure/Interaction/Chest")]
    public class AdventureChestInteractionTrait : Trait
    {
        public SpriteTrait Open => _openSprite;

        [SerializeField] private SpriteTrait _openSprite = null;
        [SerializeField] private ItemStack _item = null;
        public string SaveId;

        private bool _opened = false;

        public override void SetupController(TraitController controller)
        {
            base.SetupController(controller);
            SubscribeToMessages();
        }

        private void CheckChestData()
        {
            if (!string.IsNullOrEmpty(SaveId))
            {
                var data = PlayerDataController.GetChestDataById(SaveId);
                if (data != null)
                {
                    _opened = true;
                    var setSpriteTraitMsg = MessageFactory.GenerateSetSpriteMsg();
                    setSpriteTraitMsg.Sprite = _openSprite;
                    _controller.gameObject.SendMessageTo(setSpriteTraitMsg, _controller.transform.parent.gameObject);
                    MessageFactory.CacheMessage(setSpriteTraitMsg);
                }
            }
        }

        private void OpenChest()
        {
            var setSpriteMsg = MessageFactory.GenerateSetSpriteMsg();
            setSpriteMsg.Sprite = _openSprite;
            _controller.gameObject.SendMessageTo(setSpriteMsg, _controller.transform.parent.gameObject);
            MessageFactory.CacheMessage(setSpriteMsg);

            WorldStashController.AddItem(_item.Item, _item.Stack);

            UiAlertManager.ShowAlert($"+{_item.Stack} {_item.Item.DisplayName}", _item.Item.Icon, _item.Item.Rarity.ToRarityColor());

            PlayerDataController.SetChestData(SaveId);

            var setUnitStateMsg = MessageFactory.GenerateSetAdventureUnitStateMsg();
            setUnitStateMsg.State = AdventureUnitState.Idle;
            _controller.gameObject.SendMessageTo(setUnitStateMsg, WorldAdventureController.Player);
            MessageFactory.CacheMessage(setUnitStateMsg);
        }

        private void SubscribeToMessages()
        {
            _controller.transform.parent.gameObject.SubscribeWithFilter<InteractMessage>(Interact, _instanceId);
            _controller.transform.parent.gameObject.SubscribeWithFilter<SetAdventureChestMessage>(SetAdventureChest, _instanceId);
        }

        private void Interact(InteractMessage msg)
        {
            if (!_opened)
            {
                _opened = true;

                var setUnitStateMsg = MessageFactory.GenerateSetAdventureUnitStateMsg();
                setUnitStateMsg.State = AdventureUnitState.Interaction;
                _controller.gameObject.SendMessageTo(setUnitStateMsg, WorldAdventureController.Player);
                MessageFactory.CacheMessage(setUnitStateMsg);

                var doBumpMsg = MessageFactory.GenerateDoBumpMsg();
                doBumpMsg.Direction = (_controller.transform.parent.position.ToVector2() - msg.Owner.transform.position.ToVector2()).normalized.ToCardinal();
                doBumpMsg.OnBump = OpenChest;
                doBumpMsg.DoAfter = null;
                _controller.gameObject.SendMessageTo(doBumpMsg, WorldAdventureController.Player);
                MessageFactory.CacheMessage(doBumpMsg);
            }
        }

        private void SetAdventureChest(SetAdventureChestMessage msg)
        {
            _item = msg.Item;
            SaveId = msg.Id;
            _openSprite = msg.OpenSprite;
            CheckChestData();
        }
    }
}
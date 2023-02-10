using System.Collections.Generic;
using Assets.Resources.Ancible_Tools.Scripts.System.Items;
using Assets.Resources.Ancible_Tools.Scripts.System.UI.DragDrop;
using Assets.Resources.Ancible_Tools.Scripts.System.Windows;
using MessageBusLib;
using UnityEngine;
using UnityEngine.UI;

namespace Assets.Resources.Ancible_Tools.Scripts.System.UI.Stash
{
    public class UiStashWindowController : UiBaseWindow
    {
        private const string FILTER = "UI_STASH_WINDOW_FILTER";

        public override bool Static => true;

        [SerializeField] private UiStashItemController _itemTemplate;
        [SerializeField] private GridLayoutGroup _grid;
        [SerializeField] private RectTransform _content;

        private UiStashItemController[] _controllers = new UiStashItemController[0];

        private UiStashItemController _hoveredItem = null;

        public override void Awake()
        {
            RefreshInfo();
            SubscribeToMessages();
        }

        private void RefreshInfo()
        {
            for (var i = 0; i < _controllers.Length; i++)
            {
                Destroy(_controllers[i].gameObject);
            }
            _controllers = new UiStashItemController[0];
            var controllers = new List<UiStashItemController>();
            var items = WorldStashController.GetItems();
            for (var i = 0; i < items.Length; i++)
            {
                var controller = Instantiate(_itemTemplate, _content);
                controller.Setup(items[i], true);
                controllers.Add(controller);
            }

            _controllers = controllers.ToArray();
            var rows = _controllers.Length / _grid.constraintCount;
            var rowCheck = rows * _grid.constraintCount;
            if (rowCheck < _controllers.Length)
            {
                rows++;
            }

            var height = (rows * (_grid.cellSize.y + _grid.spacing.y)) + _grid.padding.top + _grid.padding.bottom;
            _content.SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, height);
        }

        private void SubscribeToMessages()
        {
            gameObject.Subscribe<UpdateInputStateMessage>(UpdateInputState);
            gameObject.Subscribe<StashUpdatedMessage>(StashUpdated);
            gameObject.Subscribe<SetHoveredStashItemControllerMessage>(SetHoveredStashItemController);
            gameObject.Subscribe<RemoveHoveredStashItemControllerMessage>(RemoveHoveredStashItemController);
            gameObject.SubscribeWithFilter<ReceiveDragDropItemMessage>(ReceiveDragDropItem, FILTER);
            gameObject.SubscribeWithFilter<RemoveItemMessage>(RemoveItem, FILTER);
        }

        private void StashUpdated(StashUpdatedMessage msg)
        {
            RefreshInfo();
        }

        private void SetHoveredStashItemController(SetHoveredStashItemControllerMessage msg)
        {
            _hoveredItem = msg.Controller;
        }

        private void RemoveHoveredStashItemController(RemoveHoveredStashItemControllerMessage msg)
        {
            if (_hoveredItem && _hoveredItem == msg.Controller)
            {
                _hoveredItem = null;
            }
        }

        private void RemoveItem(RemoveItemMessage msg)
        {
            WorldStashController.RemoveItem(msg.Item, msg.Stack);
        }

        private void ReceiveDragDropItem(ReceiveDragDropItemMessage msg)
        {
            if (msg.Owner != gameObject)
            {
                WorldStashController.AddItem(msg.Item, msg.Stack);
                var removeItemMsg = MessageFactory.GenerateRemoveItemMsg();
                removeItemMsg.Item = msg.Item;
                removeItemMsg.Stack = msg.Stack;
                gameObject.SendMessageTo(removeItemMsg, msg.Owner);
                MessageFactory.CacheMessage(removeItemMsg);
            }
        }

        private void UpdateInputState(UpdateInputStateMessage msg)
        {
            if (_hoveredItem && msg.Previous.LeftClick && msg.Current.LeftClick && !UiDragDropManager.Active)
            {
                UiDragDropManager.SetDragDropItem(gameObject, _hoveredItem.Stack.Item, _hoveredItem.Stack.Stack);
            }
        }
    }
}
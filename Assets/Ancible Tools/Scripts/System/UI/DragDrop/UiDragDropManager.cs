using System;
using Assets.Resources.Ancible_Tools.Scripts.System.Items;
using Assets.Resources.Ancible_Tools.Scripts.System.Windows;
using MessageBusLib;
using UnityEngine;
using UnityEngine.UI;

namespace Assets.Resources.Ancible_Tools.Scripts.System.UI.DragDrop
{
    public class UiDragDropManager : MonoBehaviour
    {
        public static bool Active => _instance._dragDropImage.gameObject.activeSelf;

        private static UiDragDropManager _instance = null;

        [SerializeField] private Image _dragDropImage;

        private WorldItem _item = null;
        private int _itemStack = 0;
        private GameObject _owner = null;
        private Action _onDropFailure = null;

        private ReceiveDragDropItemMessage _receiveDragDropItemMsg = new ReceiveDragDropItemMessage();
        private Vector2 _mousePos = Vector2.zero;
        

        void Awake()
        {
            if (_instance)
            {
                Destroy(gameObject);
                return;
            }

            _instance = this;
            _dragDropImage.gameObject.SetActive(false);
            SubscribeToMessages();
        }

        public static void SetDragDropItem(GameObject owner, WorldItem item, int stack = 1, Action onDropFailure = null)
        {
            if (!WorldCameraController.Moving)
            {
                _instance._onDropFailure = onDropFailure;
                _instance._item = item;
                _instance._itemStack = stack;
                _instance._dragDropImage.sprite = item.Icon;
                _instance._owner = owner;
                _instance._dragDropImage.transform.SetLocalPosition(_instance._mousePos);
                _instance._dragDropImage.gameObject.SetActive(true);
                UiWindowManager.RegisterWindowBlock(_instance.gameObject);
            }
        }

        private void SubscribeToMessages()
        {
            gameObject.Subscribe<UpdateInputStateMessage>(UpdateInputState);
        }

        private void UpdateInputState(UpdateInputStateMessage msg)
        {   
            if (_instance._dragDropImage.gameObject.activeSelf)
            {
                if (msg.Current.LeftClick)
                {
                    _dragDropImage.transform.SetLocalPosition(msg.Current.MousePos);
                }
                else if (!msg.Current.LeftClick && _item)
                {
                    var handled = false;
                    if (UiWindowManager.Hovered)
                    {
                        _receiveDragDropItemMsg.Item = _item;
                        _receiveDragDropItemMsg.Stack = _itemStack;
                        _receiveDragDropItemMsg.Owner = _owner;
                        _receiveDragDropItemMsg.DoAfter = () =>
                        {
                            handled = true;
                        };
                        gameObject.SendMessageTo(_receiveDragDropItemMsg, UiWindowManager.Hovered.gameObject);


                    }
                    if (!handled)
                    {
                        _onDropFailure?.Invoke();
                    }
                    _item = null;
                    _itemStack = 0;
                    _owner = null;
                    _onDropFailure = null;
                    _dragDropImage.gameObject.SetActive(false);
                    UiWindowManager.RemoveWindowBlock(gameObject);
                }
            }
            _mousePos = msg.Current.MousePos;
        }
    }
}
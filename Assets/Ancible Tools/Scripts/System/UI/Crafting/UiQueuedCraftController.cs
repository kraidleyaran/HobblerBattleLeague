using System.Collections.Generic;
using Assets.Ancible_Tools.Scripts.System.Items.Crafting;
using MessageBusLib;
using UnityEngine;
using UnityEngine.UI;

namespace Assets.Resources.Ancible_Tools.Scripts.System.UI.Crafting
{
    public class UiQueuedCraftController : MonoBehaviour
    {
        public QueuedCraft Craft { get; private set; }

        public RectTransform RectTransform;
        [SerializeField] private Image _iconImage = null;
        [SerializeField] private Image _fillImage = null;
        [SerializeField] private Text _countText = null;
        [SerializeField] private Text _queueIndexText = null;
        [SerializeField] private Button _moveUpButton = null;
        [SerializeField] private Button _moveDownButton = null;

        private int _index = 0;
        private int _maxIndex = 0;
        private GameObject _owner = null;

        public void Setup(QueuedCraft craft)
        {
            Craft = craft;
            RefreshCraft();
        }

        public void SetOwner(GameObject owner)
        {
            _owner = owner;
        }

        public void SetIndex(int index, int max)
        {
            _index = index;
            _maxIndex = max;
            _queueIndexText.text = $"{_index + 1}";
            _moveUpButton.interactable = _index > 0;
            _moveDownButton.interactable = _index > 0 && _index < _maxIndex;
        }

        public void Clear()
        {
            Craft = null;
            RefreshCraft();
        }

        public void MoveUp()
        {
            if (_index > 0)
            {
                var setCraftingIndexMsg = MessageFactory.GenerateSetCraftingIndexMsg();
                setCraftingIndexMsg.Current = _index;
                setCraftingIndexMsg.Target = _index - 1;
                gameObject.SendMessageTo(setCraftingIndexMsg, _owner);
                MessageFactory.CacheMessage(setCraftingIndexMsg);
            }
        }

        public void MoveDown()
        {
            if (_index > 0 && _index < _maxIndex)
            {
                var setCraftingIndexMsg = MessageFactory.GenerateSetCraftingIndexMsg();
                setCraftingIndexMsg.Current = _index;
                setCraftingIndexMsg.Target = _index + 1;
                gameObject.SendMessageTo(setCraftingIndexMsg, _owner);
                MessageFactory.CacheMessage(setCraftingIndexMsg);
            }
        }

        public void RefreshCraft()
        {
            var craft = Craft != null;
            if (craft)
            {
                _iconImage.sprite = Craft.Recipe.Item.Item.Icon;
                _countText.text = $"{Craft.Count}";
                var percent = (float)(Craft.Recipe.CraftingTicks - Craft.RemainingTicks) / Craft.Recipe.CraftingTicks;
                _fillImage.fillAmount = percent;
                
            }
            _iconImage.gameObject.SetActive(craft);
            _countText.gameObject.SetActive(craft);
            _fillImage.gameObject.SetActive(craft);
            _queueIndexText.gameObject.SetActive(!craft);
            _moveDownButton.gameObject.SetActive(Craft != null);
            _moveUpButton.gameObject.SetActive(Craft != null);
        }
    }
}
using System;
using System.Collections.Generic;
using Assets.Ancible_Tools.Scripts.System.Items.Crafting;
using Assets.Resources.Ancible_Tools.Scripts.System.Items;
using MessageBusLib;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace Assets.Resources.Ancible_Tools.Scripts.System.UI.Crafting
{
    public class UiQueuedCraftController : MonoBehaviour , IPointerEnterHandler, IPointerExitHandler
    {
        public QueuedCraft Craft { get; private set; }

        public RectTransform RectTransform;
        [SerializeField] private Image _iconImage = null;
        [SerializeField] private Image _fillImage = null;
        [SerializeField] private Image _qualityIconImage = null;
        [SerializeField] private Text _countText = null;
        [SerializeField] private Text _queueIndexText = null;
        [SerializeField] private Text _abilityRankText = null;
        [SerializeField] private Button _moveUpButton = null;
        [SerializeField] private Button _moveDownButton = null;
        [SerializeField] private Button _cancelButton = null;

        private int _index = 0;
        private int _maxIndex = 0;
        private GameObject _owner = null;
        private bool _hovered = false;

        void Awake()
        {
            _cancelButton.gameObject.SetActive(false);
        }

        public void Setup(QueuedCraft craft)
        {
            Craft = craft;
            _qualityIconImage.sprite = Craft.Recipe.Item.Item.Quality.ToIcon();
            _qualityIconImage.gameObject.SetActive(Craft.Recipe.Item.Item.Quality != ItemQuality.Basic);
            if (craft.Recipe.Item.Item.Type == WorldItemType.Ability && craft.Recipe.Item.Item is AbilityItem abilityItem && abilityItem.Ability.Rank > 0)
            {
                _abilityRankText.text = StaticMethods.ApplyColorToText(abilityItem.Ability.RankToString(), ColorFactoryController.AbilityRank);
                _abilityRankText.gameObject.SetActive(true);
            }
            else
            {
                _abilityRankText.gameObject.SetActive(false);
            }
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

            if (_moveUpButton)
            {
                _moveUpButton.interactable = _index > 0;
            }

            if (_moveDownButton)
            {
                _moveDownButton.interactable = _index < _maxIndex - 1;
            }


        }

        public void Clear()
        {
            Craft = null;
            _abilityRankText.gameObject.SetActive(false);
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
            if (_index < _maxIndex - 1)
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

                if (_hovered)
                {
                    var showHoverInfoMsg = MessageFactory.GenerateShowHoverInfoMsg();
                    var remainingTicks = Craft.RemainingTicks + Craft.Recipe.CraftingTicks * (Craft.Count - 1);
                    var description = $"{Craft.Recipe.Item.Item.GetDescription()}{StaticMethods.DoubleNewLine()}Remaining Time: {remainingTicks}";

                    showHoverInfoMsg.Description = description;
                    showHoverInfoMsg.Icon = Craft.Recipe.Item.Item.Icon;
                    showHoverInfoMsg.Gold = Craft.Recipe.Cost * Craft.Count;
                    showHoverInfoMsg.Owner = gameObject;

                    gameObject.SendMessage(showHoverInfoMsg);
                    MessageFactory.CacheMessage(showHoverInfoMsg);
                }

            }
            else
            {
                _qualityIconImage.gameObject.SetActive(false);
            }
            _iconImage.gameObject.SetActive(craft);
            _countText.gameObject.SetActive(craft);
            _fillImage.gameObject.SetActive(craft);
            _queueIndexText.gameObject.SetActive(!craft);
            
            _moveDownButton?.gameObject.SetActive(Craft != null);
            _moveUpButton?.gameObject.SetActive(Craft != null);


        }

        public void CancelCraft()
        {
            var cancelCraftAtIndexMsg = MessageFactory.GenerateCancelCraftingQueueAtIndexMsg();
            cancelCraftAtIndexMsg.Index = _index;
            gameObject.SendMessageTo(cancelCraftAtIndexMsg, _owner);
            MessageFactory.CacheMessage(cancelCraftAtIndexMsg);

            _cancelButton.gameObject.SetActive(false);
        }

        public void OnPointerEnter(PointerEventData eventData)
        {
            if (!_hovered)
            {
                _hovered = true;
                var showHoverInfoMsg = MessageFactory.GenerateShowHoverInfoMsg();
                if (Craft != null)
                {
                    
                    showHoverInfoMsg.Title = $"{Craft.Recipe.Item.Item.DisplayName}x{Craft.Count}";
                    var remainingTicks = Craft.RemainingTicks + Craft.Recipe.CraftingTicks * (Craft.Count - 1);
                    var description = $"{Craft.Recipe.Item.Item.GetDescription()}{StaticMethods.DoubleNewLine()}Remaining Time: {remainingTicks}";

                    showHoverInfoMsg.Description = description;
                    showHoverInfoMsg.Icon = Craft.Recipe.Item.Item.Icon;
                    showHoverInfoMsg.Gold = Craft.Recipe.Cost * Craft.Count;
                    showHoverInfoMsg.Owner = gameObject;

                    _cancelButton.gameObject.SetActive(true);
                }
                else
                {
                    showHoverInfoMsg.Title = $"Empty Slot {_index + 1}";
                    showHoverInfoMsg.Description = $"Queue an item to start crafting";
                    showHoverInfoMsg.Gold = -1;
                    showHoverInfoMsg.Owner = gameObject;
                }

                gameObject.SendMessage(showHoverInfoMsg);
                MessageFactory.CacheMessage(showHoverInfoMsg);

            }
        }

        public void OnPointerExit(PointerEventData eventData)
        {
            if (_hovered)
            {
                _hovered = false;
                _cancelButton.gameObject.SetActive(false);

                var removeHoverInfoMsg = MessageFactory.GenerateRemoveHoverInfoMsg();
                removeHoverInfoMsg.Owner = gameObject;
                gameObject.SendMessage(removeHoverInfoMsg);
                MessageFactory.CacheMessage(removeHoverInfoMsg);
            }
            
        }

        public void ShowCraftingWindow()
        {
            var showCraftingWindowMsg = MessageFactory.GenerateShowCraftingWindowMsg();
            showCraftingWindowMsg.Owner = _owner;
            gameObject.SendMessage(showCraftingWindowMsg);
            MessageFactory.CacheMessage(showCraftingWindowMsg);
        }

        void OnDisable()
        {
            if (_hovered)
            {
                _hovered = false;
                var removeHoverInfoMsg = MessageFactory.GenerateRemoveHoverInfoMsg();
                removeHoverInfoMsg.Owner = gameObject;
                gameObject.SendMessage(removeHoverInfoMsg);
                MessageFactory.CacheMessage(removeHoverInfoMsg);
            }
        }

        void OnDestroy()
        {
            if (_hovered)
            {
                _hovered = false;
                var removeHoverInfoMsg = MessageFactory.GenerateRemoveHoverInfoMsg();
                removeHoverInfoMsg.Owner = gameObject;
                gameObject.SendMessage(removeHoverInfoMsg);
                MessageFactory.CacheMessage(removeHoverInfoMsg);
            }
        }
    }
}
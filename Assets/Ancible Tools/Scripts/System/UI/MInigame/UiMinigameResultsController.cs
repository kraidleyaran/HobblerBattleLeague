using System.Collections.Generic;
using Assets.Resources.Ancible_Tools.Scripts.System.Items;
using Assets.Resources.Ancible_Tools.Scripts.System.Minigame;
using Assets.Resources.Ancible_Tools.Scripts.System.UI.Stash;
using Assets.Resources.Ancible_Tools.Scripts.System.Windows;
using MessageBusLib;
using UnityEngine;
using UnityEngine.UI;

namespace Assets.Resources.Ancible_Tools.Scripts.System.UI.MInigame
{
    public class UiMinigameResultsController : UiBaseWindow
    {
        private const string SUCCESS = "Success!";
        private const string DEFEAT = "Defeat";
        private const string ABANDON = "Abandoned";

        public override bool Movable => false;
        public override bool Static => true;

        [SerializeField] private Text _titleText = null;
        [SerializeField] private string _titlePrepend = "Results - ";
        [SerializeField] private Text _experienceText = null;
        [SerializeField] private Text _goldText = null;
        [SerializeField] private Text _monstersSlainText = null;
        [SerializeField] private Text _chestsFoundText = null;
        [SerializeField] private UiStashItemController _stashItemController;
        [SerializeField] private GridLayoutGroup _itemsGrid;
        [SerializeField] private RectTransform _itemsContent = null;

        private int _experience = 0;
        private int _gold = 0;
        private ItemStack[] _items = new ItemStack[0];
        private GameObject _proxy = null;

        private UiStashItemController[] _controllers = new UiStashItemController[0];

        public void Setup(MinigameResult result, GameObject unit, GameObject proxy)
        {
            var titleText = _titlePrepend;
            _proxy = proxy;
            switch (result)
            {
                case MinigameResult.Victory:
                    titleText = $"{titleText}{SUCCESS}";
                    break;
                case MinigameResult.Defeat:
                    titleText = $"{titleText}{DEFEAT}";
                    break;
                case MinigameResult.Abandon:
                    titleText = $"{titleText}{ABANDON}";
                    break;
            }

            _titleText.text = titleText;

            _experience = 0;
            _gold = 0;
            var chests = IntNumberRange.One;
            var monsters = IntNumberRange.One;

            var queryProxyRewardsMsg = MessageFactory.GenerateQueryProxyRewardsMsg();
            queryProxyRewardsMsg.DoAfter = (gainedXp, gainedGold, totalMonsters, totalChests, items) =>
            {
                _experience = gainedXp;
                _gold = gainedGold;
                monsters = totalMonsters;
                chests = totalChests;
                _items = items;
            };
            gameObject.SendMessageTo(queryProxyRewardsMsg, unit);
            MessageFactory.CacheMessage(queryProxyRewardsMsg);

            _experienceText.text = $"{_experience}";
            _goldText.text = $"{_gold}";
            _monstersSlainText.text = $"{monsters.Minimum} / {monsters.Maximum}";
            _chestsFoundText.text = $"{chests.Minimum} / {chests.Maximum}";

            var controllers = new List<UiStashItemController>();
            for (var i = 0; i < _items.Length; i++)
            {
                var controller = Instantiate(_stashItemController, _itemsGrid.transform);
                controller.Setup(_items[i]);
                controllers.Add(controller);
            }

            _controllers = controllers.ToArray();
            var rows = (_items.Length / _itemsGrid.constraintCount);
            var rowCheck = rows * _itemsGrid.constraintCount;
            if (rowCheck < _items.Length)
            {
                rows++;
            }
            var height = rows * (_itemsGrid.cellSize.y + _itemsGrid.spacing.y) + _itemsGrid.padding.top;
            _itemsContent.SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, height);
        }

        public override void Close()
        {
            if (_proxy)
            {
                var addExperienceMsg = MessageFactory.GenerateAddExperienceMsg();
                addExperienceMsg.Amount = _experience;
                gameObject.SendMessageTo(addExperienceMsg, _proxy);
                MessageFactory.CacheMessage(addExperienceMsg);
            }
            //TODO: Add Gold
            for (var i = 0; i < _items.Length; i++)
            {
                WorldStashController.AddItem(_items[i].Item, _items[i].Stack);
                _items[i].Destroy();
            }

            _items = null;
            _experience = 0;
            _gold = 0;
            for (var i = 0; i < _controllers.Length; i++)
            {
                Destroy(_controllers[i].gameObject);
            }

            _controllers = null;
            gameObject.SendMessage(TearDownMinigameMessage.INSTANCE);
            base.Close();
        }
    }
}
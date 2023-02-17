using System.Collections.Generic;
using System.Linq;
using Assets.Resources.Ancible_Tools.Scripts.System.UnitCommands;
using Assets.Resources.Ancible_Tools.Scripts.System.Windows;
using MessageBusLib;
using UnityEngine;
using UnityEngine.UI;

namespace Assets.Resources.Ancible_Tools.Scripts.System.UI
{
    public class UiCommandCardController : UiBaseWindow
    {
        private const string FILTER = "UI_COMMAND_CARD_CONTROLLER";

        private static UiCommandCardController _instance = null;

        [SerializeField] private GridLayoutGroup _grid;
        [SerializeField] private UiUnitCommandController _unitCommandTemplate;
        [SerializeField] private UnitCommand _backCommand = null;
        [SerializeField] private UnitCommand _moreCommand = null;
        [SerializeField] private int _maxCommands = 11;

        private GameObject _unit = null;
        private UiUnitCommandController[] _controllers = new UiUnitCommandController[0];
        private List<CommandInstance> _commandTree = new List<CommandInstance>();
        private CommandInstance[] _baseTree = new CommandInstance[0];
        private CommandInstance _backCommandInstance = null;

        public void Setup()
        {
            if (_instance)
            {
                Destroy(gameObject);
                return;
            }

            _instance = this;
            _backCommandInstance = _backCommand.GenerateInstance();
            Clear();
            SubscribeToMessages();
        }

        private void Clear()
        {
            ClearControllers();
            if (_commandTree != null)
            {
                for (var i = 0; i < _commandTree.Count; i++)
                {
                    if (_commandTree[i].DestroyOnClose)
                    {
                        _commandTree[i].Destroy();
                    }
                }
                _commandTree.Clear();
            }

            if (_baseTree != null)
            {
                for (var i = 0; i < _baseTree.Length; i++)
                {
                    if (_baseTree[i].DestroyOnClose)
                    {
                        _baseTree[i].Destroy();
                    }
                }

                _baseTree = null;
            }
            gameObject.SetActive(false);
        }

        private void ClearControllers()
        {
            for (var i = 0; i < _controllers.Length; i++)
            {
                Destroy(_controllers[i].gameObject);
            }
            _controllers = new UiUnitCommandController[0];
        }

        private void SetupControllers(CommandInstance[] commands, GameObject unit, bool baseTree)
        {
            if (commands.Length > 0)
            {
                var orderedCommands = commands.OrderByDescending(c => c.Command.Priority).ThenBy(c => c.Command.Command).ToArray();
                var controllers = new List<UiUnitCommandController>();
                var max = orderedCommands.Length > _maxCommands - 1 && orderedCommands.Length > _maxCommands ? _maxCommands - 1 : orderedCommands.Length;
                var remainingCommands = new List<CommandInstance>();
                for (var i = 0; i < orderedCommands.Length; i++)
                {
                    if (i < max)
                    {
                        var controller = Instantiate(_unitCommandTemplate, _grid.transform);
                        controller.Setup(orderedCommands[i], unit);
                        controllers.Add(controller);
                    }
                    else
                    {
                        remainingCommands.Add(orderedCommands[i]);
                    }
                }
                if (!baseTree)
                {
                    var controller = Instantiate(_unitCommandTemplate, _grid.transform);
                    controller.Setup(_backCommandInstance, unit);
                    controllers.Add(controller);
                }

                if (remainingCommands.Count > 0)
                {
                    var moreCommand = Instantiate(_moreCommand, transform);
                    var moreInstance = moreCommand.GenerateInstance();
                    moreInstance.Tree.SubCommands.AddRange(remainingCommands);
                    moreInstance.DestroyOnClose = true;
                    moreCommand.DoAfter = () =>
                    {
                        var showCommandTreeMsg = MessageFactory.GenerateShowNewCommandTreeMsg();
                        showCommandTreeMsg.Command = moreInstance;
                        moreCommand.SendMessage(showCommandTreeMsg);
                        MessageFactory.CacheMessage(showCommandTreeMsg);
                    };
                    var controller = Instantiate(_unitCommandTemplate, _grid.transform);
                    controller.Setup(moreInstance, unit);
                    controllers.Add(controller);
                }
                _controllers = controllers.ToArray();
            }

            gameObject.SetActive(true);
        }

        private void RefreshUnit(GameObject unit)
        {
            ClearControllers();
            var commands = new List<CommandInstance>();
            var queryCommandsMsg = MessageFactory.GenerateQueryCommandsMsg();
            queryCommandsMsg.DoAfter = unitCommands => commands.AddRange(unitCommands);
            gameObject.SendMessageTo(queryCommandsMsg, unit);
            MessageFactory.CacheMessage(queryCommandsMsg);
            _baseTree = commands.ToArray();
            SetupControllers(commands.ToArray(), unit, true);
        }

        private void SubscribeToMessages()
        {
            gameObject.Subscribe<ShowNewCommandTreeMessage>(ShowNewCommandTree);
            gameObject.Subscribe<RemoveCommandTreeLevelMessage>(RemoveUnitCommandTreeLevel);
            gameObject.Subscribe<UpdateSelectedUnitMessage>(UpdateSelectedUnit);
        }

        private void UpdateSelectedUnit(UpdateSelectedUnitMessage msg)
        {
            if (!_unit || _unit != msg.Unit)
            {
                if (_unit)
                {
                    _unit.UnsubscribeFromAllMessagesWithFilter(FILTER);
                }
                _unit = msg.Unit;
                if (_unit)
                {
                    RefreshUnit(_unit);
                    _unit.SubscribeWithFilter<ResetCommandCardMessage>(ResetCommandCard, FILTER);
                }
                else
                {
                    Clear();
                }
            }
        }

        private void ShowNewCommandTree(ShowNewCommandTreeMessage msg)
        {
            _commandTree.Add(msg.Command);
            ClearControllers();
            SetupControllers(msg.Command.Tree.SubCommands.ToArray(), _unit, false);
        }

        private void RemoveUnitCommandTreeLevel(RemoveCommandTreeLevelMessage msg)
        {
            if (_commandTree.Count > 0)
            {
                var commandInstance = _commandTree[_commandTree.Count - 1];

                _commandTree.RemoveAt(_commandTree.Count - 1);
                if (_commandTree.Count > 0)
                {
                    ClearControllers();
                    SetupControllers(_commandTree[_commandTree.Count - 1].Tree.SubCommands.ToArray(), _unit, false);
                }
                else
                {
                    ClearControllers();
                    SetupControllers(_baseTree, _unit, true);
                }

                if (commandInstance.DestroyOnClose)
                {
                    commandInstance.Destroy();
                }
            }
        }

        private void RefreshUnit(RefreshUnitMessage msg)
        {
            RefreshUnit(_unit);
        }

        private void ResetCommandCard(ResetCommandCardMessage msg)
        {
            RefreshUnit(_unit);
        }

        void OnDestroy()
        {
            if (_commandTree != null)
            {
                for (var i = 0; i < _commandTree.Count; i++)
                {
                    if (_commandTree[i].DestroyOnClose)
                    {
                        _commandTree[i].Destroy();
                    }
                }
                _commandTree.Clear();
            }

            if (_baseTree != null)
            {
                for (var i = 0; i < _baseTree.Length; i++)
                {
                    if (_baseTree[i].DestroyOnClose)
                    {
                        _baseTree[i].Destroy();
                    }
                }

                _baseTree = null;
            }
            
            gameObject.UnsubscribeFromAllMessages();
        }
    }
}
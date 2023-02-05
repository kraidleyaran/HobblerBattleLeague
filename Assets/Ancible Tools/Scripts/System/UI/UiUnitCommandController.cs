using System.Collections.Generic;
using Assets.Resources.Ancible_Tools.Scripts.System.UnitCommands;
using MessageBusLib;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace Assets.Resources.Ancible_Tools.Scripts.System.UI
{
    public class UiUnitCommandController : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
    {
        [SerializeField] private UiUnitCommandIconController _unitCommandIconTemplate;
        [SerializeField] private Button _button;

        private CommandInstance _command = null;
        private UiUnitCommandIconController[] _controllers = new UiUnitCommandIconController[0];
        private bool _hovered = false;

        public void Setup(CommandInstance command, GameObject unit)
        {
            _command = command;
            var controllers = new List<UiUnitCommandIconController>();
            for (var i = 0; i < command.Command.Icons.Length; i++)
            {
                var controller = Instantiate(_unitCommandIconTemplate, transform);
                controller.Setup(command.Command.Icons[i]);
                controllers.Add(controller);
            }

            _controllers = controllers.ToArray();
            if (_command.Tree.SubCommands.Count > 0)
            {
                _button.onClick.AddListener(ShowSubCommands);
            }
            else
            {
                _button.onClick.AddListener(() => { ExecuteCommand(unit); });
            }
        }

        private void ExecuteCommand(GameObject unit)
        {
            _command.Command.ApplyCommand(unit);
            gameObject.SendMessageTo(ResetCommandCardMessage.INSTANCE, unit);
        }

        private void ShowSubCommands()
        {
            var showNewCommandTreeMsg = MessageFactory.GenerateShowNewCommandTreeMsg();
            showNewCommandTreeMsg.Command = _command;
            gameObject.SendMessage(showNewCommandTreeMsg);                
            MessageFactory.CacheMessage(showNewCommandTreeMsg);
        }

        public void OnPointerEnter(PointerEventData eventData)
        {
            _hovered = true;
            var showHoverinfoMsg = MessageFactory.GenerateShowHoverInfoMsg();
            showHoverinfoMsg.Icon = _command.Command.Icons[0].Sprite;
            showHoverinfoMsg.ColorMask = _command.Command.Icons[0].ColorMask;
            showHoverinfoMsg.Title = _command.Command.Command;
            showHoverinfoMsg.Description = _command.Command.Description;
            showHoverinfoMsg.Position = transform.position.ToVector2();
            showHoverinfoMsg.World = false;
            showHoverinfoMsg.Owner = gameObject;
            showHoverinfoMsg.Gold = _command.Command.GoldValue;
            gameObject.SendMessage(showHoverinfoMsg);
            MessageFactory.CacheMessage(showHoverinfoMsg);
        }

        public void OnPointerExit(PointerEventData eventData)
        {
            if (_hovered)
            {
                _hovered = false;
                var removeHoveredInfoMsg = MessageFactory.GenerateRemoveHoverInfoMsg();
                removeHoveredInfoMsg.Owner = gameObject;
                gameObject.SendMessage(removeHoveredInfoMsg);
                MessageFactory.CacheMessage(removeHoveredInfoMsg);
            }
        }

        void OnDestroy()
        {
            if (_hovered)
            {
                var removeHoveredInfoMsg = MessageFactory.GenerateRemoveHoverInfoMsg();
                removeHoveredInfoMsg.Owner = gameObject;
                gameObject.SendMessage(removeHoveredInfoMsg);
                MessageFactory.CacheMessage(removeHoveredInfoMsg);
            }
        }
    }
}
using Assets.Resources.Ancible_Tools.Scripts.System;
using Assets.Resources.Ancible_Tools.Scripts.System.BattleLeague;
using MessageBusLib;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace Assets.Ancible_Tools.Scripts.System.UI.BattleLeague.Roster
{
    public class UiRosterUnitController : MonoBehaviour, IPointerEnterHandler,IPointerExitHandler
    {
        [SerializeField] private Image _unitIconImage = null;
        [SerializeField] private Image _borderImage = null;

        public BattleUnitData Data { get; private set; }

        private bool _hovered = false;

        public void Setup(BattleUnitData data, Color color)
        {
            Data = data;
            _unitIconImage.sprite = Data.Sprite.Sprite;
            _borderImage.color = color;
        }

        public void OnPointerEnter(PointerEventData eventData)
        {
            if (!_hovered)
            {
                _hovered = true;
                var showHoveredInfoMsg = MessageFactory.GenerateShowHoverInfoMsg();
                showHoveredInfoMsg.Icon = Data.Sprite.Sprite;
                showHoveredInfoMsg.ColorMask = Color.white;
                showHoveredInfoMsg.Description = Data.GetResultsDescription();
                showHoveredInfoMsg.Owner = gameObject;
                showHoveredInfoMsg.World = false;
                showHoveredInfoMsg.Position = transform.position.ToVector2();
                showHoveredInfoMsg.Title = Data.Name;
                gameObject.SendMessage(showHoveredInfoMsg);
                MessageFactory.CacheMessage(showHoveredInfoMsg);
            }
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
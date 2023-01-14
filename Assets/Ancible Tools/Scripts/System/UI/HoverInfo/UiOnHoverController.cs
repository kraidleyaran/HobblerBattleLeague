using MessageBusLib;
using UnityEngine;
using UnityEngine.EventSystems;

namespace Assets.Resources.Ancible_Tools.Scripts.System.UI.HoverInfo
{
    public class UiOnHoverController : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
    {
        [SerializeField] private string _title;
        [SerializeField] [TextArea(2, 5)] private string _description;
        [SerializeField] private Sprite _icon = null;

        private bool _hovered = false;

        public void OnPointerEnter(PointerEventData eventData)
        {
            if (!_hovered)
            {
                _hovered = true;
                var showHoverInfoMsg = MessageFactory.GenerateShowHoverInfoMsg();
                showHoverInfoMsg.Title = _title;
                showHoverInfoMsg.Description = _description;
                showHoverInfoMsg.Icon = _icon;
                showHoverInfoMsg.Owner = gameObject;
                gameObject.SendMessage(showHoverInfoMsg);
                MessageFactory.CacheMessage(showHoverInfoMsg);
            }
        }

        public void OnPointerExit(PointerEventData eventData)
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
                var removeHoverInfoMsg = MessageFactory.GenerateRemoveHoverInfoMsg();
                removeHoverInfoMsg.Owner = gameObject;
                gameObject.SendMessage(removeHoverInfoMsg);
                MessageFactory.CacheMessage(removeHoverInfoMsg);
            }
        }
    }
}
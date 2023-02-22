using Assets.Resources.Ancible_Tools.Scripts.System.Combat;
using MessageBusLib;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace Assets.Resources.Ancible_Tools.Scripts.System.UI.BattleLeague
{
    public class UiBattleBasicAttackInfoController : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
    {
        

        [SerializeField] private Sprite _basicAttackIcon;
        [SerializeField] private Color _colorMask = Color.red;

        private BasicAttackSetup _basicAttack = null;
        private bool _hovered = false;

        public void Setup(BasicAttackSetup basicAttack)
        {
            _basicAttack = basicAttack;
            UpdateBasicAttack(_basicAttack);
            //var queryBasicAttackMsg = MessageFactory.GenerateQueryBasicAttackSetupMsg();
            //queryBasicAttackMsg.DoAfter = UpdateBasicAttack;
            //gameObject.SendMessageTo(queryBasicAttackMsg, _owner);
            //MessageFactory.CacheMessage(queryBasicAttackMsg);
            gameObject.SetActive(true);
        }

        public void Clear()
        {
            _basicAttack = null;
            if (_hovered)
            {
                _hovered = false;
                var removeHoverInfoMsg = MessageFactory.GenerateRemoveHoverInfoMsg();
                removeHoverInfoMsg.Owner = gameObject;
                gameObject.SendMessage(removeHoverInfoMsg);
                MessageFactory.CacheMessage(removeHoverInfoMsg);
            }

            gameObject.SetActive(false);
        }

        private void UpdateBasicAttack(BasicAttackSetup setup)
        {
            _basicAttack = setup;
            if (_hovered)
            {
                var showHoverInfoMsg = MessageFactory.GenerateShowHoverInfoMsg();
                showHoverInfoMsg.Title = "Basic Attack";
                showHoverInfoMsg.Description = _basicAttack.GetDescription();
                showHoverInfoMsg.Icon = _basicAttackIcon;
                showHoverInfoMsg.ColorMask = _colorMask;
                showHoverInfoMsg.Owner = gameObject;
                gameObject.SendMessage(showHoverInfoMsg);
                MessageFactory.CacheMessage(showHoverInfoMsg);
            }

        }

        public void OnPointerEnter(PointerEventData eventData)
        {
            if (!_hovered)
            {
                _hovered = true;
                var showHoverInfoMsg = MessageFactory.GenerateShowHoverInfoMsg();
                showHoverInfoMsg.Title = "Basic Attack";
                showHoverInfoMsg.Description = _basicAttack.GetDescription();
                showHoverInfoMsg.Icon = _basicAttackIcon;
                showHoverInfoMsg.ColorMask = _colorMask;
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
                _hovered = false;
                var removeHoverInfoMsg = MessageFactory.GenerateRemoveHoverInfoMsg();
                removeHoverInfoMsg.Owner = gameObject;
                gameObject.SendMessage(removeHoverInfoMsg);
                MessageFactory.CacheMessage(removeHoverInfoMsg);
            }
        }
    }
}
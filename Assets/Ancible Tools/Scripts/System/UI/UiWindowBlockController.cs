using Assets.Resources.Ancible_Tools.Scripts.System.Windows;
using UnityEngine;
using UnityEngine.EventSystems;

namespace Assets.Resources.Ancible_Tools.Scripts.System.UI
{
    public class UiWindowBlockController : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
    {
        private bool _hovered = false;

        public void OnPointerEnter(PointerEventData eventData)
        {
            _hovered = true;
            UiWindowManager.RegisterWindowBlock(gameObject);
        }

        public void OnPointerExit(PointerEventData eventData)
        {
            _hovered = false;
            UiWindowManager.RemoveWindowBlock(gameObject);
        }

        void OnDisable()
        {
            if (_hovered)
            {
                UiWindowManager.RemoveWindowBlock(gameObject);
            }
        }

    }
}
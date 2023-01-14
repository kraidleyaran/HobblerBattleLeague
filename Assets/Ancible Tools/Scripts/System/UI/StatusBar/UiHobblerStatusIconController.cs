using Assets.Resources.Ancible_Tools.Scripts.System;
using UnityEngine;
using UnityEngine.UI;

namespace Assets.Ancible_Tools.Scripts.System.UI.StatusBar
{
    public class UiHobblerStatusIconController : MonoBehaviour
    {
        [SerializeField] private Image _imageIcon;

        public void Setup(StatusIcon icon)
        {
            _imageIcon.sprite = icon.Icon;
            _imageIcon.color = icon.Color;
        }
    }
}
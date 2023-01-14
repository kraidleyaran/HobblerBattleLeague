using Assets.Resources.Ancible_Tools.Scripts.System.UnitCommands;
using UnityEngine;
using UnityEngine.UI;

namespace Assets.Resources.Ancible_Tools.Scripts.System.UI
{
    public class UiUnitCommandIconController : MonoBehaviour
    {
        [SerializeField] private Image _iconImage = null;

        public void Setup(CommandIcon icon)
        {
            _iconImage.sprite = icon.Sprite;
            _iconImage.color = icon.ColorMask;
            _iconImage.transform.SetLocalPosition(icon.Offset).FlipX(icon.FlipX).FlipY(icon.FlipY);
        }
    }
}
using UnityEngine;
using UnityEngine.UI;

namespace Assets.Resources.Ancible_Tools.Scripts.System.UI
{
    public class UiFillBarController : MonoBehaviour
    {
        [SerializeField] private RectTransform _rectTransform;
        [SerializeField] private RectTransform.Axis _fillAxis = RectTransform.Axis.Horizontal;
        [SerializeField] private Image _maskImage;
        [SerializeField] private Image _fillImage;
        [SerializeField] private Text _text = null;

        public void Setup(float percent, string text, Color color)
        {
            var fillPercent = Mathf.Min(Mathf.Max(0, percent), 1);
            _fillImage.color = color;
            _maskImage.rectTransform.SetSizeWithCurrentAnchors(_fillAxis, fillPercent * _rectTransform.rect.size.x);
            _text.text = text;
        }

        public void Clear()
        {
            _maskImage.rectTransform.SetSizeWithCurrentAnchors(_fillAxis, 0);
            _text.text = string.Empty;
            _fillImage.color = Color.white;
        }
    }
}
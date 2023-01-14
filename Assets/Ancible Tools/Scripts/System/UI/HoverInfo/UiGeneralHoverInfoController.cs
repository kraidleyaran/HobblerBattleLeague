﻿using UnityEngine;
using UnityEngine.UI;

namespace Assets.Resources.Ancible_Tools.Scripts.System.UI.HoverInfo
{
    public class UiGeneralHoverInfoController : MonoBehaviour
    {
        [SerializeField] private Image _iconImage = null;
        [SerializeField] private Text _titleText = null;
        [SerializeField] private Text _descriptionText = null;
        [SerializeField] private RectTransform _iconTitleGroup;
        [SerializeField] private RectTransform _rectTransform = null;
        [SerializeField] private RectOffset _padding = new RectOffset();
        [SerializeField] private VerticalLayoutGroup _grid;
        [SerializeField] private float _maxWidth = 0f;

        public void Setup(Sprite icon, string title, string description)
        {
            _iconImage.sprite = icon;
            _iconImage.gameObject.SetActive(_iconImage.sprite);
            _titleText.text = title;
            var width = Mathf.Max((float)title.Length * _titleText.fontSize + _iconImage.rectTransform.rect.width, (float)description.Length * _descriptionText.fontSize);
            width =  Mathf.Min(_maxWidth, width + _padding.left + _padding.right);
            if (!_iconImage.sprite)
            {
                _titleText.rectTransform.SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, _iconImage.rectTransform.rect.width + _titleText.rectTransform.rect.width);
            }
            _descriptionText.text = description;
            var descriptionHeight = _descriptionText.GetHeightOfText(description);
            Debug.Log($"Description Height: {descriptionHeight}");
            _descriptionText.rectTransform.SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, descriptionHeight);
            var height = descriptionHeight + _iconTitleGroup.rect.height + _padding.top + _padding.bottom;
            if (descriptionHeight > 0f)
            {
                height += _grid.spacing * 2;
            }
            Debug.Log($"Total Height: {height}");
            _rectTransform.SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, Mathf.Round(height + 1));
            _rectTransform.SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, width);
        }

        public void SetPivot(Vector2 pivot)
        {
            _rectTransform.pivot = pivot;
        }

        public void Clear()
        {
            _iconImage.sprite = null;
            _titleText.text = string.Empty;
            _descriptionText.text = string.Empty;
            _rectTransform.pivot = Vector2.zero;
        }
    }
}
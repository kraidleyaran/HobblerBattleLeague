using UnityEngine;
using UnityEngine.UI;

namespace Assets.Resources.Ancible_Tools.Scripts.System
{
    public class UnitSelectorController : MonoBehaviour
    {
        [SerializeField] private Vector2 _defaultSize = new Vector2(31.25f, 31.25f);
        [SerializeField] private bool _setSprite = false;
        [SerializeField] private SpriteRenderer _sprite = null;

        void Awake()
        {
            SetSize(_defaultSize);
        }

        public void SetSize(Vector2 size)
        {
            if (_setSprite)
            {
                _sprite.size = size;
            }
            else
            {
                var scale = transform.localScale;
                scale.x = size.x;
                scale.y = size.y;
                transform.localScale = scale;
            }

        }

        public void SetParent(Transform parent, Vector2 offSet)
        {
            transform.SetParent(parent);
            var localPos = transform.localPosition;
            localPos.x = offSet.x;
            localPos.y = offSet.y;
            transform.localPosition = localPos;

            gameObject.layer = parent.gameObject.layer;
        }

        public void SetParent(Transform parent, Vector2 offset, Vector2 size)
        {
            SetParent(parent, offset);
            SetSize(size);
        }

        public void ResetSelector(Transform parent)
        {
            SetParent(parent, Vector2.zero);
            SetSize(_defaultSize);
        }
    }
}
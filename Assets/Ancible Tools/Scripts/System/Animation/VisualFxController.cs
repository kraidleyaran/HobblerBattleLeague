using Assets.Resources.Ancible_Tools.Scripts.Hitbox;
using MessageBusLib;
using UnityEngine;

namespace Assets.Resources.Ancible_Tools.Scripts.System.Animation
{
    public class VisualFxController : MonoBehaviour
    {
        [SerializeField] private Animator _animator = null;
        [SerializeField] private SpriteRenderer _renderer = null;

        private int _loopCount = 0;
        private int _maxLoops = 0;
        private GameObject _parent = null;

        public void Setup(RuntimeAnimatorController runtime, GameObject parent, int loop = 0)
        {
            _maxLoops = loop;
            _animator.runtimeAnimatorController = runtime;
            _animator.Play(0);
            _parent = parent;
        }

        public void SetLayer(CollisionLayer layer)
        {
            gameObject.layer = layer.ToLayer();
        }

        public void SetLayer(int layer)
        {
            gameObject.layer = layer;
        }

        public void SetSpriteLayer(SpriteLayer layer, int order)
        {
            _renderer.sortingLayerID = layer.Id;
            _renderer.sortingOrder = order;
        }

        public void FxAnimationFinished()
        {
            if (_maxLoops > -1)
            {
                _loopCount++;
                if (_loopCount > _maxLoops)
                {
                    if (_parent)
                    {
                        gameObject.SendMessageTo(FxAnimationFinishedMessage.INSTANCE, _parent);
                    }
                    else
                    {
                        Destroy(gameObject);
                    }
                }
                else
                {
                    _animator.Play(0);
                }
            }
        }

        public void FlipX(bool flip)
        {
            _renderer.flipX = flip;
        }

        public void FlipY(bool flip)
        {
            _renderer.flipY = flip;
        }
    }
}
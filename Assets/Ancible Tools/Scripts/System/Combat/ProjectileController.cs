using System;
using System.Linq;
using Assets.Ancible_Tools.Scripts.Traits;
using MessageBusLib;
using UnityEngine;

namespace Assets.Resources.Ancible_Tools.Scripts.System.Combat
{
    public class ProjectileController : MonoBehaviour
    {
        [SerializeField] private Rigidbody2D _rigidBody = null;
        [SerializeField] private float _detectDistance = 0f;
        [SerializeField] private SpriteRenderer _spriteRenderer = null;
        [SerializeField] private Animator _animator = null;

        private GameObject _owner = null;
        private GameObject _target = null;
        private int _pixelsPerSecond = 0;
        private Trait[] _applyOnContact = new Trait[0];
        private Action _doAfter = null;
        private bool _active = false;
        private bool _rotate = false;
        private float _rotationOffset = 0f;


        public void Setup(SpriteTrait trait, Trait[] applyOnContact, int pixelsPerSecond, GameObject target, GameObject owner, Action doAfter = null)
        {
            _owner = owner;
            _target = target;
            _pixelsPerSecond = pixelsPerSecond;
            _applyOnContact = applyOnContact.ToArray();
            _doAfter = doAfter;
            Transform spriteTransform = null;
            if (trait.RuntimeController)
            {
                spriteTransform = _animator.transform;
                _animator.runtimeAnimatorController = trait.RuntimeController;
            }
            else
            {
                spriteTransform = _spriteRenderer.transform;
                _spriteRenderer.sprite = trait.Sprite;
            }

            spriteTransform.SetLocalScaling(trait.Scaling);
            spriteTransform.SetLocalPosition(trait.Offset);
            _spriteRenderer.color = trait.ColorMask;
            _spriteRenderer.sortingOrder = trait.SortingOrder;
            _spriteRenderer.sortingLayerID = trait.SpriteLayer.Id;
            gameObject.layer = owner.layer;
            _spriteRenderer.gameObject.layer = owner.layer;
            SubscribeToMessages();
        }

        public void SetRotation(bool rotate, float rotationOffset)
        {
            _rotate = rotate;
            _rotationOffset = _rotate ? rotationOffset : 0f;
            if (_rotate)
            {
                var targetPos = _target.transform.position.ToVector2();
                var pos = _rigidBody.position;
                var diff = (targetPos - pos);
                _spriteRenderer.transform.localRotation = Quaternion.Euler(0f,0f, diff.normalized.ToZRotation() + _rotationOffset);
                
                //_rigidBody.rotation = _rotationOffset + diff.normalized.ToZRotation();
            }
        }

        private void SubscribeToMessages()
        {
            if (!_active)
            {
                _active = true;
                //gameObject.Subscribe<FixedUpdateTickMessage>(FixedUpdateTick);
                gameObject.Subscribe<UpdateTickMessage>(UpdateTick);
            }
            
        }

        private void UpdateTick(UpdateTickMessage msg)
        {
            var targetPos = _target.transform.position.ToVector2();
            var pos = _rigidBody.position;
            var diff = (targetPos - pos);
            var speed = TickController.TickRate * (_pixelsPerSecond * DataController.Interpolation);
            if (diff.magnitude > speed + _detectDistance)
            {
                var direction = diff.normalized;
                _rigidBody.position += speed * direction;
                if (_rotate)
                {
                    var rotation = direction.ToZRotation() + _rotationOffset;
                    Debug.Log($"Projectile Rotation: {rotation}");
                    _spriteRenderer.transform.localRotation = Quaternion.Euler(0f, 0f, rotation);
                }
            }
            else
            {
                var addTraitToUnitMsg = MessageFactory.GenerateAddTraitToUnitMsg();
                for (var i = 0; i < _applyOnContact.Length; i++)
                {
                    addTraitToUnitMsg.Trait = _applyOnContact[i];
                    _owner.SendMessageTo(addTraitToUnitMsg, _target);
                }
                MessageFactory.CacheMessage(addTraitToUnitMsg);
                _doAfter.Invoke();
            }
        }

        private void FixedUpdateTick(FixedUpdateTickMessage msg)
        {
            var targetPos = _target.transform.position.ToVector2();
            var pos = _rigidBody.position;
            var diff = (targetPos - pos);
            var speed = TickController.OneSecond / _pixelsPerSecond * Time.fixedDeltaTime;
            if (diff.magnitude > speed + _detectDistance)
            {
                _rigidBody.position += speed * diff.normalized;
            }
            else
            {
                var addTraitToUnitMsg = MessageFactory.GenerateAddTraitToUnitMsg();
                for (var i = 0; i < _applyOnContact.Length; i++)
                {
                    addTraitToUnitMsg.Trait = _applyOnContact[i];
                    _owner.SendMessageTo(addTraitToUnitMsg, _target);
                }
                MessageFactory.CacheMessage(addTraitToUnitMsg);
                _doAfter.Invoke();
            }
        }

        public void Destroy()
        {
            if (_active)
            {
                _doAfter = null;
                _owner = null;
                _target = null;
                _applyOnContact = new Trait[0];
                _pixelsPerSecond = 0;
                gameObject.UnsubscribeFromAllMessages();
            }
        }

        void OnDestroy()
        {
            Destroy();
        }
    }
}
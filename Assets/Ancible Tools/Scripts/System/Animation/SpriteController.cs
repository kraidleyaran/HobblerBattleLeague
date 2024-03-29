﻿using Assets.Ancible_Tools.Scripts.Traits;
using UnityEngine;

namespace Assets.Resources.Ancible_Tools.Scripts.System.Animation
{
    public class SpriteController : MonoBehaviour
    {
        private const string UNIT_ANIMATION_STATE = "Unit Animation State";
        private const string X = "X";
        private const string Y = "Y";
        private const string ATTACK_STATE = "Attack State";

        [SerializeField] private SpriteRenderer _spriteRenderer;
        [SerializeField] private Animator _animator;

        public void SetRuntimeController(RuntimeAnimatorController runtime)
        {
            _animator.runtimeAnimatorController = runtime;
        }

        public void SetSprite(Sprite sprite)
        {
            _spriteRenderer.sprite = sprite;
        }

        public void SetScale(Vector2 scale)
        {
            var localScale = transform.localScale;
            localScale.x = scale.x;
            localScale.y = scale.y;
            transform.localScale = localScale;
        }

        public void SetSortingLayerFromSpriteLayer(SpriteLayer layer)
        {
            _spriteRenderer.sortingLayerID = layer.Id;
        }

        public void SetSortingOrder(int order)
        {
            _spriteRenderer.sortingOrder = order;
        }

        //public void SetUnitAnimationState(UnitAnimationState state)
        //{
        //    if (_animator.runtimeAnimatorController)
        //    {
        //        _animator.SetFloat(UNIT_ANIMATION_STATE, (float)state);
        //        _animator.Play(0);
        //    }

        //}

        public void SetDirection(Vector2 direction)
        {
            if (_animator.runtimeAnimatorController && gameObject.activeSelf)
            {
                _animator.SetFloat(X, direction.x);
                _animator.SetFloat(Y, direction.y);
            }

        }

        public void SetPause(bool pause)
        {
            if (_animator.runtimeAnimatorController && gameObject.activeSelf)
            {
                _animator.speed = pause ? 0f : 1f;
            }
        }

        public void SetMaterial(Material material)
        {
            _spriteRenderer.material = material;
        }

        public void SetSpriteChangeColor(Color color)
        {
            _spriteRenderer.material.SetColor("_ChangeColor", color);
        }

        public void ShowChangeColor(bool show)
        {
            _spriteRenderer.material.SetInt( "_Active",show ? 1 : 0);
        }

        public void SetSpriteDrawMode(SpriteDrawMode mode)
        {
            _spriteRenderer.drawMode = mode;
        }

        public void SetWidth(float width)
        {
            var size = _spriteRenderer.size;
            size.x = width;
            _spriteRenderer.size = size;
        }

        public void SetHeight(float height)
        {
            var size = _spriteRenderer.size;
            size.y = height;
            _spriteRenderer.size = size;
        }

        public void FlipX(bool flip)
        {
            _spriteRenderer.flipX = flip;
        }

        public void FlipY(bool flip)
        {
            _spriteRenderer.flipY = flip;
        }

        public void SetOffset(Vector2 offset, bool pixelPerfect = true)
        {
            transform.SetLocalPosition(pixelPerfect ? offset.ToPixelPerfect() : offset);
        }

        public void SetColorMask(Color color)
        {
            _spriteRenderer.color = color;
        }

        public void SetRotation(float rotation)
        {
            var localRotation = transform.localRotation.eulerAngles;
            localRotation.z = rotation;
            transform.localRotation = Quaternion.Euler(localRotation);
        }

        public void SetFromTrait(SpriteTrait trait, bool pixelPerfect = true)
        {
            SetSprite(trait.Sprite);
            SetScale(trait.Scaling);
            if (trait.RuntimeController)
            {
                SetRuntimeController(trait.RuntimeController);
            }
            SetOffset(trait.Offset, pixelPerfect);
            SetSortingOrder(trait.SortingOrder);
        }

        public void SetFromEditor(SpriteTrait trait)
        {
#if UNITY_EDITOR
            SetSprite(trait.Sprite);
            SetScale(trait.Scaling);
            SetOffset(trait.Offset, false);
            SetColorMask(trait.ColorMask);
#endif
        }

        public void SetAlpha(float alpha)
        {
            var color = _spriteRenderer.color;
            color.a = alpha;
            _spriteRenderer.color = color;
        }

        public void SetUnitAnimationState(UnitAnimationState state)
        {
            _animator.SetFloat(UNIT_ANIMATION_STATE, (float)state);
        }
    }
}
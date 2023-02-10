using System;
using Assets.Resources.Ancible_Tools.Scripts.System.Minigame;
using DG.Tweening;
using UnityEngine;

namespace Assets.Resources.Ancible_Tools.Scripts.System.Animation
{
    public class ParticleFxController : MonoBehaviour
    {
        [SerializeField] private float _afterTime = 0f;

        private Action _doAfter = null;
        private ParticleSystem _particleSystem = null;
        private Sequence _particleSequence = null;
        private bool _alone = false;

        public void Setup(ParticleSystem system, Action doAfter, Vector2 offset, bool alone, int layer)
        {
            _doAfter = doAfter;
            _alone = alone;
            _particleSystem = Instantiate(system, transform);
            _particleSystem.transform.SetLocalPosition(offset);
            _particleSystem.SetObjectLayer(layer);
            _particleSequence = DOTween.Sequence().AppendInterval(_particleSystem.main.duration + _afterTime).OnComplete(SequenceFinished);
        }

        private void SequenceFinished()
        {
            _particleSequence = null;
            _doAfter?.Invoke();
            if (_alone)
            {
                Destroy(gameObject);
            }
        }

        void OnDestroy()
        {
            if (_particleSequence != null)
            {
                if (_particleSequence.IsActive())
                {
                    _particleSequence.Kill();
                }

                _particleSequence = null;
            }
        }
    }
}
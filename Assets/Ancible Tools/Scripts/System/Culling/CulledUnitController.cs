using Assets.Resources.Ancible_Tools.Scripts.Hitbox;
using MessageBusLib;
using UnityEngine;

namespace Assets.Resources.Ancible_Tools.Scripts.System.Culling
{
    public class CulledUnitController : MonoBehaviour
    {
        private GameObject _culledUnit = null;
        private HitboxController _hitboxController = null;
        private Rigidbody2D _rigidBody = null;

        void Awake()
        {
            _rigidBody = gameObject.GetComponent<Rigidbody2D>();
            var filter = $"{gameObject.GetInstanceID()}";
            SubscribeToMessages(filter);
        }

        public void Setup(GameObject unit, Hitbox.Hitbox hitbox)
        {
            _culledUnit = unit;
            _hitboxController = gameObject.SetupHitbox(hitbox, CollisionLayerFactory.UnitCulling);
            var registerCollisionMsg = MessageFactory.GenerateRegisterCollisionMsg();
            registerCollisionMsg.Object = _culledUnit;
            gameObject.SendMessageTo(registerCollisionMsg, _hitboxController.gameObject);
            MessageFactory.CacheMessage(registerCollisionMsg);
            _rigidBody.position = _culledUnit.transform.position.ToVector2();
            gameObject.SetActive(true);
        }

        public bool IsCulled()
        {
            return CullingManager.IsCulled(_hitboxController.Collider2d);
        }

        private void SubscribeToMessages(string filter)
        {
            gameObject.SubscribeWithFilter<CullingCheckMessage>(CullingCheck, filter);
        }

        private void CullingCheck(CullingCheckMessage msg)
        {
            if (_culledUnit)
            {
                gameObject.SendMessageTo(msg, _culledUnit);
            }
        }

        public void Disable()
        {
            _hitboxController.Destroy();
            Destroy(_hitboxController.gameObject);
            gameObject.SetActive(false);
        }
    }
}
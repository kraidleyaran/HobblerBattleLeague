using Assets.Resources.Ancible_Tools.Scripts.Hitbox;
using MessageBusLib;
using UnityEngine;

namespace Assets.Resources.Ancible_Tools.Scripts.System.Culling
{
    public class CullingController : MonoBehaviour
    {
        [SerializeField] private HitboxController _hitboxController;

        public void Setup(string filter)
        {
            _hitboxController.Setup(CollisionLayerFactory.Culling);
            _hitboxController.AddSubscriber(gameObject);
            SubscribeToMessages(filter);
            CullingManager.RegisterController(this);
        }

        public bool ContainsObject(Collider2D col)
        {
            return _hitboxController.IsTouching(col);
        }

        private void SubscribeToMessages(string filter)
        {
            gameObject.SubscribeWithFilter<EnterCollisionWithObjectMessage>(EnterCollisionWithObject, filter);
            gameObject.SubscribeWithFilter<ExitCollisionWithObjectMessage>(ExitCollisionWithObject, filter);
        }

        private void EnterCollisionWithObject(EnterCollisionWithObjectMessage msg)
        {
            gameObject.SendMessageTo(CullingCheckMessage.INSTANCE, msg.Object);
        }

        private void ExitCollisionWithObject(ExitCollisionWithObjectMessage msg)
        {
            gameObject.SendMessageTo(CullingCheckMessage.INSTANCE, msg.Object);
        }

        public void Destroy()
        {
            gameObject.UnsubscribeFromAllMessages();
            _hitboxController.Destroy();
            Destroy(_hitboxController.gameObject);
            CullingManager.UnregisterController(this);
        }
    }
}
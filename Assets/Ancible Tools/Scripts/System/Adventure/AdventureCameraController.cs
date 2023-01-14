using MessageBusLib;
using UnityEngine;

namespace Assets.Resources.Ancible_Tools.Scripts.System.Adventure
{
    public class AdventureCameraController : MonoBehaviour
    {
        public static Camera Camera => _instance._camera;

        private static AdventureCameraController _instance = null;

        [SerializeField] private Camera _camera = null;

        void Awake()
        {
            if (_instance)
            {
                Destroy(gameObject);
                return;
            }

            _instance = this;
            gameObject.SetActive(false);
            SubscribeToMessages();
        }

        public static void SetCameraPosition(Vector2 pos)
        {
            _instance.gameObject.transform.SetTransformPosition(pos);
        }

        private void SubscribeToMessages()
        {
            gameObject.Subscribe<UpdateWorldStateMessage>(UpdateWorldState);
        }

        private void UpdateWorldState(UpdateWorldStateMessage msg)
        {
            gameObject.SetActive(msg.State == WorldState.Adventure);
        }

        void OnDestroy()
        {
            if (_instance && _instance == this)
            {
                _instance = null;
                gameObject.UnsubscribeFromAllMessages();
            }
        }
    }
}
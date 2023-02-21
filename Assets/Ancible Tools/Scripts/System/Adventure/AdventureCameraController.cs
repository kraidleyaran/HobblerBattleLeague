using MessageBusLib;
using UnityEngine;

namespace Assets.Resources.Ancible_Tools.Scripts.System.Adventure
{
    public class AdventureCameraController : MonoBehaviour
    {
        public static Camera Camera => _instance._camera;

        private static AdventureCameraController _instance = null;

        [SerializeField] private Camera _camera = null;

        private bool _moved = false;
        private bool _correct = false;

        private Vector2 _position = Vector2.zero;

        void Awake()
        {
            if (_instance)
            {
                Destroy(gameObject);
                return;
            }

            _instance = this;
            _position = transform.position.ToVector2();
            gameObject.SetActive(false);
            SubscribeToMessages();
        }

        void LateUpdate()
        {
            //transform.SetTransformPosition(Vector2.Lerp(transform.position.ToVector2(), _position, DataController.Interpolation));
            //if (_moved)
            //{
            //    _moved = false;
            //}
            //else if (_correct)
            //{
            //    _correct = false;
            //    _position = _position.ToPixelPerfect();
            //    transform.SetTransformPosition(_position);
            //}
        }

        public static void SetCameraPosition(Vector2 pos, bool lerp = true)
        {
            _instance._position = pos;
            if (!lerp)
            {
                _instance.transform.SetTransformPosition(_instance._position);
            }
            //_instance.gameObject.transform.SetTransformPosition(Vector2.Lerp(_instance.transform.position.ToVector2(), pos, DataController.Interpolation));
            //_instance._moved = true;
            //_instance._correct = true;
        }

        private void SubscribeToMessages()
        {
            gameObject.Subscribe<UpdateWorldStateMessage>(UpdateWorldState);
            gameObject.Subscribe<UpdateTickMessage>(UpdateTick);
        }

        private void UpdateWorldState(UpdateWorldStateMessage msg)
        {
            gameObject.SetActive(msg.State == WorldState.Adventure);
        }

        private void UpdateTick(UpdateTickMessage msg)
        {
            transform.SetTransformPosition(Vector2.Lerp(transform.position.ToVector2(), _position, DataController.Interpolation).ToPixelPerfect());
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
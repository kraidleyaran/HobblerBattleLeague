using MessageBusLib;
using UnityEngine;

namespace Assets.Resources.Ancible_Tools.Scripts.System.Minigame
{
    public class MinigameCameraController : MonoBehaviour
    {
        public static Camera Camera => _instance._camera;

        private static MinigameCameraController _instance = null;

        [SerializeField] private Camera _camera;

        private Vector2 _position = Vector2.zero;

        void Awake()
        {
            if (_instance)
            {
                Destroy(gameObject);
                return;
            }

            _instance = this;
            if (Camera.main)
            {
                gameObject.SetActive(false);
            }
            else
            {
                Camera.SetupCurrent(Camera);
            }

            _position = transform.position.ToVector2();
            SubscribeToMessages();
        }

        private void SubscribeToMessages()
        {
            gameObject.Subscribe<SetMinigameCameraPositionMessage>(SetMinigameCameraPosition);
            gameObject.Subscribe<UpdateWorldStateMessage>(UpdateWorldState);
            gameObject.Subscribe<UpdateTickMessage>(UpdateTick);
        }

        private void SetMinigameCameraPosition(SetMinigameCameraPositionMessage msg)
        {
            _position = msg.Position;
        }

        private void UpdateWorldState(UpdateWorldStateMessage msg)
        {
            gameObject.SetActive(msg.State == WorldState.Minigame);
        }

        private void UpdateTick(UpdateTickMessage msg)
        {
            if (WorldController.State == WorldState.Minigame)
            {
                var pos = transform.position.ToVector2();
                if (pos != _position)
                {
                    transform.SetTransformPosition(Vector2.Lerp(pos, _position, DataController.Interpolation));
                }
            }
        }

        public static void SetActive(bool active)
        {
            _instance.gameObject.SetActive(active);
        }

        void OnDestroy()
        {
            if (_instance == this)
            {
                gameObject.UnsubscribeFromAllMessages();
                _instance = null;
            }
        }
    }
}
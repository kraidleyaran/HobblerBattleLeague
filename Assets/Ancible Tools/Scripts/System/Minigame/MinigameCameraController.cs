using MessageBusLib;
using UnityEngine;

namespace Assets.Resources.Ancible_Tools.Scripts.System.Minigame
{
    public class MinigameCameraController : MonoBehaviour
    {
        public static Camera Camera => _instance._camera;

        private static MinigameCameraController _instance = null;

        [SerializeField] private Camera _camera;

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
            SubscribeToMessages();
        }

        void LateUpdate()
        {
            var pos = transform.position.ToVector2();
            var pixelPerfect = pos.ToPixelPerfect();
            if (pixelPerfect != pos)
            {
                transform.SetTransformPosition(pixelPerfect);
            }
            
        }

        private void SubscribeToMessages()
        {
            gameObject.Subscribe<SetMinigameCameraPositionMessage>(SetMinigameCameraPosition);
            gameObject.Subscribe<UpdateWorldStateMessage>(UpdateWorldState);
        }

        private void SetMinigameCameraPosition(SetMinigameCameraPositionMessage msg)
        {
            transform.SetTransformPosition(msg.Position);
        }

        private void UpdateWorldState(UpdateWorldStateMessage msg)
        {
            gameObject.SetActive(msg.State == WorldState.Minigame);
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
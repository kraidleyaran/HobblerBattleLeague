using Assets.Resources.Ancible_Tools.Scripts.System.Building;
using Assets.Resources.Ancible_Tools.Scripts.System.UI.DragDrop;
using Assets.Resources.Ancible_Tools.Scripts.System.Windows;
using MessageBusLib;
using UnityEngine;

namespace Assets.Resources.Ancible_Tools.Scripts.System
{
    public class WorldCameraController : MonoBehaviour
    {
        public static Camera Camera => _instance?._camera;
        public static bool Moving => _instance?._moving ?? false;

        [SerializeField] private Camera _camera;
        [SerializeField] private AudioListener _audioListener;
        [SerializeField] private float _cameraMoveSpeedMultiplier = 0f;
        [SerializeField] private Vector2Int _moveCameraOffset = Vector2Int.one;
        [SerializeField] private float _borderMoveSpeed = 2f;

        private static WorldCameraController _instance = null;
        private bool _moving = false;
        private Vector2Int _screen = new Vector2Int();

        void Awake()
        {
            if (_instance)
            {
                Destroy(gameObject);
                return;
            }

            _instance = this;
            _screen = new Vector2Int(Screen.width, Screen.height);

            SubscribeToMessages();
        }

        private Vector2 CameraBoundsDirection(Vector2 pos)
        {
            var direction = Vector2.zero;
            var min = _moveCameraOffset;
            var max = _screen - _moveCameraOffset;
            if (pos.x >= 0f && pos.x <= min.x)
            {
                direction.x = -1;
            }
            else if (pos.x <= _screen.x && pos.x >= max.x)
            {
                direction.x = 1;
            }

            if (pos.y >= 0f && pos.y <= min.y)
            {
                direction.y = -1;
            }
            else if (pos.y <= _screen.y && pos.y >= max.y)
            {
                direction.y = 1;
            }

            return direction;
        }

        private bool IsInScreenBounds(Vector2 pos)
        {
            return pos.x >= 0 && pos.x <= _screen.x && pos.y >= 0 && pos.y <= _screen.y;
        }

        private void SubscribeToMessages()
        {
            gameObject.Subscribe<UpdateWorldStateMessage>(UpdateWorldState);
            gameObject.Subscribe<UpdateInputStateMessage>(UpdateInputState);
        }

        private void UpdateWorldState(UpdateWorldStateMessage msg)
        {
            gameObject.SetActive(msg.State == WorldState.World);
        }

        private void UpdateInputState(UpdateInputStateMessage msg)
        {
            if (WorldController.State == WorldState.World && IsInScreenBounds(msg.Previous.MousePos))
            {
                var pos = transform.position.ToVector2();
                
                if (msg.Current.LeftClick && (_moving || !UiWindowManager.Hovered && !UiWindowManager.Moving && !UnitSelectController.Hovered && !WorldBuildingManager.Active && !UiDragDropManager.Active))
                {
                    _moving = true;
                    var delta = msg.Current.MousePos - msg.Previous.MousePos;
                    var movePos = pos +  delta * -1 * _cameraMoveSpeedMultiplier;
                    
                    if (pos != movePos)
                    {
                        transform.SetTransformPosition(movePos);
                    }
                }
                //else if (screenMove != Vector2.zero && !UiDragDropManager.Active)
                //{
                //    var movePos = pos + (screenMove * _borderMoveSpeed);
                //    transform.SetTransformPosition(movePos);
                //}
                else if (_moving)
                {
                    var transformPos = transform.position.ToVector2().ToPixelPerfect();
                    transform.SetTransformPosition(transformPos);
                    _moving = false;
                }
            }
        }

        void OnDestroy()
        {
            gameObject.UnsubscribeFromAllMessages();
        }

    }
}
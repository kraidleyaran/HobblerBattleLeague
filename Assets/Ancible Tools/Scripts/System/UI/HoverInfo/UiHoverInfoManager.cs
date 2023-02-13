using Assets.Resources.Ancible_Tools.Scripts.System.Building;
using MessageBusLib;
using UnityEngine;

namespace Assets.Resources.Ancible_Tools.Scripts.System.UI.HoverInfo
{
    public class UiHoverInfoManager : MonoBehaviour
    {
        private static UiHoverInfoManager _instance = null;

        [SerializeField] private UiGeneralHoverInfoController _generalInfoController = null;

        private GameObject _hoverOwner = null;
        private Vector2 _mousePos = Vector2.zero;
        private bool _world = false;

        void Awake()
        {
            if (_instance)
            {
                Destroy(gameObject);
                return;
            }

            _instance = this;
            _generalInfoController.Clear();
            _generalInfoController.gameObject.SetActive(false);
            SubscribeToMessages();
        }

        private void SubscribeToMessages()
        {
            gameObject.Subscribe<ShowHoverInfoMessage>(ShowHoverInfo);
            gameObject.Subscribe<RemoveHoverInfoMessage>(RemoveHoverInfo);
            gameObject.Subscribe<UpdateInputStateMessage>(UpdateInputState);
            gameObject.Subscribe<WorldBuildingActiveMessage>(WorldBuildingActive);
        }

        private void ShowHoverInfo(ShowHoverInfoMessage msg)
        {
            if (!WorldBuildingManager.Active)
            {
                _hoverOwner = msg.Owner;
                var screenPos = _mousePos.ToVector2Int(true);
                _world = msg.World;
                if (_world)
                {
                    screenPos = WorldController.GetCurrentCamera().WorldToScreenPoint(msg.Position).ToVector2().ToVector2Int(true);
                }
                var quadrant = StaticMethods.GetMouseQuadrant(screenPos);

                _generalInfoController.transform.SetLocalPosition(screenPos);
                _generalInfoController.Setup(msg.Icon, msg.Title, msg.Description, msg.Gold);
                _generalInfoController.SetColor(msg.ColorMask);
                _generalInfoController.SetPivot(quadrant);
                _generalInfoController.gameObject.SetActive(true);
            }
        }

        private void RemoveHoverInfo(RemoveHoverInfoMessage msg)
        {
            if (_hoverOwner && msg.Owner == _hoverOwner)
            {
                _hoverOwner = null;
                _world = false;
                _generalInfoController.Clear();
                _generalInfoController.gameObject.SetActive(false);
            }
        }

        private void UpdateInputState(UpdateInputStateMessage msg)
        {
            _mousePos = msg.Current.MousePos.ToVector2Int(true);
            if (_generalInfoController.gameObject.activeSelf)
            {
                var pos = _world ? WorldCameraController.Camera.ScreenToWorldPoint(_mousePos).ToVector2() : _mousePos;
                _generalInfoController.transform.SetLocalPosition(pos);
            }
        }

        private void WorldBuildingActive(WorldBuildingActiveMessage msg)
        {
            _hoverOwner = null;
            _generalInfoController.Clear();
            _generalInfoController.gameObject.SetActive(false);
        }

        void OnDestroy()
        {
            gameObject.UnsubscribeFromAllMessages();
        }
    }
}
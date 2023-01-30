using System.Linq;
using Assets.Ancible_Tools.Scripts.System.SaveData;
using MessageBusLib;
using UnityEngine;
using UnityEngine.EventSystems;

namespace Assets.Resources.Ancible_Tools.Scripts.System.Windows
{
    public class UiBaseWindow : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
    {
        public virtual bool Movable => true;
        public virtual bool Static => false;
        public bool Instantiated { get; private set; }

        public string WorldName;

        public WorldState[] ActiveWorldStates = {WorldState.World};
        public WorldState[] DisabledStates = new WorldState[0];

        private bool _hovered = false;
        private bool _selected = false;

        private bool _destroyed = false;

        public virtual void Awake()
        {
            Instantiated = true;
            SubscribeToWindowMessages();
        }

        public void MoveDelta(Vector2 delta)
        {
            var pos = transform.localPosition.ToVector2();
            pos += delta;
            pos.x = Mathf.Max(Mathf.Min(pos.x, Screen.width / 2f), Screen.width / 2 * -1);
            pos.y = Mathf.Max(Mathf.Min(pos.y, Screen.height / 2f), Screen.height / 2 * -1);
            pos = pos.ToVector2Int(true);
            transform.SetLocalPosition(pos);
        }

        public void FixPosition()
        {
            var pos = transform.localPosition;
            var pixelPerfect = pos.ToVector2().ToVector2Int(true);
            pos.x = pixelPerfect.x;
            pos.y = pixelPerfect.y;
            transform.localPosition = pos;
        }

        private void SubscribeToWindowMessages()
        {
            gameObject.Subscribe<UpdateWorldStateMessage>(UpdateWorldState);
        }

        protected internal virtual void UpdateWorldState(UpdateWorldStateMessage msg)
        {
            if (DisabledStates.Contains(msg.State))
            {
                if (gameObject.activeSelf)
                {
                    gameObject.SetActive(false);
                    if (_hovered)
                    {
                        _hovered = false;
                        UiWindowManager.RemoveHoveredWindow(this);
                    }
                }
            }
            else if (ActiveWorldStates.Contains(msg.State))
            {
                if (!gameObject.activeSelf)
                {
                    gameObject.SetActive(true);
                }
            }
            else
            {
                Close();
            }
        }

        public void OnPointerEnter(PointerEventData eventData)
        {
            UiWindowManager.SetHoveredWindow(this);
            _hovered = true;
        }

        public void OnPointerExit(PointerEventData eventData)
        {
            _hovered = false;
            UiWindowManager.RemoveHoveredWindow(this);
        }

        public virtual void Close()
        {
            UiWindowManager.CloseWindow(this, WorldName);
        }

        public virtual void Destroy()
        {
            if (_hovered)
            {
                UiWindowManager.RemoveHoveredWindow(this);
            }
            gameObject.UnsubscribeFromAllMessages();
            _destroyed = true;
        }

        public WindowData GetData()
        {
            return new WindowData {Id = WorldName, Window = name};
        }

        void OnDestroy()
        {
            if (!_destroyed)
            {
                Destroy();
            }
        }
    }
}
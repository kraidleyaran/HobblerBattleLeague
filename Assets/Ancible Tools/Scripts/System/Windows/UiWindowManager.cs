using System.Collections.Generic;
using System.Linq;
using MessageBusLib;
using UnityEngine;

namespace Assets.Resources.Ancible_Tools.Scripts.System.Windows
{
    public class UiWindowManager : MonoBehaviour
    {
        public static bool WindowBlock => _instance._windowBlocks.Count > 0;
        public static UiBaseWindow Hovered { get; private set; }
        public static bool Moving { get; private set; }

        private static UiWindowManager _instance = null;

        private UiBaseWindow _selected = null;
        
        private List<UiBaseWindow> _openWindows = new List<UiBaseWindow>();
        private Dictionary<string, UiBaseWindow> _staticWindows = new Dictionary<string, UiBaseWindow>();
        private List<GameObject> _windowBlocks = new List<GameObject>();

        void Awake()
        {
            if (_instance)
            {
                Destroy(gameObject);
                return;
            }

            _instance = this;
            SubscribeToMessages();
        }


        public static void SetHoveredWindow(UiBaseWindow window)
        {
            Hovered = window;
            Debug.Log($"Window hovered - {Hovered.name}");
        }

        public static void RemoveHoveredWindow(UiBaseWindow window)
        {
            if (Hovered && Hovered == window)
            {
                Debug.Log($"Window Unhovered - {Hovered.name}");
                Hovered = null;
            }
        }

        public static T OpenWindow<T>(T template, string id = null) where T : UiBaseWindow
        {
            if (_instance.gameObject.activeSelf)
            {
                if (template.Static)
                {
                    if (!_instance._staticWindows.TryGetValue(template.name, out var window))
                    {
                        window = Instantiate(template, _instance.transform);
                        window.name = template.name;
                        _instance._staticWindows.Add(template.name, window);
                    }
                    window.transform.SetAsLastSibling();
                    return (T)window;
                }
                else
                {
                    if (string.IsNullOrEmpty(id))
                    {
                        var window = Instantiate(template, _instance.transform);
                        _instance._openWindows.Add(window);
                        window.transform.SetAsLastSibling();
                        return window;
                    }

                    var windowName = $"{template.name} {id}";
                    var openWindow = _instance._openWindows.FirstOrDefault(w => w.name == windowName);
                    if (!openWindow)
                    {
                        openWindow = Instantiate(template, _instance.transform);
                        openWindow.name = windowName;
                        _instance._openWindows.Add(openWindow);
                    }
                    openWindow.transform.SetAsLastSibling();
                    return (T)openWindow;

                }
            }

            return null;

        }

        public static bool CloseWindow(UiBaseWindow window, string id = "")
        {
            if (window.Static)
            {
                if (_instance._staticWindows.TryGetValue(window.name, out var openWindow))
                {
                    openWindow.Destroy();
                    _instance._staticWindows.Remove(window.name);
                    Destroy(openWindow.gameObject);
                    return true;
                }
            }
            else if (!string.IsNullOrEmpty(id))
            {
                var windowName = window.Instantiated ? window.name : $"{window.name} {id}";
                var openWindow = _instance._openWindows.FirstOrDefault(w => w.name == windowName);
                if (openWindow)
                {
                    openWindow.Destroy();
                    _instance._openWindows.Remove(openWindow);
                    Destroy(openWindow.gameObject);
                }
            }
            else if (_instance._openWindows.Contains(window))
            {
                window.Destroy();
                _instance._openWindows.Remove(window);
                Destroy(window.gameObject);
                return true;
            }

            return false;
        }

        public static T ToggleWindow<T>(T template) where T : UiBaseWindow
        {
            if (_instance._staticWindows.TryGetValue(template.name, out var window))
            {
                CloseWindow(window);
                return null;
            }

            if (_instance.gameObject.activeSelf)
            {
                return OpenWindow(template);
            }

            return null;
        }

        public static void RegisterWindowBlock(GameObject block)
        {
            if (!_instance._windowBlocks.Contains(block))
            {
                _instance._windowBlocks.Add(block);
            }
        }

        public static void RemoveWindowBlock(GameObject block)
        {
            _instance._windowBlocks.Remove(block);
        }

        private void SubscribeToMessages()
        {
            gameObject.Subscribe<UpdateInputStateMessage>(UpdateInputState);
            gameObject.Subscribe<WorldBuildingActiveMessage>(WorldBuildingActive);
            gameObject.Subscribe<WorldBuildingStoppedMessage>(WorldBuildingStopped);
        }

        private void UpdateInputState(UpdateInputStateMessage msg)
        {
            if (!msg.Previous.LeftClick && msg.Current.LeftClick)
            {
                if (Hovered)
                {
                    _selected = Hovered;
                    _selected.transform.SetAsLastSibling();
                }
            }
            else if (msg.Previous.LeftClick && msg.Current.LeftClick)
            {
                if (_selected && _selected.Movable && !WindowBlock)
                {
                    Moving = true;
                    _selected.MoveDelta(msg.Current.MouseDelta);
                    //var pos = msg.Current.MousePos - msg.Previous.MousePos;
                    //if (pos != Vector2.zero)
                    //{
                        
                    //}
                }
            }
            else if (msg.Previous.LeftClick && !msg.Current.LeftClick)
            {
                if (_selected)
                {
                    Moving = false;
                    _selected.FixPosition();
                    //_selected.MoveDelta(msg.Current.MouseDelta);
                    _selected = null;
                }
                
            }
        }

        private void WorldBuildingActive(WorldBuildingActiveMessage msg)
        {
            Hovered = null;
            _selected = null;
            gameObject.SetActive(false);
        }

        private void WorldBuildingStopped(WorldBuildingStoppedMessage msg)
        {
            gameObject.SetActive(true);
        }

        void OnDestroy()
        {
            gameObject.UnsubscribeFromAllMessages();
        }
        
    }
}
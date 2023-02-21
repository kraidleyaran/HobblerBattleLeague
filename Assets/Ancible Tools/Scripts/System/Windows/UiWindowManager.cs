using System.Collections.Generic;
using System.Linq;
using Assets.Ancible_Tools.Scripts.System.SaveData;
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

        [SerializeField] private string[] _windowFolders = new string[0];
        [SerializeField] private GameObject _windowLayerTemplate = null;
        [SerializeField] private UiBaseWindow[] _startupWindows = new UiBaseWindow[0];

        private UiBaseWindow _selected = null;
        
        private List<UiBaseWindow> _openWindows = new List<UiBaseWindow>();
        private Dictionary<string, UiBaseWindow> _staticWindows = new Dictionary<string, UiBaseWindow>();
        private List<GameObject> _windowBlocks = new List<GameObject>();

        private Dictionary<string, WindowData> _windowData = new Dictionary<string, WindowData>();
        private Dictionary<int, Transform> _layers = new Dictionary<int, Transform>();

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

        void Start()
        {
            foreach (var window in _startupWindows)
            {
                OpenWindow(window);
            }
        }


        public static void SetHoveredWindow(UiBaseWindow window)
        {
            Hovered = window;
            //Debug.Log($"Window hovered - {Hovered.name}");
        }

        public static void RemoveHoveredWindow(UiBaseWindow window)
        {
            if (Hovered && Hovered == window)
            {
                //Debug.Log($"Window Unhovered - {Hovered.name}");
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
                        window = Instantiate(template, GetWindowLayer(template.Layer));
                        
                        window.name = template.name;
                        window.WorldName = template.name;
                        if (_instance._windowData.TryGetValue(window.WorldName, out var data))
                        {
                            window.transform.SetLocalPosition(data.Position.ToVector());
                        }
                        else
                        {
                            _instance._windowData.Add(window.WorldName, new WindowData { Window = template.name, Id = window.WorldName, Position = Vector2Int.zero.ToData() });
                        }
                        _instance._staticWindows.Add(template.name, window);
                    }
                    window.transform.SetAsLastSibling();
                    return (T)window;
                }
                else
                {
                    if (string.IsNullOrEmpty(id))
                    {
                        var window = Instantiate(template, GetWindowLayer(template.Layer));
                        _instance._openWindows.Add(window);
                        window.transform.SetAsLastSibling();

                        return window;
                    }

                    var windowName = $"{template.name} {id}";
                    var openWindow = _instance._openWindows.FirstOrDefault(w => w.WorldName == windowName);
                    if (!openWindow)
                    {
                        openWindow = Instantiate(template, GetWindowLayer(template.Layer));
                        openWindow.name = template.name;
                        openWindow.WorldName = windowName;
                        if (_instance._windowData.TryGetValue(openWindow.WorldName, out var data))
                        {
                            openWindow.transform.SetLocalPosition(data.Position.ToVector());
                        }
                        else
                        {
                            _instance._windowData.Add(openWindow.WorldName, new WindowData{Window = template.name, Id = windowName, Position = Vector2Int.zero.ToData()});
                        }
                        _instance._openWindows.Add(openWindow);
                    }
                    openWindow.transform.SetAsLastSibling();
                    return (T)openWindow;

                }
            }

            return null;

        }

        public static bool CloseWindow(UiBaseWindow window)
        {
            if (window.Static)
            {
                if (_instance._staticWindows.TryGetValue(window.WorldName, out var openWindow))
                {
                    _instance._staticWindows.Remove(window.name);
                    if (_instance._windowData.TryGetValue(openWindow.WorldName, out var data))
                    {
                        data.Position = openWindow.transform.localPosition.ToVector2().ToVector2Int().ToData();
                    }
                    else
                    {
                        _instance._windowData.Add(openWindow.WorldName, new WindowData { Window = window.name, Id = window.name, Position = openWindow.transform.localPosition.ToVector2().ToVector2Int().ToData()});
                    }
                    openWindow.Destroy();
                    Destroy(openWindow.gameObject);
                    return true;
                }
            }
            else
            {
                var openWindow = _instance._openWindows.FirstOrDefault(w => w == window);
                if (openWindow)
                {
                    if (_instance._windowData.TryGetValue(openWindow.WorldName, out var data))
                    {
                        data.Position = openWindow.transform.localPosition.ToVector2().ToVector2Int().ToData();
                    }
                    else
                    {
                        _instance._windowData.Add(openWindow.WorldName, new WindowData { Window = window.name, Id = window.WorldName, Position = openWindow.transform.localPosition.ToVector2().ToVector2Int().ToData() });
                    }
                    _instance._openWindows.Remove(openWindow);
                    openWindow.Destroy();
                    Destroy(openWindow.gameObject);
                    return true;
                }
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

        public static WindowData[] GetWindowData()
        {
            var openWindows = _instance._openWindows.ToList();
            openWindows.AddRange(_instance._staticWindows.Values);
            for (var i = 0; i < openWindows.Count; i++)
            {
                if (_instance._windowData.TryGetValue(openWindows[i].WorldName, out var data))
                {
                    data.Position = openWindows[i].transform.localPosition.ToVector2().ToVector2Int().ToData();
                }
            }
            return _instance._windowData.Values.ToArray();

        }

        public static void SetWindowData(WindowData[] data)
        {
            foreach (var windowData in data)
            {
                if (!_instance._windowData.ContainsKey(windowData.Id))
                {
                    _instance._windowData.Add(windowData.Id, windowData);
                }
            }
        }

        private static Transform GetWindowLayer(int layer)
        {
            if (!_instance._layers.TryGetValue(layer, out var windowLayer))
            {
                windowLayer = Instantiate(_instance._windowLayerTemplate, _instance.transform).transform;
                _instance._layers.Add(layer, windowLayer);
                var orderedLayers = _instance._layers.OrderBy(kv => kv.Key).ToArray();
                for (var i = 0; i < orderedLayers.Length; i++)
                {
                    orderedLayers[i].Value.SetSiblingIndex(i);
                }
            }

            return windowLayer;
        }

        private void SubscribeToMessages()
        {
            gameObject.Subscribe<UpdateInputStateMessage>(UpdateInputState);
            gameObject.Subscribe<WorldBuildingActiveMessage>(WorldBuildingActive);
            gameObject.Subscribe<WorldBuildingStoppedMessage>(WorldBuildingStopped);
            gameObject.Subscribe<ClearWorldMessage>(ClearWorld);
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
                    _selected.MoveDelta(msg.Current.MousePos - msg.Previous.MousePos);
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

        private void ClearWorld(ClearWorldMessage msg)
        {
            var windows = _openWindows.ToArray();
            foreach (var window in windows)
            {
                window.Close();
            }
        }

        void OnDestroy()
        {
            gameObject.UnsubscribeFromAllMessages();
        }
        
    }
}
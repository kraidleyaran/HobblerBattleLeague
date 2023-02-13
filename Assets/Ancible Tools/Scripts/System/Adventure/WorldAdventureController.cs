using System.Collections.Generic;
using System.Linq;
using Assets.Ancible_Tools.Scripts.Traits;
using Assets.Resources.Ancible_Tools.Scripts.System.Pathing;
using Assets.Resources.Ancible_Tools.Scripts.System.Templates;
using MessageBusLib;
using UnityEngine;

namespace Assets.Resources.Ancible_Tools.Scripts.System.Adventure
{
    public class WorldAdventureController : MonoBehaviour
    {
        public static AdventureMapController MapController => _instance._mapController;
        public static AdventureMap Default => _instance._defaultMap;
        public static AdventureMap Current => _instance._currentMap ?? _instance._defaultMap;
        public static GameObject Player { get; private set; }
        public static MapTile PlayerTile { get; private set; }
        public static int BattleBumpSpeed => _instance._battleBumpSpeed;
        public static float BattleBumpDistance => _instance._battleBumpDistance;
        public static AdventureState State { get; private set; }

        private static WorldAdventureController _instance = null;

        [SerializeField] private UnitTemplate _playerTemplate = null;
        [SerializeField] private AdventureMap _defaultMap = null;
        [SerializeField] private int _battleBumpSpeed = 35;
        [SerializeField] private float _battleBumpDistance = 5f;
        [SerializeField] private string _mapFolderPath = string.Empty;

        private AdventureMap _currentMap = null;
        private AdventureMapController _mapController = null;
        
        private UpdateAdventureStateMessage _updateAdventureStateMsg = new UpdateAdventureStateMessage();
        private Dictionary<string, AdventureMap> _maps = new Dictionary<string, AdventureMap>();

        private List<GameObject> _registeredObjects = new List<GameObject>();

        void Awake()
        {
            if (_instance)
            {
                Destroy(gameObject);
                return;
            }

            _instance = this;
            _maps = UnityEngine.Resources.LoadAll<AdventureMap>(_mapFolderPath).ToDictionary(m => m.name, m => m);
            SubscribeToMessages();
        }

        void Start()
        {
            Setup(Default, Default.DefaultTile, false);
        }

        public static void Setup(AdventureMap map, Vector2Int pos, bool switchMode = true)
        {
            if (!Player)
            {
                Player = _instance._playerTemplate.GenerateUnit(_instance.transform, _instance.transform.position.ToVector2()).gameObject;
            }
            if (_instance._currentMap)
            {
                Destroy(_instance._mapController.gameObject);
            }

            _instance._currentMap = map;
            _instance._mapController = Instantiate(map.MapController, _instance.transform);
            _instance._mapController.gameObject.layer = _instance.gameObject.layer;
            _instance._mapController.Setup(pos);
            if (switchMode)
            {
                WorldController.SetWorldState(WorldState.Adventure);
            }
        }

        public static void SetAdventureState(AdventureState state)
        {
            if (State != state)
            {
                State = state;
                _instance._updateAdventureStateMsg.State = State;
                _instance.gameObject.SendMessage(_instance._updateAdventureStateMsg);
            }
        }

        public static AdventureMap GetAdventuerMapByName(string mapName)
        {
            if (_instance._maps.TryGetValue(mapName, out var map))
            {
                return map;
            }

            return null;
        }

        public static void TransitionToMap(AdventureMap map, Vector2Int pos, Vector2Int direction)
        {
            if (_instance._currentMap != map)
            {
                var objs = _instance._registeredObjects.ToArray();
                foreach (var obj in objs)
                {
                    Destroy(obj);
                }
                _instance._registeredObjects.Clear();

                Destroy(_instance._mapController.gameObject);

                _instance._currentMap = map;
                _instance._mapController = Instantiate(map.MapController, _instance.transform);
                _instance._mapController.gameObject.layer = _instance.gameObject.layer;
                _instance._mapController.Setup(pos);

                var setFacingDirectionMsg = MessageFactory.GenerateSetFaceDirectionMsg();
                setFacingDirectionMsg.Direction = direction;
                _instance.gameObject.SendMessageTo(setFacingDirectionMsg, Player);
                MessageFactory.CacheMessage(setFacingDirectionMsg);
            }
        }

        private void SubscribeToMessages()
        {
            gameObject.Subscribe<ClearWorldMessage>(ClearWorld);
        }

        public static void RegisterObject(GameObject obj)
        {
            if (!_instance._registeredObjects.Contains(obj))
            {
                _instance._registeredObjects.Add(obj);
            }
        }

        public static void UnregisterObject(GameObject obj, bool destroy = false)
        {
            _instance._registeredObjects.Remove(obj);
            if (destroy)
            {
                Destroy(obj);
            }
        }

        public static void SetPlayerTile(MapTile tile)
        {
            PlayerTile = tile;
        }

        private void ClearWorld(ClearWorldMessage msg)
        {
            if (_currentMap)
            {
                Destroy(_mapController.gameObject);
            }

            _currentMap = null;
            Destroy(Player);
            Player = null;
        }

        void OnDestroy()
        {
            gameObject.UnsubscribeFromAllMessages();
        }
    }
}
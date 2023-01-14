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
        public static GameObject Player { get; private set; }
        public static int BattleBumpSpeed => _instance._battleBumpSpeed;
        public static float BattleBumpDistance => _instance._battleBumpDistance;
        public static AdventureState State { get; private set; }

        private static WorldAdventureController _instance = null;

        [SerializeField] private UnitTemplate _playerTemplate = null;
        [SerializeField] private AdventureMap _defaultMap = null;
        [SerializeField] private int _battleBumpSpeed = 35;
        [SerializeField] private float _battleBumpDistance = 5f;

        private AdventureMap _currentMap = null;
        private AdventureMapController _mapController = null;
        
        private UpdateAdventureStateMessage _updateAdventureStateMsg = new UpdateAdventureStateMessage();

        void Awake()
        {
            if (_instance)
            {
                Destroy(gameObject);
                return;
            }

            _instance = this;
            
        }

        public static void Setup(AdventureMap map, Vector2Int pos)
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
            WorldController.SetWorldState(WorldState.Adventure);
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
    }
}
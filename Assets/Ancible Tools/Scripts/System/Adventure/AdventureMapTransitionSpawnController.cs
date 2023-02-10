using Assets.Ancible_Tools.Scripts.Traits;
using Assets.Resources.Ancible_Tools.Scripts.System.Templates;
using MessageBusLib;
using UnityEngine;

namespace Assets.Resources.Ancible_Tools.Scripts.System.Adventure
{
    public class AdventureMapTransitionSpawnController : MonoBehaviour
    {
        [SerializeField] private AdventureMap _map;
        [SerializeField] private Vector2Int _position = Vector2Int.zero;
        [SerializeField] private Vector2Int _direction = Vector2Int.down;
        [SerializeField] private UnitTemplate _mapTransitionTemplate = null;

        private GameObject _obj = null;

        void Awake()
        {
            if (_map)
            {
                SubscribeToMessages();
            }
        }

        private void SubscribeToMessages()
        {
            gameObject.Subscribe<SpawnAdventureUnitsMessage>(SpawnAdventureUnits);
        }

        private void SpawnAdventureUnits(SpawnAdventureUnitsMessage msg)
        {
            gameObject.Unsubscribe<SpawnAdventureUnitsMessage>();
            var mapTile = WorldAdventureController.MapController.PlayerPathing.GetMapTileByWorldPosition(transform.position.ToVector2());
            if (mapTile != null)
            {
                var obj = _mapTransitionTemplate.GenerateUnit(WorldAdventureController.MapController.transform, mapTile.World).gameObject;

                var setAdventureMapTransitionMsg = MessageFactory.GenerateSetAdventureMapTransitionMsg();
                setAdventureMapTransitionMsg.Direction = _direction;
                setAdventureMapTransitionMsg.Map = _map;
                setAdventureMapTransitionMsg.Position = _position;
                gameObject.SendMessageTo(setAdventureMapTransitionMsg, obj);
                MessageFactory.CacheMessage(setAdventureMapTransitionMsg);

                var setMapTileMsg = MessageFactory.GenerateSetMapTileMsg();
                setMapTileMsg.Tile = mapTile;
                gameObject.SendMessageTo(setMapTileMsg, obj);
                MessageFactory.CacheMessage(setMapTileMsg);

                WorldAdventureController.RegisterObject(obj);
            }
            
        }

        void OnDestroy()
        {
            gameObject.UnsubscribeFromAllMessages();
        }
    }
}
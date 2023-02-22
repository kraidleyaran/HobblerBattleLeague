using System.Collections.Generic;
using Assets.Ancible_Tools.Scripts.Traits;
using Assets.Resources.Ancible_Tools.Scripts.System.Pathing;
using Assets.Resources.Ancible_Tools.Scripts.System.WorldEvents;
using CreativeSpore.SuperTilemapEditor;
using MessageBusLib;
using UnityEngine;

namespace Assets.Resources.Ancible_Tools.Scripts.System.Adventure
{
    public class AdventureWorldEventTriggerSpawnController : MonoBehaviour
    {
        [SerializeField] private WorldEvent _event;
        [SerializeField] private STETilemap _triggerTilemap;

        private MapTile[] _subscribedTiles = new MapTile[0];

        void Awake()
        {
            SubscribeToMessages();
        }

        private void ApplyToUnit(GameObject unit)
        {
            WorldEventManager.TriggerWorldEvent(_event);
        }

        private void SubscribeToMessages()
        {
            gameObject.Subscribe<SpawnAdventureUnitsMessage>(SpawnAdventureUnits);
        }

        private void SpawnAdventureUnits(SpawnAdventureUnitsMessage msg)
        {
            gameObject.Unsubscribe<SpawnAdventureUnitsMessage>();
            var pos = new Vector2Int(_triggerTilemap.MinGridX, _triggerTilemap.MinGridY);
            var mapTiles = new List<MapTile>();
            while (pos.y <= _triggerTilemap.MaxGridY)
            {
                var worldPos = TilemapUtils.GetGridWorldPos(_triggerTilemap, pos.x, pos.y).ToVector2();
                var mapTile = WorldAdventureController.MapController.PlayerPathing.GetMapTileByWorldPosition(worldPos);
                if (mapTile != null)
                {
                    mapTiles.Add(mapTile);
                }

                pos.x++;
                if (pos.x > _triggerTilemap.MaxGridX)
                {
                    pos.x = _triggerTilemap.MinGridX;
                    pos.y++;
                }
            }

            _subscribedTiles = mapTiles.ToArray();
            foreach (var tile in _subscribedTiles)
            {
                tile.OnObjectEnteringTile += ApplyToUnit;
            }
        }

        void OnDestroy()
        {
            gameObject.UnsubscribeFromAllMessages();
        }
    }
}
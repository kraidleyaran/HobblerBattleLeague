using System.Security.Cryptography;
using MessageBusLib;
using UnityEngine;

namespace Assets.Resources.Ancible_Tools.Scripts.System.Minigame
{
    public class MinigameUnitSpawnController : UnitSpawnController
    {
        private GameObject _spawnedUnit = null;

        void Awake()
        {
            MinigameController.RegisterMinigameObject(gameObject);
            SubscribeToMessages();
        }

        public override void Start()
        {

        }

        private void SpawnUnit()
        {
            if (!_spawnedUnit)
            {
                var mapTile = MinigameController.Pathing.GetMapTileByWorldPosition(transform.position.ToVector2());
                if (mapTile != null)
                {
                    UnitController unitController = null;
                    unitController = _template ? _template.GenerateUnit(MinigameController.Transform, mapTile.World) : Instantiate(FactoryController.UNIT_CONTROLLER, MinigameController.Transform);
                    unitController.gameObject.layer = MinigameController.Transform.gameObject.layer;
                    if (!_template)
                    {
                        unitController.transform.SetLocalPosition(mapTile.World);
                    }
                    var addTraitToUnitMsg = MessageFactory.GenerateAddTraitToUnitMsg();
                    if (_sprite)
                    {
                        addTraitToUnitMsg.Trait = _sprite;
                        gameObject.SendMessageTo(addTraitToUnitMsg, unitController.gameObject);
                    }

                    for (var i = 0; i < _additionalTraits.Length; i++)
                    {
                        addTraitToUnitMsg.Trait = _additionalTraits[i];
                        gameObject.SendMessageTo(addTraitToUnitMsg, unitController.gameObject);
                    }
                    MessageFactory.CacheMessage(addTraitToUnitMsg);

                    var setMapTileMsg = MessageFactory.GenerateSetMapTileMsg();
                    setMapTileMsg.Tile = mapTile;
                    gameObject.SendMessageTo(setMapTileMsg, unitController.gameObject);
                    MessageFactory.CacheMessage(setMapTileMsg);
                    _spawnedUnit = unitController.gameObject;
                    MinigameController.RegisterMinigameObject(unitController.gameObject);
                }
            }

        }

        private void SubscribeToMessages()
        {
            gameObject.Subscribe<MinigameSpawnUnitsMessage>(MinigameSpawnUnits);
            gameObject.Subscribe<DespawnMinigameUnitsMessage>(DespawnMinigameUnits);
        }

        private void MinigameSpawnUnits(MinigameSpawnUnitsMessage msg)
        {
            SpawnUnit();
        }

        private void DespawnMinigameUnits(DespawnMinigameUnitsMessage msg)
        {
            if (_spawnedUnit)
            {
                MinigameController.UnregisterMinigameObject(_spawnedUnit);
                Destroy(_spawnedUnit);
            }
            Destroy(gameObject);
        }

        void OnDestroy()
        {
            gameObject.UnsubscribeFromAllMessages();
        }


    }
}
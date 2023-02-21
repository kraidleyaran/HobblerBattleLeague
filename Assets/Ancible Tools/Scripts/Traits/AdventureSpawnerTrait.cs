using System.Linq;
using Assets.Resources.Ancible_Tools.Scripts.System;
using Assets.Resources.Ancible_Tools.Scripts.System.Adventure;
using Assets.Resources.Ancible_Tools.Scripts.System.Pathing;
using Assets.Resources.Ancible_Tools.Scripts.System.Templates;
using Assets.Resources.Ancible_Tools.Scripts.System.TickTimers;
using MessageBusLib;
using UnityEngine;

namespace Assets.Ancible_Tools.Scripts.Traits
{
    [CreateAssetMenu(fileName = "Adventure Spawner Trait", menuName = "Ancible Tools/Traits/Adventure/Adventure Spawner")]
    public class AdventureSpawnerTrait : Trait
    {
        [SerializeField] private UnitTemplate _template;
        [SerializeField] private Trait[] _additionalTraits;
        [SerializeField] private int _spawnCooldown = 0;
        [SerializeField] [Range(0f, 1f)] private float _chanceToSpawn = 1f;
        [SerializeField] private int _spawnCheckCooldown = 0;
        [SerializeField] private bool _isMonster = false;
        [SerializeField] private int _spawnArea = 1;

        private GameObject _spawnedUnit = null;
        private TickTimer _spawnCheckTimer = null;
        private TickTimer _spawnCooldownTimer = null;

        private MapTile[] _spawnTiles = new MapTile[0];

        public override void SetupController(TraitController controller)
        {
            base.SetupController(controller);
            SubscribeToMessages();
        }

        private void SpawnUnit()
        {
            var availableTiles = _spawnTiles.Where(t => !t.Block).ToArray();
            if (availableTiles.Length > 0)
            {
                var tile = availableTiles.GetRandom();
                if (!_spawnedUnit)
                {
                    _spawnedUnit = _template ? _template.GenerateUnit(WorldAdventureController.MapController.transform, tile.World).gameObject : Instantiate(FactoryController.UNIT_CONTROLLER, WorldAdventureController.MapController.transform).gameObject;
                    _spawnedUnit.gameObject.layer = WorldAdventureController.MapController.gameObject.layer;
                    if (_additionalTraits.Length > 0)
                    {
                        var addTraitToUnitMsg = MessageFactory.GenerateAddTraitToUnitMsg();
                        foreach (var trait in _additionalTraits)
                        {
                            addTraitToUnitMsg.Trait = trait;
                            _controller.gameObject.SendMessageTo(addTraitToUnitMsg, _spawnedUnit);
                        }
                        MessageFactory.CacheMessage(addTraitToUnitMsg);
                    }

                    var setUnitSpawnerMsg = MessageFactory.GenerateSetUnitSpawnerMsg();
                    setUnitSpawnerMsg.Spawner = _controller.transform.parent.gameObject;
                    _controller.gameObject.SendMessageTo(setUnitSpawnerMsg, _spawnedUnit);
                    MessageFactory.CacheMessage(setUnitSpawnerMsg);
                }

                var setMapTileMsg = MessageFactory.GenerateSetMapTileMsg();
                setMapTileMsg.Tile = tile;
                _controller.gameObject.SendMessageTo(setMapTileMsg, _spawnedUnit);
                MessageFactory.CacheMessage(setMapTileMsg);

                var setUnitStateMsg = MessageFactory.GenerateSetAdventureUnitStateMsg();
                setUnitStateMsg.State = AdventureUnitState.Idle;
                _controller.gameObject.SendMessageTo(setUnitStateMsg, _spawnedUnit);
                MessageFactory.CacheMessage(setUnitStateMsg);

                WorldAdventureController.RegisterObject(_spawnedUnit);
            }
            else
            {
                _spawnCooldownTimer = new TickTimer(_spawnCooldown, 0, SpawnCooldownFinish, null);
            }
            

        }

        private bool CanSpawn()
        {
            return _chanceToSpawn >= Random.Range(0f, 1f);
        }

        private void SpawnCooldownFinish()
        {
            _spawnCooldownTimer?.Destroy();
            _spawnCooldownTimer = null;
            if (CanSpawn())
            {
                SpawnUnit();
            }
            else
            {
                _spawnCheckTimer = new TickTimer(_spawnCheckCooldown, 0, SpawnCheckFinish, null);
            }
        }

        private void SpawnCheckFinish()
        {
            _spawnCheckTimer?.Destroy();
            _spawnCheckTimer = null;
            SpawnCooldownFinish();
        }

        private void SubscribeToMessages()
        {
            _controller.transform.parent.gameObject.SubscribeWithFilter<DespawnUnitMessage>(DespawnUnit, _instanceId);
            _controller.transform.parent.gameObject.SubscribeWithFilter<SetMapTileMessage>(SetMapTile, _instanceId);
        }

        private void DespawnUnit(DespawnUnitMessage msg)
        {
            _spawnedUnit.gameObject.SetActive(false);
            _spawnCooldownTimer = new TickTimer(_spawnCooldown, 0, SpawnCooldownFinish, null);
        }

        private void SetMapTile(SetMapTileMessage msg)
        {
            _spawnTiles = _isMonster ? WorldAdventureController.MapController.MonsterPathing.GetMapTilesInArea(msg.Tile.Position, _spawnArea) : WorldAdventureController.MapController.PlayerPathing.GetMapTilesInArea(msg.Tile.Position, _spawnArea);
            if (CanSpawn())
            {
                SpawnUnit();
            }
        }

        public override void Destroy()
        {
            _spawnCheckTimer?.Destroy();
            _spawnCheckTimer = null;

            _spawnCooldownTimer?.Destroy();
            _spawnCooldownTimer = null;
            if (_spawnedUnit)
            {
                WorldAdventureController.UnregisterObject(_spawnedUnit, true);
            }
            
            base.Destroy();
        }
    }
}
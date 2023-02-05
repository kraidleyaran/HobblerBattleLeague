using Assets.Resources.Ancible_Tools.Scripts.System;
using Assets.Resources.Ancible_Tools.Scripts.System.Items;
using Assets.Resources.Ancible_Tools.Scripts.System.Minigame;
using Assets.Resources.Ancible_Tools.Scripts.System.Pathing;
using Assets.Resources.Ancible_Tools.Scripts.System.Templates;
using MessageBusLib;
using UnityEngine;

namespace Assets.Ancible_Tools.Scripts.Traits
{
    [CreateAssetMenu(fileName = "Minigame Monster Loot Trait", menuName = "Ancible Tools/Traits/Minigame/Minigame Monster Loot")]
    public class MinigameMonsterLootTrait : Trait
    {
        [SerializeField] private UnitTemplate _chestSpawnTemplate;
        [SerializeField] private LootTable _lootTable;
        [SerializeField] [Range(0f, 1f)] private float _chanceToSpawnLoot = 0f;

        private MapTile _mapTile = null;

        public override void SetupController(TraitController controller)
        {
            base.SetupController(controller);
            SubscribeToMessages();
        }

        private void SubscribeToMessages()
        {
            _controller.transform.parent.gameObject.SubscribeWithFilter<UpdateMapTileMessage>(UpdateMapTile, _instanceId);
        }

        private void UnitDied(UnitDiedMessage msg)
        {
            if (_chestSpawnTemplate && _lootTable)
            {
                var canSpawn = _chanceToSpawnLoot >= Random.Range(0f, 1f);
                if (canSpawn)
                {
                    var chest = _chestSpawnTemplate.GenerateUnit(MinigameController.Transform, _mapTile.World);

                    var setLootTableMsg = MessageFactory.GenerateSetLootTableMsg();
                    setLootTableMsg.Table = _lootTable;
                    _controller.gameObject.SendMessageTo(setLootTableMsg, chest.gameObject);
                    MessageFactory.CacheMessage(setLootTableMsg);

                    var setMapTileMsg = MessageFactory.GenerateSetMapTileMsg();
                    setMapTileMsg.Tile = _mapTile;
                    _controller.gameObject.SendMessageTo(setMapTileMsg, chest.gameObject);
                    MessageFactory.CacheMessage(setMapTileMsg);

                    MinigameController.RegisterMinigameObject(chest.gameObject);
                }

            }
            _controller.transform.parent.gameObject.UnsubscribeFromFilter<UnitDiedMessage>(_instanceId);
        }

        private void UpdateMapTile(UpdateMapTileMessage msg)
        {
            if (_mapTile == null)
            {
                _controller.transform.parent.gameObject.SubscribeWithFilter<UnitDiedMessage>(UnitDied, _instanceId);
            }
            _mapTile = msg.Tile;
        }
    }
}
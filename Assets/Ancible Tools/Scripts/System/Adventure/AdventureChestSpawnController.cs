using Assets.Ancible_Tools.Scripts.Traits;
using Assets.Resources.Ancible_Tools.Scripts.System.Items;
using Assets.Resources.Ancible_Tools.Scripts.System.Templates;
using MessageBusLib;
using UnityEngine;

namespace Assets.Resources.Ancible_Tools.Scripts.System.Adventure
{
    public class AdventureChestSpawnController : MonoBehaviour
    {
        [SerializeField] private ItemStack _item = new ItemStack {Item = null, Stack = 1};
        [SerializeField] private SpriteTrait _closedSprite = null;
        [SerializeField] private SpriteTrait _openSprite = null;
        [SerializeField] private UnitTemplate _chestUnitTemplate = null;
        public string SaveId;

        [Header("Editor References")]
        [SerializeField] private SpriteRenderer _spriteRenderer;
        [SerializeField] private SpriteRenderer _itemRenderer;


        void Awake()
        {
            SubscribeToMessages();
        }

        public void RefreshEditorSprite()
        {
#if UNITY_EDITOR
            if (_spriteRenderer && _closedSprite)
            {
                _spriteRenderer.sprite = _closedSprite.Sprite;
                _spriteRenderer.transform.SetLocalScaling(_closedSprite.Scaling);
                _spriteRenderer.transform.SetLocalPosition(_closedSprite.Offset);
            }

            if (_itemRenderer)
            {
                if (_item != null && _item.Item)
                {
                    _itemRenderer.sprite = _item.Item.Icon;   
                }
                _itemRenderer.gameObject.SetActive(_item != null && _item.Item);
            }
#endif
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
                var unitController = _chestUnitTemplate.GenerateUnit(WorldAdventureController.MapController.transform, mapTile.World);

                gameObject.AddTraitToUnit(_closedSprite, unitController.gameObject);

                var setAdventureChestMsg = MessageFactory.GenerateSetAdventureChestMsg();
                setAdventureChestMsg.Id = SaveId;
                setAdventureChestMsg.Item = _item.Clone();
                setAdventureChestMsg.OpenSprite = _openSprite;
                gameObject.SendMessageTo(setAdventureChestMsg, unitController.gameObject);
                MessageFactory.CacheMessage(setAdventureChestMsg);

                var setMapTileMsg = MessageFactory.GenerateSetMapTileMsg();
                setMapTileMsg.Tile = mapTile;
                gameObject.SendMessageTo(setMapTileMsg, unitController.gameObject);
                MessageFactory.CacheMessage(setMapTileMsg);

                WorldAdventureController.RegisterObject(unitController.gameObject);
            }

            WorldAdventureController.RegisterObject(gameObject);
        }

        void OnDestroy()
        {
            gameObject.UnsubscribeFromAllMessages();
        }


    }
}
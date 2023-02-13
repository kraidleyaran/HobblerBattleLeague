using Assets.Ancible_Tools.Scripts.Traits;
using Assets.Resources.Ancible_Tools.Scripts.System.Adventure;
using Assets.Resources.Ancible_Tools.Scripts.System.Templates;
using MessageBusLib;
using UnityEngine;

namespace Assets.Resources.Ancible_Tools.Scripts.System.SpawnControllers
{
    public class AdventureCustomDialogueSpawnController : MonoBehaviour
    {
        [SerializeField] [TextArea(3,5)] private string[] _dialogue = new string[0];
        [SerializeField] private SpriteTrait _sprite = null;

        [Header("Editor References")]
        [SerializeField] private SpriteRenderer _spriteRenderer = null;
        [SerializeField] private SpriteTrait _defaultSprite = null;
        [SerializeField] private UnitTemplate _template = null;

        void Awake()
        {
            WorldAdventureController.RegisterObject(gameObject);
            SubscribeToMessages();
        }

        public void RefreshEditorSprite()
        {
#if UNITY_EDITOR
            if (_spriteRenderer)
            {
                var sprite = _sprite ? _sprite : _defaultSprite;
                if (sprite)
                {
                    _spriteRenderer.sprite = sprite.Sprite;
                    _spriteRenderer.transform.SetLocalScaling(sprite.Scaling);
                    _spriteRenderer.transform.SetLocalPosition(sprite.Offset);
                }
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
                var unitController = _template.GenerateUnit(WorldAdventureController.MapController.transform, mapTile.World);

                var sprite = _sprite ? _sprite : _defaultSprite;

                var addTraitToUnitMsg = MessageFactory.GenerateAddTraitToUnitMsg();
                addTraitToUnitMsg.Trait = sprite;
                gameObject.SendMessageTo(addTraitToUnitMsg, unitController.gameObject);
                MessageFactory.CacheMessage(addTraitToUnitMsg);

                var setCustomDialogueMsg = MessageFactory.GenerateSetCustomDialogueMsg();
                setCustomDialogueMsg.Dialogue = _dialogue;
                gameObject.SendMessageTo(setCustomDialogueMsg, unitController.gameObject);
                MessageFactory.CacheMessage(setCustomDialogueMsg);

                var setMapTileMsg = MessageFactory.GenerateSetMapTileMsg();
                setMapTileMsg.Tile = mapTile;
                gameObject.SendMessageTo(setMapTileMsg, unitController.gameObject);
                MessageFactory.CacheMessage(setMapTileMsg);

                WorldAdventureController.RegisterObject(unitController.gameObject);
                
            }
            
        }

        void OnDestroy()
        {
            WorldAdventureController.UnregisterObject(gameObject);
            gameObject.UnsubscribeFromAllMessages();
        }
    }
}
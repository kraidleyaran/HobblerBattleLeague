using System.Linq;
using Assets.Ancible_Tools.Scripts.Traits;
using Assets.Resources.Ancible_Tools.Scripts.System.Animation;
using Assets.Resources.Ancible_Tools.Scripts.System.Templates;
using MessageBusLib;
using UnityEngine;

namespace Assets.Resources.Ancible_Tools.Scripts.System.Adventure
{
    public class AdventureUnitSpawnController : MonoBehaviour
    {
        [SerializeField] private UnitTemplate _template;
        [SerializeField] private Trait[] _additionalTraits;
        [SerializeField] private SpriteTrait _sprite;
        [SerializeField] private bool _isMonster = false;
        [SerializeField] private Vector2Int _faceDirection = Vector2Int.down;
        [Space]
        [Header("Editor References")]
        [SerializeField] private SpriteController _editorSprite;
        [SerializeField] private SpriteTrait _defaultEditorSprite;

        void Awake()
        {
            SubscribeToMessages();
        }

        public void RefreshEditorSprite()
        {
#if UNITY_EDITOR
            var sprite = _sprite ? _sprite : _template?.GetTraits().Select(t => t as SpriteTrait).Where(t => t).OrderByDescending(t => t.SortingOrder).FirstOrDefault();
            if (!sprite)
            {
                sprite = _defaultEditorSprite;
            }

            if (_editorSprite && sprite)
            {
                _editorSprite.SetSprite(sprite.Sprite);
                _editorSprite.SetScale(sprite.Scaling);
                _editorSprite.SetOffset(sprite.Offset, false);
                _editorSprite.SetColorMask(sprite.ColorMask);
                _editorSprite.SetSortingOrder(sprite.SortingOrder);
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

            var pathingGrid = _isMonster ? WorldAdventureController.MapController.MonsterPathing : WorldAdventureController.MapController.PlayerPathing;
            var spawnTile = pathingGrid.GetMapTileByWorldPosition(transform.position.ToVector2());
            if (spawnTile != null)
            {
                var controller = _template ? _template.GenerateUnit(WorldAdventureController.MapController.transform, spawnTile.World) : Instantiate(FactoryController.UNIT_CONTROLLER, WorldAdventureController.MapController.transform);
                controller.gameObject.layer = WorldAdventureController.MapController.transform.gameObject.layer;
                if (!_template)
                {
                    controller.transform.SetTransformPosition(spawnTile.World);
                }
                var addTraitToUnitMsg = MessageFactory.GenerateAddTraitToUnitMsg();
                if (_sprite)
                {
                    addTraitToUnitMsg.Trait = _sprite;
                    gameObject.SendMessageTo(addTraitToUnitMsg, controller.gameObject);
                }
                
                if (_additionalTraits.Length > 0)
                {
                    foreach (var trait in _additionalTraits)
                    {
                        addTraitToUnitMsg.Trait = trait;
                        gameObject.SendMessageTo(addTraitToUnitMsg, controller.gameObject);
                    }
                }
                MessageFactory.CacheMessage(addTraitToUnitMsg);

                var setMapTileMsg = MessageFactory.GenerateSetMapTileMsg();
                setMapTileMsg.Tile = spawnTile;
                gameObject.SendMessageTo(setMapTileMsg, controller.gameObject);
                MessageFactory.CacheMessage(setMapTileMsg);

                var setFaceDirectionMsg = MessageFactory.GenerateSetFaceDirectionMsg();
                setFaceDirectionMsg.Direction = _faceDirection;
                gameObject.SendMessageTo(setFaceDirectionMsg, controller.gameObject);
                MessageFactory.CacheMessage(setFaceDirectionMsg);
            }
            
        }

        void OnDestroy()
        {
            gameObject.UnsubscribeFromAllMessages();
        }

    }
}
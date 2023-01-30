using Assets.Ancible_Tools.Scripts.Traits;
using Assets.Resources.Ancible_Tools.Scripts.System.Pathing;
using Assets.Resources.Ancible_Tools.Scripts.System.Templates;
using MessageBusLib;
using UnityEngine;

namespace Assets.Resources.Ancible_Tools.Scripts.System
{
    public class UnitSpawnController : MonoBehaviour
    {
        [SerializeField] protected internal UnitTemplate _template = null;
        [SerializeField] protected internal SpriteTrait _sprite = null;
        [SerializeField] protected internal Trait[] _additionalTraits = new Trait[0];
        
        [Header("Editor References")]
        [SerializeField] private SpriteRenderer _iconRenderer;
        [SerializeField] private Sprite _defaultIcon;
        [SerializeField] private Vector2 _defaultScaling = new Vector2(31.25f,31.25f);
        [SerializeField] private Color _defaultColor = Color.white;

        public virtual void Start()
        {
            var mapTile = WorldController.Pathing.GetMapTileByWorldPosition(transform.position.ToVector2());
            if (mapTile != null)
            {
                UnitController unitController = null;
                unitController = _template ? _template.GenerateUnit(WorldController.Transform, mapTile.World) : Instantiate(FactoryController.UNIT_CONTROLLER,WorldController.Transform);
                if (!_template)
                {
                    unitController.gameObject.layer = WorldController.Transform.gameObject.layer;
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
            }
            
        }

        public void RefreshSprite()
        {
#if UNITY_EDITOR
            if (_iconRenderer)
            {
                if (_sprite)
                {
                    _iconRenderer.sprite = _sprite.Sprite;
                    _iconRenderer.color = _sprite.ColorMask;
                    _iconRenderer.transform.SetLocalScaling(_sprite.Scaling);
                    _iconRenderer.transform.SetLocalPosition(_sprite.Offset);
                }
                else
                {
                    _iconRenderer.sprite = _defaultIcon;
                    _iconRenderer.color = _defaultColor;
                    _iconRenderer.transform.SetLocalScaling(_defaultScaling);
                    _iconRenderer.transform.SetLocalPosition(Vector2.zero);
                }
            }

#endif
        }
    }
}
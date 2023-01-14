using CreativeSpore.SuperTilemapEditor;
using UnityEngine;

namespace Assets.Ancible_Tools.Scripts.Traits
{
    [CreateAssetMenu(fileName = "Tilemap Sprite Trait", menuName = "Ancible Tools/Traits/Animation/Sprite")]
    public class TilemapSpriteTrait : Trait
    {
        [SerializeField] private STETilemap _tilemap;

        private STETilemap _tilemapSprite = null;

        public override void SetupController(TraitController controller)
        {
            base.SetupController(controller);
            _tilemapSprite = Instantiate(_tilemap, _controller.transform.parent);
        }

        public override void Destroy()
        {
            Destroy(_tilemapSprite.gameObject);
            base.Destroy();
        }
    }
}
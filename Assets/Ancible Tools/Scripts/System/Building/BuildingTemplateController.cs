using System.Collections.Generic;
using Assets.Resources.Ancible_Tools.Scripts.System.Animation;
using UnityEngine;

namespace Assets.Resources.Ancible_Tools.Scripts.System.Building
{
    public class BuildingTemplateController : MonoBehaviour
    {
        public WorldBuilding Building { get; private set; }

        [SerializeField] private float _templateAlpha = .66f;

        private SpriteController[] _sprites = new SpriteController[0];

        public void Setup(WorldBuilding building)
        {
            Building = building;
            var sprites = new List<SpriteController>();
            for (var i = 0; i < Building.Sprites.Length; i++)
            {
                var controller = Instantiate(FactoryController.SPRITE_CONTROLLER, transform);
                controller.SetFromTrait(Building.Sprites[i]);
                controller.SetAlpha(_templateAlpha);
                controller.SetSortingLayerFromSpriteLayer(SpriteLayerFactory.Over);
                sprites.Add(controller);
            }
            _sprites = sprites.ToArray();
            gameObject.SetActive(true);
        }

        public void Clear()
        {
            Building = null;
            for (var i = 0; i < _sprites.Length; i++)
            {
                Destroy(_sprites[i].gameObject);
            }
            _sprites = new SpriteController[0];
            gameObject.SetActive(false);
        }
    }
}
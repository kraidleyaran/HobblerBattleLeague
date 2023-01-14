using Assets.Resources.Ancible_Tools.Scripts.System.Culling;
using UnityEngine;

namespace Assets.Ancible_Tools.Scripts.Traits
{
    [CreateAssetMenu(fileName = "Player Culling Trait", menuName = "Ancible Tools/Traits/Culling/Player Culling")]
    public class PlayerCullingTrait : Trait
    {
        [SerializeField] private CullingController _controllerTemplate;

        private CullingController _cullingController = null;

        public override void SetupController(TraitController controller)
        {
            base.SetupController(controller);
            _cullingController = Instantiate(_controllerTemplate, _controller.transform.parent);
            _cullingController.Setup(_instanceId);
        }

        public override void Destroy()
        {
            _cullingController.Destroy();
            Destroy(_cullingController.gameObject);
            base.Destroy();
        }
    }
}
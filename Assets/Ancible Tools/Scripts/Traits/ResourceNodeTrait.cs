using Assets.Ancible_Tools.Scripts.System.WorldNodes;
using Assets.Resources.Ancible_Tools.Scripts.System.Items;
using Assets.Resources.Ancible_Tools.Scripts.System.Pathing;
using UnityEngine;

namespace Assets.Ancible_Tools.Scripts.Traits
{
    [CreateAssetMenu(fileName = "Resource Node Trait", menuName = "Ancible Tools/Traits/Node/Resource Node")]
    public class ResourceNodeTrait : WorldNodeTrait
    {
        [SerializeField] private ItemStack _item;

        protected internal override void ApplyToUnit(GameObject obj)
        {
            base.ApplyToUnit(obj);
            WorldStashController.AddItem(_item.Item, _item.Stack);
        }

        protected internal override void RegisterNode(MapTile tile)
        {
            _registeredNode = WorldNodeManager.RegisterResourceNode(_controller.transform.parent.gameObject, tile, new[]{_item.Item});;
        }

        protected internal override void UnregisterNode()
        {
            WorldNodeManager.UnregisterNode(_controller.transform.parent.gameObject, WorldNodeType.Resource);
            _registeredNode = null;
        }
    }
}
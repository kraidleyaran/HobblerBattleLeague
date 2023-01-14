using UnityEngine;

namespace Assets.Resources.Ancible_Tools.Scripts.System.Items
{
    [CreateAssetMenu(fileName = "Resource Item", menuName = "Ancible Tools/Items/Resource Item")]
    public class ResourceItem : WorldItem
    {
        public override WorldItemType Type => WorldItemType.Resource;
        public int RequiredLevel = 0;

    }
}
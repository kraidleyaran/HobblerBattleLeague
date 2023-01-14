using UnityEngine;

namespace Assets.Resources.Ancible_Tools.Scripts.System.Items
{
    [CreateAssetMenu(fileName = "Gold Item", menuName = "Ancible Tools/Items/Gold Item")]
    public class GoldItem : ResourceItem
    {
        public override WorldItemType Type => WorldItemType.Gold;
    }
}
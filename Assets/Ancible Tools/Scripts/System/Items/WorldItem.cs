using UnityEngine;

namespace Assets.Resources.Ancible_Tools.Scripts.System.Items
{
    [CreateAssetMenu(fileName = "World Item", menuName = "Ancible Tools/Items/World Item")]
    public class WorldItem : ScriptableObject
    {
        public virtual WorldItemType Type => WorldItemType.Generic;

        public string DisplayName;
        [TextArea(3, 10)] public string Description;
        public Sprite Icon;
        public int MaxStack = 1;

        public virtual string GetDescription()
        {
            return Description;
        }
    }
}
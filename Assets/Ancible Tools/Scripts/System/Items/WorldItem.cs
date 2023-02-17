using UnityEngine;

namespace Assets.Resources.Ancible_Tools.Scripts.System.Items
{
    [CreateAssetMenu(fileName = "World Item", menuName = "Ancible Tools/Items/World Item")]
    public class WorldItem : ScriptableObject
    {
        public virtual WorldItemType Type => WorldItemType.Generic;

        public string DisplayName;
        [TextArea(3, 10)] public string Description;
        public virtual Sprite Icon => _icon;
        [SerializeField] private Sprite _icon = null;
        public int MaxStack = 1;
        public int GoldValue = -1;
        public ItemRarity Rarity = ItemRarity.Common;

        public virtual string GetDescription()
        {
            return Description;
        }

        public void SetIcon(Sprite icon)
        {
#if UNITY_EDITOR
            _icon = icon;
#endif
        }
    }
}
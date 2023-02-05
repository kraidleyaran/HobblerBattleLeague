using Assets.Ancible_Tools.Scripts.Traits;
using MessageBusLib;
using UnityEngine;

namespace Assets.Resources.Ancible_Tools.Scripts.System.Items
{
    [CreateAssetMenu(fileName = "Instant Item", menuName = "Ancible Tools/Items/Instant Item")]
    public class InstantItem : WorldItem
    {
        public override WorldItemType Type => WorldItemType.Instant;

        [SerializeField] private Trait[] _applyOnUse = new Trait[0];

        public void Use(GameObject owner)
        {
            var addTraitToUnitMsg = MessageFactory.GenerateAddTraitToUnitMsg();
            foreach (var trait in _applyOnUse)
            {
                addTraitToUnitMsg.Trait = trait;
                owner.SendMessageTo(addTraitToUnitMsg, owner);
            }
            MessageFactory.CacheMessage(addTraitToUnitMsg);
        }
        
    }
}
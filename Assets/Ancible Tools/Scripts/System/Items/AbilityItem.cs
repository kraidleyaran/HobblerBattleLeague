﻿using Assets.Resources.Ancible_Tools.Scripts.System.Abilities;
using UnityEngine;

namespace Assets.Resources.Ancible_Tools.Scripts.System.Items
{
    [CreateAssetMenu(fileName = "Ability Item", menuName = "Ancible Tools/Items/Ability")]
    public class AbilityItem : WorldItem
    {
        public override WorldItemType Type => WorldItemType.Ability;
        public WorldAbility Ability;
        public int RequiredLevel = 0;
        public override Sprite Icon => Ability.Icon;

        public override string GetDescription()
        {
            return Ability.GetDescription();
        }
    }
}
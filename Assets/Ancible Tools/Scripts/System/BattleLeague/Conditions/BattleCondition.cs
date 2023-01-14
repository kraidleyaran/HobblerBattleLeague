using Assets.Resources.Ancible_Tools.Scripts.System.Abilities;
using UnityEngine;

namespace Assets.Ancible_Tools.Scripts.System.BattleLeague.Conditions
{
    
    public class BattleCondition : ScriptableObject
    {
        public virtual bool PassesCondition(GameObject owner, GameObject target)
        {
            return true;
        }
    }
}
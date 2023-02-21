using System;
using Assets.Resources.Ancible_Tools.Scripts.System.TickTimers;
using UnityEngine;

namespace Assets.Resources.Ancible_Tools.Scripts.System.Abilities
{
    [Serializable]
    public class AbilityInstance : WorldInstance<WorldAbility>
    {
        public bool OnCooldown { get; private set; }
        public TickTimer CooldownTimer => _cooldown;

        private TickTimer _cooldown = null;

        public AbilityInstance(WorldAbility ability)
        {
            Instance = ability;
            OnCooldown = false;
            _cooldown = new TickTimer(Instance.Cooldown, 0, CooldownFinish, null, false);
        }

        public void UseAbility(GameObject owner, GameObject target)
        {
            Instance.UseAbility(owner, target);
            OnCooldown = true;
            
            _cooldown.Play();
        }

        private void CooldownFinish()
        {
            OnCooldown = false;
        }

        public override void Destroy()
        {
            if (_cooldown != null)
            {
                _cooldown.Destroy();
                _cooldown = null;
            }

            Instance = null;
            base.Destroy();
        }
    }
}
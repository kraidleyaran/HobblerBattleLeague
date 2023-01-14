using Assets.Ancible_Tools.Scripts.System.UI.StatusBar;
using Assets.Resources.Ancible_Tools.Scripts.System;
using MessageBusLib;
using UnityEngine;

namespace Assets.Ancible_Tools.Scripts.Traits
{
    [CreateAssetMenu(fileName = "Hobbler Status Bar Trait", menuName = "Ancible Tools/Traits/Hobbler/Hobbler Status Bar")]
    public class HobblerStatusBarTrait : Trait
    {
        [SerializeField] private Vector2 _offset = Vector2.zero;

        private MonsterState _monsterState = MonsterState.Idle;

        public override void SetupController(TraitController controller)
        {
            base.SetupController(controller);
            UiHobblerStatusBarManager.RegisterHobbler(_controller.transform.parent.gameObject, _offset);
            SubscribeToMessages();
        }

        private void SubscribeToMessages()
        {
            _controller.transform.parent.gameObject.SubscribeWithFilter<UpdateMonsterStateMessage>(UpdateMonsterState, _instanceId);
        }

        private void UpdateMonsterState(UpdateMonsterStateMessage msg)
        {
            if (msg.State == MonsterState.Resting)
            {
                UiHobblerStatusBarManager.SetStatusBarActive(_controller.transform.parent.gameObject, false);
            }
            else if (_monsterState == MonsterState.Idle)
            {
                UiHobblerStatusBarManager.SetStatusBarActive(_controller.transform.parent.gameObject, true);
            }
        }

        public override void Destroy()
        {
            UiHobblerStatusBarManager.UnregisterHobbler(_controller.transform.parent.gameObject);
            base.Destroy();
        }
    }
}
using Assets.Resources.Ancible_Tools.Scripts.System;
using MessageBusLib;
using UnityEngine;

namespace Assets.Ancible_Tools.Scripts.Traits
{
    [CreateAssetMenu(fileName = "Set Monster State Trait", menuName = "Ancible Tools/Traits/Hobbler/State/Set Monster State")]
    public class SetMonsterStateTrait : Trait
    {
        public override bool Instant => true;

        [SerializeField] private MonsterState _monsterState = MonsterState.Idle;

        public override void SetupController(TraitController controller)
        {
            base.SetupController(controller);
            var setMonsterStateMsg = MessageFactory.GenerateSetMonsterStateMsg();
            setMonsterStateMsg.State = _monsterState;
            _controller.gameObject.SendMessageTo(setMonsterStateMsg, _controller.transform.parent.gameObject);
            MessageFactory.CacheMessage(setMonsterStateMsg);
        }
    }
}
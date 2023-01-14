using Assets.Ancible_Tools.Scripts.System.WorldNodes;
using Assets.Resources.Ancible_Tools.Scripts.System;
using MessageBusLib;
using UnityEngine;

namespace Assets.Ancible_Tools.Scripts.Traits
{
    [CreateAssetMenu(fileName = "Search for Node Trait", menuName = "Ancible Tools/Traits/Node/Search for Node")]
    public class SearchForNodeTrait : Trait
    {
        public override bool Instant => true;

        [SerializeField] private WorldNodeType _nodeType;

        public override void SetupController(TraitController controller)
        {
            base.SetupController(controller);
            var searchForNodeTraitMsg = MessageFactory.GenerateSearchForNodeMsg();
            searchForNodeTraitMsg.Type = _nodeType;
            searchForNodeTraitMsg.DoAfter = SetAiState;
            _controller.gameObject.SendMessageTo(searchForNodeTraitMsg, _controller.transform.parent.gameObject);
            MessageFactory.CacheMessage(searchForNodeTraitMsg);
        }

        private void SetAiState()
        {
            var setHobblerAiStateMsg = MessageFactory.GenerateSetHobblerAiStateMsg();
            setHobblerAiStateMsg.State = HobblerAiState.Command;
            _controller.gameObject.SendMessageTo(setHobblerAiStateMsg, _controller.transform.parent.gameObject);
            MessageFactory.CacheMessage(setHobblerAiStateMsg);
        }
    }
}
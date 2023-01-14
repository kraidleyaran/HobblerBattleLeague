using Assets.Resources.Ancible_Tools.Scripts.System;
using Assets.Resources.Ancible_Tools.Scripts.System.Combat;
using Assets.Resources.Ancible_Tools.Scripts.System.Minigame;
using MessageBusLib;
using UnityEngine;

namespace Assets.Ancible_Tools.Scripts.Traits
{
    [CreateAssetMenu(fileName = "Minigame Player Interaction Trait", menuName = "Ancible Tools/Traits/Minigame/Player/Minigame Player Interaction")]
    public class MinigamePlayerInteractionTrait : Trait
    {
        private MinigameUnitState _unitState = MinigameUnitState.Idle;

        public override void SetupController(TraitController controller)
        {
            base.SetupController(controller);
            SubscribeToMessages();
        }

        private void SubscribeToMessages()
        {
            _controller.transform.parent.gameObject.SubscribeWithFilter<ObstacleMessage>(Obstacle, _instanceId);
            _controller.transform.parent.gameObject.SubscribeWithFilter<UpdateMinigameUnitStateMessage>(UpdateMinigameUnitState, _instanceId);
        }

        private void Obstacle(ObstacleMessage msg)
        {
            if (_unitState == MinigameUnitState.Idle)
            {
                var alignment = CombatAlignment.Neutral;
                var queryCombatAlignmentMsg = MessageFactory.GenerateQueryCombatAlignmentMsg();
                queryCombatAlignmentMsg.DoAfter = objAlignment => alignment = objAlignment;
                _controller.gameObject.SendMessageTo(queryCombatAlignmentMsg, msg.Obstacle);
                MessageFactory.CacheMessage(queryCombatAlignmentMsg);

                switch (alignment)
                {
                    case CombatAlignment.Player:
                        break;
                    case CombatAlignment.Enemy:
                        var doBasicAttackMsg = MessageFactory.GenerateDoBasicAttackMsg();
                        doBasicAttackMsg.Target = msg.Obstacle;
                        doBasicAttackMsg.Direction = msg.Direction.ToVector2Int();
                        _controller.gameObject.SendMessageTo(doBasicAttackMsg, _controller.transform.parent.gameObject);
                        MessageFactory.CacheMessage(doBasicAttackMsg);
                        break;
                    case CombatAlignment.Neutral:
                        var minigameInteractMsg = MessageFactory.GenerateMinigameInteractMsg();
                        minigameInteractMsg.Owner = _controller.transform.parent.gameObject;
                        minigameInteractMsg.Direction = msg.Direction;
                        _controller.gameObject.SendMessageTo(minigameInteractMsg, msg.Obstacle);
                        MessageFactory.CacheMessage(minigameInteractMsg);
                        break;
                }

            }

        }

        private void UpdateMinigameUnitState(UpdateMinigameUnitStateMessage msg)
        {
            _unitState = msg.State;
        }
    }
}
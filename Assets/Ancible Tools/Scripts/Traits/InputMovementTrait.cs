using Assets.Resources.Ancible_Tools.Scripts.System;
using MessageBusLib;
using UnityEngine;

namespace Assets.Ancible_Tools.Scripts.Traits
{
    [CreateAssetMenu(fileName = "Input Movement Trait", menuName = "Ancible Tools/Traits/Input/Input Movement")]
    public class InputMovementTrait : Trait
    {
        [SerializeField] private bool _allowDiagonals = false;

        private Vector2Int _previousDirection = Vector2Int.zero;

        public override void SetupController(TraitController controller)
        {
            base.SetupController(controller);
            SubscribeToMessages();
        }

        protected internal virtual void SubscribeToMessages()
        {
            _controller.gameObject.Subscribe<UpdateInputStateMessage>(UpdateInputState);
        }

        protected internal virtual void UpdateInputState(UpdateInputStateMessage msg)
        {
            var direction = Vector2Int.zero;
            if (_allowDiagonals)
            {
                if (msg.Current.Up || msg.Current.Down)
                {
                    if (msg.Current.Up && (_previousDirection.y != 1 || !msg.Current.Down))
                    {
                        direction.y = 1;
                    }
                    else if (msg.Current.Down && (_previousDirection.y != -1 || !msg.Current.Up))
                    {
                        direction.y = -1;
                    }
                }

                if (msg.Current.Left || msg.Current.Right)
                {
                    if (msg.Current.Right && (_previousDirection.x != 1 || !msg.Current.Left))
                    {
                        direction.x = 1;
                    }
                    else if (msg.Current.Left && (_previousDirection.x != -1 || !msg.Current.Right))
                    {
                        direction.y = -1;
                    }
                }
            }
            else
            {
                if (msg.Current.Up)
                {
                    direction = Vector2Int.up;
                }
                else if (msg.Current.Down)
                {
                    direction = Vector2Int.down;
                }
                else if (msg.Current.Left)
                {
                    direction = Vector2Int.left;
                }
                else if (msg.Current.Right)
                {
                    direction = Vector2Int.right;
                }
            }

            if (_previousDirection != direction)
            {
                var setDirectionMsg = MessageFactory.GenerateSetDirectionMsg();
                setDirectionMsg.Direction = direction;
                _controller.gameObject.SendMessageTo(setDirectionMsg, _controller.transform.parent.gameObject);
                MessageFactory.CacheMessage(setDirectionMsg);
                _previousDirection = direction;
            }
        }

        
    }
}
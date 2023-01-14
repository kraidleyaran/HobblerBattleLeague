using MessageBusLib;
using UnityEngine;
using UnityEngine.InputSystem;

namespace Assets.Resources.Ancible_Tools.Scripts.System.Input
{
    public class WorldInputController : MonoBehaviour
    {
        private static WorldInputController _instance = null;

        [SerializeField] private Key _up = Key.W;
        [SerializeField] private Key _down = Key.S;
        [SerializeField] private Key _left = Key.A;
        [SerializeField] private Key _right = Key.D;
        [SerializeField] private Key _interact = Key.Space;
        [SerializeField] private Key _stash = Key.I;
        [SerializeField] private Key _build = Key.B;
        [SerializeField] private Key _roster = Key.R;

        [Header("Minigame")]
        [SerializeField] private Key _attack = Key.Digit1;
        [SerializeField] private Key _abilitySlot0 = Key.Digit2;
        [SerializeField] private Key _abilitySlot1 = Key.Digit3;
        [SerializeField] private Key _abilitySlot2 = Key.Digit4;
        [SerializeField] private Key _abilitySlot3 = Key.Digit5;

        private WorldInputState _previous = new WorldInputState();
        private UpdateInputStateMessage _updateInputStateMsg = new UpdateInputStateMessage();

        void Awake()
        {
            if (_instance)
            {
                Destroy(gameObject);
                return;
            }

            _instance = this;
            SubscribeToMessages();
        }

        private void SubscribeToMessages()
        {
            gameObject.Subscribe<UpdateTickMessage>(UpdateTick);
        }

        private void UpdateTick(UpdateTickMessage msg)
        {
            var current = new WorldInputState
            {
                MousePos = Mouse.current.position.ReadValue(),
                MouseDelta = Mouse.current.delta.ReadValue(),
                ScrollDelta = Mouse.current.scroll.ReadValue(),
                LeftClick = Mouse.current.leftButton.isPressed,
                RightClick = Mouse.current.rightButton.isPressed,
                Up = Keyboard.current[_up].isPressed,
                Down = Keyboard.current[_down].isPressed,
                Left = Keyboard.current[_left].isPressed,
                Right = Keyboard.current[_right].isPressed,
                Stash = Keyboard.current[_stash].isPressed,
                Build = Keyboard.current[_build].isPressed,
                Attack = Keyboard.current[_attack].isPressed,
                Roster = Keyboard.current[_roster].isPressed,
                Interact = Keyboard.current[_interact].isPressed,
                AbilitySlot0 = Keyboard.current[_abilitySlot0].isPressed,
                AbilitySlot1 = Keyboard.current[_abilitySlot1].isPressed,
                AbilitySlot2 = Keyboard.current[_abilitySlot2].isPressed,
                AbilitySlot3 = Keyboard.current[_abilitySlot3].isPressed
            };
            _updateInputStateMsg.Current = current;
            _updateInputStateMsg.Previous = _previous;
            gameObject.SendMessage(_updateInputStateMsg);
            _previous = current;
        }

        void OnDestroy()
        {
            gameObject.UnsubscribeFromAllMessages();
        }
    }
}
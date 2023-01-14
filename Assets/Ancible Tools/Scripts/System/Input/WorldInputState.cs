using UnityEngine;

namespace Assets.Resources.Ancible_Tools.Scripts.System.Input
{
    public struct WorldInputState
    {
        public Vector2 MousePos;
        public Vector2 MouseDelta;
        public Vector2 ScrollDelta;
        public bool LeftClick;
        public bool RightClick;

        public bool Up;
        public bool Down;
        public bool Left;
        public bool Right;
        public bool Interact;

        public bool Stash;
        public bool Build;
        public bool Roster;

        public bool Attack;
        public bool AbilitySlot0;
        public bool AbilitySlot1;
        public bool AbilitySlot2;
        public bool AbilitySlot3;

    }
}
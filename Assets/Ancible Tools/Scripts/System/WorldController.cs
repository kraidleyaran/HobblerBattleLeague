﻿using Assets.Resources.Ancible_Tools.Scripts.System.Adventure;
using Assets.Resources.Ancible_Tools.Scripts.System.BattleLeague;
using Assets.Resources.Ancible_Tools.Scripts.System.Minigame;
using Assets.Resources.Ancible_Tools.Scripts.System.Pathing;
using MessageBusLib;
using UnityEngine;

namespace Assets.Resources.Ancible_Tools.Scripts.System
{
    public class WorldController : MonoBehaviour
    {
        public static WorldState State { get; private set; }
        public static PathingGridController Pathing => _instance._pathing;
        public static Transform Transform => _instance.transform;
        private static WorldController _instance = null;

        private static UpdateWorldStateMessage _updateWorldStateMsg = new UpdateWorldStateMessage();

        [SerializeField] private PathingGridController _pathing;

        void Awake()
        {
            if (_instance)
            {
                Destroy(gameObject);
                return;
            }

            _instance = this;
            _pathing.Setup();
        }


        public static void SetWorldState(WorldState state)
        {
            if (state != State)
            {
                State = state;
                _updateWorldStateMsg.State = State;
                _instance.gameObject.SendMessage(_updateWorldStateMsg);
            }

        }

        public static Camera GetCurrentCamera()
        {
            switch (State)
            {
                case WorldState.World:
                    return WorldCameraController.Camera;
                case WorldState.Minigame:
                    return MinigameCameraController.Camera;
                case WorldState.Battle:
                    return BattleLeagueCameraController.Camera;
                case WorldState.Adventure:
                    return AdventureCameraController.Camera;
            }
            return WorldCameraController.Camera;
        }
    }
}
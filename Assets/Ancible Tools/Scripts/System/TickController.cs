using Assets.Resources.Ancible_Tools.Scripts.System.TickTimers;
using MessageBusLib;
using UnityEngine;

namespace Assets.Resources.Ancible_Tools.Scripts.System
{
    public class TickController : MonoBehaviour
    {
        public static float TickRate => 1f / Application.targetFrameRate;
        public static float OneSecond => TickRate * Application.targetFrameRate;
        public static float WorldTickRate => TickRate * _instance._ticksPerWorldTick;

        private static TickController _instance = null;

        [SerializeField] private int _ticksPerWorldTick = 1;

        private TickTimer _worldTickTimer = null;

        void Awake()
        {
            if (_instance)
            {
                Destroy(gameObject);
                return;
            }
            
            _instance = this;
        }

        void Start()
        {
            _worldTickTimer = new TickTimer(_ticksPerWorldTick, -1, WorldTick, null, false);
        }

        private void WorldTick()
        {
            gameObject.SendMessage(WorldTickMessage.INSTANCE);
        }

        void Update()
        {
            gameObject.SendMessage(UpdateTickMessage.INSTANCE);
        }

        void FixedUpdate()
        {
            gameObject.SendMessage(FixedUpdateTickMessage.INSTANCE);
        }

        void LateUpdate()
        {
            gameObject.SendMessage(LateTickUpdateMessage.INSTANCE);
        }

        void OnDestroy()
        {
            _worldTickTimer?.Destroy();
            _worldTickTimer = null;
        }
    }
}
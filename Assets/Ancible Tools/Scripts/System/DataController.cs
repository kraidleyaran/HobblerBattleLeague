using MessageBusLib;
using UnityEngine;

namespace Assets.Resources.Ancible_Tools.Scripts.System
{
    public class DataController : MonoBehaviour
    {
        public static float Interpolation => _instance._interpoliation;
        public static Vector2 TrueZero => new Vector2(Interpolation / 2f, Interpolation / 2f);
        public static int MaxHobblerAbilities => _instance._maxHobblerAbilities;

        private static DataController _instance = null;

        [SerializeField] private int _framesPerSecond = 60;
        [SerializeField] private bool _permanent = false;
        [SerializeField] private float _interpoliation = .3f;
        [SerializeField] private int _maxHobblerAbilities = 4;
        

        void Awake()
        {
            if (_instance)
            {
                Destroy(gameObject);
                return;
            }

            _instance = this;
            //In order to force a frame count, we have to set the vSyncCount to 0 or it will ignore the targetFrameRate varaible;
            QualitySettings.vSyncCount = 0;
            //This forces unity to make sure that FixedUpdate ticks are equally distributed within a given Update tick always instead of sometimes being less or more
            Time.fixedDeltaTime = 1f / _framesPerSecond;
            Application.targetFrameRate = _framesPerSecond;
            if (_permanent)
            {
                DontDestroyOnLoad(gameObject);
            }

        }

        public static bool Roll()
        {
            return Random.Range(0f, 1f) > .5f;
        }

        void OnDestroy()
        {
            gameObject.UnsubscribeFromAllMessages();
        }
    }
}
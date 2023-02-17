using UnityEngine;

namespace Assets.Resources.Ancible_Tools.Scripts.System
{
    public class IconFactoryController : MonoBehaviour
    {
        private static IconFactoryController _instance = null;

        public static Sprite DefaultBasicAttack => _instance._defaultBasicAttackIcon;
        public static Sprite Gold => _instance._goldIcon;
        public static Sprite DefaultAlertIcon => _instance._defaultAlertIcon;

        [SerializeField] private Sprite _defaultBasicAttackIcon = null;
        [SerializeField] private Sprite _goldIcon = null;
        [SerializeField] private Sprite _defaultAlertIcon = null;

        void Awake()
        {
            if (_instance)
            {
                Destroy(gameObject);
                return;
            }

            _instance = this;
        }

        
    }
}
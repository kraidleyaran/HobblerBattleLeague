using Assets.Resources.Ancible_Tools.Scripts.Hitbox;
using UnityEngine;

namespace Assets.Resources.Ancible_Tools.Scripts.System
{
    public class CollisionLayerFactory : MonoBehaviour
    {
        public static CollisionLayer UnitSelect => _instance._unitSelect;
        public static CollisionLayer Minigame => _instance._minigame;
        public static CollisionLayer Culling => _instance._culling;
        public static CollisionLayer UnitCulling => _instance._unitCulling;
        public static CollisionLayer BattleSelect => _instance._battleSelect;
        public static CollisionLayer MinigameSelect => _instance._minigameSelect;

        private static CollisionLayerFactory _instance = null;

        [SerializeField] private CollisionLayer _unitSelect = null;
        [SerializeField] private CollisionLayer _minigame = null;
        [SerializeField] private CollisionLayer _culling = null;
        [SerializeField] private CollisionLayer _unitCulling = null;
        [SerializeField] private CollisionLayer _battleSelect = null;
        [SerializeField] private CollisionLayer _minigameSelect = null;

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
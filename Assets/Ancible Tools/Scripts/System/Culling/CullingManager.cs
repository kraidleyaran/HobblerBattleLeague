using System.Collections.Generic;
using UnityEngine;

namespace Assets.Resources.Ancible_Tools.Scripts.System.Culling
{
    public class CullingManager : MonoBehaviour
    {
        public static CulledUnitController CulledUnit => _instance._culledUnitTemplate;

        private static CullingManager _instance = null;

        private static List<CullingController> _controllers = new List<CullingController>();

        [SerializeField] private CulledUnitController _culledUnitTemplate;

        void Awake()
        {
            if (_instance)
            {
                Destroy(gameObject);
                return;
            }

            _instance = this;
        }

        public static bool IsCulled(Collider2D col)
        {
            var controllers = _controllers.ToArray();
            for (var i = 0; i < controllers.Length; i++)
            {
                if (controllers[i].ContainsObject(col))
                {
                    return false;
                }
            }
            return true;
        }

        public static void RegisterController(CullingController controller)
        {
            if (!_controllers.Contains(controller))
            {
                _controllers.Add(controller);
            }
        }

        public static void UnregisterController(CullingController controller)
        {
            _controllers.Remove(controller);
        }
    }
}
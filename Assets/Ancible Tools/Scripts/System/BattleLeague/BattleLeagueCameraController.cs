using UnityEngine;

namespace Assets.Resources.Ancible_Tools.Scripts.System.BattleLeague
{
    public class BattleLeagueCameraController : MonoBehaviour
    {
        public static Camera Camera => _instance._camera;

        private static BattleLeagueCameraController _instance = null;

        [SerializeField] private Camera _camera = null;

        public void WakeUp()
        {
            if (_instance)
            {
                Destroy(gameObject);
                return;
            }

            _instance = this;
            gameObject.SetActive(false);
        }

        public static void SetActive(bool active)
        {
            _instance.gameObject.SetActive(active);
        }
    }
}
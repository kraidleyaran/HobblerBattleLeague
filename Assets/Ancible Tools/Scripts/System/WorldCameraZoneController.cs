using System.Linq;
using CreativeSpore.SuperTilemapEditor;
using UnityEngine;

namespace Assets.Resources.Ancible_Tools.Scripts.System
{
    public class WorldCameraZoneController : MonoBehaviour
    {
        private static WorldCameraZoneController _instance = null;

        [SerializeField] private STETilemap _tilemap;

        private Collider2D[] _cameraZones = new Collider2D[0];

        void Awake()
        {
            if (_instance)
            {
                Destroy(gameObject);
                return;
            }

            _instance = this;
        }

        public void Setup(Vector2Int[] tiles)
        {
            _tilemap.ClearMap();
            foreach (var tile in tiles)
            {
                _tilemap.SetTile(tile.x, tile.y, 0);
            }
            _tilemap.Refresh(true,true,true,true);
            StartCoroutine(StaticMethods.WaitForFrames(1, () => { _cameraZones = gameObject.GetComponentsInChildren<Collider2D>(); }));

        }

        public static Vector2 GetClosestPosition(Vector2 pos)
        {
            var closestMag = -1f;
            var closestPos = Vector2.zero;
            foreach (var zone in _instance._cameraZones)
            {
                if (zone.OverlapPoint(pos))
                {
                    return pos;
                }

                var closePos = zone.ClosestPoint(pos);
                var mag = (pos - closePos).sqrMagnitude;
                if (closestMag < 0f || mag < closestMag)
                {
                    closestMag = mag;
                    closestPos = closePos;
                }

            }
            return closestPos;
        }
    }
}
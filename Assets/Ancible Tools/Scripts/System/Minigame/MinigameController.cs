using System.Collections.Generic;
using Assets.Resources.Ancible_Tools.Scripts.System.Minigame.Settings;
using Assets.Resources.Ancible_Tools.Scripts.System.Pathing;
using MessageBusLib;
using UnityEngine;

namespace Assets.Resources.Ancible_Tools.Scripts.System.Minigame
{
    public class MinigameController : MonoBehaviour
    {
        public static PathingGridController Pathing { get; private set; }
        public static Transform Transform => _instance.transform;

        private static MinigameController _instance = null;

        private List<GameObject> _registeredObjects = new List<GameObject>();

        public virtual void Setup(MinigameSettings settings, GameObject proxy)
        {
            if (_instance)
            {
                Destroy(_instance.gameObject);
            }
            _instance = this;
        }

        public static void SetPathingGrid(PathingGridController pathing)
        {
            Pathing = pathing;
        }

        public static void RegisterMinigameObject(GameObject obj)
        {
            if (!_instance._registeredObjects.Contains(obj))
            {
                _instance._registeredObjects.Add(obj);
            }
        }

        public static void UnregisterMinigameObject(GameObject obj)
        {
            if (_instance._registeredObjects.Contains(obj))
            {
                _instance._registeredObjects.Remove(obj);
                Destroy(obj);
            }
        }

        public static void CullingCheck()
        {
            var objs = _instance._registeredObjects.ToArray();
            for (var i = 0; i < objs.Length; i++)
            {
                _instance.gameObject.SendMessageTo(CullingCheckMessage.INSTANCE, objs[i]);
            }
        }

        public virtual void Destroy()
        {
            gameObject.SendMessage(DespawnMinigameUnitsMessage.INSTANCE);
            var objs = _registeredObjects.ToArray();
            for (var i = 0; i < objs.Length; i++)
            {
                if (objs[i])
                {
                    Destroy(objs[i]);
                }
            }
            _registeredObjects.Clear();
            if (Pathing)
            {
                Destroy(Pathing.gameObject);
            }

            Pathing = null;

        }
    }
}
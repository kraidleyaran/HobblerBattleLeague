using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Assets.Resources.Ancible_Tools.Scripts.System
{
    public class TraitFactory : MonoBehaviour
    {
        private static TraitFactory _instance = null;

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
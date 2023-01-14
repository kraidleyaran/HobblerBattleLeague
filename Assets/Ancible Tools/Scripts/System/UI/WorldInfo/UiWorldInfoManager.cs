using Assets.Resources.Ancible_Tools.Scripts.System;
using Assets.Resources.Ancible_Tools.Scripts.System.Windows;
using MessageBusLib;
using UnityEngine;

namespace Assets.Ancible_Tools.Scripts.System.UI.WorldInfo
{
    public class UiWorldInfoManager : UiBaseWindow
    {
        private static UiWorldInfoManager _instance = null;

        public override bool Movable => false;
        public override bool Static => true;

        public override void Awake()
        {
            if (_instance)
            {
                Destroy(gameObject);
                return;
            }

            _instance = this;
            base.Awake();
        }

        public static void SetActive(bool active)
        {
            _instance.gameObject.SetActive(active);
        }

        void OnDestroy()
        {
            if (_instance && _instance == this)
            {
                _instance = null;
            }
        }
    }

}
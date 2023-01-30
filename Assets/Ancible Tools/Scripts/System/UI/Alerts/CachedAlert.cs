using System;
using UnityEngine;
using UnityEngine.UI;

namespace Assets.Resources.Ancible_Tools.Scripts.System.UI.Alerts
{
    public class CachedAlert : IDisposable
    {
        public string Text;
        public Sprite Icon;

        public CachedAlert(string text, Sprite icon)
        {
            Text = text;
            Icon = icon;
        }

        public void Destroy()
        {
            Text = null;
            Icon = null;
        }

        public void Dispose()
        {
            Destroy();
        }
    }
}
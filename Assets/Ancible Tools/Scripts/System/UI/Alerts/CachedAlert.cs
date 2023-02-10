using System;
using UnityEngine;
using UnityEngine.UI;

namespace Assets.Resources.Ancible_Tools.Scripts.System.UI.Alerts
{
    public class CachedAlert : IDisposable
    {
        public string Text;
        public Sprite Icon;
        public Color BorderColor = Color.white;

        public CachedAlert(string text, Sprite icon, Color color)
        {
            Text = text;
            Icon = icon;
            BorderColor = color;
        }

        public void Destroy()
        {
            Text = null;
            Icon = null;
            BorderColor = Color.white;
        }

        public void Dispose()
        {
            Destroy();
        }
    }
}
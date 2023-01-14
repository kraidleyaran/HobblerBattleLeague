using Assets.Ancible_Tools.Scripts.Traits;
using Assets.Resources.Ancible_Tools.Scripts.System.Windows;
using UnityEngine;

namespace Assets.Resources.Ancible_Tools.Scripts.System.Minigame.Settings
{
    
    public class MinigameSettings : ScriptableObject
    {
        public virtual MinigameType Type => MinigameType.Maze;
        public Trait[] ApplyOnVictory = new Trait[0];
        public Trait[] ApplyOnDefeat = new Trait[0];
        public UiBaseWindow[] MinigameWindows = new UiBaseWindow[0];
    }
}
using Assets.Resources.Ancible_Tools.Scripts.System.Maze;
using Assets.Resources.Ancible_Tools.Scripts.System.Minigame.Settings;
using MessageBusLib;
using UnityEngine;

namespace Assets.Resources.Ancible_Tools.Scripts.System.Minigame
{
    public class MazeMinigameController : MinigameController
    {
        [SerializeField] private WorldMazeController _mazeController = null;

        public override void Setup(MinigameSettings settings, GameObject proxy)
        {
            base.Setup(settings, proxy);
            if (settings is MazeMinigameSettings mazeSettings)
            {
                _mazeController.Setup(mazeSettings, proxy);
                gameObject.SendMessage(MinigameSpawnUnitsMessage.INSTANCE);
            }
        }
    }
}
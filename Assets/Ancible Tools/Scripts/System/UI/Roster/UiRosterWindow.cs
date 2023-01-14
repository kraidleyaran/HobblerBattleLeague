using Assets.Resources.Ancible_Tools.Scripts.System.Adventure;
using Assets.Resources.Ancible_Tools.Scripts.System.BattleLeague;
using Assets.Resources.Ancible_Tools.Scripts.System.Windows;

namespace Assets.Resources.Ancible_Tools.Scripts.System.UI.Roster
{
    public class UiRosterWindow : UiBaseWindow
    {
        public override bool Movable => true;
        public override bool Static => true;

        public void Battle()
        {
            WorldAdventureController.Setup(WorldAdventureController.Default, WorldAdventureController.Default.DefaultTile);
        }
    }
}
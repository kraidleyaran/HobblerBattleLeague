using System.Collections.Generic;
using Assets.Resources.Ancible_Tools.Scripts.System;
using Assets.Resources.Ancible_Tools.Scripts.System.BattleLeague;
using UnityEngine;
using UnityEngine.UI;

namespace Assets.Ancible_Tools.Scripts.System.UI.BattleLeague.Roster
{
    public class UiRosterController : MonoBehaviour
    {
        [SerializeField] private GridLayoutGroup _grid;
        [SerializeField] private Color _rosterColor = Color.white;

        private UiRosterUnitController[] _unitControllers = new UiRosterUnitController[0];

        public void Setup(BattleUnitData[] data)
        {
            var controllers = new List<UiRosterUnitController>();
            for (var i = 0; i < data.Length; i++)
            {
                var controller = Instantiate(FactoryController.ROSTER_UNIT_CONTROLLER, _grid.transform);
                controller.Setup(data[i], _rosterColor);
                controllers.Add(controller);
            }

            _unitControllers = controllers.ToArray();
        }
    }
}